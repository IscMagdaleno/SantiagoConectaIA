USE Engrama;
GO

-- ==========================================================================================
-- 1. TABLA: [SCIA].[PublicacionCiudadano]
-- ==========================================================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PublicacionCiudadano' AND schema_id = SCHEMA_ID('SCIA'))
BEGIN
    CREATE TABLE [SCIA].[PublicacionCiudadano]
    (
        [iIdPublicacion] INT IDENTITY(1,1) NOT NULL,
        [iIdCiudadano] INT NOT NULL,
        [nvchTitulo] VARCHAR(200) NULL,
        [nvchContenidoTexto] NVARCHAR(MAX) NOT NULL,
        [vchCategoriaPublicacion] VARCHAR(50) NOT NULL CONSTRAINT DF_Publicacion_Categoria DEFAULT 'Comunidad',
        [dtFechaCreacion] DATETIME NOT NULL CONSTRAINT DF_Publicacion_Fecha DEFAULT GETDATE(),
        [dtFechaActualizacion] DATETIME NULL,
        [bActiva] BIT NOT NULL CONSTRAINT DF_Publicacion_Activa DEFAULT 1,
        [bAprobada] BIT NOT NULL CONSTRAINT DF_Publicacion_Aprobada DEFAULT 1,
        CONSTRAINT [PK_PublicacionCiudadano] PRIMARY KEY CLUSTERED ([iIdPublicacion] ASC),
        CONSTRAINT [FK_PublicacionCiudadano_Ciudadano] FOREIGN KEY ([iIdCiudadano]) REFERENCES [SCIA].[Ciudadano]([iIdCiudadano])
    );

    CREATE NONCLUSTERED INDEX [IX_PublicacionCiudadano_Feed] 
    ON [SCIA].[PublicacionCiudadano] ([bActiva], [bAprobada], [dtFechaCreacion] DESC)
    INCLUDE ([iIdPublicacion], [iIdCiudadano], [vchCategoriaPublicacion]);

    CREATE NONCLUSTERED INDEX [IX_PublicacionCiudadano_Ciudadano] 
    ON [SCIA].[PublicacionCiudadano] ([iIdCiudadano], [dtFechaCreacion] DESC);
END
GO

-- ==========================================================================================
-- 2. SP: [SCIA].[spSavePublicacionCiudadano]
-- Guarda o edita el encabezado de la publicación
-- ==========================================================================================
CREATE OR ALTER PROCEDURE [SCIA].[spSavePublicacionCiudadano]
    @iIdPublicacion INT = 0,
    @iIdCiudadano INT,
    @nvchTitulo VARCHAR(200) = NULL,
    @nvchContenidoTexto NVARCHAR(MAX),
    @vchCategoriaPublicacion VARCHAR(50) = 'Comunidad'
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        IF @iIdCiudadano IS NULL OR @iIdCiudadano <= 0
        BEGIN
            RAISERROR('El ciudadano es requerido.', 16, 1);
        END

        IF @nvchContenidoTexto IS NULL OR LTRIM(RTRIM(@nvchContenidoTexto)) = ''
        BEGIN
            RAISERROR('El contenido de la publicación no puede estar vacío.', 16, 1);
        END

        IF @iIdPublicacion = 0 OR @iIdPublicacion IS NULL
        BEGIN
            -- INSERTAR PUBLICACIÓN
            INSERT INTO [SCIA].[PublicacionCiudadano]
            (
                [iIdCiudadano],
                [nvchTitulo],
                [nvchContenidoTexto],
                [vchCategoriaPublicacion],
                [dtFechaCreacion],
                [bActiva],
                [bAprobada]
            )
            VALUES
            (
                @iIdCiudadano,
                @nvchTitulo,
                @nvchContenidoTexto,
                ISNULL(@vchCategoriaPublicacion, 'Comunidad'),
                GETDATE(),
                1,
                1
            );

            SET @iIdPublicacion = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            -- ACTUALIZAR PUBLICACIÓN (Verificando propiedad)
            IF NOT EXISTS (SELECT 1 FROM [SCIA].[PublicacionCiudadano] WHERE [iIdPublicacion] = @iIdPublicacion AND [iIdCiudadano] = @iIdCiudadano)
            BEGIN
                RAISERROR('No tienes permisos para editar esta publicación.', 16, 1);
            END

            UPDATE [SCIA].[PublicacionCiudadano]
            SET [nvchTitulo] = @nvchTitulo,
                [nvchContenidoTexto] = @nvchContenidoTexto,
                [vchCategoriaPublicacion] = ISNULL(@vchCategoriaPublicacion, [vchCategoriaPublicacion]),
                [dtFechaActualizacion] = GETDATE()
            WHERE [iIdPublicacion] = @iIdPublicacion;
        END

        COMMIT TRANSACTION;

        SELECT 
            1 AS bResult,
            'Publicación guardada exitosamente.' AS vchMessage,
            @iIdPublicacion AS iIdPublicacion;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

        SELECT 
            0 AS bResult,
            ERROR_MESSAGE() AS vchMessage,
            0 AS iIdPublicacion;
    END CATCH
