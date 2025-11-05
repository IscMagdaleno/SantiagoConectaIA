IF OBJECT_ID('spGetRequisitosPorTramite') IS NOT NULL
    DROP PROCEDURE spGetRequisitosPorTramite;
GO

CREATE PROCEDURE spGetRequisitosPorTramite
(
    @iIdTramite INT
)
AS
BEGIN
    CREATE TABLE #Result
    (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdRequisito INT DEFAULT(-1),
        iIdTramite INT DEFAULT(-1),
        vchNombre VARCHAR(250) DEFAULT(''),
        nvchDetalle NVARCHAR(1000) DEFAULT(''),
        bObligatorio BIT DEFAULT(1),
        bActivo BIT DEFAULT(1)
    );

    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO #Result (iIdRequisito, iIdTramite, vchNombre, nvchDetalle, bObligatorio, bActivo)
        SELECT r.iIdRequisito, r.iIdTramite, r.vchNombre, r.nvchDetalle, r.bObligatorio, r.bActivo
        FROM dbo.Requisito r WITH(NOLOCK)
        WHERE r.iIdTramite = @iIdTramite;

        IF NOT EXISTS (SELECT 1 FROM #Result)
            INSERT INTO #Result (bResult, vchMessage) VALUES (0, 'No hay requisitos para el trámite.');
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE()));
    END CATCH;

    SELECT * FROM #Result;
    DROP TABLE #Result;
END
GO
