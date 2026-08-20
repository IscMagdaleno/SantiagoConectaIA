/*
** Migration: Opiniones ligadas a contenido del feed
** Tipos: NOTICIA, TRAMITE, EVENTO, CAPSULA
** Date: 2026-08-20
*/

SET NOCOUNT ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'SCIA')
    EXEC('CREATE SCHEMA SCIA');
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables
    WHERE name = N'Opinion' AND schema_id = SCHEMA_ID(N'SCIA')
)
BEGIN
    CREATE TABLE SCIA.Opinion
    (
        iIdOpinion INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
        vchTipoEntidad VARCHAR(20) NOT NULL,
        iIdEntidad INT NOT NULL,
        iIdCiudadano INT NOT NULL,
        iIdOpinionPadre INT NULL,
        nvchTexto NVARCHAR(1000) NOT NULL,
        dtFechaCreacion DATETIME NOT NULL CONSTRAINT DF_Opinion_dtFechaCreacion DEFAULT (GETDATE()),
        bActivo BIT NOT NULL CONSTRAINT DF_Opinion_bActivo DEFAULT (1),
        CONSTRAINT FK_Opinion_Ciudadano FOREIGN KEY (iIdCiudadano) REFERENCES SCIA.Ciudadano(iIdCiudadano),
        CONSTRAINT FK_Opinion_Padre FOREIGN KEY (iIdOpinionPadre) REFERENCES SCIA.Opinion(iIdOpinion)
    );

    CREATE INDEX IX_Opinion_Contenido
        ON SCIA.Opinion (vchTipoEntidad, iIdEntidad, bActivo, iIdOpinionPadre, dtFechaCreacion);
END
GO

IF OBJECT_ID('SCIA.spGetOpiniones') IS NULL
    EXEC('CREATE PROCEDURE SCIA.spGetOpiniones AS SET NOCOUNT ON;');
GO

ALTER PROCEDURE [SCIA].[spGetOpiniones]
(
    @vchTipoEntidad VARCHAR(20),
    @iIdEntidad INT
)
AS
/*
** Propósito: Listar opiniones y respuestas de un contenido.
** Última fecha: 20/08/2026
*/
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #Result
    (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdOpinion INT DEFAULT(-1),
        vchTipoEntidad VARCHAR(20) DEFAULT(''),
        iIdEntidad INT DEFAULT(-1),
        iIdCiudadano INT DEFAULT(-1),
        vchAlias NVARCHAR(80) DEFAULT(''),
        iIdOpinionPadre INT NULL,
        nvchTexto NVARCHAR(1000) DEFAULT(''),
        dtFechaCreacion DATETIME NULL
    );

    BEGIN TRY
        SET @vchTipoEntidad = UPPER(LTRIM(RTRIM(ISNULL(@vchTipoEntidad, ''))));

        IF @vchTipoEntidad NOT IN ('NOTICIA', 'TRAMITE', 'EVENTO', 'CAPSULA') OR @iIdEntidad <= 0
        BEGIN
            INSERT INTO #Result (bResult, vchMessage)
            VALUES (0, 'Contenido inválido para opiniones.');
        END
        ELSE
        BEGIN
            INSERT INTO #Result
            (
                iIdOpinion,
                vchTipoEntidad,
                iIdEntidad,
                iIdCiudadano,
                vchAlias,
                iIdOpinionPadre,
                nvchTexto,
                dtFechaCreacion
            )
            SELECT
                O.iIdOpinion,
                O.vchTipoEntidad,
                O.iIdEntidad,
                O.iIdCiudadano,
                ISNULL(C.vchAlias, N'Ciudadano'),
                O.iIdOpinionPadre,
                O.nvchTexto,
                O.dtFechaCreacion
            FROM SCIA.Opinion O WITH (NOLOCK)
            INNER JOIN SCIA.Ciudadano C WITH (NOLOCK) ON C.iIdCiudadano = O.iIdCiudadano
            WHERE O.vchTipoEntidad = @vchTipoEntidad
              AND O.iIdEntidad = @iIdEntidad
              AND O.bActivo = 1
              AND C.bActivo = 1
            ORDER BY
                CASE WHEN O.iIdOpinionPadre IS NULL THEN O.iIdOpinion ELSE O.iIdOpinionPadre END ASC,
                CASE WHEN O.iIdOpinionPadre IS NULL THEN 0 ELSE 1 END ASC,
                O.dtFechaCreacion ASC;

            IF NOT EXISTS (SELECT 1 FROM #Result WHERE iIdOpinion > 0)
            BEGIN
                INSERT INTO #Result (bResult, vchMessage)
                VALUES (1, 'Sin opiniones.');
            END
        END
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE()));
    END CATCH

    SELECT * FROM #Result ORDER BY
        CASE WHEN iIdOpinionPadre IS NULL THEN iIdOpinion ELSE iIdOpinionPadre END ASC,
        CASE WHEN iIdOpinionPadre IS NULL THEN 0 ELSE 1 END ASC,
        dtFechaCreacion ASC;
    DROP TABLE #Result;
