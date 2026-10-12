-- ==========================================================================================
-- 03 - FEED CON PRIORIDADES + COMENTARIOS EN PUBLICACIONES DE CIUDADANOS
-- Ejecutar ANTES de publicar el API (el API envía el nuevo parámetro @bSoloAleatorio).
--
-- Cambios:
--   1. [SCIA].[spGetFeed]
--        - Se quitan los trámites (y emprendimientos) del feed. Solo: PUBLICACION, NOTICIA, EVENTO, CAPSULA.
--        - Prioridad 1: publicaciones de ciudadanos creadas HOY (hora de México), las más nuevas primero.
--        - Prioridad 2: lo del día en curso: noticias publicadas hoy + eventos de hoy o en curso,
--                       intercalados (noticia, evento, noticia, evento...).
--        - Prioridad 3: todo lo demás en orden aleatorio estable por sesión (@vchSessionSeed),
--                       mezclando los tipos de forma proporcional.
--        - @bSoloAleatorio = 1 omite las prioridades 1 y 2 (el cliente lo usa para seguir
--          mostrando contenido aleatorio cuando ya se acabó todo lo anterior).
--        - Devuelve autor, avatar, categoría/lugar, total de comentarios y la prioridad de cada tarjeta.
--   2. [SCIA].[spGetOpiniones] y [SCIA].[spSaveOpinion] aceptan el tipo 'PUBLICACION'
--      (antes se rechazaban los comentarios en las publicaciones de la comunidad).
--
-- Supuestos: las fechas de la base están en UTC (así lo asume DateTimeExtensions.ToUsuarioLocal);
-- "hoy" se calcula con la zona 'Central Standard Time (Mexico)'.
-- ==========================================================================================

