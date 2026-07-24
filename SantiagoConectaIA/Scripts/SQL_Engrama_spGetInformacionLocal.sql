IF OBJECT_ID('SCIA.spGetInformacionLocal') IS NULL
    EXEC ('CREATE PROCEDURE SCIA.spGetInformacionLocal AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE SCIA.spGetInformacionLocal
(
    @bActivo BIT = 1
)
AS
/*
** Creador:      Santiago Conecta AI
** Propósito:    Obtener los registros de la tabla InformacionLocal (Datos Curiosos)
** Última fecha: 2026-07-17
*/
BEGIN
    -- Crear tabla temporal para los resultados
    CREATE TABLE #Result
    (
        bResult BIT DEFAULT (1),
        vchMessage VARCHAR(500) DEFAULT (''),
        iIdInformacionLocal INT,
        nvchCategoria NVARCHAR(100),
        nvchTitulo NVARCHAR(255),
        nvchPalabrasClave NVARCHAR(500),
        nvchDescripcionCorta NVARCHAR(1000),
        nvchContenidoDetallado NVARCHAR(MAX),
        nvchUbicacion_LatLong NVARCHAR(100),
        dtFechaCreacion DATETIME,
        bActivo BIT
    );

    SET NOCOUNT ON;

    BEGIN TRY
        -- Insertar resultados en la tabla temporal
        INSERT INTO #Result
        (
            iIdInformacionLocal, nvchCategoria, nvchTitulo,
            nvchPalabrasClave, nvchDescripcionCorta, nvchContenidoDetallado,
            nvchUbicacion_LatLong, dtFechaCreacion, bActivo
        )
        SELECT
            IL.iIdInformacionLocal, IL.nvchCategoria, IL.nvchTitulo,
            IL.nvchPalabrasClave, IL.nvchDescripcionCorta, IL.nvchContenidoDetallado,
            IL.nvchUbicacion_LatLong, IL.dtFechaCreacion, IL.bActivo
        FROM
            SCIA.InformacionLocal IL WITH(NOLOCK)
        WHERE
            (IL.bActivo = @bActivo OR @bActivo = 0)
		ORDER BY IL.dtFechaCreacion DESC;

        -- Validar si no se encontraron registros
        IF NOT EXISTS (SELECT 1 FROM #Result)
        BEGIN
            INSERT INTO #Result (bResult, vchMessage)
            VALUES (0, 'No se encontraron registros de Información Local.');
        END
    END TRY
    BEGIN CATCH
        -- Capturar error
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE()));
        
        PRINT CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE());
    END CATCH;

    -- Devolver resultados
    SELECT * FROM #Result;

    -- Eliminar tabla temporal
    DROP TABLE #Result;
END
GO
