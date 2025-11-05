--------------------------------------------------------------------------------
-- spSaveDependencia (insert / update)
-- Parámetros: @iIdDependencia (0 -> insert), @vchNombre, @nvchDescripcion, @vchUrlOficial, @bActivo
--------------------------------------------------------------------------------
IF OBJECT_ID('spSaveDependencia') IS NOT NULL
    DROP PROCEDURE spSaveDependencia;
GO

CREATE PROCEDURE spSaveDependencia
(
    @iIdDependencia INT = 0,
    @vchNombre VARCHAR(250),
    @nvchDescripcion NVARCHAR(MAX) = NULL,
    @vchUrlOficial VARCHAR(500) = NULL,
    @bActivo BIT = 1
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @vchError VARCHAR(500) = '';

    DECLARE @Result TABLE (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdDependencia INT DEFAULT(-1)
    );

    BEGIN TRY
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;

        IF @vchNombre IS NULL OR LTRIM(RTRIM(@vchNombre)) = ''
        BEGIN
            SET @vchError = 'El nombre de la dependencia es obligatorio.';
            THROW 51010, @vchError, 1;
        END

        IF @iIdDependencia IS NULL OR @iIdDependencia <= 0
        BEGIN
            INSERT INTO dbo.Dependencia (vchNombre, nvchDescripcion, vchUrlOficial, bActivo, dtFechaCreacion)
            VALUES (@vchNombre, @nvchDescripcion, @vchUrlOficial, @bActivo, GETDATE());

            SET @iIdDependencia = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            UPDATE dbo.Dependencia
            SET vchNombre = @vchNombre,
                nvchDescripcion = @nvchDescripcion,
                vchUrlOficial = @vchUrlOficial,
                bActivo = @bActivo
            WHERE iIdDependencia = @iIdDependencia;
        END

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
        INSERT INTO @Result (bResult, vchMessage, iIdDependencia) VALUES (1, '', @iIdDependencia);

    SELECT * FROM @Result;
END
GO
