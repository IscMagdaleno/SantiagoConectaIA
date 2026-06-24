-- Recreate Fase table (required by Mensaje FK)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Fase')
BEGIN
    CREATE TABLE Fase
    (
        iIdFase INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
        iIdProyecto INT NOT NULL DEFAULT (1),
        smNumeroSecuencia SMALLINT NOT NULL DEFAULT (1),
        nvchTitulo NVARCHAR(200) NOT NULL DEFAULT (''),
        nvchDescripcion NVARCHAR(500) NOT NULL DEFAULT (''),
        dtCreadoEn DATETIME NOT NULL DEFAULT (GETUTCDATE()),
        dtActualizadoEn DATETIME NOT NULL DEFAULT (GETUTCDATE())
    );

    -- Insert default record so chat works
    INSERT INTO Fase (iIdProyecto, smNumeroSecuencia, nvchTitulo, nvchDescripcion)
    VALUES (1, 1, 'Conversación General', 'Fase general de interacción con el asistente de trámites');

    PRINT 'Table Fase recreated successfully.';
END
ELSE
BEGIN
    PRINT 'Table Fase already exists.';
END
GO
