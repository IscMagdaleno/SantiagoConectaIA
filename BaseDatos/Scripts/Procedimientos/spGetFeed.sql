IF OBJECT_ID('SCIA.spGetFeed') IS NULL
    EXEC('CREATE PROCEDURE SCIA.spGetFeed AS SET NOCOUNT ON;');
GO

ALTER PROCEDURE [SCIA].[spGetFeed]
(
    @iPage INT = 1,
    @iPageSize INT = 10,
    @vchSessionSeed VARCHAR(64) = NULL
)
AS
/*
** Propósito: Feed mixto paginado con patrón Facebook:
**   1 TRAMITE → 2 NOTICIA → 1 CAPSULA → 1 EVENTO (se repite).
** Eventos se incluyen sin filtrar por fecha (pueden ser antiguos).
** Última fecha: 12/08/2026
*/
BEGIN
    SET NOCOUNT ON;

    IF @iPage IS NULL OR @iPage < 1 SET @iPage = 1;
    IF @iPageSize IS NULL OR @iPageSize < 1 SET @iPageSize = 10;
    IF @iPageSize > 50 SET @iPageSize = 50;

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
        iTotalRegistros INT DEFAULT(0)
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
                T.dtFechaCreacion AS dtFecha
            FROM dbo.Tramite T WITH (NOLOCK)
            WHERE ISNULL(T.bActivo, 1) = 1

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
        ),
        Ranked AS
        (
            SELECT
                F.*,
                ROW_NUMBER() OVER (
                    PARTITION BY F.vchTipoEntidad
                    ORDER BY F.dtFecha DESC, F.iIdEntidad DESC
                ) AS iRnTipo
            FROM FeedUnion F
        ),
        Patterned AS
        (
            SELECT
                R.*,
                /* Patrón de 5: T, N, N, C, E */
                CASE R.vchTipoEntidad
                    WHEN 'TRAMITE' THEN ((R.iRnTipo - 1) * 5) + 1
                    WHEN 'NOTICIA' THEN (((R.iRnTipo - 1) / 2) * 5) + 2 + ((R.iRnTipo - 1) % 2)
                    WHEN 'CAPSULA' THEN ((R.iRnTipo - 1) * 5) + 4
                    WHEN 'EVENTO'  THEN ((R.iRnTipo - 1) * 5) + 5
                    ELSE 999999
                END AS iFeedOrder,
                COUNT(1) OVER() AS iTotalRegistros
            FROM Ranked R
        )
        INSERT INTO #Result
        (
            vchTipoEntidad, iIdEntidad, vchTitulo, nvchDescripcion,
            nvchContenidoDetallado, vchImagenUrl, dtFecha, iTotalRegistros
        )
        SELECT
            P.vchTipoEntidad,
            P.iIdEntidad,
            P.vchTitulo,
            P.nvchDescripcion,
            P.nvchContenidoDetallado,
            P.vchImagenUrl,
            P.dtFecha,
            P.iTotalRegistros
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