-- ------------------------------------------------------------------------------------------
-- 1. [SCIA].[spGetFeed]
-- ------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE [SCIA].[spGetFeed]
(
    @iPage INT = 1,
    @iPageSize INT = 10,
    @vchSessionSeed VARCHAR(64) = NULL,
    @vchTipoFiltro VARCHAR(20) = NULL,
    @bSoloAleatorio BIT = 0
)
AS
/*
** Propósito: Feed público con prioridades.
**   TODO / NULL : 1) publicaciones de hoy  2) noticias y eventos del día  3) aleatorio infinito
**   PUBLICACION | NOTICIA | EVENTO | CAPSULA : solo ese tipo (hoy primero, luego por fecha / aleatorio)
*/
BEGIN
    SET NOCOUNT ON;

    IF @iPage IS NULL OR @iPage < 1 SET @iPage = 1;
    IF @iPageSize IS NULL OR @iPageSize < 1 SET @iPageSize = 10;
    IF @iPageSize > 50 SET @iPageSize = 50;

    SET @vchTipoFiltro = UPPER(LTRIM(RTRIM(ISNULL(@vchTipoFiltro, 'TODO'))));
    IF @vchTipoFiltro NOT IN ('PUBLICACION', 'NOTICIA', 'EVENTO', 'CAPSULA')
        SET @vchTipoFiltro = 'TODO';

    SET @vchSessionSeed = ISNULL(@vchSessionSeed, '');
    SET @bSoloAleatorio = ISNULL(@bSoloAleatorio, 0);
    IF @vchTipoFiltro <> 'TODO' SET @bSoloAleatorio = 0;

    DECLARE @Offset INT = (@iPage - 1) * @iPageSize;

    -- Ventana del día en curso (hora de México) expresada en UTC
    DECLARE @AhoraMx DATETIME2(0) =
        CAST(CAST(SYSUTCDATETIME() AS DATETIMEOFFSET) AT TIME ZONE 'Central Standard Time (Mexico)' AS DATETIME2(0));
    DECLARE @InicioHoyMx DATETIME2(0) = CAST(CAST(@AhoraMx AS DATE) AS DATETIME2(0));
    DECLARE @InicioHoyUtc DATETIME2(0) =
        CAST((@InicioHoyMx AT TIME ZONE 'Central Standard Time (Mexico)') AT TIME ZONE 'UTC' AS DATETIME2(0));
    DECLARE @FinHoyUtc DATETIME2(0) = DATEADD(DAY, 1, @InicioHoyUtc);

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
        nvchImagenesJson NVARCHAR(MAX) NULL,
        iTotalComentarios INT DEFAULT(0),
        vchAutor NVARCHAR(100) DEFAULT(''),
        vchAvatarUrl NVARCHAR(500) NULL,
        bAutorVerificado BIT DEFAULT(0),
        vchEtiqueta NVARCHAR(100) DEFAULT(''),
        iPrioridad INT DEFAULT(3)
    );

    BEGIN TRY
        ;WITH FeedUnion AS
        (
            -- NOTICIAS
            SELECT
                CAST('NOTICIA' AS VARCHAR(20)) AS vchTipoEntidad,
                N.iIdNoticia AS iIdEntidad,
                CAST(N.vchTitulo AS NVARCHAR(300)) AS vchTitulo,
                CAST(LEFT(ISNULL(N.nvchContenido, N''), 400) AS NVARCHAR(1000)) AS nvchDescripcion,
                CAST(NULL AS NVARCHAR(MAX)) AS nvchContenidoDetallado,
                CAST(ISNULL(N.vchImagenPortada, N'') AS NVARCHAR(500)) AS vchImagenUrl,
                CAST(N.dtFechaPublicacion AS DATETIME2(0)) AS dtFecha,
                CAST(NULL AS DATETIME2(0)) AS dtFin,
                CAST(NULL AS NVARCHAR(MAX)) AS nvchImagenesJson,
                (
                    SELECT COUNT(1) FROM SCIA.Opinion O WITH (NOLOCK)
                    WHERE O.vchTipoEntidad = 'NOTICIA' AND O.iIdEntidad = N.iIdNoticia AND O.bActivo = 1
                ) AS iTotalComentarios,
                CAST(N'' AS NVARCHAR(100)) AS vchAutor,
                CAST(NULL AS NVARCHAR(500)) AS vchAvatarUrl,
                CAST(0 AS BIT) AS bAutorVerificado,
                CAST(N'' AS NVARCHAR(100)) AS vchEtiqueta
            FROM SCIA.Noticias N WITH (NOLOCK)
            WHERE ISNULL(N.bActivo, 1) = 1

            UNION ALL

            -- EVENTOS (los que ya terminaron antes de hoy no se muestran)
            SELECT
                CAST('EVENTO' AS VARCHAR(20)),
                E.iIdEvento,
                CAST(E.vchNombre AS NVARCHAR(300)),
                CAST(LEFT(ISNULL(E.nvchDescripcion, N''), 400) AS NVARCHAR(1000)),
                CAST(NULL AS NVARCHAR(MAX)),
                CAST(ISNULL(Img.vchUrlImagen, N'') AS NVARCHAR(500)),
                CAST(ISNULL(E.dtFechaInicio, E.dtFechaRegistro) AS DATETIME2(0)),
                CAST(ISNULL(E.dtFechaFin, ISNULL(E.dtFechaInicio, E.dtFechaRegistro)) AS DATETIME2(0)),
                (
                    SELECT IR2.vchUrlImagen
                    FROM SCIA.ImagenesRegistro IR2 WITH (NOLOCK)
                    WHERE IR2.vchTablaOrigen = N'Eventos'
                      AND IR2.iIdRegistro = E.iIdEvento
                      AND IR2.bActivo = 1
                    ORDER BY IR2.iOrden ASC, IR2.iIdImagen ASC
                    FOR JSON PATH
                ),
                (
                    SELECT COUNT(1) FROM SCIA.Opinion O WITH (NOLOCK)
                    WHERE O.vchTipoEntidad = 'EVENTO' AND O.iIdEntidad = E.iIdEvento AND O.bActivo = 1
                ),
                CAST(N'' AS NVARCHAR(100)),
                CAST(NULL AS NVARCHAR(500)),
                CAST(0 AS BIT),
                CAST(LEFT(ISNULL(E.vchLugar, N''), 100) AS NVARCHAR(100))
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
              AND ISNULL(E.dtFechaFin, ISNULL(E.dtFechaInicio, E.dtFechaRegistro)) >= @InicioHoyUtc

            UNION ALL

            -- CÁPSULAS / "CONOCE SANTIAGO" (datos curiosos)
            SELECT
                CAST('CAPSULA' AS VARCHAR(20)),
                I.iIdInformacionLocal,
                CAST(I.nvchTitulo AS NVARCHAR(300)),
                CAST(LEFT(ISNULL(I.nvchDescripcionCorta, N''), 400) AS NVARCHAR(1000)),
                I.nvchContenidoDetallado,
                CAST(N'' AS NVARCHAR(500)),
                CAST(I.dtFechaCreacion AS DATETIME2(0)),
                CAST(NULL AS DATETIME2(0)),
                CAST(NULL AS NVARCHAR(MAX)),
                (
                    SELECT COUNT(1) FROM SCIA.Opinion O WITH (NOLOCK)
                    WHERE O.vchTipoEntidad = 'CAPSULA' AND O.iIdEntidad = I.iIdInformacionLocal AND O.bActivo = 1
                ),
                CAST(N'' AS NVARCHAR(100)),
                CAST(NULL AS NVARCHAR(500)),
                CAST(0 AS BIT),
                CAST(LEFT(ISNULL(I.nvchCategoria, N''), 100) AS NVARCHAR(100))
            FROM SCIA.InformacionLocal I WITH (NOLOCK)
            WHERE ISNULL(I.bActivo, 1) = 1

            UNION ALL

            -- PUBLICACIONES DE CIUDADANOS
            SELECT
                CAST('PUBLICACION' AS VARCHAR(20)),
                P.iIdPublicacion,
                CAST(ISNULL(P.nvchTitulo, N'') AS NVARCHAR(300)),
                CAST(LEFT(ISNULL(P.nvchContenidoTexto, N''), 400) AS NVARCHAR(1000)),
                CAST(P.nvchContenidoTexto AS NVARCHAR(MAX)),
                CAST(ISNULL(ImgPub.vchUrlImagen, N'') AS NVARCHAR(500)),
                CAST(P.dtFechaCreacion AS DATETIME2(0)),
                CAST(NULL AS DATETIME2(0)),
                (
                    SELECT IR2.vchUrlImagen
                    FROM SCIA.ImagenesRegistro IR2 WITH (NOLOCK)
                    WHERE IR2.vchTablaOrigen = N'PublicacionCiudadano'
                      AND IR2.iIdRegistro = P.iIdPublicacion
                      AND IR2.bActivo = 1
                    ORDER BY IR2.iOrden ASC, IR2.iIdImagen ASC
                    FOR JSON PATH
                ),
                (
                    SELECT COUNT(1) FROM SCIA.Opinion O WITH (NOLOCK)
                    WHERE O.vchTipoEntidad = 'PUBLICACION' AND O.iIdEntidad = P.iIdPublicacion AND O.bActivo = 1
                ),
                CAST(LEFT(ISNULL(C.vchAlias, N'Ciudadano de Santiago'), 100) AS NVARCHAR(100)),
                CAST(C.vchAvatarUrl AS NVARCHAR(500)),
                CAST(ISNULL(C.bCuentaVerificada, 0) AS BIT),
                CAST(LEFT(ISNULL(P.vchCategoriaPublicacion, N'Comunidad'), 100) AS NVARCHAR(100))
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
              AND ISNULL(C.bActivo, 1) = 1
        ),
        Filtered AS
        (
            SELECT F.*
            FROM FeedUnion F
            WHERE @vchTipoFiltro = 'TODO'
               OR F.vchTipoEntidad = @vchTipoFiltro
        ),
        Clasificado AS
        (
            SELECT
                F.*,
                CASE
                    WHEN @bSoloAleatorio = 1 THEN 3
                    -- 1) Lo que la comunidad publicó hoy: siempre al principio
                    WHEN F.vchTipoEntidad = 'PUBLICACION'
                         AND F.dtFecha >= @InicioHoyUtc AND F.dtFecha < @FinHoyUtc THEN 1
                    -- 2) Lo del día en curso: noticias de hoy y eventos de hoy o en curso
                    WHEN F.vchTipoEntidad = 'NOTICIA'
                         AND F.dtFecha >= @InicioHoyUtc AND F.dtFecha < @FinHoyUtc THEN 2
                    WHEN F.vchTipoEntidad = 'EVENTO'
                         AND F.dtFecha < @FinHoyUtc AND F.dtFin >= @InicioHoyUtc THEN 2
                    -- 3) Todo lo demás
                    ELSE 3
                END AS iPrioridad
            FROM Filtered F
        ),
        Ranked AS
        (
            SELECT
                C.*,
                -- Orden por fecha dentro de cada tipo: lo nuevo primero; los eventos, el más próximo primero
                ROW_NUMBER() OVER (
                    PARTITION BY C.iPrioridad, C.vchTipoEntidad
                    ORDER BY
                        CASE WHEN C.vchTipoEntidad = 'EVENTO'
                             THEN DATEDIFF_BIG(SECOND, '2000-01-01', C.dtFecha)
                             ELSE -DATEDIFF_BIG(SECOND, '2000-01-01', C.dtFecha)
                        END ASC,
                        C.iIdEntidad DESC
                ) AS iRnFecha,
                -- Orden aleatorio estable por sesión dentro de cada tipo
                ROW_NUMBER() OVER (
                    PARTITION BY C.iPrioridad, C.vchTipoEntidad
                    ORDER BY CHECKSUM(@vchSessionSeed, C.vchTipoEntidad, C.iIdEntidad), C.iIdEntidad
                ) AS iRnAleatorio,
                COUNT(1) OVER() AS iTotalRegistros
            FROM Clasificado C
        ),
        Ordenado AS
        (
            SELECT
                R.*,
                CASE
                    WHEN R.iPrioridad = 1 THEN CAST(R.iRnFecha AS FLOAT)
                    -- Noticia, evento, noticia, evento...
                    WHEN R.iPrioridad = 2 THEN CAST(R.iRnFecha AS FLOAT) + CASE WHEN R.vchTipoEntidad = 'EVENTO' THEN 0.5 ELSE 0 END
                    -- Aleatorio mezclando tipos de forma proporcional (peso = tarjetas por ciclo)
                    WHEN @vchTipoFiltro = 'TODO' THEN
                        CAST(R.iRnAleatorio AS FLOAT)
                        / CASE R.vchTipoEntidad WHEN 'NOTICIA' THEN 2.0 WHEN 'PUBLICACION' THEN 2.0 ELSE 1.0 END
                        + (ABS(CAST(CHECKSUM(@vchSessionSeed, R.vchTipoEntidad, R.iIdEntidad) AS BIGINT)) % 1000) / 100000.0
                    -- Con filtro por tipo: por fecha, salvo los datos curiosos que van al azar
                    WHEN R.vchTipoEntidad = 'CAPSULA' THEN CAST(R.iRnAleatorio AS FLOAT)
                    ELSE CAST(R.iRnFecha AS FLOAT)
                END AS fOrden
            FROM Ranked R
        )
        INSERT INTO #Result
        (
            vchTipoEntidad, iIdEntidad, vchTitulo, nvchDescripcion,
            nvchContenidoDetallado, vchImagenUrl, dtFecha, iTotalRegistros,
            nvchImagenesJson, iTotalComentarios, vchAutor, vchAvatarUrl,
            bAutorVerificado, vchEtiqueta, iPrioridad
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
            O.nvchImagenesJson,
            O.iTotalComentarios,
            O.vchAutor,
            O.vchAvatarUrl,
            O.bAutorVerificado,
            O.vchEtiqueta,
            O.iPrioridad
        FROM Ordenado O
        ORDER BY O.iPrioridad ASC, O.fOrden ASC, O.vchTipoEntidad ASC, O.iIdEntidad DESC
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

-- ------------------------------------------------------------------------------------------
-- 2. [SCIA].[spGetOpiniones]  (ahora acepta PUBLICACION)
-- ------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE [SCIA].[spGetOpiniones]
(
    @vchTipoEntidad VARCHAR(20),
    @iIdEntidad INT
)
AS
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #Result
    (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdOpinion INT DEFAULT(-1),
        vchTipoEntidad VARCHAR(20) DEFAULT(''),
        iIdEntidad INT DEFAULT(-1),
        iIdCiudadano INT DEFAULT(-1),
        vchAlias NVARCHAR(80) DEFAULT(''),
        iIdOpinionPadre INT NULL,
        nvchTexto NVARCHAR(1000) DEFAULT(''),
        dtFechaCreacion DATETIME NULL
    );

    BEGIN TRY
        SET @vchTipoEntidad = UPPER(LTRIM(RTRIM(ISNULL(@vchTipoEntidad, ''))));

        IF @vchTipoEntidad NOT IN ('NOTICIA', 'TRAMITE', 'EVENTO', 'CAPSULA', 'PUBLICACION') OR @iIdEntidad <= 0
        BEGIN
            INSERT INTO #Result (bResult, vchMessage)
            VALUES (0, 'Contenido inválido para opiniones.');
        END
        ELSE
        BEGIN
            INSERT INTO #Result
            (
                iIdOpinion,
                vchTipoEntidad,
                iIdEntidad,
                iIdCiudadano,
                vchAlias,
                iIdOpinionPadre,
                nvchTexto,
                dtFechaCreacion
            )
            SELECT
                O.iIdOpinion,
                O.vchTipoEntidad,
                O.iIdEntidad,
                O.iIdCiudadano,
                ISNULL(C.vchAlias, N'Ciudadano'),
                O.iIdOpinionPadre,
                O.nvchTexto,
                O.dtFechaCreacion
            FROM SCIA.Opinion O WITH (NOLOCK)
            INNER JOIN SCIA.Ciudadano C WITH (NOLOCK) ON C.iIdCiudadano = O.iIdCiudadano
            WHERE O.vchTipoEntidad = @vchTipoEntidad
              AND O.iIdEntidad = @iIdEntidad
              AND O.bActivo = 1
              AND C.bActivo = 1
            ORDER BY
                CASE WHEN O.iIdOpinionPadre IS NULL THEN O.iIdOpinion ELSE O.iIdOpinionPadre END ASC,
                CASE WHEN O.iIdOpinionPadre IS NULL THEN 0 ELSE 1 END ASC,
                O.dtFechaCreacion ASC;

            IF NOT EXISTS (SELECT 1 FROM #Result WHERE iIdOpinion > 0)
            BEGIN
                INSERT INTO #Result (bResult, vchMessage)
                VALUES (1, 'Sin opiniones.');
            END
        END
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE()));
    END CATCH

    SELECT * FROM #Result ORDER BY
        CASE WHEN iIdOpinionPadre IS NULL THEN iIdOpinion ELSE iIdOpinionPadre END ASC,
        CASE WHEN iIdOpinionPadre IS NULL THEN 0 ELSE 1 END ASC,
        dtFechaCreacion ASC;
    DROP TABLE #Result;
