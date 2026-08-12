IF OBJECT_ID('SCIA.spSearchFeed') IS NULL
    EXEC('CREATE PROCEDURE SCIA.spSearchFeed AS SET NOCOUNT ON;');
GO

ALTER PROCEDURE [SCIA].[spSearchFeed]
(
    @vchTexto NVARCHAR(500) = NULL,
    @iPage INT = 1,
    @iPageSize INT = 50
)
AS
/*
** Propósito: Búsqueda unificada en trámites, noticias, eventos y cápsulas.
** Última fecha: 12/08/2026
*/
BEGIN
    SET NOCOUNT ON;

    IF @iPage IS NULL OR @iPage < 1 SET @iPage = 1;
    IF @iPageSize IS NULL OR @iPageSize < 1 SET @iPageSize = 50;
    IF @iPageSize > 100 SET @iPageSize = 100;

    DECLARE @Offset INT = (@iPage - 1) * @iPageSize;
    DECLARE @Texto NVARCHAR(500) = NULLIF(LTRIM(RTRIM(@vchTexto)), N'');

    CREATE TABLE #Result
    (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        vchTipoEntidad VARCHAR(20) DEFAULT(''),
        iIdEntidad INT DEFAULT(-1),
        vchTitulo NVARCHAR(300) DEFAULT(''),
        nvchDescripcion NVARCHAR(1000) DEFAULT(''),
        nvchContenidoDetallado NVARCHAR(MAX) NULL,
        vchImagenUrl NVARCHAR(500) DEFAULT(''),
        dtFecha DATETIME NULL,
        iTotalRegistros INT DEFAULT(0)
    );

    BEGIN TRY
        IF @Texto IS NULL
        BEGIN
            INSERT INTO #Result (bResult, vchMessage)
            VALUES (0, 'Debe indicar un texto de búsqueda.');
        END
        ELSE
        BEGIN
            ;WITH FeedUnion AS
            (
                SELECT
                    CAST('TRAMITE' AS VARCHAR(20)) AS vchTipoEntidad,
                    T.iIdTramite AS iIdEntidad,
                    CAST(T.vchNombre AS NVARCHAR(300)) AS vchTitulo,
                    CAST(LEFT(ISNULL(T.nvchDescripcion, N''), 400) AS NVARCHAR(1000)) AS nvchDescripcion,
                    CAST(NULL AS NVARCHAR(MAX)) AS nvchContenidoDetallado,
                    CAST(N'' AS NVARCHAR(500)) AS vchImagenUrl,
                    T.dtFechaCreacion AS dtFecha
                FROM dbo.Tramite T WITH (NOLOCK)
                WHERE ISNULL(T.bActivo, 1) = 1
                  AND (
                        T.vchNombre LIKE N'%' + @Texto + N'%'
                     OR T.nvchDescripcion LIKE N'%' + @Texto + N'%'
                  )

                UNION ALL

                SELECT
                    CAST('NOTICIA' AS VARCHAR(20)),
                    N.iIdNoticia,
                    CAST(N.vchTitulo AS NVARCHAR(300)),
                    CAST(LEFT(ISNULL(N.nvchContenido, N''), 400) AS NVARCHAR(1000)),
                    CAST(NULL AS NVARCHAR(MAX)),
                    CAST(ISNULL(N.vchImagenPortada, N'') AS NVARCHAR(500)),
                    N.dtFechaPublicacion
                FROM dbo.Noticias N WITH (NOLOCK)
                WHERE ISNULL(N.bActivo, 1) = 1
                  AND (
                        N.vchTitulo LIKE N'%' + @Texto + N'%'
                     OR N.nvchContenido LIKE N'%' + @Texto + N'%'
                  )

                UNION ALL

                SELECT
                    CAST('EVENTO' AS VARCHAR(20)),
                    E.iIdEvento,
                    CAST(E.vchNombre AS NVARCHAR(300)),
                    CAST(LEFT(ISNULL(E.nvchDescripcion, N''), 400) AS NVARCHAR(1000)),
                    CAST(NULL AS NVARCHAR(MAX)),
                    CAST(ISNULL(Img.vchUrlImagen, N'') AS NVARCHAR(500)),
                    ISNULL(E.dtFechaInicio, E.dtFechaRegistro)
                FROM SCIA.Eventos E WITH (NOLOCK)
                OUTER APPLY
                (
                    SELECT TOP (1) IR.vchUrlImagen
                    FROM SCIA.ImagenesRegistro IR WITH (NOLOCK)
                    WHERE IR.vchTablaOrigen = N'Eventos'
                      AND IR.iIdRegistro = E.iIdEvento
                    ORDER BY IR.iOrden ASC, IR.iIdImagen ASC
                ) Img
                WHERE ISNULL(E.bEstatus, 1) = 1
                  AND (
                        E.vchNombre LIKE N'%' + @Texto + N'%'
                     OR E.nvchDescripcion LIKE N'%' + @Texto + N'%'
                  )

                UNION ALL

                SELECT
                    CAST('CAPSULA' AS VARCHAR(20)),
                    I.iIdInformacionLocal,
                    CAST(I.nvchTitulo AS NVARCHAR(300)),
                    CAST(LEFT(ISNULL(I.nvchDescripcionCorta, N''), 400) AS NVARCHAR(1000)),
                    I.nvchContenidoDetallado,
                    CAST(N'' AS NVARCHAR(500)),
                    I.dtFechaCreacion
                FROM SCIA.InformacionLocal I WITH (NOLOCK)
                WHERE ISNULL(I.bActivo, 1) = 1
                  AND (
                        I.nvchTitulo LIKE N'%' + @Texto + N'%'
                     OR I.nvchDescripcionCorta LIKE N'%' + @Texto + N'%'
                     OR I.nvchPalabrasClave LIKE N'%' + @Texto + N'%'
                     OR I.nvchContenidoDetallado LIKE N'%' + @Texto + N'%'
                  )
            ),
            OrderedFeed AS
            (
                SELECT
                    F.*,
                    COUNT(1) OVER() AS iTotalRegistros
                FROM FeedUnion F
            )
            INSERT INTO #Result
            (
                vchTipoEntidad, iIdEntidad, vchTitulo, nvchDescripcion,
                nvchContenidoDetallado, vchImagenUrl, dtFecha, iTotalRegistros
            )
            SELECT
                O.vchTipoEntidad,
                O.iIdEntidad,
                O.vchTitulo,
                O.nvchDescripcion,
                O.nvchContenidoDetallado,
                O.vchImagenUrl,
                O.dtFecha,
                O.iTotalRegistros
            FROM OrderedFeed O
            ORDER BY O.vchTipoEntidad, O.dtFecha DESC
            OFFSET @Offset ROWS FETCH NEXT @iPageSize ROWS ONLY;

            IF NOT EXISTS (SELECT 1 FROM #Result WHERE iIdEntidad <> -1)
            BEGIN
                INSERT INTO #Result (bResult, vchMessage)
                VALUES (0, 'No se encontraron resultados para la búsqueda.');
            END
        END
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE()));
    END CATCH

    SELECT * FROM #Result;
    DROP TABLE #Result;
END
GO
