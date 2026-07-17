IF OBJECT_ID('SCIA.spGetInformacionLocalById') IS NULL
    EXEC ('CREATE PROCEDURE SCIA.spGetInformacionLocalById AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE SCIA.spGetInformacionLocalById
(
    @iIdInformacionLocal INT
)
AS
/*
** Proposito:    Obtiene un registro de informacion local por su ID
** Ultima fecha: 17/07/2026
*/
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #Result
    (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdInformacionLocal INT DEFAULT(-1),
        nvchCategoria NVARCHAR(100) DEFAULT(''),
        nvchTitulo NVARCHAR(255) DEFAULT(''),
        nvchPalabrasClave NVARCHAR(500) DEFAULT(''),
        nvchDescripcionCorta NVARCHAR(1000) DEFAULT(''),
        nvchContenidoDetallado NVARCHAR(MAX) NULL,
        nvchUbicacion_LatLong NVARCHAR(100) NULL,
        dtFechaCreacion DATETIME NULL,
        bActivo BIT DEFAULT(1)
    );

    BEGIN TRY
        INSERT INTO #Result
        (
            iIdInformacionLocal, nvchCategoria, nvchTitulo, nvchPalabrasClave,
            nvchDescripcionCorta, nvchContenidoDetallado, nvchUbicacion_LatLong,
            dtFechaCreacion, bActivo
        )
        SELECT
            iIdInformacionLocal, nvchCategoria, nvchTitulo, nvchPalabrasClave,
            nvchDescripcionCorta, nvchContenidoDetallado, nvchUbicacion_LatLong,
            dtFechaCreacion, bActivo
        FROM Engrama.SCIA.InformacionLocal I WITH(NOLOCK)
        WHERE I.iIdInformacionLocal = @iIdInformacionLocal;

        IF NOT EXISTS (SELECT 1 FROM #Result WHERE iIdInformacionLocal <> -1)
        BEGIN
            INSERT INTO #Result (bResult, vchMessage)
            VALUES (0, CONCAT('No se encontro informacion local con ID ', @iIdInformacionLocal, '.'));
        END
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Linea ', ERROR_LINE()));
    END CATCH

    SELECT * FROM #Result;
    DROP TABLE #Result;
END
GO
