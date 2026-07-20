-- ============================================================
-- Procedimiento: spSaveWhatsAppUser
-- Propósito: Insertar o actualizar un usuario de WhatsApp
-- Autor: SantiagoConectaIA
-- Fecha: 2026-07-20
-- ============================================================
IF OBJECT_ID('SCIA.spSaveWhatsAppUser') IS NULL
    EXEC ('CREATE PROCEDURE SCIA.spSaveWhatsAppUser AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE SCIA.spSaveWhatsAppUser
(
    @iIdWhatsAppUser INT,
    @nvchPhoneNumber NVARCHAR(20),
    @nvchName NVARCHAR(200)
)
AS
/*
** Creador:      SantiagoConectaIA
** Propósito:    Insertar o actualizar un usuario de WhatsApp según el número de teléfono.
** Última fecha: 2026-07-20
*/
BEGIN
    DECLARE @trancount INT = -1,
            @vchError VARCHAR(200) = '';

    DECLARE @Result AS TABLE (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdWhatsAppUser INT DEFAULT(-1)
    );

    SET NOCOUNT ON;

    BEGIN TRY
        SET XACT_ABORT ON;

        -- Verificar si el usuario ya existe por número de teléfono
        IF EXISTS (SELECT 1 FROM SCIA.WhatsAppUser WU WHERE WU.nvchPhoneNumber = @nvchPhoneNumber AND WU.iIdWhatsAppUser <> @iIdWhatsAppUser)
        BEGIN
            -- Actualizar usuario existente
            UPDATE SCIA.WhatsAppUser WITH(ROWLOCK)
            SET
                nvchName = CASE WHEN LEN(@nvchName) > 0 THEN @nvchName ELSE nvchName END,
                dtLastContact = GETDATE(),
                iTotalMessages = iTotalMessages + 1
            WHERE nvchPhoneNumber = @nvchPhoneNumber;

            SET @iIdWhatsAppUser = (SELECT iIdWhatsAppUser FROM SCIA.WhatsAppUser WHERE nvchPhoneNumber = @nvchPhoneNumber);
        END
        ELSE IF @iIdWhatsAppUser <= 0
        BEGIN
            -- Insertar nuevo usuario
            INSERT INTO SCIA.WhatsAppUser
            (
                nvchPhoneNumber, nvchName,
                dtFirstContact, dtLastContact,
                iTotalMessages, bActive
            )
            VALUES
            (
                @nvchPhoneNumber, @nvchName,
                GETDATE(), GETDATE(),
                1, 1
            );

            SET @iIdWhatsAppUser = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            -- Actualizar usuario existente por ID
            UPDATE SCIA.WhatsAppUser WITH(ROWLOCK)
            SET
                nvchName = CASE WHEN LEN(@nvchName) > 0 THEN @nvchName ELSE nvchName END,
                dtLastContact = GETDATE(),
                iTotalMessages = iTotalMessages + 1
            WHERE iIdWhatsAppUser = @iIdWhatsAppUser;
        END
    END TRY
    BEGIN CATCH
        SELECT @vchError = CONVERT(VARCHAR(200), CONCAT('spSaveWhatsAppUser: ', ERROR_MESSAGE(), ' in ', ERROR_PROCEDURE(), ' at line ', ERROR_LINE()));
        PRINT CONCAT('spSaveWhatsAppUser: ', ERROR_MESSAGE(), ' in ', ERROR_PROCEDURE(), ' at line ', ERROR_LINE());
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
            (bResult, vchMessage, iIdWhatsAppUser)
        VALUES
            (1, '', @iIdWhatsAppUser);
    END

    SELECT *
    FROM @Result;

    SET NOCOUNT OFF;
END
GO
