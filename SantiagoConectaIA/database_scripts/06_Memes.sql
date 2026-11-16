-- ==========================================================================================
-- 06 - MEMES CON IA (pestana "Memes" de Publicaciones en la PWA)
-- Ejecutar ANTES de publicar el API (el API lee y escribe esta tabla).
--
-- Las plantillas se leen de createa.meme (solo sus endpoints GRATUITOS de plantillas y
-- tendencias). La imagen final la genera Gemini con el mismo flujo de edicion de imagenes
-- de las publicaciones (parametro gemini.imagen).
--
-- Cambios:
--   1. Tabla [SCIA].[MemesCreados]: una fila por cada meme creado con IA. Sirve para el
--      historial ("Memes recientes") y para el limite diario.
--   2. [SCIA].[spSaveMemeCreado], [spGetMemesCreados], [spGetMemesResumen], [spDescartarMeme].
--   3. Parametros:
--        memes.plantillas    Valor1 = URL base de la API (termina en /v1)   Valor2 = API KEY (secreta)
--        memes.limite.diario Valor1 = maximo de memes por dia (todos los administradores), 10
--
-- DESPUES de ejecutarlo captura la llave gratuita de https://dashboard.createa.meme
-- (empieza con cam_live_) en Valor2 del parametro memes.plantillas:
--   UPDATE SCIA.Parametros SET nvchValor2 = N'<llave>' WHERE nvchAlias = N'memes.plantillas';
--
-- "Hoy" se calcula con la zona 'Central Standard Time (Mexico)'. Es seguro ejecutarlo mas de una vez.
-- ==========================================================================================
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- ------------------------------------------------------------------------------------------
-- 1. Tabla
-- ------------------------------------------------------------------------------------------
IF OBJECT_ID(N'[SCIA].[MemesCreados]', N'U') IS NULL
BEGIN
    CREATE TABLE [SCIA].[MemesCreados]
    (
        iIdMeme             INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_MemesCreados PRIMARY KEY,
        vchPlantillaId      VARCHAR(80)    NOT NULL,
        nvchPlantillaNombre NVARCHAR(120)  NOT NULL
            CONSTRAINT DF_MemesCreados_PlantillaNombre DEFAULT (N''),
        vchPlantillaUrl     NVARCHAR(600)  NOT NULL
            CONSTRAINT DF_MemesCreados_PlantillaUrl DEFAULT (N''),
        nvchIdea            NVARCHAR(500)  NOT NULL,
        vchResultadoUrl     NVARCHAR(1000) NOT NULL,
        iIdUsuario          INT            NULL,
        bDescartado         BIT            NOT NULL
            CONSTRAINT DF_MemesCreados_Descartado DEFAULT (0),
        dtCreacion          DATETIME2(0)   NOT NULL
            CONSTRAINT DF_MemesCreados_Creacion DEFAULT (SYSUTCDATETIME()),     -- UTC
        dtFechaLocal        DATE           NOT NULL                              -- dia en hora de Mexico
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MemesCreados_FechaLocal'
               AND object_id = OBJECT_ID(N'[SCIA].[MemesCreados]'))
    CREATE INDEX IX_MemesCreados_FechaLocal ON [SCIA].[MemesCreados] (dtFechaLocal);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MemesCreados_Recientes'
               AND object_id = OBJECT_ID(N'[SCIA].[MemesCreados]'))
    CREATE INDEX IX_MemesCreados_Recientes ON [SCIA].[MemesCreados] (bDescartado, iIdMeme DESC);
GO

-- ------------------------------------------------------------------------------------------
-- 2. Procedimientos
-- ------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE [SCIA].[spSaveMemeCreado]
(
    @vchPlantillaId      VARCHAR(80),
    @nvchPlantillaNombre NVARCHAR(120)  = NULL,
    @vchPlantillaUrl     NVARCHAR(600)  = NULL,
    @nvchIdea            NVARCHAR(500),
    @vchResultadoUrl     NVARCHAR(1000),
    @iIdUsuario          INT            = NULL
)
AS
/*
** Proposito: Registra un meme creado con IA.
*/
BEGIN
    SET NOCOUNT ON;

    DECLARE @Id INT = 0;
    DECLARE @Ahora DATETIME2(0) = CAST(SYSUTCDATETIME() AS DATETIME2(0));

    BEGIN TRY
        IF ISNULL(LTRIM(RTRIM(@vchPlantillaId)), '') = '' OR ISNULL(LTRIM(RTRIM(@vchResultadoUrl)), N'') = N''
        BEGIN
            SELECT CAST(0 AS BIT) AS bResult, CAST('Faltan la plantilla o la imagen del meme.' AS VARCHAR(500)) AS vchMessage, 0 AS iIdMeme;
            RETURN;
        END;

        INSERT INTO [SCIA].[MemesCreados]
            (vchPlantillaId, nvchPlantillaNombre, vchPlantillaUrl, nvchIdea, vchResultadoUrl, iIdUsuario, dtCreacion, dtFechaLocal)
        VALUES
            (@vchPlantillaId, ISNULL(@nvchPlantillaNombre, N''), ISNULL(@vchPlantillaUrl, N''), @nvchIdea, @vchResultadoUrl, NULLIF(@iIdUsuario, 0), @Ahora,
             CAST((@Ahora AT TIME ZONE 'UTC') AT TIME ZONE 'Central Standard Time (Mexico)' AS DATE));

        SET @Id = CAST(SCOPE_IDENTITY() AS INT);

        SELECT CAST(1 AS BIT) AS bResult, CAST('Ok' AS VARCHAR(500)) AS vchMessage, @Id AS iIdMeme;
    END TRY
    BEGIN CATCH
        SELECT CAST(0 AS BIT) AS bResult, CAST(LEFT(ERROR_MESSAGE(), 500) AS VARCHAR(500)) AS vchMessage, 0 AS iIdMeme;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE [SCIA].[spGetMemesCreados]
