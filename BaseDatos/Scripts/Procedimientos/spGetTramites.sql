IF OBJECT_ID('spGetTramites') IS NULL
    EXEC('CREATE PROCEDURE spGetTramites AS SET NOCOUNT ON;');
GO

ALTER PROCEDURE spGetTramites
(
    @iIdTramite INT = 0,
    @bActivo BIT = 0
)
AS
BEGIN
    CREATE TABLE #Result
    (
        bResult BIT DEFAULT (1),
        vchMessage VARCHAR(500) DEFAULT (''),
        iIdTramite INT DEFAULT(-1),
        vchNombre VARCHAR(250) DEFAULT(''),
        nvchDescripcion NVARCHAR(MAX) DEFAULT(''),
        iIdCategoria INT DEFAULT(0),
        bModalidadEnLinea BIT DEFAULT(0),
        mCosto MONEY DEFAULT(0),
        iIdOficina INT DEFAULT(-1),
        dtFechaCreacion DATETIME NULL,
        dtFechaActualizacion DATETIME NULL,
        bActivo BIT DEFAULT(0)
    );

    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO #Result (iIdTramite, vchNombre, nvchDescripcion, iIdCategoria, bModalidadEnLinea, mCosto, iIdOficina, dtFechaCreacion, dtFechaActualizacion, bActivo)
        SELECT
            T.iIdTramite, T.vchNombre, T.nvchDescripcion, T.iIdCategoria, T.bModalidadEnLinea, T.mCosto, T.iIdOficina, T.dtFechaCreacion, T.dtFechaActualizacion, T.bActivo
        FROM dbo.Tramite T WITH(NOLOCK)
        WHERE (@iIdTramite = 0 OR T.iIdTramite = @iIdTramite)
          AND (@bActivo = 0 OR T.bActivo = @bActivo);

        IF NOT EXISTS (SELECT 1 FROM #Result)
        BEGIN
            INSERT INTO #Result (bResult, vchMessage)
            VALUES (0, 'No se encontraron trámites.');
        END
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE()));
    END CATCH;

    SELECT * FROM #Result;
    DROP TABLE #Result;
END
GO
exec spGetTramites