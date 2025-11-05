IF OBJECT_ID('spSearchCostoForChat') IS NULL
    EXEC ('CREATE PROCEDURE spSearchCostoForChat AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE spSearchCostoForChat
(
    @iIdTramite INT
)
AS
/*
** Propósito:    Obtiene el costo y la modalidad en línea de un trámite específico.
** Última fecha: 11/04/2025
*/
BEGIN
    SET NOCOUNT ON;

    -- Usamos #Result para la salida, incluyendo campos obligatorios
    CREATE TABLE #Result
    (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        mCosto MONEY NULL,
        bModalidadEnLinea BIT NULL
    );

    BEGIN TRY
        -- Insertar el costo y la modalidad del trámite
        INSERT INTO #Result (mCosto, bModalidadEnLinea)
        SELECT TOP 1
            T.mCosto,
            T.bModalidadEnLinea
        FROM dbo.Tramite T WITH(NOLOCK)
        WHERE T.iIdTramite = @iIdTramite AND T.bActivo = 1;

        -- Validar si el trámite existe
        IF NOT EXISTS (SELECT 1 FROM #Result WHERE mCosto IS NOT NULL)
        BEGIN
            INSERT INTO #Result (bResult, vchMessage)
            VALUES (0, CONCAT('El Trámite con ID ', @iIdTramite, ' no fue encontrado o no está activo.'));
        END
    END TRY
    BEGIN CATCH
        -- Capturar error
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE()));
    END CATCH

    -- Devolver resultados
    SELECT * FROM #Result;

    -- Limpieza
    DROP TABLE #Result;
END
GO