-- ==========================================================================================
-- SCRIPT DE ACTUALIZACIÓN: FEED Y BÚSQUEDA GENERAL CON EMPRENDIMIENTOS Y PRODUCTOS
-- Módulo: FeedModule / Busqueda / Emprendimientos
-- ==========================================================================================

-- 1. ACTUALIZAR PROCEDIMIENTO [SCIA].[spSearchFeed]
CREATE OR ALTER PROCEDURE [SCIA].[spSearchFeed]
(
    @vchTexto NVARCHAR(500) = NULL,
    @iPage INT = 1,
    @iPageSize INT = 50
)
AS
/*
** Propósito: Búsqueda unificada en emprendimientos, productos, trámites, noticias, eventos, cápsulas y comunidad.
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
        iTotalRegistros INT DEFAULT(0),
        nvchImagenesJson NVARCHAR(MAX) NULL
    );

    BEGIN TRY
        IF @Texto IS NULL
        BEGIN
            INSERT INTO #Result (bResult, vchMessage)
            VALUES (0, 'Debe indicar un texto de búsqueda.');
        END
        ELSE
        BEGIN
            -- Tabla temporal de palabras clave para búsqueda multi-término (ej. "pay cajeta")
            CREATE TABLE #TerminosBusqueda
            (
                vchTermino NVARCHAR(100) COLLATE DATABASE_DEFAULT
            );

            INSERT INTO #TerminosBusqueda (vchTermino)
            SELECT DISTINCT LOWER(LTRIM(RTRIM(value)))
            FROM STRING_SPLIT(@Texto, ' ')
            WHERE LEN(LTRIM(RTRIM(value))) > 0;

            DECLARE @NumTerminos INT = (SELECT COUNT(1) FROM #TerminosBusqueda);

            ;WITH FeedUnion AS
            (
                -- 1. EMPRENDIMIENTOS / NEGOCIOS
                SELECT
                    CAST('EMPRENDIMIENTO' AS VARCHAR(20)) AS vchTipoEntidad,
                    E.iIdEmpresa AS iIdEntidad,
                    CAST(E.vchNombreComercial AS NVARCHAR(300)) AS vchTitulo,
                    CAST(LEFT(ISNULL(NULLIF(E.nvchDescripcion, N''), ISNULL(E.vchSlogan, N'Negocio local en Santiago Papasquiaro')), 400) AS NVARCHAR(1000)) AS nvchDescripcion,
                    CAST(E.nvchDescripcion AS NVARCHAR(MAX)) AS nvchContenidoDetallado,
                    CAST(ISNULL(E.vchLogoUrl, N'') AS NVARCHAR(500)) AS vchImagenUrl,
                    CAST(NULL AS DATETIME) AS dtFecha,
                    CAST(NULL AS NVARCHAR(MAX)) AS nvchImagenesJson
                FROM SCIA.Empresa E WITH (NOLOCK)
                WHERE ISNULL(E.bEstatus, 1) = 1
                  AND (
                        -- Coincidencia de la frase completa
                        E.vchNombreComercial LIKE N'%' + @Texto + N'%'
                     OR E.vchSlogan LIKE N'%' + @Texto + N'%'
                     OR E.nvchDescripcion LIKE N'%' + @Texto + N'%'
                     OR E.nvchHistoria LIKE N'%' + @Texto + N'%'
                     OR (
                        -- O coincidencia de todas las palabras clave separadas
                        @NumTerminos > 1 AND (
                            SELECT COUNT(1)
                            FROM #TerminosBusqueda TB
                            WHERE CONCAT(ISNULL(E.vchNombreComercial, N''), N' ', ISNULL(E.vchSlogan, N''), N' ', ISNULL(E.nvchDescripcion, N''), N' ', ISNULL(E.nvchHistoria, N'')) LIKE N'%' + TB.vchTermino + N'%'
                        ) = @NumTerminos
                     )
                  )

                UNION ALL

                -- 2. PRODUCTOS Y SERVICIOS DE EMPRENDIMIENTOS
                SELECT
                    CAST('PRODUCTO' AS VARCHAR(20)) AS vchTipoEntidad,
                    P.iIdProducto AS iIdEntidad,
                    CAST(P.vchNombre AS NVARCHAR(300)) AS vchTitulo,
                    CAST(LEFT(
                        CONCAT(
                            -- Descripción o categoría
                            ISNULL(NULLIF(LTRIM(RTRIM(P.nvchDescripcionCorta)), N''), ISNULL(C.vchNombre, N'Producto')),
                            -- Precio / Descuento
                            CASE 
                                WHEN ISNULL(P.bAplicaDescuento, 0) = 1 AND ISNULL(P.mPrecioDescuento, 0) > 0 THEN 
                                    CONCAT(N' · $', FORMAT(P.mPrecioDescuento, N'N2'), N' (Antes $', FORMAT(P.mPrecio, N'N2'), N')')
                                WHEN ISNULL(P.mPrecio, 0) > 0 THEN 
                                    CONCAT(N' · $', FORMAT(P.mPrecio, N'N2'))
                                ELSE N'' 
                            END,
                            -- Nombre del negocio
                            CASE 
                                WHEN EmpP.vchNombreComercial IS NOT NULL THEN CONCAT(N' — ', EmpP.vchNombreComercial) 
                                ELSE N'' 
                            END
                        ), 400
                    ) AS NVARCHAR(1000)) AS nvchDescripcion,
                    CAST(NULLIF(LTRIM(RTRIM(P.nvchDescripcionCorta)), N'') AS NVARCHAR(MAX)) AS nvchContenidoDetallado,
                    CAST(ISNULL(P.vchImagenUrl, ISNULL(EmpP.vchLogoUrl, N'')) AS NVARCHAR(500)) AS vchImagenUrl,
                    CAST(NULL AS DATETIME) AS dtFecha,
                    CAST((SELECT EmpP.iIdEmpresa AS iIdEmpresa FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS NVARCHAR(MAX)) AS nvchImagenesJson
                FROM SCIA.ProductoServicio P WITH (NOLOCK)
                INNER JOIN SCIA.CategoriaCatalogo C WITH (NOLOCK) ON P.iIdCategoriaCat = C.iIdCategoriaCat
                INNER JOIN SCIA.Empresa EmpP WITH (NOLOCK) ON C.iIdEmpresa = EmpP.iIdEmpresa
                WHERE ISNULL(P.bEstatus, 1) = 1
                  AND ISNULL(EmpP.bEstatus, 1) = 1
                  AND (
                        -- Coincidencia frase completa
                        P.vchNombre LIKE N'%' + @Texto + N'%'
                     OR P.nvchDescripcionCorta LIKE N'%' + @Texto + N'%'
                     OR C.vchNombre LIKE N'%' + @Texto + N'%'
                     OR EmpP.vchNombreComercial LIKE N'%' + @Texto + N'%'
                     OR (
                        -- O coincidencia de todas las palabras clave separadas
                        @NumTerminos > 1 AND (
                            SELECT COUNT(1)
                            FROM #TerminosBusqueda TB
                            WHERE CONCAT(ISNULL(P.vchNombre, N''), N' ', ISNULL(P.nvchDescripcionCorta, N''), N' ', ISNULL(C.vchNombre, N''), N' ', ISNULL(EmpP.vchNombreComercial, N'')) LIKE N'%' + TB.vchTermino + N'%'
                        ) = @NumTerminos
                     )
                  )

                UNION ALL

                -- 3. TRÁMITES
                SELECT
                    CAST('TRAMITE' AS VARCHAR(20)) AS vchTipoEntidad,
                    T.iIdTramite AS iIdEntidad,
                    CAST(T.vchNombre AS NVARCHAR(300)) AS vchTitulo,
                    CAST(LEFT(ISNULL(T.nvchDescripcion, N''), 400) AS NVARCHAR(1000)) AS nvchDescripcion,
                    CAST(NULL AS NVARCHAR(MAX)) AS nvchContenidoDetallado,
                    CAST(N'' AS NVARCHAR(500)) AS vchImagenUrl,
                    T.dtFechaCreacion AS dtFecha,
                    CAST(NULL AS NVARCHAR(MAX)) AS nvchImagenesJson
                FROM [SCIA].Tramite T WITH (NOLOCK)
                WHERE ISNULL(T.bActivo, 1) = 1
                  AND (
                        T.vchNombre LIKE N'%' + @Texto + N'%'
                     OR T.nvchDescripcion LIKE N'%' + @Texto + N'%'
                     OR (
                        @NumTerminos > 1 AND (
                            SELECT COUNT(1)
                            FROM #TerminosBusqueda TB
                            WHERE CONCAT(ISNULL(T.vchNombre, N''), N' ', ISNULL(T.nvchDescripcion, N'')) LIKE N'%' + TB.vchTermino + N'%'
                        ) = @NumTerminos
                     )
                  )

                UNION ALL

                -- 4. NOTICIAS
                SELECT
                    CAST('NOTICIA' AS VARCHAR(20)),
                    N.iIdNoticia,
                    CAST(N.vchTitulo AS NVARCHAR(300)),
                    CAST(LEFT(ISNULL(N.nvchContenido, N''), 400) AS NVARCHAR(1000)),
                    CAST(NULL AS NVARCHAR(MAX)),
                    CAST(ISNULL(N.vchImagenPortada, N'') AS NVARCHAR(500)),
                    N.dtFechaPublicacion,
                    CAST(NULL AS NVARCHAR(MAX))
                FROM [SCIA].Noticias N WITH (NOLOCK)
                WHERE ISNULL(N.bActivo, 1) = 1
                  AND (
                        N.vchTitulo LIKE N'%' + @Texto + N'%'
                     OR N.nvchContenido LIKE N'%' + @Texto + N'%'
                     OR (
                        @NumTerminos > 1 AND (
                            SELECT COUNT(1)
                            FROM #TerminosBusqueda TB
                            WHERE CONCAT(ISNULL(N.vchTitulo, N''), N' ', ISNULL(N.nvchContenido, N'')) LIKE N'%' + TB.vchTermino + N'%'
                        ) = @NumTerminos
                     )
                  )

                UNION ALL

                -- 5. EVENTOS
                SELECT
                    CAST('EVENTO' AS VARCHAR(20)),
                    E.iIdEvento,
                    CAST(E.vchNombre AS NVARCHAR(300)),
                    CAST(LEFT(ISNULL(E.nvchDescripcion, N''), 400) AS NVARCHAR(1000)),
                    CAST(NULL AS NVARCHAR(MAX)),
                    CAST(ISNULL(Img.vchUrlImagen, N'') AS NVARCHAR(500)),
                    ISNULL(E.dtFechaInicio, E.dtFechaRegistro),
                    (
                        SELECT IR2.vchUrlImagen
                        FROM SCIA.ImagenesRegistro IR2 WITH (NOLOCK)
                        WHERE IR2.vchTablaOrigen = N'Eventos'
                          AND IR2.iIdRegistro = E.iIdEvento
                          AND IR2.bActivo = 1
                        ORDER BY IR2.iOrden ASC, IR2.iIdImagen ASC
                        FOR JSON PATH
                    )
                FROM SCIA.Eventos E WITH (NOLOCK)
                OUTER APPLY
                (
                    SELECT TOP (1) IR.vchUrlImagen
                    FROM SCIA.ImagenesRegistro IR WITH (NOLOCK)
                    WHERE IR.vchTablaOrigen = N'Eventos'
                      AND IR.iIdRegistro = E.iIdEvento
                      AND IR.bActivo = 1
                    ORDER BY IR.iOrden ASC, IR.iIdImagen ASC
                ) Img
                WHERE ISNULL(E.bEstatus, 1) = 1
                  AND (
                        E.vchNombre LIKE N'%' + @Texto + N'%'
                     OR E.nvchDescripcion LIKE N'%' + @Texto + N'%'
                     OR (
                        @NumTerminos > 1 AND (
                            SELECT COUNT(1)
                            FROM #TerminosBusqueda TB
                            WHERE CONCAT(ISNULL(E.vchNombre, N''), N' ', ISNULL(E.nvchDescripcion, N'')) LIKE N'%' + TB.vchTermino + N'%'
                        ) = @NumTerminos
                     )
                  )

                UNION ALL

                -- 6. CÁPSULAS / DATO CURIOSO
                SELECT
                    CAST('CAPSULA' AS VARCHAR(20)),
                    I.iIdInformacionLocal,
                    CAST(I.nvchTitulo AS NVARCHAR(300)),
                    CAST(LEFT(ISNULL(I.nvchDescripcionCorta, N''), 400) AS NVARCHAR(1000)),
                    I.nvchContenidoDetallado,
                    CAST(N'' AS NVARCHAR(500)),
                    I.dtFechaCreacion,
                    CAST(NULL AS NVARCHAR(MAX))
                FROM SCIA.InformacionLocal I WITH (NOLOCK)
                WHERE ISNULL(I.bActivo, 1) = 1
                  AND (
                        I.nvchTitulo LIKE N'%' + @Texto + N'%'
                     OR I.nvchDescripcionCorta LIKE N'%' + @Texto + N'%'
                     OR I.nvchPalabrasClave LIKE N'%' + @Texto + N'%'
                     OR I.nvchContenidoDetallado LIKE N'%' + @Texto + N'%'
                     OR (
                        @NumTerminos > 1 AND (
                            SELECT COUNT(1)
                            FROM #TerminosBusqueda TB
                            WHERE CONCAT(ISNULL(I.nvchTitulo, N''), N' ', ISNULL(I.nvchDescripcionCorta, N''), N' ', ISNULL(I.nvchPalabrasClave, N''), N' ', ISNULL(I.nvchContenidoDetallado, N'')) LIKE N'%' + TB.vchTermino + N'%'
                        ) = @NumTerminos
                     )
                  )

                UNION ALL

                -- 7. PUBLICACIONES DE CIUDADANOS
                SELECT
                    CAST('PUBLICACION' AS VARCHAR(20)),
                    P.iIdPublicacion,
                    CAST(ISNULL(NULLIF(P.nvchTitulo, ''), ISNULL(C.vchAlias, 'Ciudadano de Santiago')) AS NVARCHAR(300)),
                    CAST(LEFT(ISNULL(P.nvchContenidoTexto, N''), 400) AS NVARCHAR(1000)),
                    CAST(P.nvchContenidoTexto AS NVARCHAR(MAX)),
                    CAST(ISNULL(ImgPub.vchUrlImagen, N'') AS NVARCHAR(500)),
                    P.dtFechaCreacion,
                    (
                        SELECT IR2.vchUrlImagen
                        FROM SCIA.ImagenesRegistro IR2 WITH (NOLOCK)
                        WHERE IR2.vchTablaOrigen = N'PublicacionCiudadano'
                          AND IR2.iIdRegistro = P.iIdPublicacion
                          AND IR2.bActivo = 1
                        ORDER BY IR2.iOrden ASC, IR2.iIdImagen ASC
                        FOR JSON PATH
                    )
                FROM SCIA.PublicacionCiudadano P WITH (NOLOCK)
                INNER JOIN SCIA.Ciudadano C WITH (NOLOCK) ON P.iIdCiudadano = C.iIdCiudadano
                OUTER APPLY
                (
                    SELECT TOP (1) IR.vchUrlImagen
                    FROM SCIA.ImagenesRegistro IR WITH (NOLOCK)
                    WHERE IR.vchTablaOrigen = N'PublicacionCiudadano'
                      AND IR.iIdRegistro = P.iIdPublicacion
                      AND IR.bActivo = 1
                    ORDER BY IR.iOrden ASC, IR.iIdImagen ASC
                ) ImgPub
                WHERE ISNULL(P.bActiva, 1) = 1
                  AND ISNULL(P.bAprobada, 1) = 1
                  AND (
                        P.nvchTitulo LIKE N'%' + @Texto + N'%'
                     OR P.nvchContenidoTexto LIKE N'%' + @Texto + N'%'
                     OR P.vchCategoriaPublicacion LIKE N'%' + @Texto + N'%'
                     OR C.vchAlias LIKE N'%' + @Texto + N'%'
                     OR (
                        @NumTerminos > 1 AND (
                            SELECT COUNT(1)
                            FROM #TerminosBusqueda TB
                            WHERE CONCAT(ISNULL(P.nvchTitulo, N''), N' ', ISNULL(P.nvchContenidoTexto, N''), N' ', ISNULL(P.vchCategoriaPublicacion, N''), N' ', ISNULL(C.vchAlias, N'')) LIKE N'%' + TB.vchTermino + N'%'
                        ) = @NumTerminos
                     )
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
                nvchContenidoDetallado, vchImagenUrl, dtFecha, iTotalRegistros,
                nvchImagenesJson
            )
            SELECT
                O.vchTipoEntidad,
                O.iIdEntidad,
                O.vchTitulo,
                O.nvchDescripcion,
                O.nvchContenidoDetallado,
                O.vchImagenUrl,
                O.dtFecha,
                O.iTotalRegistros,
                O.nvchImagenesJson
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

-- 2. ACTUALIZAR PROCEDIMIENTO [SCIA].[spGetFeed]
CREATE OR ALTER PROCEDURE [SCIA].[spGetFeed]
(
    @iPage INT = 1,
    @iPageSize INT = 10,
    @vchSessionSeed VARCHAR(64) = NULL,
    @vchTipoFiltro VARCHAR(20) = NULL
)
AS
/*
** Propósito: Feed principal unificado con filtro por Emprendimientos, Comunidad, Trámites, Noticias, Eventos y Cápsulas.
*/
BEGIN
    SET NOCOUNT ON;

    IF @iPage IS NULL OR @iPage < 1 SET @iPage = 1;
    IF @iPageSize IS NULL OR @iPageSize < 1 SET @iPageSize = 10;
    IF @iPageSize > 50 SET @iPageSize = 50;

    SET @vchTipoFiltro = UPPER(LTRIM(RTRIM(ISNULL(@vchTipoFiltro, 'TODO'))));
    IF @vchTipoFiltro = '' OR @vchTipoFiltro = 'ALL'
        SET @vchTipoFiltro = 'TODO';

    DECLARE @Offset INT = (@iPage - 1) * @iPageSize;

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
        iTotalRegistros INT DEFAULT(0),
        nvchImagenesJson NVARCHAR(MAX) NULL
    );

    BEGIN TRY
        ;WITH FeedUnion AS
        (
            -- 1. TRÁMITES
            SELECT
                CAST('TRAMITE' AS VARCHAR(20)) AS vchTipoEntidad,
                T.iIdTramite AS iIdEntidad,
                CAST(T.vchNombre AS NVARCHAR(300)) AS vchTitulo,
                CAST(LEFT(ISNULL(T.nvchDescripcion, N''), 400) AS NVARCHAR(1000)) AS nvchDescripcion,
                CAST(NULL AS NVARCHAR(MAX)) AS nvchContenidoDetallado,
                CAST(N'' AS NVARCHAR(500)) AS vchImagenUrl,
                T.dtFechaCreacion AS dtFecha,
                CAST(NULL AS NVARCHAR(MAX)) AS nvchImagenesJson
            FROM SCIA.Tramite T WITH (NOLOCK)
            WHERE ISNULL(T.bActivo, 1) = 1

            UNION ALL

            -- 2. NOTICIAS
            SELECT
                CAST('NOTICIA' AS VARCHAR(20)),
                N.iIdNoticia,
                CAST(N.vchTitulo AS NVARCHAR(300)),
                CAST(LEFT(ISNULL(N.nvchContenido, N''), 400) AS NVARCHAR(1000)),
                CAST(NULL AS NVARCHAR(MAX)),
                CAST(ISNULL(N.vchImagenPortada, N'') AS NVARCHAR(500)),
                N.dtFechaPublicacion,
                CAST(NULL AS NVARCHAR(MAX))
            FROM SCIA.Noticias N WITH (NOLOCK)
            WHERE ISNULL(N.bActivo, 1) = 1

            UNION ALL

            -- 3. EVENTOS
            SELECT
                CAST('EVENTO' AS VARCHAR(20)),
                E.iIdEvento,
                CAST(E.vchNombre AS NVARCHAR(300)),
                CAST(LEFT(ISNULL(E.nvchDescripcion, N''), 400) AS NVARCHAR(1000)),
                CAST(NULL AS NVARCHAR(MAX)),
                CAST(ISNULL(Img.vchUrlImagen, N'') AS NVARCHAR(500)),
                ISNULL(E.dtFechaInicio, E.dtFechaRegistro),
                (
                    SELECT IR2.vchUrlImagen
                    FROM SCIA.ImagenesRegistro IR2 WITH (NOLOCK)
                    WHERE IR2.vchTablaOrigen = N'Eventos'
                      AND IR2.iIdRegistro = E.iIdEvento
                      AND IR2.bActivo = 1
                    ORDER BY IR2.iOrden ASC, IR2.iIdImagen ASC
                    FOR JSON PATH
                )
            FROM SCIA.Eventos E WITH (NOLOCK)
            OUTER APPLY
            (
                SELECT TOP (1) IR.vchUrlImagen
                FROM SCIA.ImagenesRegistro IR WITH (NOLOCK)
                WHERE IR.vchTablaOrigen = N'Eventos'
                  AND IR.iIdRegistro = E.iIdEvento
                  AND IR.bActivo = 1
                ORDER BY IR.iOrden ASC, IR.iIdImagen ASC
            ) Img
            WHERE ISNULL(E.bEstatus, 1) = 1

            UNION ALL

            -- 4. CÁPSULAS / DATO CURIOSO
            SELECT
                CAST('CAPSULA' AS VARCHAR(20)),
                I.iIdInformacionLocal,
                CAST(I.nvchTitulo AS NVARCHAR(300)),
                CAST(LEFT(ISNULL(I.nvchDescripcionCorta, N''), 400) AS NVARCHAR(1000)),
                I.nvchContenidoDetallado,
                CAST(N'' AS NVARCHAR(500)),
                I.dtFechaCreacion,
                CAST(NULL AS NVARCHAR(MAX))
            FROM SCIA.InformacionLocal I WITH (NOLOCK)
            WHERE ISNULL(I.bActivo, 1) = 1

            UNION ALL

            -- 5. PUBLICACIONES DE CIUDADANOS
            SELECT
                CAST('PUBLICACION' AS VARCHAR(20)),
                P.iIdPublicacion,
                CAST(ISNULL(NULLIF(P.nvchTitulo, ''), ISNULL(C.vchAlias, 'Ciudadano de Santiago')) AS NVARCHAR(300)),
                CAST(LEFT(ISNULL(P.nvchContenidoTexto, N''), 400) AS NVARCHAR(1000)),
                CAST(P.nvchContenidoTexto AS NVARCHAR(MAX)),
                CAST(ISNULL(ImgPub.vchUrlImagen, N'') AS NVARCHAR(500)),
                P.dtFechaCreacion,
                (
                    SELECT IR2.vchUrlImagen
                    FROM SCIA.ImagenesRegistro IR2 WITH (NOLOCK)
                    WHERE IR2.vchTablaOrigen = N'PublicacionCiudadano'
                      AND IR2.iIdRegistro = P.iIdPublicacion
                      AND IR2.bActivo = 1
                    ORDER BY IR2.iOrden ASC, IR2.iIdImagen ASC
                    FOR JSON PATH
                )
            FROM SCIA.PublicacionCiudadano P WITH (NOLOCK)
            INNER JOIN SCIA.Ciudadano C WITH (NOLOCK) ON P.iIdCiudadano = C.iIdCiudadano
            OUTER APPLY
            (
                SELECT TOP (1) IR.vchUrlImagen
                FROM SCIA.ImagenesRegistro IR WITH (NOLOCK)
                WHERE IR.vchTablaOrigen = N'PublicacionCiudadano'
                  AND IR.iIdRegistro = P.iIdPublicacion
                  AND IR.bActivo = 1
                ORDER BY IR.iOrden ASC, IR.iIdImagen ASC
            ) ImgPub
            WHERE ISNULL(P.bActiva, 1) = 1
              AND ISNULL(P.bAprobada, 1) = 1

            UNION ALL

            -- 6. EMPRENDIMIENTOS LOCALES
            SELECT
                CAST('EMPRENDIMIENTO' AS VARCHAR(20)),
                Emp.iIdEmpresa,
                CAST(Emp.vchNombreComercial AS NVARCHAR(300)),
                CAST(LEFT(ISNULL(NULLIF(Emp.nvchDescripcion, N''), ISNULL(Emp.vchSlogan, N'Negocio local en Santiago Papasquiaro')), 400) AS NVARCHAR(1000)),
                Emp.nvchDescripcion,
                CAST(ISNULL(Emp.vchLogoUrl, N'') AS NVARCHAR(500)),
                NULL,
                CAST(NULL AS NVARCHAR(MAX))
            FROM SCIA.Empresa Emp WITH (NOLOCK)
            WHERE ISNULL(Emp.bEstatus, 1) = 1
        ),
        Filtered AS
        (
            SELECT F.*
            FROM FeedUnion F
            WHERE @vchTipoFiltro = 'TODO'
               OR F.vchTipoEntidad = @vchTipoFiltro
        ),
        Ranked AS
        (
            SELECT
                F.*,
                ROW_NUMBER() OVER (
                    PARTITION BY F.vchTipoEntidad
                    ORDER BY F.dtFecha DESC, F.iIdEntidad DESC
                ) AS iRnTipo,
                ROW_NUMBER() OVER (
                    ORDER BY F.dtFecha DESC, F.iIdEntidad DESC
                ) AS iRnFecha
            FROM Filtered F
        ),
        Patterned AS
        (
            SELECT
                R.*,
                CASE
                    WHEN @vchTipoFiltro <> 'TODO' THEN R.iRnFecha
                    WHEN R.vchTipoEntidad = 'EMPRENDIMIENTO' THEN ((R.iRnTipo - 1) * 7) + 1
                    WHEN R.vchTipoEntidad = 'TRAMITE'        THEN ((R.iRnTipo - 1) * 7) + 2
                    WHEN R.vchTipoEntidad = 'NOTICIA'        THEN (((R.iRnTipo - 1) / 2) * 7) + 3 + ((R.iRnTipo - 1) % 2)
                    WHEN R.vchTipoEntidad = 'PUBLICACION'    THEN ((R.iRnTipo - 1) * 7) + 5
                    WHEN R.vchTipoEntidad = 'CAPSULA'        THEN ((R.iRnTipo - 1) * 7) + 6
                    WHEN R.vchTipoEntidad = 'EVENTO'         THEN ((R.iRnTipo - 1) * 7) + 7
                    ELSE 999999
                END AS iFeedOrder,
                COUNT(1) OVER() AS iTotalRegistros
            FROM Ranked R
        )
        INSERT INTO #Result
        (
            vchTipoEntidad, iIdEntidad, vchTitulo, nvchDescripcion,
            nvchContenidoDetallado, vchImagenUrl, dtFecha, iTotalRegistros,
            nvchImagenesJson
        )
        SELECT
            P.vchTipoEntidad,
            P.iIdEntidad,
            P.vchTitulo,
            P.nvchDescripcion,
            P.nvchContenidoDetallado,
            P.vchImagenUrl,
            P.dtFecha,
            P.iTotalRegistros,
            P.nvchImagenesJson
        FROM Patterned P
        ORDER BY P.iFeedOrder ASC, P.dtFecha DESC
        OFFSET @Offset ROWS FETCH NEXT @iPageSize ROWS ONLY;

        IF NOT EXISTS (SELECT 1 FROM #Result WHERE iIdEntidad <> -1)
        BEGIN
            INSERT INTO #Result (bResult, vchMessage)
            VALUES (0, 'No se encontró contenido para el feed.');
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
