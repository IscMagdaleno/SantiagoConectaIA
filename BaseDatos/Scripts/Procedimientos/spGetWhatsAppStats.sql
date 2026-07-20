-- ============================================================
-- Procedimiento: spGetWhatsAppStats
-- Propósito: Obtener estadísticas generales de WhatsApp
-- Autor: SantiagoConectaIA
-- Fecha: 2026-07-20
-- ============================================================
IF OBJECT_ID('SCIA.spGetWhatsAppStats') IS NULL
    EXEC ('CREATE PROCEDURE SCIA.spGetWhatsAppStats AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE SCIA.spGetWhatsAppStats
AS
/*
** Creador:      SantiagoConectaIA
** Propósito:    Obtener estadísticas generales del módulo de WhatsApp.
** Última fecha: 2026-07-20
*/
BEGIN
    CREATE TABLE #Result
    (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iTotalUsers INT DEFAULT(0),
        iTotalConversations INT DEFAULT(0),
        iTotalMessages INT DEFAULT(0),
        iActiveUsersToday INT DEFAULT(0),
        iActiveConversations INT DEFAULT(0),
        iMessagesToday INT DEFAULT(0),
        iNewUsersToday INT DEFAULT(0),
        flAvgMessagesPerConversation FLOAT DEFAULT(0)
    );

    SET NOCOUNT ON;

    BEGIN TRY
        INSERT INTO #Result
        (
            iTotalUsers, iTotalConversations,
            iTotalMessages, iActiveUsersToday,
            iActiveConversations, iMessagesToday,
            iNewUsersToday, flAvgMessagesPerConversation
        )
        SELECT
            (SELECT COUNT(*) FROM SCIA.WhatsAppUser),
            (SELECT COUNT(*) FROM SCIA.WhatsAppConversation),
            (SELECT COUNT(*) FROM SCIA.WhatsAppMessage),
            (SELECT COUNT(*) FROM SCIA.WhatsAppUser WHERE CAST(dtLastContact AS DATE) = CAST(GETDATE() AS DATE)),
            (SELECT COUNT(*) FROM SCIA.WhatsAppConversation WHERE nvchStatus = 'active'),
            (SELECT COUNT(*) FROM SCIA.WhatsAppMessage WHERE CAST(dtTimestamp AS DATE) = CAST(GETDATE() AS DATE)),
            (SELECT COUNT(*) FROM SCIA.WhatsAppUser WHERE CAST(dtFirstContact AS DATE) = CAST(GETDATE() AS DATE)),
            CASE 
                WHEN (SELECT COUNT(*) FROM SCIA.WhatsAppConversation) > 0 
                THEN CAST((SELECT COUNT(*) FROM SCIA.WhatsAppMessage) AS FLOAT) / CAST((SELECT COUNT(*) FROM SCIA.WhatsAppConversation) AS FLOAT)
                ELSE 0 
            END;

        IF NOT EXISTS (SELECT 1 FROM #Result WHERE iTotalUsers > 0 OR iTotalConversations > 0)
        BEGIN
            INSERT INTO #Result (bResult, vchMessage)
            VALUES (0, 'No hay datos disponibles.');
        END
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE()));

        PRINT CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE());
    END CATCH;

    SELECT * FROM #Result;
    DROP TABLE #Result;
END
GO
