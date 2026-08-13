/*
** Migration: Emprendimientos + Productos Feed
** Random session-stable mix of Empresa and ProductoServicio.
** Date: 2026-08-13
*/

SET NOCOUNT ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'SCIA')
    EXEC('CREATE SCHEMA SCIA');
GO

/* ========== SCIA.spGetEmprendimientosFeed ========== */
CREATE OR ALTER PROCEDURE [SCIA].[spGetEmprendimientosFeed]
(
    @iPage INT = 1,
    @iPageSize INT = 10,
    @vchSessionSeed VARCHAR(64) = NULL
)
AS
/*
** Propósito: Feed mixto Emprendimientos + Productos.
** Mezcla intercalada: 1 E → 3 P → 2 E → 3 P (se repite).
** Dentro de cada tipo el orden es aleatorio y estable por @vchSessionSeed.
*/
BEGIN
    SET NOCOUNT ON;

    IF @iPage IS NULL OR @iPage < 1 SET @iPage = 1;
    IF @iPageSize IS NULL OR @iPageSize < 1 SET @iPageSize = 10;
    IF @iPageSize > 50 SET @iPageSize = 50;

    DECLARE @Offset INT = (@iPage - 1) * @iPageSize;
    DECLARE @Seed VARCHAR(64) = ISNULL(LTRIM(RTRIM(@vchSessionSeed)), '');

    IF @Seed = ''
        SET @Seed = CONVERT(VARCHAR(64), NEWID());

    CREATE TABLE #Result
    (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        vchTipoEntidad VARCHAR(20) DEFAULT(''),
        iIdEntidad INT DEFAULT(-1),
        iIdEmpresa INT DEFAULT(-1),
        vchTitulo NVARCHAR(300) DEFAULT(''),
        nvchDescripcion NVARCHAR(1000) DEFAULT(''),
        vchImagenUrl NVARCHAR(500) DEFAULT(''),
        vchNombreEmpresa NVARCHAR(300) DEFAULT(''),
        mPrecio MONEY DEFAULT(0),
        bAplicaDescuento BIT DEFAULT(0),
        mPrecioDescuento MONEY DEFAULT(0),
        iTotalRegistros INT DEFAULT(0)
    );

    BEGIN TRY
        ;WITH FeedUnion AS
        (
            SELECT
                CAST('EMPRENDIMIENTO' AS VARCHAR(20)) AS vchTipoEntidad,
                E.iIdEmpresa AS iIdEntidad,
                E.iIdEmpresa AS iIdEmpresa,
                CAST(E.vchNombreComercial AS NVARCHAR(300)) AS vchTitulo,
                CAST(LEFT(ISNULL(E.vchSlogan, ISNULL(E.nvchDescripcion, N'')), 400) AS NVARCHAR(1000)) AS nvchDescripcion,
                CAST(ISNULL(E.vchLogoUrl, N'') AS NVARCHAR(500)) AS vchImagenUrl,
                CAST(E.vchNombreComercial AS NVARCHAR(300)) AS vchNombreEmpresa,
                CAST(0 AS MONEY) AS mPrecio,
                CAST(0 AS BIT) AS bAplicaDescuento,
                CAST(0 AS MONEY) AS mPrecioDescuento
            FROM SCIA.Empresa E WITH (NOLOCK)
            WHERE ISNULL(E.bEstatus, 1) = 1

            UNION ALL

            SELECT
                CAST('PRODUCTO' AS VARCHAR(20)),
                P.iIdProducto,
                C.iIdEmpresa,
                CAST(P.vchNombre AS NVARCHAR(300)),
                CAST(LEFT(ISNULL(P.nvchDescripcionCorta, N''), 400) AS NVARCHAR(1000)),
                CAST(ISNULL(P.vchImagenUrl, N'') AS NVARCHAR(500)),
                CAST(ISNULL(E.vchNombreComercial, N'') AS NVARCHAR(300)),
                ISNULL(P.mPrecio, 0),
                ISNULL(P.bAplicaDescuento, 0),
                ISNULL(P.mPrecioDescuento, 0)
            FROM SCIA.ProductoServicio P WITH (NOLOCK)
            INNER JOIN SCIA.CategoriaCatalogo C WITH (NOLOCK) ON C.iIdCategoriaCat = P.iIdCategoriaCat
            INNER JOIN SCIA.Empresa E WITH (NOLOCK) ON E.iIdEmpresa = C.iIdEmpresa
            WHERE ISNULL(P.bEstatus, 1) = 1
              AND ISNULL(E.bEstatus, 1) = 1
        ),
        Ranked AS
        (
            SELECT
                F.*,
                ROW_NUMBER() OVER (
                    PARTITION BY F.vchTipoEntidad
                    ORDER BY CHECKSUM(@Seed, F.iIdEntidad), F.iIdEntidad
                ) AS iRnTipo
            FROM FeedUnion F
        ),
        Patterned AS
        (
            SELECT
                R.*,
                /*
                  Ciclo de 9: 1 E, 3 P, 2 E, 3 P
                  E slots: 1, 5, 6 | P slots: 2, 3, 4, 7, 8, 9
                */
                CASE R.vchTipoEntidad
                    WHEN 'EMPRENDIMIENTO' THEN
                        ((R.iRnTipo - 1) / 3) * 9
                        + CASE ((R.iRnTipo - 1) % 3)
                              WHEN 0 THEN 1
                              WHEN 1 THEN 5
                              ELSE 6
                          END
                    WHEN 'PRODUCTO' THEN
                        ((R.iRnTipo - 1) / 6) * 9
                        + CASE
                              WHEN ((R.iRnTipo - 1) % 6) < 3
                                  THEN 2 + ((R.iRnTipo - 1) % 6)
                              ELSE 7 + (((R.iRnTipo - 1) % 6) - 3)
                          END
                    ELSE 999999
                END AS iFeedOrder,
                COUNT(1) OVER() AS iTotalRegistros
            FROM Ranked R
        )
        INSERT INTO #Result
        (
            vchTipoEntidad, iIdEntidad, iIdEmpresa, vchTitulo, nvchDescripcion,
            vchImagenUrl, vchNombreEmpresa, mPrecio, bAplicaDescuento, mPrecioDescuento, iTotalRegistros
        )
        SELECT
            P.vchTipoEntidad,
            P.iIdEntidad,
            P.iIdEmpresa,
            P.vchTitulo,
            P.nvchDescripcion,
            P.vchImagenUrl,
            P.vchNombreEmpresa,
            P.mPrecio,
            P.bAplicaDescuento,
            P.mPrecioDescuento,
            P.iTotalRegistros
        FROM Patterned P
        ORDER BY P.iFeedOrder ASC, P.iIdEntidad ASC
        OFFSET @Offset ROWS FETCH NEXT @iPageSize ROWS ONLY;

        IF NOT EXISTS (SELECT 1 FROM #Result WHERE iIdEntidad <> -1)
        BEGIN
            INSERT INTO #Result (bResult, vchMessage)
            VALUES (0, 'No se encontró contenido para el feed de emprendimientos.');
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

/* ========== SCIA.spSearchEmprendimientosFeed ========== */
CREATE OR ALTER PROCEDURE [SCIA].[spSearchEmprendimientosFeed]
(
    @vchTexto NVARCHAR(500) = NULL,
    @iPage INT = 1,
    @iPageSize INT = 50
)
AS
/*
** Propósito: Búsqueda unificada en emprendimientos y productos.
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
        iIdEmpresa INT DEFAULT(-1),
        vchTitulo NVARCHAR(300) DEFAULT(''),
        nvchDescripcion NVARCHAR(1000) DEFAULT(''),
        vchImagenUrl NVARCHAR(500) DEFAULT(''),
        vchNombreEmpresa NVARCHAR(300) DEFAULT(''),
        mPrecio MONEY DEFAULT(0),
        bAplicaDescuento BIT DEFAULT(0),
        mPrecioDescuento MONEY DEFAULT(0),
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
                    CAST('EMPRENDIMIENTO' AS VARCHAR(20)) AS vchTipoEntidad,
                    E.iIdEmpresa AS iIdEntidad,
                    E.iIdEmpresa AS iIdEmpresa,
                    CAST(E.vchNombreComercial AS NVARCHAR(300)) AS vchTitulo,
                    CAST(LEFT(ISNULL(E.vchSlogan, ISNULL(E.nvchDescripcion, N'')), 400) AS NVARCHAR(1000)) AS nvchDescripcion,
                    CAST(ISNULL(E.vchLogoUrl, N'') AS NVARCHAR(500)) AS vchImagenUrl,
                    CAST(E.vchNombreComercial AS NVARCHAR(300)) AS vchNombreEmpresa,
                    CAST(0 AS MONEY) AS mPrecio,
                    CAST(0 AS BIT) AS bAplicaDescuento,
                    CAST(0 AS MONEY) AS mPrecioDescuento
                FROM SCIA.Empresa E WITH (NOLOCK)
                WHERE ISNULL(E.bEstatus, 1) = 1
                  AND (
                        E.vchNombreComercial LIKE N'%' + @Texto + N'%'
                     OR ISNULL(E.vchSlogan, N'') LIKE N'%' + @Texto + N'%'
                     OR ISNULL(E.nvchDescripcion, N'') LIKE N'%' + @Texto + N'%'
                  )

                UNION ALL

                SELECT
                    CAST('PRODUCTO' AS VARCHAR(20)),
                    P.iIdProducto,
                    C.iIdEmpresa,
                    CAST(P.vchNombre AS NVARCHAR(300)),
                    CAST(LEFT(ISNULL(P.nvchDescripcionCorta, N''), 400) AS NVARCHAR(1000)),
                    CAST(ISNULL(P.vchImagenUrl, N'') AS NVARCHAR(500)),
                    CAST(ISNULL(E.vchNombreComercial, N'') AS NVARCHAR(300)),
                    ISNULL(P.mPrecio, 0),
                    ISNULL(P.bAplicaDescuento, 0),
                    ISNULL(P.mPrecioDescuento, 0)
                FROM SCIA.ProductoServicio P WITH (NOLOCK)
                INNER JOIN SCIA.CategoriaCatalogo C WITH (NOLOCK) ON C.iIdCategoriaCat = P.iIdCategoriaCat
                INNER JOIN SCIA.Empresa E WITH (NOLOCK) ON E.iIdEmpresa = C.iIdEmpresa
                WHERE ISNULL(P.bEstatus, 1) = 1
                  AND ISNULL(E.bEstatus, 1) = 1
                  AND (
                        P.vchNombre LIKE N'%' + @Texto + N'%'
                     OR ISNULL(P.nvchDescripcionCorta, N'') LIKE N'%' + @Texto + N'%'
                     OR ISNULL(E.vchNombreComercial, N'') LIKE N'%' + @Texto + N'%'
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
                vchTipoEntidad, iIdEntidad, iIdEmpresa, vchTitulo, nvchDescripcion,
                vchImagenUrl, vchNombreEmpresa, mPrecio, bAplicaDescuento, mPrecioDescuento, iTotalRegistros
            )
            SELECT
                O.vchTipoEntidad,
                O.iIdEntidad,
                O.iIdEmpresa,
                O.vchTitulo,
                O.nvchDescripcion,
                O.vchImagenUrl,
                O.vchNombreEmpresa,
                O.mPrecio,
                O.bAplicaDescuento,
                O.mPrecioDescuento,
                O.iTotalRegistros
            FROM OrderedFeed O
            ORDER BY O.vchTipoEntidad, O.vchTitulo
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
