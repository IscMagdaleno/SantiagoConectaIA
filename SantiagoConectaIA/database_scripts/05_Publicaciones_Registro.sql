-- ==========================================================================================
-- 05 - REGISTRO DE PUBLICACIONES (noticias y emprendimientos publicados en Facebook)
-- Ejecutar ANTES de publicar el API (el API ya lee y escribe esta tabla).
--
-- Antes, "que se publico" vivia en un archivo App_Data/publicacion-automatica.json que un deploy
-- o un reinicio puede borrar, y la publicacion repetia la noticia mas reciente si el archivo se perdia.
--
-- Cambios:
--   1. Tabla [SCIA].[PublicacionesRegistro]: una fila por cada publicacion (exitosa o fallida).
--        - Una NOTICIA solo puede tener UNA publicacion exitosa (indice unico filtrado): nunca se repite.
--        - Los EMPRENDIMIENTOS pueden aparecer varias veces (la rotacion vuelve a empezar al terminar).
--        - dtFechaLocal es el dia en hora de Mexico; con eso la API sabe si "ya se publico hoy".
--   2. [SCIA].[spSavePublicacionRegistro] y [SCIA].[spGetPublicacionesRegistro].
--   3. Vista [SCIA].[vwPublicacionesResumenDiario] para analizar cuantas se publican por dia y tipo.
--   4. Parametro publicaciones.auto.noticias (Valor1) = hora de la publicacion diaria, formato HH:mm.
--      Se deja en 12:30. (Tambien se acepta una hora sola, por ejemplo 13.)
--
-- Supuestos: "hoy" se calcula con la zona 'Central Standard Time (Mexico)' (igual que 03_Feed...).
-- Es seguro ejecutarlo mas de una vez.
-- ==========================================================================================
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- ------------------------------------------------------------------------------------------
-- 1. Tabla
-- ------------------------------------------------------------------------------------------
IF OBJECT_ID(N'[SCIA].[PublicacionesRegistro]', N'U') IS NULL
BEGIN
    CREATE TABLE [SCIA].[PublicacionesRegistro]
    (
        iIdPublicacionRegistro INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_PublicacionesRegistro PRIMARY KEY,
        vchTipo        VARCHAR(20)    NOT NULL,                       -- NOTICIA | EMPRENDIMIENTO
        iIdReferencia  INT            NOT NULL,                       -- iIdNoticia o iIdEmpresa
        vchTitulo      NVARCHAR(300)  NOT NULL
            CONSTRAINT DF_PublicacionesRegistro_Titulo DEFAULT (N''),
        vchCanal       VARCHAR(30)    NOT NULL
            CONSTRAINT DF_PublicacionesRegistro_Canal DEFAULT ('FACEBOOK'),
        vchOrigen      VARCHAR(20)    NOT NULL,                       -- AUTOMATICA | MANUAL | MIGRACION
        bExitoso       BIT            NOT NULL,
        vchResultado   NVARCHAR(500)  NULL,                           -- motivo cuando fallo
        nvchMensaje    NVARCHAR(MAX)  NULL,                           -- texto publicado
        vchImagenUrl   NVARCHAR(1000) NULL,
        dtPublicacion  DATETIME2(0)   NOT NULL
            CONSTRAINT DF_PublicacionesRegistro_Fecha DEFAULT (SYSUTCDATETIME()),   -- UTC
        dtFechaLocal   DATE           NOT NULL,                       -- dia en hora de Mexico
        CONSTRAINT CK_PublicacionesRegistro_Tipo   CHECK (vchTipo   IN ('NOTICIA', 'EMPRENDIMIENTO')),
        CONSTRAINT CK_PublicacionesRegistro_Origen CHECK (vchOrigen IN ('AUTOMATICA', 'MANUAL', 'MIGRACION'))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PublicacionesRegistro_Tipo_Fecha'
               AND object_id = OBJECT_ID(N'[SCIA].[PublicacionesRegistro]'))
    CREATE INDEX IX_PublicacionesRegistro_Tipo_Fecha
        ON [SCIA].[PublicacionesRegistro] (vchTipo, dtFechaLocal)
        INCLUDE (bExitoso, vchOrigen, dtPublicacion);
