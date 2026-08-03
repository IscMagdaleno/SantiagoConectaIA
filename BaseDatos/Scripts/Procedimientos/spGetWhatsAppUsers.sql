CREATE OR ALTER PROCEDURE [SCIA].[spGetWhatsAppUsers]
(
    @iTopRows INT = 100
)
AS
/*
** Creador:      SantiagoConectaIA
** Propósito:    Listar usuarios de WhatsApp ordenados por fecha de registro.
*/
BEGIN
    SET NOCOUNT ON;

    IF @iTopRows IS NULL OR @iTopRows <= 0
        SET @iTopRows = 100;

    SELECT TOP (@iTopRows)
        CAST(1 AS BIT) AS bResult,
        CAST('' AS VARCHAR(500)) AS vchMessage,
        iIdWhatsAppUser,
        nvchPhoneNumber,
        nvchName,
        dtFirstContact,
        dtLastContact,
        iTotalMessages,
        bActive
    FROM SCIA.WhatsAppUser WITH(NOLOCK)
    ORDER BY dtFirstContact DESC;
END
GO
