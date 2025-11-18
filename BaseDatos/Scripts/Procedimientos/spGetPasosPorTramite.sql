IF OBJECT_ID('spGetPasosPorTramite') IS NULL
    EXEC ('CREATE PROCEDURE spGetPasosPorTramite AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE spGetPasosPorTramite
(
    @iIdTramite INT
)
AS
/*
** Creador:      Santiago Conecta (Asistente)
** Propósito:    Obtener la lista de pasos para un trámite.
** Metodología:  Engrama (Tipo Get)
*/
BEGIN
    CREATE TABLE #Result
    (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        -- Campos de TramitePaso
        iIdTramitePaso INT DEFAULT(-1),
        iIdTramite INT DEFAULT(-1),
        iOrden SMALLINT DEFAULT(0),
        nvchDescripcion NVARCHAR(1000) DEFAULT(''),
        bActivo BIT DEFAULT(1)
    );

    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO #Result (iIdTramitePaso, iIdTramite, iOrden, nvchDescripcion, bActivo)
        SELECT 
            P.iIdTramitePaso, P.iIdTramite, P.iOrden, P.nvchDescripcion, P.bActivo
        FROM dbo.TramitePaso P WITH(NOLOCK)
        WHERE P.iIdTramite = @iIdTramite
        ORDER BY P.iOrden ASC;

        IF NOT EXISTS (SELECT 1 FROM #Result WHERE iIdTramitePaso > 0)
            INSERT INTO #Result (bResult, vchMessage) VALUES (0, 'No se encontraron pasos para el trámite.');
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE()));
    END CATCH;

    SELECT * FROM #Result;
    DROP TABLE #Result;
END
GO