GO

-- Una noticia no puede tener dos publicaciones exitosas.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_PublicacionesRegistro_Noticia_Exitosa'
               AND object_id = OBJECT_ID(N'[SCIA].[PublicacionesRegistro]'))
    CREATE UNIQUE INDEX UX_PublicacionesRegistro_Noticia_Exitosa
        ON [SCIA].[PublicacionesRegistro] (iIdReferencia)
        WHERE bExitoso = 1 AND vchTipo = 'NOTICIA';
GO

-- ------------------------------------------------------------------------------------------
-- 2. Procedimientos
-- ------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE [SCIA].[spSavePublicacionRegistro]
(
    @vchTipo        VARCHAR(20),
    @iIdReferencia  INT,
    @vchTitulo      NVARCHAR(300)  = NULL,
    @vchCanal       VARCHAR(30)    = 'FACEBOOK',
    @vchOrigen      VARCHAR(20)    = 'AUTOMATICA',
    @bExitoso       BIT,
    @vchResultado   NVARCHAR(500)  = NULL,
    @nvchMensaje    NVARCHAR(MAX)  = NULL,
    @vchImagenUrl   NVARCHAR(1000) = NULL,
    @dtPublicacion  DATETIME2(0)   = NULL          -- UTC; NULL = ahora
)
AS
/*
** Proposito: Guarda una publicacion (exitosa o fallida) de una noticia o un emprendimiento.
*/
BEGIN
    SET NOCOUNT ON;

    DECLARE @Id INT = 0;

    BEGIN TRY
        SET @vchTipo = UPPER(LTRIM(RTRIM(ISNULL(@vchTipo, ''))));
        SET @vchOrigen = UPPER(LTRIM(RTRIM(ISNULL(@vchOrigen, 'AUTOMATICA'))));
        SET @vchCanal = UPPER(LTRIM(RTRIM(ISNULL(NULLIF(@vchCanal, ''), 'FACEBOOK'))));
        SET @dtPublicacion = ISNULL(@dtPublicacion, CAST(SYSUTCDATETIME() AS DATETIME2(0)));

        IF @vchTipo NOT IN ('NOTICIA', 'EMPRENDIMIENTO')
        BEGIN
            SELECT CAST(0 AS BIT) AS bResult, CAST('El tipo debe ser NOTICIA o EMPRENDIMIENTO.' AS VARCHAR(500)) AS vchMessage, 0 AS iIdPublicacionRegistro;
            RETURN;
        END;

        IF ISNULL(@iIdReferencia, 0) <= 0
        BEGIN
            SELECT CAST(0 AS BIT) AS bResult, CAST('Falta el identificador de la noticia o el emprendimiento.' AS VARCHAR(500)) AS vchMessage, 0 AS iIdPublicacionRegistro;
            RETURN;
        END;

        INSERT INTO [SCIA].[PublicacionesRegistro]
            (vchTipo, iIdReferencia, vchTitulo, vchCanal, vchOrigen, bExitoso, vchResultado, nvchMensaje, vchImagenUrl, dtPublicacion, dtFechaLocal)
        VALUES
            (@vchTipo, @iIdReferencia, ISNULL(@vchTitulo, N''), @vchCanal, @vchOrigen, @bExitoso, @vchResultado, @nvchMensaje, @vchImagenUrl, @dtPublicacion,
             CAST((@dtPublicacion AT TIME ZONE 'UTC') AT TIME ZONE 'Central Standard Time (Mexico)' AS DATE));

        SET @Id = CAST(SCOPE_IDENTITY() AS INT);

        SELECT CAST(1 AS BIT) AS bResult, CAST('Ok' AS VARCHAR(500)) AS vchMessage, @Id AS iIdPublicacionRegistro;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() IN (2601, 2627)
            SELECT CAST(0 AS BIT) AS bResult, CAST('La noticia ya tiene una publicacion exitosa registrada.' AS VARCHAR(500)) AS vchMessage, 0 AS iIdPublicacionRegistro;
        ELSE
            SELECT CAST(0 AS BIT) AS bResult, CAST(LEFT(ERROR_MESSAGE(), 500) AS VARCHAR(500)) AS vchMessage, 0 AS iIdPublicacionRegistro;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE [SCIA].[spGetPublicacionesRegistro]
