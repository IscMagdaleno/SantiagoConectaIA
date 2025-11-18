IF OBJECT_ID('spGetDocumentosPorTramite') IS NULL
    EXEC ('CREATE PROCEDURE spGetDocumentosPorTramite AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE spGetDocumentosPorTramite
(
    @iIdTramite INT
)
AS
/*
** Creador:      Santiago Conecta (Asistente)
** Propósito:    Obtener la lista de documentos para un trámite.
** Metodología:  Engrama (Tipo Get)
*/
BEGIN
    CREATE TABLE #Result
    (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        -- Campos de Documento
        iIdDocumento INT DEFAULT(-1),
        iIdTramite INT DEFAULT(-1),
        vchNombre VARCHAR(250) DEFAULT(''),
        vchUrl VARCHAR(500) DEFAULT(''),
        bActivo BIT DEFAULT(1)
    );

    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO #Result (iIdDocumento, iIdTramite, vchNombre, vchUrl, bActivo)
        SELECT 
            D.iIdDocumento, D.iIdTramite, D.vchNombre, D.vchUrl, D.bActivo
        FROM dbo.Documento D WITH(NOLOCK)
        WHERE D.iIdTramite = @iIdTramite;

        IF NOT EXISTS (SELECT 1 FROM #Result WHERE iIdDocumento > 0)
            INSERT INTO #Result (bResult, vchMessage) VALUES (0, 'No se encontraron documentos para el trámite.');
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE()));
    END CATCH;

    SELECT * FROM #Result;
    DROP TABLE #Result;
END
GO