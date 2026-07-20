-- ============================================================
-- Procedimiento: spSaveWhatsAppMessage
-- Propósito: Insertar un mensaje de WhatsApp y actualizar contadores
-- Autor: SantiagoConectaIA
-- Fecha: 2026-07-20
-- ============================================================
IF OBJECT_ID('SCIA.spSaveWhatsAppMessage') IS NULL
    EXEC ('CREATE PROCEDURE SCIA.spSaveWhatsAppMessage AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE SCIA.spSaveWhatsAppMessage
(
    @iIdConversation INT,
    @nvchWhatsAppMessageId NVARCHAR(100),
    @nvchDirection NVARCHAR(10),
    @nvchMessageType NVARCHAR(20),
    @nvchContent NVARCHAR(MAX),
    @dtTimestamp DATETIME
)
AS
/*
** Creador:      SantiagoConectaIA
** Propósito:    Insertar un mensaje de WhatsApp y actualizar contadores de la conversación.
** Última fecha: 2026-07-20
*/
BEGIN
    DECLARE @trancount INT = -1,
    @iIdWhatsAppMessage INT = -1,
            @vchError VARCHAR(200) = '';

    DECLARE @Result AS TABLE (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdWhatsAppMessage INT DEFAULT(-1)
    );

    SET NOCOUNT ON;

    BEGIN TRY
        SET XACT_ABORT ON;

        SET @trancount = @@TRANCOUNT;

        IF @trancount > 0
            SAVE TRANSACTION spSaveWhatsAppMessage;
        ELSE
            BEGIN TRANSACTION;

        -- Insertar el mensaje
        INSERT INTO SCIA.WhatsAppMessage
        (
            iIdConversation, nvchWhatsAppMessageId,
            nvchDirection, nvchMessageType,
            nvchContent, dtTimestamp
        )
        VALUES
        (
            @iIdConversation, @nvchWhatsAppMessageId,
            @nvchDirection, @nvchMessageType,
            @nvchContent, @dtTimestamp
        );

        SET @iIdWhatsAppMessage = SCOPE_IDENTITY();

        -- Actualizar el contador de mensajes de la conversación
        UPDATE SCIA.WhatsAppConversation WITH(ROWLOCK)
        SET iMessageCount = iMessageCount + 1
        WHERE iIdConversation = @iIdConversation;

        -- Actualizar la fecha de último contacto del usuario
        UPDATE SCIA.WhatsAppUser WITH(ROWLOCK)
        SET dtLastContact = GETDATE()
        WHERE iIdWhatsAppUser = (
            SELECT iIdWhatsAppUser FROM SCIA.WhatsAppConversation WHERE iIdConversation = @iIdConversation
        );

        IF @trancount = 0
            COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        SELECT @vchError = CONVERT(VARCHAR(200), CONCAT('spSaveWhatsAppMessage: ', ERROR_MESSAGE(), ' in ', ERROR_PROCEDURE(), ' at line ', ERROR_LINE()));
        PRINT CONCAT('spSaveWhatsAppMessage: ', ERROR_MESSAGE(), ' in ', ERROR_PROCEDURE(), ' at line ', ERROR_LINE());

        IF @trancount = 0
            ROLLBACK TRANSACTION;
        ELSE IF @trancount <> -1 AND XACT_STATE() <> -1
            ROLLBACK TRANSACTION spSaveWhatsAppMessage;
    END CATCH;

_Fin:
    IF LEN(@vchError) > 0
    BEGIN
        INSERT INTO @Result
            (bResult, vchMessage)
        VALUES
            (0, @vchError);
    END
    ELSE
    BEGIN
        INSERT INTO @Result
            (bResult, vchMessage, iIdWhatsAppMessage)
        VALUES
            (1, '', @iIdWhatsAppMessage);
    END

    SELECT *
    FROM @Result;

    SET NOCOUNT OFF;
END
GO
