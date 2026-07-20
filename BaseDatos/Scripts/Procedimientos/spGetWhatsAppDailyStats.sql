-- ============================================================
-- Procedimiento: spGetWhatsAppDailyStats
-- Propósito: Obtener estadísticas diarias de WhatsApp para gráficas
-- Autor: SantiagoConectaIA
-- Fecha: 2026-07-20
-- ============================================================
IF OBJECT_ID('SCIA.spGetWhatsAppDailyStats') IS NULL
    EXEC ('CREATE PROCEDURE SCIA.spGetWhatsAppDailyStats AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE SCIA.spGetWhatsAppDailyStats
(
    @iDays INT = 30
)
AS
/*
** Creador:      SantiagoConectaIA
** Propósito:    Obtener estadísticas diarias de WhatsApp para los últimos N días.
** Última fecha: 2026-07-20
*/
BEGIN
    CREATE TABLE #Result
    (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        dtDate DATE DEFAULT('1900-01-01'),
        iNewUsers INT DEFAULT(0),
        iMessagesInbound INT DEFAULT(0),
        iMessagesOutbound INT DEFAULT(0),
        iTotalMessages INT DEFAULT(0),
        iActiveConversations INT DEFAULT(0)
    );

    SET NOCOUNT ON;

    BEGIN TRY
        -- Generar rango de fechas
        DECLARE @StartDate DATE = DATEADD(DAY, -@iDays, CAST(GETDATE() AS DATE));
        DECLARE @EndDate DATE = CAST(GETDATE() AS DATE);
        DECLARE @CurrentDate DATE = @StartDate;

        WHILE @CurrentDate <= @EndDate
        BEGIN
            INSERT INTO #Result
            (
                dtDate, iNewUsers, iMessagesInbound,
                iMessagesOutbound, iTotalMessages,
                iActiveConversations
            )
            SELECT
                @CurrentDate,
                ISNULL((SELECT COUNT(*) FROM SCIA.WhatsAppUser WHERE CAST(dtFirstContact AS DATE) = @CurrentDate), 0),
                ISNULL((SELECT COUNT(*) FROM SCIA.WhatsAppMessage WHERE CAST(dtTimestamp AS DATE) = @CurrentDate AND nvchDirection = 'inbound'), 0),
                ISNULL((SELECT COUNT(*) FROM SCIA.WhatsAppMessage WHERE CAST(dtTimestamp AS DATE) = @CurrentDate AND nvchDirection = 'outbound'), 0),
                ISNULL((SELECT COUNT(*) FROM SCIA.WhatsAppMessage WHERE CAST(dtTimestamp AS DATE) = @CurrentDate), 0),
                ISNULL((SELECT COUNT(*) FROM SCIA.WhatsAppConversation WHERE CAST(dtStartTime AS DATE) = @CurrentDate), 0);

            SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate);
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
