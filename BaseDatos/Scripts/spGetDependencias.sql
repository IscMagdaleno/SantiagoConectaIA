--------------------------------------------------------------------------------
-- spGetDependencias
-- Parámetros:
--   @iIdDependencia INT = 0
--   @bActivo BIT = 0
--------------------------------------------------------------------------------
IF OBJECT_ID('spGetDependencias') IS NOT NULL
    DROP PROCEDURE spGetDependencias;
GO

CREATE PROCEDURE spGetDependencias
(
    @iIdDependencia INT = 0,
    @bActivo BIT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #Result
    (
        bResult BIT DEFAULT (1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdDependencia INT DEFAULT(-1),
        vchNombre VARCHAR(250) DEFAULT(''),
        nvchDescripcion NVARCHAR(MAX) DEFAULT(''),
        vchUrlOficial VARCHAR(500) DEFAULT(''),
        bActivo BIT DEFAULT(0),
        dtFechaCreacion DATETIME NULL
    );

    BEGIN TRY
        INSERT INTO #Result (iIdDependencia, vchNombre, nvchDescripcion, vchUrlOficial, bActivo, dtFechaCreacion)
        SELECT D.iIdDependencia, D.vchNombre, D.nvchDescripcion, D.vchUrlOficial, D.bActivo, D.dtFechaCreacion
        FROM dbo.Dependencia D WITH(NOLOCK)
        WHERE (@iIdDependencia = 0 OR D.iIdDependencia = @iIdDependencia)
          AND (@bActivo = 0 OR D.bActivo = @bActivo);

        IF NOT EXISTS (SELECT 1 FROM #Result)
            INSERT INTO #Result (bResult, vchMessage) VALUES (0, 'No se encontraron dependencias.');
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE()));
    END CATCH;

    SELECT * FROM #Result;
    DROP TABLE #Result;
END
GO
