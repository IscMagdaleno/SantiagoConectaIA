IF OBJECT_ID('spSearchTramites') IS NULL
    EXEC('CREATE PROCEDURE spSearchTramites AS SET NOCOUNT ON;');
GO

ALTER PROCEDURE spSearchTramites
(
    @vchTexto VARCHAR(500) = NULL,
    @iIdCategoria INT = 0,
    @iPage INT = 1,
    @iPageSize INT = 20
)
AS
BEGIN
    CREATE TABLE #Result
    (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdTramite INT DEFAULT(-1),
        vchNombre VARCHAR(250) DEFAULT(''),
        nvchDescripcion NVARCHAR(MAX) DEFAULT(''),
        iIdCategoria INT DEFAULT(0),
        bModalidadEnLinea BIT DEFAULT(0),
        mCosto MONEY DEFAULT(0),
        dtFechaCreacion DATETIME NULL
    );

    SET NOCOUNT ON;
    BEGIN TRY
        DECLARE @Offset INT = (@iPage - 1) * @iPageSize;

        INSERT INTO #Result (iIdTramite, vchNombre, nvchDescripcion, iIdCategoria, bModalidadEnLinea, mCosto, dtFechaCreacion)
        SELECT T.iIdTramite, T.vchNombre, T.nvchDescripcion, T.iIdCategoria, T.bModalidadEnLinea, T.mCosto, T.dtFechaCreacion
        FROM dbo.Tramite T WITH(NOLOCK)
        WHERE (@iIdCategoria = 0 OR T.iIdCategoria = @iIdCategoria)
          AND (@vchTexto IS NULL OR (T.vchNombre LIKE '%' + @vchTexto + '%' OR T.nvchDescripcion LIKE '%' + @vchTexto + '%'))
        ORDER BY T.dtFechaCreacion DESC
        OFFSET @Offset ROWS FETCH NEXT @iPageSize ROWS ONLY;

        IF NOT EXISTS (SELECT 1 FROM #Result)
            INSERT INTO #Result (bResult, vchMessage) VALUES (0, 'No se encontraron resultados.');
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE()));
    END CATCH;

    SELECT * FROM #Result;
    DROP TABLE #Result;
END
GO
EXEC spSearchTramites