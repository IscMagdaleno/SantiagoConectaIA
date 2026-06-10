IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Noticias')
BEGIN
    CREATE TABLE Noticias
    (
        iIdNoticia INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
        vchTitulo VARCHAR(200) NOT NULL,
        vchTituloEn VARCHAR(200) NULL,
        nvchContenido NVARCHAR(MAX) NOT NULL,
        nvchContenidoEn NVARCHAR(MAX) NULL,
        vchImagenPortada VARCHAR(500) NULL,
        dtFechaPublicacion DATETIME NOT NULL,
        bActivo BIT NOT NULL DEFAULT 1
    );
END;

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Noticias_Imagenes')
BEGIN
    CREATE TABLE Noticias_Imagenes
    (
        iIdNoticiaImagen INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
        iIdNoticia INT NOT NULL,
        vchUrlImagen VARCHAR(500) NOT NULL,
        FOREIGN KEY (iIdNoticia) REFERENCES Noticias(iIdNoticia)
    );
END;