END
GO

IF OBJECT_ID('SCIA.spSaveOpinion') IS NULL
    EXEC('CREATE PROCEDURE SCIA.spSaveOpinion AS SET NOCOUNT ON;');
GO

ALTER PROCEDURE [SCIA].[spSaveOpinion]
(
    @vchTipoEntidad VARCHAR(20),
    @iIdEntidad INT,
    @iIdCiudadano INT,
    @iIdOpinionPadre INT = NULL,
    @nvchTexto NVARCHAR(1000)
)
AS
/*
** Propósito: Guardar opinión o respuesta (1 nivel).
** Última fecha: 20/08/2026
*/
BEGIN
    DECLARE @vchError VARCHAR(200) = '';
    DECLARE @iIdOpinion INT = -1;

    DECLARE @Result AS TABLE
    (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdOpinion INT DEFAULT(-1),
        vchTipoEntidad VARCHAR(20) DEFAULT(''),
        iIdEntidad INT DEFAULT(-1),
        iIdCiudadano INT DEFAULT(-1),
        vchAlias NVARCHAR(80) DEFAULT(''),
        iIdOpinionPadre INT NULL,
        nvchTexto NVARCHAR(1000) DEFAULT(''),
        dtFechaCreacion DATETIME NULL
    );

    SET NOCOUNT ON;

    BEGIN TRY
        SET @vchTipoEntidad = UPPER(LTRIM(RTRIM(ISNULL(@vchTipoEntidad, ''))));
        SET @nvchTexto = LTRIM(RTRIM(ISNULL(@nvchTexto, N'')));

        IF @vchTipoEntidad NOT IN ('NOTICIA', 'TRAMITE', 'EVENTO', 'CAPSULA') OR @iIdEntidad <= 0
        BEGIN
            SET @vchError = 'Contenido inválido para opiniones.';
            GOTO _Fin;
        END

        IF @iIdCiudadano <= 0 OR NOT EXISTS (SELECT 1 FROM SCIA.Ciudadano WITH (NOLOCK) WHERE iIdCiudadano = @iIdCiudadano AND bActivo = 1)
        BEGIN
            SET @vchError = 'Debes iniciar sesión para opinar.';
            GOTO _Fin;
        END

        IF LEN(@nvchTexto) < 1 OR LEN(@nvchTexto) > 1000
        BEGIN
            SET @vchError = 'La opinión debe tener entre 1 y 1000 caracteres.';
            GOTO _Fin;
        END

        IF @iIdOpinionPadre IS NOT NULL AND @iIdOpinionPadre > 0
        BEGIN
            IF NOT EXISTS
            (
                SELECT 1
                FROM SCIA.Opinion P WITH (NOLOCK)
                WHERE P.iIdOpinion = @iIdOpinionPadre
                  AND P.vchTipoEntidad = @vchTipoEntidad
                  AND P.iIdEntidad = @iIdEntidad
                  AND P.iIdOpinionPadre IS NULL
                  AND P.bActivo = 1
            )
            BEGIN
                SET @vchError = 'No se puede responder a esa opinión.';
                GOTO _Fin;
            END
        END
        ELSE
        BEGIN
            SET @iIdOpinionPadre = NULL;
        END

        INSERT INTO SCIA.Opinion
        (
            vchTipoEntidad,
            iIdEntidad,
            iIdCiudadano,
            iIdOpinionPadre,
            nvchTexto,
            dtFechaCreacion,
            bActivo
        )
        VALUES
        (
            @vchTipoEntidad,
            @iIdEntidad,
            @iIdCiudadano,
            @iIdOpinionPadre,
            @nvchTexto,
            GETDATE(),
            1
        );

        SET @iIdOpinion = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        SET @vchError = CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE());
    END CATCH

_Fin:
    IF LEN(@vchError) > 0
        INSERT INTO @Result (bResult, vchMessage) VALUES (0, @vchError);
    ELSE
        INSERT INTO @Result
        (
            bResult,
            vchMessage,
            iIdOpinion,
            vchTipoEntidad,
            iIdEntidad,
            iIdCiudadano,
            vchAlias,
            iIdOpinionPadre,
            nvchTexto,
            dtFechaCreacion
        )
        SELECT
            1,
            'Opinión publicada.',
            O.iIdOpinion,
            O.vchTipoEntidad,
            O.iIdEntidad,
            O.iIdCiudadano,
            C.vchAlias,
            O.iIdOpinionPadre,
            O.nvchTexto,
            O.dtFechaCreacion
        FROM SCIA.Opinion O WITH (NOLOCK)
        INNER JOIN SCIA.Ciudadano C WITH (NOLOCK) ON C.iIdCiudadano = O.iIdCiudadano
        WHERE O.iIdOpinion = @iIdOpinion;

    SELECT TOP 1 * FROM @Result ORDER BY bResult DESC;
END
GO