END
GO

-- ==========================================================================================
-- 3. SP: [SCIA].[spGetPublicacionesCiudadano]
-- Para el feed de la comunidad consultando imágenes desde [SCIA].[ImagenesRegistro]
-- ==========================================================================================
CREATE OR ALTER PROCEDURE [SCIA].[spGetPublicacionesCiudadano]
    @iPage INT = 1,
    @iPageSize INT = 10,
    @vchCategoria VARCHAR(50) = NULL,
    @nvchBusqueda NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @iPage < 1 SET @iPage = 1;
    IF @iPageSize < 1 SET @iPageSize = 10;
    SET @vchCategoria = LTRIM(RTRIM(@vchCategoria));
    SET @nvchBusqueda = LTRIM(RTRIM(@nvchBusqueda));

    ;WITH PublicacionesFiltradas AS (
        SELECT 
            p.[iIdPublicacion],
            p.[iIdCiudadano],
            ISNULL(c.[vchAlias], 'Ciudadano') AS [vchAliasCiudadano],
            c.[vchAvatarUrl] AS [vchAvatarCiudadano],
            ISNULL(c.[bCuentaVerificada], 0) AS [bCiudadanoVerificado],
            p.[nvchTitulo],
            p.[nvchContenidoTexto],
            p.[vchCategoriaPublicacion],
            p.[dtFechaCreacion],
            p.[bActiva],
            (
                SELECT COUNT(1) 
                FROM [SCIA].[Opinion] o 
                WHERE o.[vchTipoEntidad] = 'PUBLICACION' 
                  AND o.[iIdEntidad] = p.[iIdPublicacion] 
                  AND o.[bActivo] = 1
            ) AS [iTotalComentarios],
            (
                SELECT [vchUrlImagen] AS [nvchUrlImagen]
                FROM [SCIA].[ImagenesRegistro] img 
                WHERE img.[vchTablaOrigen] = 'PublicacionCiudadano'
                  AND img.[iIdRegistro] = p.[iIdPublicacion]
                  AND img.[bActivo] = 1
                ORDER BY img.[iOrden] ASC
                FOR JSON PATH
            ) AS [nvchImagenesJson],
            (
                SELECT TOP 1 [vchUrlImagen]
                FROM [SCIA].[ImagenesRegistro] img 
                WHERE img.[vchTablaOrigen] = 'PublicacionCiudadano'
                  AND img.[iIdRegistro] = p.[iIdPublicacion]
                  AND img.[bActivo] = 1
                ORDER BY img.[iOrden] ASC
            ) AS [vchPrimeraImagenUrl],
            COUNT(1) OVER() AS [iTotalRegistros]
        FROM [SCIA].[PublicacionCiudadano] p
        INNER JOIN [SCIA].[Ciudadano] c ON p.[iIdCiudadano] = c.[iIdCiudadano]
        WHERE p.[bActiva] = 1 
          AND p.[bAprobada] = 1
          AND (@vchCategoria IS NULL OR @vchCategoria = '' OR @vchCategoria = 'TODOS' OR p.[vchCategoriaPublicacion] = @vchCategoria)
          AND (@nvchBusqueda IS NULL OR @nvchBusqueda = '' OR p.[nvchContenidoTexto] LIKE '%' + @nvchBusqueda + '%' OR p.[nvchTitulo] LIKE '%' + @nvchBusqueda + '%')
    )
    SELECT 
        1 AS bResult,
        'Ok' AS vchMessage,
        pf.[iIdPublicacion],
        pf.[iIdCiudadano],
        pf.[vchAliasCiudadano],
        pf.[vchAvatarCiudadano],
        pf.[bCiudadanoVerificado],
        pf.[nvchTitulo],
        pf.[nvchContenidoTexto],
        pf.[vchCategoriaPublicacion],
        pf.[dtFechaCreacion],
        pf.[bActiva],
        pf.[iTotalComentarios],
        pf.[nvchImagenesJson],
        pf.[vchPrimeraImagenUrl],
        pf.[iTotalRegistros]
    FROM PublicacionesFiltradas pf
    ORDER BY pf.[dtFechaCreacion] DESC
    OFFSET (@iPage - 1) * @iPageSize ROWS
    FETCH NEXT @iPageSize ROWS ONLY;
