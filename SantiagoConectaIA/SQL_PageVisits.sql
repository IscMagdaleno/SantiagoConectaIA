-- =============================================
-- Visitor Tracking - PageVisits Table & Stored Procedures
-- For SantiagoConectaIA Database
-- =============================================

-- 1. Create the PageVisits table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PageVisits')
BEGIN
    CREATE TABLE PageVisits
    (
        iIdPageVisit INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
        vchPageUrl NVARCHAR(500) NOT NULL,
        vchPageName NVARCHAR(200) NOT NULL DEFAULT (''),
        vchIpAddress NVARCHAR(50) NOT NULL DEFAULT (''),
        vchUserAgent NVARCHAR(500) NOT NULL DEFAULT (''),
        vchReferrer NVARCHAR(500) NOT NULL DEFAULT (''),
        vchBrowser NVARCHAR(100) NOT NULL DEFAULT (''),
        vchOperatingSystem NVARCHAR(100) NOT NULL DEFAULT (''),
        vchDeviceType NVARCHAR(50) NOT NULL DEFAULT (''),
        bIsUniqueVisitor BIT NOT NULL DEFAULT (1),
        dtVisitDate DATETIME NOT NULL DEFAULT (GETUTCDATE()),
        dtCreatedAt DATETIME NOT NULL DEFAULT (GETUTCDATE())
    );

    CREATE NONCLUSTERED INDEX IX_PageVisits_VisitDate ON PageVisits (dtVisitDate);
    CREATE NONCLUSTERED INDEX IX_PageVisits_PageUrl ON PageVisits (vchPageUrl);
    CREATE NONCLUSTERED INDEX IX_PageVisits_IpAddress ON PageVisits (vchIpAddress);
    CREATE NONCLUSTERED INDEX IX_PageVisits_IpDate ON PageVisits (vchIpAddress, dtVisitDate);

    PRINT 'Table PageVisits created successfully.';
END
ELSE
BEGIN
    PRINT 'Table PageVisits already exists.';
END
GO

