/*
** Migration: Fix unique visitors (empty IP) + WhatsApp users list
** Run against the SantiagoConectaIA database (schema SCIA).
** Date: 2026-08-03
*/

SET NOCOUNT ON;
GO

/* ========== 1. Add visitor id column ========== */
IF COL_LENGTH('SCIA.PageVisits', 'vchVisitorId') IS NULL
BEGIN
    ALTER TABLE SCIA.PageVisits
        ADD vchVisitorId NVARCHAR(64) NOT NULL
            CONSTRAINT DF_PageVisits_vchVisitorId DEFAULT ('');
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_PageVisits_VisitorId_Date'
      AND object_id = OBJECT_ID('SCIA.PageVisits')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_PageVisits_VisitorId_Date
        ON SCIA.PageVisits (vchVisitorId, dtVisitDate)
        INCLUDE (vchIpAddress, vchUserAgent, bIsUniqueVisitor);
END
GO

/* ========== 2. spSavePageVisit ========== */
CREATE OR ALTER PROCEDURE [SCIA].[spSavePageVisit]
    @vchPageUrl NVARCHAR(500),
    @vchPageName NVARCHAR(200) = '',
    @vchIpAddress NVARCHAR(50) = '',
    @vchUserAgent NVARCHAR(500) = '',
    @vchReferrer NVARCHAR(500) = '',
    @vchBrowser NVARCHAR(100) = '',
    @vchOperatingSystem NVARCHAR(100) = '',
    @vchDeviceType NVARCHAR(50) = '',
    @vchVisitorId NVARCHAR(64) = ''
AS
/*
** Propósito: Registrar visita y marcar unicidad por VisitorId / IP / UserAgent.
*/
BEGIN
    DECLARE @trancount INT = -1,
            @vchError VARCHAR(200) = '';

    DECLARE @Result AS TABLE (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdPageVisit INT DEFAULT(-1),
        bIsUniqueVisitor BIT DEFAULT(0)
    );

    SET NOCOUNT ON;

    BEGIN TRY
        SET XACT_ABORT ON;

        DECLARE @bIsUnique BIT = 1;
        DECLARE @TodayStart DATETIME = CAST(GETUTCDATE() AS DATE);
        DECLARE @VisitorKey NVARCHAR(100);

        SET @vchVisitorId = ISNULL(LTRIM(RTRIM(@vchVisitorId)), '');
        SET @vchIpAddress = ISNULL(LTRIM(RTRIM(@vchIpAddress)), '');
        SET @vchUserAgent = ISNULL(@vchUserAgent, '');

        SET @VisitorKey =
            CASE
                WHEN @vchVisitorId <> '' THEN N'VID:' + @vchVisitorId
                WHEN @vchIpAddress <> '' THEN N'IP:' + @vchIpAddress
                WHEN @vchUserAgent <> '' THEN N'UA:' + CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', @vchUserAgent), 2)
                ELSE N''
            END;

        IF @VisitorKey <> ''
        BEGIN
            IF EXISTS (
                SELECT 1
                FROM SCIA.PageVisits WITH(NOLOCK)
                WHERE dtVisitDate >= @TodayStart
                  AND (
                        (@vchVisitorId <> '' AND vchVisitorId = @vchVisitorId)
                     OR (@vchVisitorId = '' AND @vchIpAddress <> '' AND vchIpAddress = @vchIpAddress)
                     OR (
                            @vchVisitorId = ''
                        AND @vchIpAddress = ''
                        AND @vchUserAgent <> ''
                        AND vchUserAgent = @vchUserAgent
                        )
                  )
            )
            BEGIN
                SET @bIsUnique = 0;
            END
        END

        SET @trancount = @@TRANCOUNT;

        IF @trancount > 0
            SAVE TRANSACTION sspSavePageVisit;
        ELSE
            BEGIN TRANSACTION;

        INSERT INTO SCIA.PageVisits (
            vchPageUrl, vchPageName, vchIpAddress, vchUserAgent,
            vchReferrer, vchBrowser, vchOperatingSystem, vchDeviceType,
            bIsUniqueVisitor, dtVisitDate, dtCreatedAt, vchVisitorId
        )
        VALUES (
            @vchPageUrl, @vchPageName, @vchIpAddress, @vchUserAgent,
            @vchReferrer, @vchBrowser, @vchOperatingSystem, @vchDeviceType,
            @bIsUnique, GETUTCDATE(), GETUTCDATE(), @vchVisitorId
        );

        DECLARE @iIdPageVisit INT = SCOPE_IDENTITY();

        IF @trancount = 0
            COMMIT TRANSACTION;

        INSERT INTO @Result (bResult, vchMessage, iIdPageVisit, bIsUniqueVisitor)
        VALUES (1, 'Visit recorded successfully.', @iIdPageVisit, @bIsUnique);
    END TRY
    BEGIN CATCH
        SELECT @vchError = CONVERT(VARCHAR(200), CONCAT('spSavePageVisit: ', ERROR_MESSAGE(), ' in ', ERROR_PROCEDURE(), ' at line ', ERROR_LINE()));
        PRINT @vchError;

        IF @trancount = 0
            ROLLBACK TRANSACTION;
        ELSE IF @trancount <> -1 AND XACT_STATE() <> -1
            ROLLBACK TRANSACTION sspSavePageVisit;
    END CATCH;

