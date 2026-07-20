IF OBJECT_ID('SCIA.spGetInformacionLocal') IS NULL
    EXEC ('CREATE PROCEDURE SCIA.spGetInformacionLocal AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE SCIA.spGetInformacionLocal
(
    @nvchCategoria NVARCHAR(100) = NULL,
    @nvchTexto NVARCHAR(500) = NULL,
    @bActivo BIT = NULL
)
AS
/*
** Proposito:    Busca informacion local activa por categoria y/o texto
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
        FROM SCIA.InformacionLocal I WITH(NOLOCK)
        WHERE
            (@bActivo IS NULL OR I.bActivo = @bActivo)
            AND (@nvchCategoria IS NULL OR I.nvchCategoria = @nvchCategoria)
            AND (
                @nvchTexto IS NULL
                OR I.nvchTitulo LIKE '%' + @nvchTexto + '%'
                OR I.nvchDescripcionCorta LIKE '%' + @nvchTexto + '%'
                OR I.nvchPalabrasClave LIKE '%' + @nvchTexto + '%'
                OR I.nvchContenidoDetallado LIKE '%' + @nvchTexto + '%'
            )
        ORDER BY I.nvchCategoria, I.dtFechaCreacion DESC;

        IF NOT EXISTS (SELECT 1 FROM #Result WHERE iIdInformacionLocal <> -1)
        BEGIN
            INSERT INTO #Result (bResult, vchMessage)
            VALUES (0, 'No se encontro informacion local para los criterios especificados.');
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
