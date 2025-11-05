IF OBJECT_ID('spSearchTramitesForChat') IS NULL
    EXEC ('CREATE PROCEDURE spSearchTramitesForChat AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE spSearchTramitesForChat
(
    @vchTexto NVARCHAR(500),
    @iLimit INT  
)
AS
/*
** Propósito:    Busca trámites activos por texto, priorizando la BÚSQUEDA DE TEXTO COMPLETO (FREETEXT)
** Última fecha: 11/04/2025
*/
BEGIN

    -- [Estructura de la tabla #Result omitida por brevedad, se mantiene igual a la versión completa]
    CREATE TABLE #Result
    (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdTramite INT DEFAULT(-1),
        vchNombre VARCHAR(250) DEFAULT(''),
        nvchDescripcion NVARCHAR(MAX) DEFAULT(''),
        iIdCategoria INT NULL,
        bModalidadEnLinea BIT NULL,
        mCosto MONEY NULL,
        iIdOficina INT NULL,
        dtFechaCreacion DATETIME NULL,
        dtFechaActualizacion DATETIME NULL,
        bActivo BIT NULL
    );
    SET NOCOUNT ON;

    BEGIN TRY
        INSERT INTO #Result (
            iIdTramite, vchNombre, nvchDescripcion, iIdCategoria,
            bModalidadEnLinea, mCosto, iIdOficina, dtFechaCreacion,
            dtFechaActualizacion, bActivo
        )
        SELECT TOP (@iLimit)
            T.iIdTramite,
            T.vchNombre,
            T.nvchDescripcion,
            T.iIdCategoria,
            T.bModalidadEnLinea,
            T.mCosto,
            T.iIdOficina,
            T.dtFechaCreacion,
            T.dtFechaActualizacion,
            T.bActivo
        FROM dbo.Tramite T WITH(NOLOCK)
        WHERE T.bActivo = 1
          AND (
                @vchTexto IS NULL
                -- Búsqueda robusta FREETEXT: Busca coincidencias de significado en ambos campos.
                OR FREETEXT((T.vchNombre, T.nvchDescripcion), @vchTexto)
                -- Fallback de LIKE para garantizar que no se pierdan coincidencias literales
                OR T.vchNombre LIKE '%' + @vchTexto + '%'
              )
        -- La cláusula ORDER BY no puede depender de FREETEXT directamente sin usar una tabla WEIGHT
        ORDER BY
            -- Prioriza los resultados que coinciden con el nombre de forma literal (mejor score)
            CASE WHEN T.vchNombre LIKE '%' + @vchTexto + '%' THEN 0 ELSE 1 END,
            T.dtFechaCreacion DESC

        IF NOT EXISTS (SELECT 1 FROM #Result WHERE iIdTramite <> -1)
            INSERT INTO #Result (bResult, vchMessage) VALUES (0, 'No se encontraron trámites relevantes.');
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE()));
    END CATCH

    SELECT * FROM #Result
    DROP TABLE #Result
END
GO