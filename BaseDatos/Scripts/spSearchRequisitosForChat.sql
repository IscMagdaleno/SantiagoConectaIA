IF OBJECT_ID('spSearchRequisitosForChat') IS NULL
    EXEC ('CREATE PROCEDURE spSearchRequisitosForChat AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE spSearchRequisitosForChat
(
    @iIdTramite INT
)
AS
/*
** Propósito:    Obtiene el detalle de requisitos y documentos para un trámite específico.
** Última fecha: 11/04/2025
*/
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #Result
    (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        vchTipo VARCHAR(50) DEFAULT(''), -- 'Requisito' o 'Documento'
        vchNombre VARCHAR(250) DEFAULT(''),
        nvchDetalle NVARCHAR(2000) DEFAULT(''),
        bObligatorio BIT DEFAULT(1)
    );

    BEGIN TRY
        -- 1. Insertar Requisitos
        INSERT INTO #Result (vchTipo, vchNombre, nvchDetalle, bObligatorio)
        SELECT
            'Requisito' AS vchTipo,
            R.vchNombre,
            R.nvchDetalle,
            R.bObligatorio
        FROM dbo.Requisito R WITH(NOLOCK)
        WHERE R.iIdTramite = @iIdTramite AND R.bActivo = 1;

        -- 2. Insertar Documentos (considerados como un tipo de requisito/información)
        INSERT INTO #Result (vchTipo, vchNombre, nvchDetalle, bObligatorio)
        SELECT
            'Documento' AS vchTipo,
            D.vchNombre,
            CONCAT('Enlace: ', ISNULL(D.vchUrl, 'No aplica o no disponible')) AS nvchDetalle,
            1 -- Asumimos que los documentos listados son obligatorios para el trámite
        FROM dbo.Documento D WITH(NOLOCK)
        WHERE D.iIdTramite = @iIdTramite AND D.bActivo = 1;

        IF NOT EXISTS (SELECT 1 FROM #Result)
            INSERT INTO #Result (bResult, vchMessage) VALUES (0, 'No se encontraron requisitos o documentos para este trámite.');
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE()));
    END CATCH

    SELECT * FROM #Result
    DROP TABLE #Result;
END
GO