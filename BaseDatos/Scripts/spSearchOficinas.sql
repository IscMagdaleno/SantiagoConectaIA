--------------------------------------------------------------------------------
-- spSearchOficinas
-- Parámetros:
--   @vchTexto VARCHAR(500) = NULL  -> búsqueda en nombre/dirección
--   @iIdDependencia INT = 0        -> filtrar por dependencia
--   @iPage INT = 1, @iPageSize INT = 20
--   @bIncluirContacto BIT = 0      -> controla si devuelve contacto
-- Retorna: lista paginada (offset/fetch)
--------------------------------------------------------------------------------
IF OBJECT_ID('spSearchOficinas') IS NOT NULL
    DROP PROCEDURE spSearchOficinas
GO

CREATE PROCEDURE spSearchOficinas
(
    @vchTexto VARCHAR(500) = NULL,
    @iIdDependencia INT = 0,
    @iPage INT = 1,
    @iPageSize INT = 20,
    @bIncluirContacto BIT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #Result
    (
        bResult BIT DEFAULT (1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdOficina INT DEFAULT(-1),
        iIdDependencia INT DEFAULT(-1),
        vchNombre VARCHAR(250) DEFAULT(''),
        vchDireccion VARCHAR(500) DEFAULT(''),
        vchHorario NVARCHAR(250) DEFAULT(''),
        flLatitud FLOAT NULL,
        flLongitud FLOAT NULL,
        vchTelefono VARCHAR(50) DEFAULT(''),
        vchEmail VARCHAR(150) DEFAULT(''),
        bActivo BIT DEFAULT(0),
        dtFechaCreacion DATETIME NULL
    );

    BEGIN TRY
        DECLARE @Offset INT = (@iPage - 1) * @iPageSize;

        INSERT INTO #Result (iIdOficina, iIdDependencia, vchNombre, vchDireccion, vchHorario, flLatitud, flLongitud, vchTelefono, vchEmail, bActivo, dtFechaCreacion)
        SELECT
            O.iIdOficina,
            O.iIdDependencia,
            O.vchNombre,
            O.vchDireccion,
            O.vchHorario,
            O.flLatitud,
            O.flLongitud,
            CASE WHEN @bIncluirContacto = 1 THEN O.vchTelefono ELSE NULL END,
            CASE WHEN @bIncluirContacto = 1 THEN O.vchEmail ELSE NULL END,
            O.bActivo,
            O.dtFechaCreacion
        FROM dbo.Oficina O WITH(NOLOCK)
        WHERE (@iIdDependencia = 0 OR O.iIdDependencia = @iIdDependencia)
          AND (
                @vchTexto IS NULL
                OR O.vchNombre LIKE '%' + @vchTexto + '%'
                OR O.vchDireccion LIKE '%' + @vchTexto + '%'
              )
        ORDER BY O.dtFechaCreacion DESC
        OFFSET @Offset ROWS FETCH NEXT @iPageSize ROWS ONLY

        IF NOT EXISTS (SELECT 1 FROM #Result)
            INSERT INTO #Result (bResult, vchMessage) VALUES (0, 'No se encontraron oficinas.')
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE()))
    END CATCH

    SELECT * FROM #Result
    DROP TABLE #Result
END
GO