END
GO

-- ------------------------------------------------------------------------------------------
-- 3. [SCIA].[spSaveOpinion]  (ahora acepta PUBLICACION)
-- ------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE [SCIA].[spSaveOpinion]
(
    @vchTipoEntidad VARCHAR(20),
    @iIdEntidad INT,
    @iIdCiudadano INT,
    @iIdOpinionPadre INT = NULL,
    @nvchTexto NVARCHAR(1000)
)
AS
BEGIN
    DECLARE @vchError VARCHAR(200) = '';
    DECLARE @iIdOpinion INT = -1;

    DECLARE @Result AS TABLE
    (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdOpinion INT DEFAULT(-1),
        vchTipoEntidad VARCHAR(20) DEFAULT(''),
        iIdEntidad INT DEFAULT(-1),
        iIdCiudadano INT DEFAULT(-1),
        vchAlias NVARCHAR(80) DEFAULT(''),
        iIdOpinionPadre INT NULL,
        nvchTexto NVARCHAR(1000) DEFAULT(''),
        dtFechaCreacion DATETIME NULL
    );

    SET NOCOUNT ON;

    BEGIN TRY
        SET @vchTipoEntidad = UPPER(LTRIM(RTRIM(ISNULL(@vchTipoEntidad, ''))));
        SET @nvchTexto = LTRIM(RTRIM(ISNULL(@nvchTexto, N'')));

        IF @vchTipoEntidad NOT IN ('NOTICIA', 'TRAMITE', 'EVENTO', 'CAPSULA', 'PUBLICACION') OR @iIdEntidad <= 0
        BEGIN
            SET @vchError = 'Contenido inválido para opiniones.';
            GOTO _Fin;
        END

        IF @iIdCiudadano <= 0 OR NOT EXISTS (SELECT 1 FROM SCIA.Ciudadano WITH (NOLOCK) WHERE iIdCiudadano = @iIdCiudadano AND bActivo = 1)
        BEGIN
            SET @vchError = 'Debes iniciar sesión para opinar.';
            GOTO _Fin;
        END

        IF LEN(@nvchTexto) < 1 OR LEN(@nvchTexto) > 1000
        BEGIN
            SET @vchError = 'La opinión debe tener entre 1 y 1000 caracteres.';
            GOTO _Fin;
        END

        IF @iIdOpinionPadre IS NOT NULL AND @iIdOpinionPadre > 0
        BEGIN
            IF NOT EXISTS
            (
                SELECT 1
                FROM SCIA.Opinion P WITH (NOLOCK)
                WHERE P.iIdOpinion = @iIdOpinionPadre
                  AND P.vchTipoEntidad = @vchTipoEntidad
                  AND P.iIdEntidad = @iIdEntidad
                  AND P.iIdOpinionPadre IS NULL
                  AND P.bActivo = 1
            )
            BEGIN
                SET @vchError = 'No se puede responder a esa opinión.';
                GOTO _Fin;
            END
        END
        ELSE
        BEGIN
            SET @iIdOpinionPadre = NULL;
        END

        INSERT INTO SCIA.Opinion
        (
            vchTipoEntidad,
            iIdEntidad,
            iIdCiudadano,
            iIdOpinionPadre,
            nvchTexto,
            dtFechaCreacion,
            bActivo
        )
        VALUES
        (
            @vchTipoEntidad,
            @iIdEntidad,
            @iIdCiudadano,
            @iIdOpinionPadre,
            @nvchTexto,
            GETDATE(),
            1
        );

        SET @iIdOpinion = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        SET @vchError = CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE());
    END CATCH

_Fin:
    IF LEN(@vchError) > 0
        INSERT INTO @Result (bResult, vchMessage) VALUES (0, @vchError);
    ELSE
        INSERT INTO @Result
        (
            bResult,
            vchMessage,
            iIdOpinion,
            vchTipoEntidad,
            iIdEntidad,
            iIdCiudadano,
            vchAlias,
            iIdOpinionPadre,
            nvchTexto,
            dtFechaCreacion
        )
        SELECT
            1,
            'Opinión publicada.',
            O.iIdOpinion,
            O.vchTipoEntidad,
            O.iIdEntidad,
            O.iIdCiudadano,
            C.vchAlias,
            O.iIdOpinionPadre,
            O.nvchTexto,
            O.dtFechaCreacion
        FROM SCIA.Opinion O WITH (NOLOCK)
        INNER JOIN SCIA.Ciudadano C WITH (NOLOCK) ON C.iIdCiudadano = O.iIdCiudadano
        WHERE O.iIdOpinion = @iIdOpinion;

    SELECT TOP 1 * FROM @Result ORDER BY bResult DESC;
END
GO