_Fin:
    IF LEN(@vchError) > 0
        INSERT INTO @Result (bResult, vchMessage) VALUES (0, @vchError);

    SELECT * FROM @Result;
    SET NOCOUNT OFF;
END
GO

/* ========== 3. Summary / ByPage / Daily — distinct visitor key ========== */
CREATE OR ALTER PROCEDURE [SCIA].[spGetPageVisitsSummary]
    @dtStartDate DATETIME = NULL,
    @dtEndDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #Result
    (
        bResult BIT DEFAULT (1),
        vchMessage VARCHAR(500) DEFAULT (''),
        TotalVisits INT DEFAULT (0),
        UniqueVisitors INT DEFAULT (0),
        NewVisitors INT DEFAULT (0),
        ReturningVisitors INT DEFAULT (0),
        StartDate DATETIME DEFAULT ('1900-01-01'),
        EndDate DATETIME DEFAULT ('1900-01-01')
    );

    BEGIN TRY
        IF @dtStartDate IS NULL
            SET @dtStartDate = DATEADD(DAY, -30, CAST(GETUTCDATE() AS DATE));
        IF @dtEndDate IS NULL
            SET @dtEndDate = DATEADD(DAY, 1, CAST(GETUTCDATE() AS DATE));

        ;WITH Filtered AS (
            SELECT
                iIdPageVisit,
                dtVisitDate,
                CASE
                    WHEN NULLIF(LTRIM(RTRIM(vchVisitorId)), '') IS NOT NULL THEN N'VID:' + LTRIM(RTRIM(vchVisitorId))
                    WHEN NULLIF(LTRIM(RTRIM(vchIpAddress)), '') IS NOT NULL THEN N'IP:' + LTRIM(RTRIM(vchIpAddress))
                    WHEN NULLIF(LTRIM(RTRIM(vchUserAgent)), '') IS NOT NULL
                        THEN N'UA:' + CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', vchUserAgent), 2)
                    ELSE N'ROW:' + CAST(iIdPageVisit AS NVARCHAR(20))
                END AS VisitorKey
            FROM SCIA.PageVisits WITH(NOLOCK)
            WHERE dtVisitDate >= @dtStartDate
              AND dtVisitDate < @dtEndDate
        ),
        AllKeys AS (
            SELECT
                CASE
                    WHEN NULLIF(LTRIM(RTRIM(vchVisitorId)), '') IS NOT NULL THEN N'VID:' + LTRIM(RTRIM(vchVisitorId))
                    WHEN NULLIF(LTRIM(RTRIM(vchIpAddress)), '') IS NOT NULL THEN N'IP:' + LTRIM(RTRIM(vchIpAddress))
                    WHEN NULLIF(LTRIM(RTRIM(vchUserAgent)), '') IS NOT NULL
                        THEN N'UA:' + CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', vchUserAgent), 2)
                    ELSE N'ROW:' + CAST(iIdPageVisit AS NVARCHAR(20))
                END AS VisitorKey,
                MIN(dtVisitDate) AS FirstVisit
            FROM SCIA.PageVisits WITH(NOLOCK)
            GROUP BY
                CASE
                    WHEN NULLIF(LTRIM(RTRIM(vchVisitorId)), '') IS NOT NULL THEN N'VID:' + LTRIM(RTRIM(vchVisitorId))
                    WHEN NULLIF(LTRIM(RTRIM(vchIpAddress)), '') IS NOT NULL THEN N'IP:' + LTRIM(RTRIM(vchIpAddress))
                    WHEN NULLIF(LTRIM(RTRIM(vchUserAgent)), '') IS NOT NULL
                        THEN N'UA:' + CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', vchUserAgent), 2)
                    ELSE N'ROW:' + CAST(iIdPageVisit AS NVARCHAR(20))
                END
        ),
        RangeKeys AS (
            SELECT DISTINCT f.VisitorKey
            FROM Filtered f
        )
        INSERT INTO #Result (
            TotalVisits, UniqueVisitors, NewVisitors, ReturningVisitors, StartDate, EndDate
        )
        SELECT
            (SELECT COUNT(*) FROM Filtered),
            (SELECT COUNT(*) FROM RangeKeys),
            (
                SELECT COUNT(*)
                FROM RangeKeys rk
                INNER JOIN AllKeys ak ON ak.VisitorKey = rk.VisitorKey
                WHERE ak.FirstVisit >= @dtStartDate AND ak.FirstVisit < @dtEndDate
            ),
            (
                SELECT COUNT(*)
                FROM RangeKeys rk
                INNER JOIN AllKeys ak ON ak.VisitorKey = rk.VisitorKey
                WHERE ak.FirstVisit < @dtStartDate
            ),
            @dtStartDate,
            @dtEndDate;

        IF NOT EXISTS (SELECT 1 FROM #Result WHERE TotalVisits > 0)
            INSERT INTO #Result (bResult, vchMessage)
            VALUES (0, 'No visits found for the specified date range.');
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Line ', ERROR_LINE()));
    END CATCH;

    SELECT * FROM #Result;
    DROP TABLE #Result;
END
GO

CREATE OR ALTER PROCEDURE [SCIA].[spGetPageVisitsByPage]
    @dtStartDate DATETIME = NULL,
    @dtEndDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #Result
    (
        bResult BIT DEFAULT (1),
        vchMessage VARCHAR(500) DEFAULT (''),
        vchPageUrl NVARCHAR(500) DEFAULT (''),
        vchPageName NVARCHAR(200) DEFAULT (''),
        TotalVisits INT DEFAULT (0),
        UniqueVisitors INT DEFAULT (0),
        dtLastVisit DATETIME DEFAULT ('1900-01-01')
    );

    BEGIN TRY
        IF @dtStartDate IS NULL
            SET @dtStartDate = DATEADD(DAY, -30, CAST(GETUTCDATE() AS DATE));
        IF @dtEndDate IS NULL
            SET @dtEndDate = DATEADD(DAY, 1, CAST(GETUTCDATE() AS DATE));

        INSERT INTO #Result (
            vchPageUrl, vchPageName, TotalVisits, UniqueVisitors, dtLastVisit
        )
        SELECT
            pv.vchPageUrl,
            pv.vchPageName,
            COUNT(*),
            COUNT(DISTINCT
                CASE
                    WHEN NULLIF(LTRIM(RTRIM(pv.vchVisitorId)), '') IS NOT NULL THEN N'VID:' + LTRIM(RTRIM(pv.vchVisitorId))
                    WHEN NULLIF(LTRIM(RTRIM(pv.vchIpAddress)), '') IS NOT NULL THEN N'IP:' + LTRIM(RTRIM(pv.vchIpAddress))
                    WHEN NULLIF(LTRIM(RTRIM(pv.vchUserAgent)), '') IS NOT NULL
                        THEN N'UA:' + CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', pv.vchUserAgent), 2)
                    ELSE N'ROW:' + CAST(pv.iIdPageVisit AS NVARCHAR(20))
                END
            ),
            MAX(pv.dtVisitDate)
        FROM SCIA.PageVisits pv WITH(NOLOCK)
        WHERE pv.dtVisitDate >= @dtStartDate
          AND pv.dtVisitDate < @dtEndDate
        GROUP BY pv.vchPageUrl, pv.vchPageName
        ORDER BY COUNT(*) DESC;

        IF NOT EXISTS (SELECT 1 FROM #Result WHERE TotalVisits > 0)
            INSERT INTO #Result (bResult, vchMessage)
            VALUES (0, 'No visits found for the specified date range.');
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Line ', ERROR_LINE()));
    END CATCH;

    SELECT * FROM #Result;
    DROP TABLE #Result;
END
GO

CREATE OR ALTER PROCEDURE [SCIA].[spGetDailyTraffic]
    @dtStartDate DATETIME = NULL,
    @dtEndDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #Result
    (
        bResult BIT DEFAULT (1),
        vchMessage VARCHAR(500) DEFAULT (''),
        dtVisitDay DATETIME DEFAULT ('1900-01-01'),
        TotalVisits INT DEFAULT (0),
        UniqueVisitors INT DEFAULT (0),
        NewVisitors INT DEFAULT (0)
    );

    BEGIN TRY
        IF @dtStartDate IS NULL
            SET @dtStartDate = DATEADD(DAY, -30, CAST(GETUTCDATE() AS DATE));
        IF @dtEndDate IS NULL
            SET @dtEndDate = DATEADD(DAY, 1, CAST(GETUTCDATE() AS DATE));

        INSERT INTO #Result (
            dtVisitDay, TotalVisits, UniqueVisitors, NewVisitors
        )
        SELECT
            CAST(dtVisitDate AS DATE),
            COUNT(*),
            COUNT(DISTINCT
                CASE
                    WHEN NULLIF(LTRIM(RTRIM(vchVisitorId)), '') IS NOT NULL THEN N'VID:' + LTRIM(RTRIM(vchVisitorId))
                    WHEN NULLIF(LTRIM(RTRIM(vchIpAddress)), '') IS NOT NULL THEN N'IP:' + LTRIM(RTRIM(vchIpAddress))
                    WHEN NULLIF(LTRIM(RTRIM(vchUserAgent)), '') IS NOT NULL
                        THEN N'UA:' + CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', vchUserAgent), 2)
                    ELSE N'ROW:' + CAST(iIdPageVisit AS NVARCHAR(20))
                END
            ),
            COUNT(CASE WHEN bIsUniqueVisitor = 1 THEN 1 END)
        FROM SCIA.PageVisits WITH(NOLOCK)
        WHERE dtVisitDate >= @dtStartDate
          AND dtVisitDate < @dtEndDate
        GROUP BY CAST(dtVisitDate AS DATE)
        ORDER BY CAST(dtVisitDate AS DATE) ASC;

        IF NOT EXISTS (SELECT 1 FROM #Result WHERE TotalVisits > 0)
            INSERT INTO #Result (bResult, vchMessage)
            VALUES (0, 'No visits found for the specified date range.');
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Line ', ERROR_LINE()));
    END CATCH;

    SELECT * FROM #Result;
    DROP TABLE #Result;
END
GO

CREATE OR ALTER PROCEDURE [SCIA].[spGetPageVisitsStats]
(
    @vchPageName NVARCHAR(200) = '',
    @dtStartDate DATETIME = NULL,
    @dtEndDate DATETIME = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @dtStartDate IS NULL SET @dtStartDate = CAST(GETDATE() AS DATE);
    IF @dtEndDate IS NULL SET @dtEndDate = GETDATE();

    CREATE TABLE #Result
    (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        vchPageName NVARCHAR(200) DEFAULT(''),
        iHour INT DEFAULT(0),
        iTotalVisits INT DEFAULT(0),
        iUniqueVisits INT DEFAULT(0)
    );

    BEGIN TRY
        INSERT INTO #Result (vchPageName, iHour, iTotalVisits, iUniqueVisits)
        SELECT
            vchPageName,
            DATEPART(HOUR, dtVisitDate) AS iHour,
            COUNT(iIdPageVisit) AS iTotalVisits,
            COUNT(DISTINCT
                CASE
                    WHEN NULLIF(LTRIM(RTRIM(vchVisitorId)), '') IS NOT NULL THEN N'VID:' + LTRIM(RTRIM(vchVisitorId))
                    WHEN NULLIF(LTRIM(RTRIM(vchIpAddress)), '') IS NOT NULL THEN N'IP:' + LTRIM(RTRIM(vchIpAddress))
                    WHEN NULLIF(LTRIM(RTRIM(vchUserAgent)), '') IS NOT NULL
                        THEN N'UA:' + CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', vchUserAgent), 2)
                    ELSE N'ROW:' + CAST(iIdPageVisit AS NVARCHAR(20))
                END
            ) AS iUniqueVisits
        FROM SCIA.PageVisits WITH(NOLOCK)
        WHERE
            dtVisitDate >= @dtStartDate AND dtVisitDate <= @dtEndDate
            AND (@vchPageName = '' OR vchPageName = @vchPageName)
        GROUP BY
            vchPageName,
            DATEPART(HOUR, dtVisitDate)
        ORDER BY
            vchPageName,
            iHour;

        IF NOT EXISTS (SELECT 1 FROM #Result)
            INSERT INTO #Result (bResult, vchMessage)
            VALUES (0, 'No se encontraron visitas en el periodo especificado.');
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE()));
    END CATCH;

    SELECT * FROM #Result;
    DROP TABLE #Result;
END
GO

/* ========== 4. WhatsApp users list ========== */
CREATE OR ALTER PROCEDURE [SCIA].[spGetWhatsAppUsers]
(
    @iTopRows INT = 100
)
AS
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

PRINT 'Migration AnalyticsVisitorId_WhatsAppUsers completed.';
GO