END
GO

-- ==========================================================================================
-- 4. SP: [SCIA].[spGetMisPublicacionesCiudadano]
-- Para el panel del ciudadano en /unete consultando imágenes desde [SCIA].[ImagenesRegistro]
-- ==========================================================================================
CREATE OR ALTER PROCEDURE [SCIA].[spGetMisPublicacionesCiudadano]
    @iIdCiudadano INT,
    @iPage INT = 1,
    @iPageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    IF @iPage < 1 SET @iPage = 1;
    IF @iPageSize < 1 SET @iPageSize = 20;

    ;WITH MisPubs AS (
        SELECT 
            p.[iIdPublicacion],
            p.[iIdCiudadano],
            ISNULL(c.[vchAlias], 'Yo') AS [vchAliasCiudadano],
            c.[vchAvatarUrl] AS [vchAvatarCiudadano],
            ISNULL(c.[bCuentaVerificada], 0) AS [bCiudadanoVerificado],
            p.[nvchTitulo],
            p.[nvchContenidoTexto],
            p.[vchCategoriaPublicacion],
            p.[dtFechaCreacion],
            p.[bActiva],
            (
                SELECT COUNT(1) 
                FROM [SCIA].[Opinion] o 
                WHERE o.[vchTipoEntidad] = 'PUBLICACION' 
                  AND o.[iIdEntidad] = p.[iIdPublicacion] 
                  AND o.[bActivo] = 1
            ) AS [iTotalComentarios],
            (
                SELECT [vchUrlImagen] AS [nvchUrlImagen]
                FROM [SCIA].[ImagenesRegistro] img 
                WHERE img.[vchTablaOrigen] = 'PublicacionCiudadano'
                  AND img.[iIdRegistro] = p.[iIdPublicacion]
                  AND img.[bActivo] = 1
                ORDER BY img.[iOrden] ASC
                FOR JSON PATH
            ) AS [nvchImagenesJson],
            (
                SELECT TOP 1 [vchUrlImagen]
                FROM [SCIA].[ImagenesRegistro] img 
                WHERE img.[vchTablaOrigen] = 'PublicacionCiudadano'
                  AND img.[iIdRegistro] = p.[iIdPublicacion]
                  AND img.[bActivo] = 1
                ORDER BY img.[iOrden] ASC
            ) AS [vchPrimeraImagenUrl],
            COUNT(1) OVER() AS [iTotalRegistros]
        FROM [SCIA].[PublicacionCiudadano] p
        INNER JOIN [SCIA].[Ciudadano] c ON p.[iIdCiudadano] = c.[iIdCiudadano]
        WHERE p.[iIdCiudadano] = @iIdCiudadano
          AND p.[bActiva] = 1
    )
    SELECT 
        1 AS bResult,
        'Ok' AS vchMessage,
        mp.[iIdPublicacion],
        mp.[iIdCiudadano],
        mp.[vchAliasCiudadano],
        mp.[vchAvatarCiudadano],
        mp.[bCiudadanoVerificado],
        mp.[nvchTitulo],
        mp.[nvchContenidoTexto],
        mp.[vchCategoriaPublicacion],
        mp.[dtFechaCreacion],
        mp.[bActiva],
        mp.[iTotalComentarios],
        mp.[nvchImagenesJson],
        mp.[vchPrimeraImagenUrl],
        mp.[iTotalRegistros]
    FROM MisPubs mp
    ORDER BY mp.[dtFechaCreacion] DESC
    OFFSET (@iPage - 1) * @iPageSize ROWS
    FETCH NEXT @iPageSize ROWS ONLY;
