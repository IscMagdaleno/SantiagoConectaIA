-- Tabla: Tramite
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tramite')
BEGIN
CREATE TABLE Tramite
(
    iIdTramite INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    vchNombre VARCHAR(250) NOT NULL,
    nvchDescripcion NVARCHAR(MAX) NOT NULL,
    iIdCategoria INT NOT NULL DEFAULT(0),
    bModalidadEnLinea BIT NOT NULL DEFAULT 0,
    mCosto MONEY NOT NULL DEFAULT 0,
    iIdOficina INT NULL,
    dtFechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
    dtFechaActualizacion DATETIME NULL,
    bActivo BIT NOT NULL DEFAULT 1
);
END;
GO

-- Tabla: Requisito
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Requisito')
BEGIN
CREATE TABLE Requisito
(
    iIdRequisito INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    iIdTramite INT NOT NULL,
    vchNombre VARCHAR(250) NOT NULL,
    nvchDetalle NVARCHAR(1000) NULL,
    bObligatorio BIT NOT NULL DEFAULT 1,
    bActivo BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Requisito_Tramite FOREIGN KEY (iIdTramite) REFERENCES Tramite(iIdTramite)
);
END;
GO

-- Tabla: Documento
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Documento')
BEGIN
CREATE TABLE Documento
(
    iIdDocumento INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    iIdTramite INT NOT NULL,
    vchNombre VARCHAR(250) NOT NULL,
    vchUrl VARCHAR(500) NULL,
    bActivo BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Documento_Tramite FOREIGN KEY (iIdTramite) REFERENCES Tramite(iIdTramite)
);
END;
GO