-- =============================================
-- 2. Stored Procedure: spSavePageVisit
-- Records a new page visit
-- =============================================
IF OBJECT_ID('spSavePageVisit') IS NULL
    EXEC ('CREATE PROCEDURE spSavePageVisit AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE spSavePageVisit
    @vchPageUrl NVARCHAR(500),
    @vchPageName NVARCHAR(200) = '',
    @vchIpAddress NVARCHAR(50) = '',
    @vchUserAgent NVARCHAR(500) = '',
    @vchReferrer NVARCHAR(500) = '',
    @vchBrowser NVARCHAR(100) = '',
    @vchOperatingSystem NVARCHAR(100) = '',
    @vchDeviceType NVARCHAR(50) = ''
AS
/*
** Creador:      SantiagoConectaIA
** Propósito:    Registrar una visita a una página del sitio web.
** Última fecha: 2026-06-24
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

        IF @vchIpAddress <> ''
        BEGIN
            IF EXISTS (
                SELECT 1
                FROM PageVisits WITH(NOLOCK)
                WHERE vchIpAddress = @vchIpAddress
                  AND dtVisitDate >= @TodayStart
            )
            BEGIN
                SET @bIsUnique = 0;
            END
        END

        SET @trancount = @@TRANCOUNT;

        IF @trancount > 0
            SAVE TRANSACTION spSavePageVisit;
        ELSE
            BEGIN TRANSACTION;

        INSERT INTO PageVisits (
            vchPageUrl, vchPageName, vchIpAddress, vchUserAgent,
            vchReferrer, vchBrowser, vchOperatingSystem, vchDeviceType,
            bIsUniqueVisitor, dtVisitDate, dtCreatedAt
        )
        VALUES (
            @vchPageUrl, @vchPageName, @vchIpAddress, @vchUserAgent,
            @vchReferrer, @vchBrowser, @vchOperatingSystem, @vchDeviceType,
            @bIsUnique, GETUTCDATE(), GETUTCDATE()
        );

        DECLARE @iIdPageVisit INT = SCOPE_IDENTITY();

        IF @trancount = 0
            COMMIT TRANSACTION;

        INSERT INTO @Result (bResult, vchMessage, iIdPageVisit, bIsUniqueVisitor)
        VALUES (1, 'Visit recorded successfully.', @iIdPageVisit, @bIsUnique);
    END TRY
    BEGIN CATCH
        SELECT @vchError = CONVERT(VARCHAR(200), CONCAT('spSavePageVisit: ', ERROR_MESSAGE(), ' in ', ERROR_PROCEDURE(), ' at line ', ERROR_LINE()));
        PRINT CONCAT('spSavePageVisit: ', ERROR_MESSAGE(), ' in ', ERROR_PROCEDURE(), ' at line ', ERROR_LINE());

        IF @trancount = 0
            ROLLBACK TRANSACTION;
        ELSE IF @trancount <> -1 AND XACT_STATE() <> -1
            ROLLBACK TRANSACTION spSavePageVisit;
    END CATCH;

_Fin:
    IF LEN(@vchError) > 0
    BEGIN
        INSERT INTO @Result (bResult, vchMessage)
        VALUES (0, @vchError);
    END

    SELECT * FROM @Result;

    SET NOCOUNT OFF;
END
GO

PRINT 'Procedure spSavePageVisit created successfully.';
GO

-- =============================================
-- 3. Stored Procedure: spGetPageVisitsSummary
-- Returns overall visit statistics
-- =============================================
IF OBJECT_ID('spGetPageVisitsSummary') IS NULL
    EXEC ('CREATE PROCEDURE spGetPageVisitsSummary AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE spGetPageVisitsSummary
    @dtStartDate DATETIME = NULL,
    @dtEndDate DATETIME = NULL
AS
/*
** Creador:      SantiagoConectaIA
** Propósito:    Obtener resumen de visitas (totales, únicas, nuevas, recurrentes).
** Última fecha: 2026-06-24
*/
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

        INSERT INTO #Result (
            TotalVisits, UniqueVisitors, NewVisitors, ReturningVisitors, StartDate, EndDate
        )
        SELECT
            COUNT(*),
            COUNT(DISTINCT vchIpAddress),
            COUNT(CASE WHEN bIsUniqueVisitor = 1 THEN 1 END),
            COUNT(CASE WHEN bIsUniqueVisitor = 0 THEN 1 END),
            @dtStartDate,
            @dtEndDate
        FROM PageVisits WITH(NOLOCK)
        WHERE dtVisitDate >= @dtStartDate
          AND dtVisitDate < @dtEndDate;

        IF NOT EXISTS (SELECT 1 FROM #Result WHERE TotalVisits > 0)
        BEGIN
            INSERT INTO #Result (bResult, vchMessage)
            VALUES (0, 'No visits found for the specified date range.');
        END
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Line ', ERROR_LINE()));
        PRINT CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Line ', ERROR_LINE());
    END CATCH;

    SELECT * FROM #Result;
    DROP TABLE #Result;
END
GO

PRINT 'Procedure spGetPageVisitsSummary created successfully.';
GO

-- =============================================
-- 4. Stored Procedure: spGetPageVisitsByPage
-- Returns visit counts grouped by page
-- =============================================
IF OBJECT_ID('spGetPageVisitsByPage') IS NULL
    EXEC ('CREATE PROCEDURE spGetPageVisitsByPage AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE spGetPageVisitsByPage
    @dtStartDate DATETIME = NULL,
    @dtEndDate DATETIME = NULL
AS
/*
** Creador:      SantiagoConectaIA
** Propósito:    Obtener visitas agrupadas por página.
** Última fecha: 2026-06-24
*/
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
            COUNT(DISTINCT pv.vchIpAddress),
            MAX(pv.dtVisitDate)
        FROM PageVisits pv WITH(NOLOCK)
        WHERE pv.dtVisitDate >= @dtStartDate
          AND pv.dtVisitDate < @dtEndDate
        GROUP BY pv.vchPageUrl, pv.vchPageName
        ORDER BY COUNT(*) DESC;

        IF NOT EXISTS (SELECT 1 FROM #Result WHERE TotalVisits > 0)
        BEGIN
            INSERT INTO #Result (bResult, vchMessage)
            VALUES (0, 'No visits found for the specified date range.');
        END
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Line ', ERROR_LINE()));
        PRINT CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Line ', ERROR_LINE());
    END CATCH;

    SELECT * FROM #Result;
    DROP TABLE #Result;
END
GO

PRINT 'Procedure spGetPageVisitsByPage created successfully.';
GO

-- =============================================
-- 5. Stored Procedure: spGetDailyTraffic
-- Returns daily visit counts (for charts)
-- =============================================
IF OBJECT_ID('spGetDailyTraffic') IS NULL
    EXEC ('CREATE PROCEDURE spGetDailyTraffic AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE spGetDailyTraffic
    @dtStartDate DATETIME = NULL,
    @dtEndDate DATETIME = NULL
AS
/*
** Creador:      SantiagoConectaIA
** Propósito:    Obtener tráfico diario del sitio web.
** Última fecha: 2026-06-24
*/
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
            COUNT(DISTINCT vchIpAddress),
            COUNT(CASE WHEN bIsUniqueVisitor = 1 THEN 1 END)
        FROM PageVisits WITH(NOLOCK)
        WHERE dtVisitDate >= @dtStartDate
          AND dtVisitDate < @dtEndDate
        GROUP BY CAST(dtVisitDate AS DATE)
        ORDER BY CAST(dtVisitDate AS DATE) ASC;

        IF NOT EXISTS (SELECT 1 FROM #Result WHERE TotalVisits > 0)
        BEGIN
            INSERT INTO #Result (bResult, vchMessage)
            VALUES (0, 'No visits found for the specified date range.');
        END
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Line ', ERROR_LINE()));
        PRINT CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Line ', ERROR_LINE());
    END CATCH;

    SELECT * FROM #Result;
    DROP TABLE #Result;
END
GO

PRINT 'Procedure spGetDailyTraffic created successfully.';
GO

-- =============================================
-- 6. Stored Procedure: spGetRecentVisits
-- Returns recent visit records (for admin view)
-- =============================================
IF OBJECT_ID('spGetRecentVisits') IS NULL
    EXEC ('CREATE PROCEDURE spGetRecentVisits AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE spGetRecentVisits
    @iTopRows INT = 100
AS
/*
** Creador:      SantiagoConectaIA
** Propósito:    Obtener las visitas más recientes al sitio web.
** Última fecha: 2026-06-24
*/
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #Result
    (
        bResult BIT DEFAULT (1),
        vchMessage VARCHAR(500) DEFAULT (''),
        iIdPageVisit INT DEFAULT (-1),
        vchPageUrl NVARCHAR(500) DEFAULT (''),
        vchPageName NVARCHAR(200) DEFAULT (''),
        vchIpAddress NVARCHAR(50) DEFAULT (''),
        vchBrowser NVARCHAR(100) DEFAULT (''),
        vchOperatingSystem NVARCHAR(100) DEFAULT (''),
        vchDeviceType NVARCHAR(50) DEFAULT (''),
        vchReferrer NVARCHAR(500) DEFAULT (''),
        bIsUniqueVisitor BIT DEFAULT (0),
        dtVisitDate DATETIME DEFAULT ('1900-01-01')
    );

    BEGIN TRY
        INSERT INTO #Result (
            iIdPageVisit, vchPageUrl, vchPageName, vchIpAddress,
            vchBrowser, vchOperatingSystem, vchDeviceType, vchReferrer,
            bIsUniqueVisitor, dtVisitDate
        )
        SELECT TOP (@iTopRows)
            iIdPageVisit, vchPageUrl, vchPageName, vchIpAddress,
            vchBrowser, vchOperatingSystem, vchDeviceType, vchReferrer,
            bIsUniqueVisitor, dtVisitDate
        FROM PageVisits WITH(NOLOCK)
        ORDER BY dtVisitDate DESC;

        IF NOT EXISTS (SELECT 1 FROM #Result WHERE iIdPageVisit > 0)
        BEGIN
            INSERT INTO #Result (bResult, vchMessage)
            VALUES (0, 'No visits found.');
        END
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Line ', ERROR_LINE()));
        PRINT CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Line ', ERROR_LINE());
    END CATCH;

    SELECT * FROM #Result;
    DROP TABLE #Result;
END
GO

PRINT 'Procedure spGetRecentVisits created successfully.';
GO

PRINT '=============================================';
PRINT 'All visitor tracking objects created successfully!';
PRINT '=============================================';
