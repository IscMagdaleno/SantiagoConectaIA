-- ============================================================
-- Procedimiento: spSaveWhatsAppConversation
-- Propósito: Crear o actualizar una conversación de WhatsApp
-- Autor: SantiagoConectaIA
-- Fecha: 2026-07-20
-- ============================================================
IF OBJECT_ID('SCIA.spSaveWhatsAppConversation') IS NULL
    EXEC ('CREATE PROCEDURE SCIA.spSaveWhatsAppConversation AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE SCIA.spSaveWhatsAppConversation
(
    @iIdConversation INT,
    @iIdWhatsAppUser INT,
    @nvchStatus NVARCHAR(20)
)
AS
/*
** Creador:      SantiagoConectaIA
** Propósito:    Crear o actualizar una sesión de conversación de WhatsApp.
** Última fecha: 2026-07-20
*/
BEGIN
    DECLARE @trancount INT = -1,
            @vchError VARCHAR(200) = '';

    DECLARE @Result AS TABLE (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdConversation INT DEFAULT(-1)
    );

    SET NOCOUNT ON;

    BEGIN TRY
        SET XACT_ABORT ON;

        SET @trancount = @@TRANCOUNT;

        IF @trancount > 0
            SAVE TRANSACTION spSaveWhatsAppConversation;
        ELSE
            BEGIN TRANSACTION;

        IF @iIdConversation <= 0
        BEGIN
            -- Crear nueva conversación
            INSERT INTO SCIA.WhatsAppConversation
            (
                iIdWhatsAppUser, dtStartTime,
                iMessageCount, nvchStatus
            )
            VALUES
            (
                @iIdWhatsAppUser, GETDATE(),
                0, @nvchStatus
            );

            SET @iIdConversation = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            -- Actualizar conversación existente
            UPDATE SCIA.WhatsAppConversation WITH(ROWLOCK)
            SET
                nvchStatus = @nvchStatus,
                dtEndTime = CASE WHEN @nvchStatus <> 'active' THEN GETDATE() ELSE dtEndTime END,
                iMessageCount = iMessageCount + 1
            WHERE iIdConversation = @iIdConversation;
        END

        IF @trancount = 0
            COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        SELECT @vchError = CONVERT(VARCHAR(200), CONCAT('spSaveWhatsAppConversation: ', ERROR_MESSAGE(), ' in ', ERROR_PROCEDURE(), ' at line ', ERROR_LINE()));
        PRINT CONCAT('spSaveWhatsAppConversation: ', ERROR_MESSAGE(), ' in ', ERROR_PROCEDURE(), ' at line ', ERROR_LINE());

        IF @trancount = 0
            ROLLBACK TRANSACTION;
        ELSE IF @trancount <> -1 AND XACT_STATE() <> -1
            ROLLBACK TRANSACTION spSaveWhatsAppConversation;
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
            (bResult, vchMessage, iIdConversation)
        VALUES
            (1, '', @iIdConversation);
    END

    SELECT *
    FROM @Result;

    SET NOCOUNT OFF;
END
GO
