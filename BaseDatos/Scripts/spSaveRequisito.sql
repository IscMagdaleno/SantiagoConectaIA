IF OBJECT_ID('spSaveRequisito') IS NOT NULL
    DROP PROCEDURE spSaveRequisito;
GO

CREATE PROCEDURE spSaveRequisito
(
    @iIdRequisito INT = 0,
    @iIdTramite INT,
    @vchNombre VARCHAR(250),
    @nvchDetalle NVARCHAR(1000) = NULL,
    @bObligatorio BIT = 1,
    @bActivo BIT = 1
)
AS
BEGIN
    DECLARE @vchError VARCHAR(500) = '';

    DECLARE @Result TABLE (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdRequisito INT DEFAULT(-1)
    );

    SET NOCOUNT ON;
    BEGIN TRY
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;

        IF @iIdRequisito IS NULL OR @iIdRequisito <= 0
        BEGIN
            INSERT INTO dbo.Requisito (iIdTramite, vchNombre, nvchDetalle, bObligatorio, bActivo)
            VALUES (@iIdTramite, @vchNombre, @nvchDetalle, @bObligatorio, @bActivo);

            SET @iIdRequisito = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            UPDATE dbo.Requisito
            SET vchNombre = @vchNombre,
                nvchDetalle = @nvchDetalle,
                bObligatorio = @bObligatorio,
                bActivo = @bActivo
            WHERE iIdRequisito = @iIdRequisito;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        SET @vchError = CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE());
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    END CATCH

    IF LEN(@vchError) > 0
        INSERT INTO @Result (bResult, vchMessage) VALUES (0, @vchError);
    ELSE
        INSERT INTO @Result (bResult, vchMessage, iIdRequisito) VALUES (1, '', @iIdRequisito);

    SELECT * FROM @Result;
END
GO