(
    @vchTipo   VARCHAR(20) = NULL,    -- NOTICIA | EMPRENDIMIENTO | NULL = todos
    @bExitoso  BIT         = NULL,    -- NULL = exitosas y fallidas
    @dtDesde   DATE        = NULL,    -- dia local (Mexico), inclusive
    @dtHasta   DATE        = NULL,    -- dia local (Mexico), inclusive
    @iTop      INT         = 200
)
AS
/*
** Proposito: Consulta el registro de publicaciones, las mas recientes primero.
*/
BEGIN
    SET NOCOUNT ON;

    IF @iTop IS NULL OR @iTop < 1 SET @iTop = 200;
    IF @iTop > 5000 SET @iTop = 5000;
    SET @vchTipo = NULLIF(UPPER(LTRIM(RTRIM(ISNULL(@vchTipo, '')))), '');

    SELECT TOP (@iTop)
        CAST(1 AS BIT) AS bResult,
        CAST('' AS VARCHAR(500)) AS vchMessage,
        r.iIdPublicacionRegistro,
        r.vchTipo,
        r.iIdReferencia,
        r.vchTitulo,
        r.vchCanal,
        r.vchOrigen,
        r.bExitoso,
        r.vchResultado,
        r.nvchMensaje,
        r.vchImagenUrl,
        r.dtPublicacion,
        r.dtFechaLocal
    FROM [SCIA].[PublicacionesRegistro] r
    WHERE (@vchTipo IS NULL OR r.vchTipo = @vchTipo)
      AND (@bExitoso IS NULL OR r.bExitoso = @bExitoso)
      AND (@dtDesde IS NULL OR r.dtFechaLocal >= @dtDesde)
      AND (@dtHasta IS NULL OR r.dtFechaLocal <= @dtHasta)
    ORDER BY r.dtPublicacion DESC, r.iIdPublicacionRegistro DESC;
END;
GO

-- ------------------------------------------------------------------------------------------
-- 3. Vista para analisis: publicaciones por dia y tipo
-- ------------------------------------------------------------------------------------------
CREATE OR ALTER VIEW [SCIA].[vwPublicacionesResumenDiario]
AS
SELECT
    dtFechaLocal,
    vchTipo,
    SUM(CASE WHEN bExitoso = 1 THEN 1 ELSE 0 END) AS iExitosas,
    SUM(CASE WHEN bExitoso = 0 THEN 1 ELSE 0 END) AS iFallidas,
    SUM(CASE WHEN bExitoso = 1 AND vchOrigen = 'AUTOMATICA' THEN 1 ELSE 0 END) AS iAutomaticas,
    SUM(CASE WHEN bExitoso = 1 AND vchOrigen = 'MANUAL' THEN 1 ELSE 0 END) AS iManuales,
    MAX(CASE WHEN bExitoso = 1 THEN dtPublicacion END) AS dtUltimaExitosaUtc
FROM [SCIA].[PublicacionesRegistro]
GROUP BY dtFechaLocal, vchTipo;
GO

-- ------------------------------------------------------------------------------------------
-- 4. Hora de la publicacion diaria de noticias: 12:30 (hora de Mexico)
-- ------------------------------------------------------------------------------------------
UPDATE [SCIA].[Parametros]
SET nvchValor1 = N'12:30'
WHERE nvchAlias = N'publicaciones.auto.noticias';
GO

-- Consultas utiles:
--   SELECT * FROM SCIA.vwPublicacionesResumenDiario ORDER BY dtFechaLocal DESC;
--   EXEC SCIA.spGetPublicacionesRegistro @vchTipo = 'NOTICIA', @iTop = 50;
--   EXEC SCIA.spGetPublicacionesRegistro @bExitoso = 0;       -- solo los intentos fallidos
