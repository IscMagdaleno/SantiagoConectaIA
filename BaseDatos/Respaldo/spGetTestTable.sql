ALTER PROCEDURE spGetTestTable
AS
/*
** Name:          spGetTestTable
** Purpose:       This stored procedure retrieves data from the dbo.Test_Table and outputs the results, or an error message if no records are found or if an error occurs.
*/

-- Create a temporary table to store the response
CREATE TABLE #Result
(
    bResult BIT DEFAULT (1),                       
    vchMessage VARCHAR(500) DEFAULT(''),          
    iIdTest_Table INT DEFAULT (-1),                     
    vchName VARCHAR (100) DEFAULT (''),                 
    vchEmail VARCHAR (100) DEFAULT (''),               
    dtRegistered DATETIME DEFAULT ('1900-01-01')          
);

-- Set no count on to avoid extra result sets
SET NOCOUNT ON;

BEGIN TRY
    -- Insert data from dbo.Test_Table to the temporary table
    INSERT INTO #Result (iIdTest_Table, vchName, vchEmail, dtRegistered)
    SELECT iIdTest_Table, vchName, vchEmail, dtRegistered
    FROM dbo.Test_Table TT WITH(NOLOCK);

    -- Check if no data was inserted
    IF NOT EXISTS (SELECT 1 FROM #Result WHERE iIdTest_Table != -1)
    BEGIN
        -- Insert a no-records-found message if no data present
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, 'No se encontraron registros.');
    END
END TRY

BEGIN CATCH
    -- Insert error information into the temporary table on exception
    INSERT INTO #Result (bResult, vchMessage)
    SELECT 0, CONCAT(ERROR_PROCEDURE(), ' : ', ERROR_MESSAGE(), ' - ', ERROR_LINE());

    -- Optional: Print the error message (for debugging purposes)
    PRINT CONCAT(ERROR_PROCEDURE(), ' : ', ERROR_MESSAGE(), ' - ', ERROR_LINE());
END CATCH;

-- Select all data from the temporary table
SELECT * FROM #Result;

-- Clean up: Drop the temporary table
DROP TABLE #Result;