END
GO

-- ==========================================================================================
-- 5. SP: [SCIA].[spDeletePublicacionCiudadano]
-- Baja lógica de la publicación y sus imágenes en [SCIA].[ImagenesRegistro]
-- ==========================================================================================
CREATE OR ALTER PROCEDURE [SCIA].[spDeletePublicacionCiudadano]
    @iIdPublicacion INT,
    @iIdCiudadano INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [SCIA].[PublicacionCiudadano] WHERE [iIdPublicacion] = @iIdPublicacion AND [iIdCiudadano] = @iIdCiudadano)
        BEGIN
            SELECT 0 AS bResult, 'Publicación no encontrada o sin permisos para eliminarla.' AS vchMessage;
            RETURN;
        END

        UPDATE [SCIA].[PublicacionCiudadano]
        SET [bActiva] = 0,
            [dtFechaActualizacion] = GETDATE()
        WHERE [iIdPublicacion] = @iIdPublicacion AND [iIdCiudadano] = @iIdCiudadano;

        UPDATE [SCIA].[ImagenesRegistro]
        SET [bActivo] = 0
        WHERE [vchTablaOrigen] = 'PublicacionCiudadano' AND [iIdRegistro] = @iIdPublicacion;

        SELECT 1 AS bResult, 'Publicación eliminada correctamente.' AS vchMessage;
    END TRY
    BEGIN CATCH
        SELECT 0 AS bResult, ERROR_MESSAGE() AS vchMessage;
    END CATCH
END
GO

-- ==========================================================================================
-- 6. SP: [SCIA].[spGetFeed] (ACTUALIZADO CON PUBLICACIONES DE CIUDADANOS)
-- Incluye 'PUBLICACION' en el CTE FeedUnion y en el cálculo de orden para el Feed
-- ==========================================================================================
CREATE OR ALTER PROCEDURE [SCIA].[spGetFeed]
(
    @iPage INT = 1,
    @iPageSize INT = 10,
    @vchSessionSeed VARCHAR(64) = NULL,
    @vchTipoFiltro VARCHAR(20) = NULL
)
AS
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

            -- NUEVA ENTIDAD: PUBLICACIONES DE CIUDADANOS
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
                    WHEN R.vchTipoEntidad = 'TRAMITE'     THEN ((R.iRnTipo - 1) * 6) + 1
                    WHEN R.vchTipoEntidad = 'NOTICIA'     THEN (((R.iRnTipo - 1) / 2) * 6) + 2 + ((R.iRnTipo - 1) % 2)
                    WHEN R.vchTipoEntidad = 'PUBLICACION' THEN ((R.iRnTipo - 1) * 6) + 4
                    WHEN R.vchTipoEntidad = 'CAPSULA'     THEN ((R.iRnTipo - 1) * 6) + 5
                    WHEN R.vchTipoEntidad = 'EVENTO'      THEN ((R.iRnTipo - 1) * 6) + 6
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

-- ==========================================================================================
-- 7. SP: [SCIA].[spSearchFeed] (ACTUALIZADO CON BÚSQUEDA EN PUBLICACIONES DE CIUDADANOS)
-- Permite buscar términos en Título, Contenido, Categoría o Alias del autor
-- ==========================================================================================
CREATE OR ALTER PROCEDURE [SCIA].[spSearchFeed]
(
    @vchTexto NVARCHAR(500) = NULL,
    @iPage INT = 1,
    @iPageSize INT = 50
)
AS
/*
** Propósito: Búsqueda unificada en trámites, noticias, eventos, cápsulas y publicaciones de la comunidad.
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
            ;WITH FeedUnion AS
            (
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
                  )

                UNION ALL

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
                  )

                UNION ALL

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
                  )

                UNION ALL

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
                  )

                UNION ALL

                -- PUBLICACIONES DE CIUDADANOS (NUEVO)
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
