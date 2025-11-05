IF OBJECT_ID('spSaveHistorialConsultaSimple') IS NOT NULL
    DROP PROCEDURE spSaveHistorialConsultaSimple;
GO

CREATE PROCEDURE spSaveHistorialConsultaSimple
(
    @iIdUsuario INT = NULL,
    @nvchPregunta NVARCHAR(MAX),
    @nvchRespuesta NVARCHAR(MAX),
    @vchOrigen VARCHAR(50) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @vchError VARCHAR(500) = '';
    DECLARE @iIdHistorial INT = 0;

    DECLARE @Result TABLE (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdHistorial INT DEFAULT(-1)
    );

    BEGIN TRY
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;

        -- Validaciones básicas
        IF @nvchPregunta IS NULL OR LTRIM(RTRIM(@nvchPregunta)) = ''
        BEGIN
            SET @vchError = 'Pregunta vacía. No se puede guardar el historial.';
            THROW 51030, @vchError, 1;
        END

        IF @nvchRespuesta IS NULL
        BEGIN
            SET @nvchRespuesta = N''; -- permitir respuestas vacías si aplica
        END

        INSERT INTO dbo.HistorialConsulta (iIdUsuario, nvchPregunta, nvchRespuesta, dtFecha, vchOrigen)
        VALUES (@iIdUsuario, @nvchPregunta, @nvchRespuesta, GETDATE(), @vchOrigen);

        SET @iIdHistorial = SCOPE_IDENTITY();

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        IF LEN(@vchError) = 0
            SET @vchError = CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE());
    END CATCH

    IF LEN(@vchError) > 0
        INSERT INTO @Result (bResult, vchMessage) VALUES (0, @vchError);
    ELSE
        INSERT INTO @Result (bResult, vchMessage, iIdHistorial) VALUES (1, '', @iIdHistorial);

    SELECT * FROM @Result;
END
GO
