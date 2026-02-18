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

GO

-- spGetNoticias
CREATE OR ALTER PROCEDURE spGetNoticias
    @bActivo BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        n.iIdNoticia,
        n.vchTitulo,
        n.vchTituloEn,
        n.nvchContenido,
        n.nvchContenidoEn,
        n.vchImagenPortada,
        n.dtFechaPublicacion,
        n.bActivo,
        ni.iIdNoticiaImagen,
        ni.vchUrlImagen
    FROM Noticias n
    LEFT JOIN Noticias_Imagenes ni ON n.iIdNoticia = ni.iIdNoticia
    WHERE (@bActivo IS NULL OR n.bActivo = @bActivo)
    ORDER BY n.dtFechaPublicacion DESC;
END;
GO

-- spSaveNoticia
CREATE OR ALTER PROCEDURE spSaveNoticia
    @iIdNoticia INT,
    @vchTitulo VARCHAR(200),
    @vchTituloEn VARCHAR(200) = NULL,
    @nvchContenido NVARCHAR(MAX),
    @nvchContenidoEn NVARCHAR(MAX) = NULL,
    @vchImagenPortada VARCHAR(500) = NULL,
    @dtFechaPublicacion DATETIME,
    @bActivo BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @bResult BIT = 0;
    DECLARE @vchMessage VARCHAR(255) = '';

    BEGIN TRY
        IF @iIdNoticia = 0
        BEGIN
            INSERT INTO Noticias (vchTitulo, vchTituloEn, nvchContenido, nvchContenidoEn, vchImagenPortada, dtFechaPublicacion, bActivo)
            VALUES (@vchTitulo, @vchTituloEn, @nvchContenido, @nvchContenidoEn, @vchImagenPortada, @dtFechaPublicacion, @bActivo);

            SET @iIdNoticia = SCOPE_IDENTITY();
            SET @vchMessage = 'Noticia creada correctamente.';
        END
        ELSE
        BEGIN
            UPDATE Noticias
            SET vchTitulo = @vchTitulo,
                vchTituloEn = @vchTituloEn,
                nvchContenido = @nvchContenido,
                nvchContenidoEn = @nvchContenidoEn,
                vchImagenPortada = ISNULL(@vchImagenPortada, vchImagenPortada), -- Keep existing if null passed
                dtFechaPublicacion = @dtFechaPublicacion,
                bActivo = @bActivo
            WHERE iIdNoticia = @iIdNoticia;

            SET @vchMessage = 'Noticia actualizada correctamente.';
        END

        SET @bResult = 1;
    END TRY
    BEGIN CATCH
        SET @bResult = 0;
        SET @vchMessage = ERROR_MESSAGE();
    END CATCH

    SELECT @bResult AS bResult, @vchMessage AS vchMessage, @iIdNoticia AS iIdNoticia;
END;
GO

-- spSaveNoticiaImagen
CREATE OR ALTER PROCEDURE spSaveNoticiaImagen
    @iIdNoticia INT,
    @vchUrlImagen VARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO Noticias_Imagenes (iIdNoticia, vchUrlImagen)
    VALUES (@iIdNoticia, @vchUrlImagen);
    
    -- Update cover if null
    UPDATE Noticias 
    SET vchImagenPortada = @vchUrlImagen 
    WHERE iIdNoticia = @iIdNoticia AND (vchImagenPortada IS NULL OR vchImagenPortada = '');
    
    SELECT 1 AS bResult, 'Imagen guardada' AS vchMessage;
END;
GO
