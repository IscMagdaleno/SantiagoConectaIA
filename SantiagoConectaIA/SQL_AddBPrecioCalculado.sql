-- Add bPrecioCalculado column to Tramite table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Tramite') AND name = 'bPrecioCalculado')
BEGIN
    ALTER TABLE Tramite
    ADD bPrecioCalculado BIT NOT NULL DEFAULT (0);

    PRINT 'Column bPrecioCalculado added to Tramite table.';
END
ELSE
BEGIN
    PRINT 'Column bPrecioCalculado already exists.';
END
GO
