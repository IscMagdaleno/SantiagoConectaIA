IF OBJECT_ID('SCIA.spGetOpiniones') IS NULL
    EXEC('CREATE PROCEDURE SCIA.spGetOpiniones AS SET NOCOUNT ON;');
GO

ALTER PROCEDURE [SCIA].[spGetOpiniones]
(
    @vchTipoEntidad VARCHAR(20),
    @iIdEntidad INT
)
AS
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
