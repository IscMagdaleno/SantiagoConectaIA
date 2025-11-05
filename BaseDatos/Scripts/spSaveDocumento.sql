IF OBJECT_ID('spSaveDocumento') IS NOT NULL
    DROP PROCEDURE spSaveDocumento;
GO

CREATE PROCEDURE spSaveDocumento
(
    @iIdDocumento INT = 0,
    @iIdTramite INT,
    @vchNombre VARCHAR(250),
    @vchUrl VARCHAR(500) = NULL,
    @bActivo BIT = 1
)
AS
BEGIN
    DECLARE @vchError VARCHAR(500) = '';

    DECLARE @Result TABLE (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdDocumento INT DEFAULT(-1)
    );

    SET NOCOUNT ON;
    BEGIN TRY
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;

        IF @iIdDocumento IS NULL OR @iIdDocumento <= 0
        BEGIN
            INSERT INTO dbo.Documento (iIdTramite, vchNombre, vchUrl, bActivo)
            VALUES (@iIdTramite, @vchNombre, @vchUrl, @bActivo);

            SET @iIdDocumento = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            UPDATE dbo.Documento
            SET vchNombre = @vchNombre,
                vchUrl = @vchUrl,
                bActivo = @bActivo
            WHERE iIdDocumento = @iIdDocumento;
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
        INSERT INTO @Result (bResult, vchMessage, iIdDocumento) VALUES (1, '', @iIdDocumento);

    SELECT * FROM @Result;
END
GO