(
    @iTop INT = 12
)
AS
/*
** Proposito: Memes recientes (sin los descartados), los mas nuevos primero. Compartidos por todos los administradores.
*/
BEGIN
    SET NOCOUNT ON;

    IF @iTop IS NULL OR @iTop < 1 SET @iTop = 12;
    IF @iTop > 100 SET @iTop = 100;

    SELECT TOP (@iTop)
        CAST(1 AS BIT) AS bResult,
        CAST('' AS VARCHAR(500)) AS vchMessage,
        m.iIdMeme,
        m.vchPlantillaId,
        m.nvchPlantillaNombre,
        m.vchPlantillaUrl,
        m.nvchIdea,
        m.vchResultadoUrl,
        m.dtCreacion
    FROM [SCIA].[MemesCreados] m
    WHERE m.bDescartado = 0
    ORDER BY m.iIdMeme DESC;
END;
GO

CREATE OR ALTER PROCEDURE [SCIA].[spGetMemesResumen]
AS
/*
** Proposito: Cuantos memes se crearon hoy (hora de Mexico). Los descartados tambien cuentan: ya se gasto la IA.
*/
BEGIN
    SET NOCOUNT ON;

    DECLARE @Hoy DATE = CAST((SYSUTCDATETIME() AT TIME ZONE 'UTC') AT TIME ZONE 'Central Standard Time (Mexico)' AS DATE);

    SELECT
        CAST(1 AS BIT) AS bResult,
        CAST('' AS VARCHAR(500)) AS vchMessage,
        (SELECT COUNT(*) FROM [SCIA].[MemesCreados] WHERE dtFechaLocal = @Hoy) AS iUsoHoy;
END;
GO

CREATE OR ALTER PROCEDURE [SCIA].[spDescartarMeme]
(
    @iIdMeme INT
)
AS
/*
** Proposito: Quita un meme del historial (no se borra: sigue contando para el limite diario).
*/
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        UPDATE [SCIA].[MemesCreados] SET bDescartado = 1 WHERE iIdMeme = @iIdMeme;

        IF @@ROWCOUNT = 0
            SELECT CAST(0 AS BIT) AS bResult, CAST('No se encontro el meme.' AS VARCHAR(500)) AS vchMessage;
        ELSE
            SELECT CAST(1 AS BIT) AS bResult, CAST('Ok' AS VARCHAR(500)) AS vchMessage;
    END TRY
    BEGIN CATCH
        SELECT CAST(0 AS BIT) AS bResult, CAST(LEFT(ERROR_MESSAGE(), 500) AS VARCHAR(500)) AS vchMessage;
    END CATCH
END;
GO

-- ------------------------------------------------------------------------------------------
-- 3. Parametros (solo se insertan los que faltan)
-- ------------------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM [SCIA].[Parametros] WHERE nvchAlias = N'memes.plantillas')
    INSERT INTO [SCIA].[Parametros]
        (iIdParametroPadre, nvchAlias, nvchNombre, nvchNombreEN, nvchDescripcion, nvchDescripcionEN, iSecuencia, bTieneValores, nvchValor1, nvchValor2, bHabilitado)
    VALUES
        (NULL, N'memes.plantillas', N'Plantillas de memes (createa.meme)', N'Meme templates (createa.meme)',
         N'Valor1 = URL base de la API (termina en /v1, sin diagonal final). Valor2 = API key gratuita de dashboard.createa.meme (cam_live_...). Solo lista plantillas y tendencias.',
         N'Value1 = API base URL. Value2 = free API key.',
         2, 1, N'https://api.createa.meme/v1', N'', 1);
GO

IF NOT EXISTS (SELECT 1 FROM [SCIA].[Parametros] WHERE nvchAlias = N'memes.limite.diario')
    INSERT INTO [SCIA].[Parametros]
        (iIdParametroPadre, nvchAlias, nvchNombre, nvchNombreEN, nvchDescripcion, nvchDescripcionEN, iSecuencia, bTieneValores, nvchValor1, nvchValor2, bHabilitado)
    VALUES
        (NULL, N'memes.limite.diario', N'Limite diario de memes con IA', N'Daily AI meme limit',
         N'Maximo de memes que se pueden crear por dia entre todos los administradores (Valor1). Cada meme gasta una generacion de imagen de Gemini.',
         N'Maximum memes per day for all administrators (Value1).',
         2, 1, N'10', NULL, 1);
GO

-- Consultas utiles:
--   EXEC SCIA.spGetMemesResumen;
--   EXEC SCIA.spGetMemesCreados @iTop = 20;
--   SELECT dtFechaLocal, COUNT(*) AS iMemes FROM SCIA.MemesCreados GROUP BY dtFechaLocal ORDER BY dtFechaLocal DESC;
