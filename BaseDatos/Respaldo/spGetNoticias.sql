ALTER PROCEDURE spGetNoticias
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
