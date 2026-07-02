-- ============================================================
-- Schema: SCIA
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'SCIA')
BEGIN
    EXEC('CREATE SCHEMA SCIA');
END;
GO

-- ============================================================
-- Tabla: SCIA.CategoriaEvento
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CategoriaEvento' AND schema_id = SCHEMA_ID('SCIA'))
BEGIN
    CREATE TABLE SCIA.CategoriaEvento
    (
        iIdCategoriaEvento INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
        vchNombre NVARCHAR(100) NOT NULL,
        vchIconoUrl NVARCHAR(500) NULL
    );
END;
GO

-- ============================================================
-- Tabla: SCIA.Eventos
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Eventos' AND schema_id = SCHEMA_ID('SCIA'))
BEGIN
    CREATE TABLE SCIA.Eventos
    (
        iIdEvento INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
        iIdCategoriaEvento INT NULL,
        vchNombre NVARCHAR(200) NOT NULL,
        nvchDescripcion NVARCHAR(MAX) NULL,
        dtFechaInicio DATETIME NOT NULL,
        dtFechaFin DATETIME NULL,
        vchLugar NVARCHAR(200) NULL,
        vchDireccion NVARCHAR(500) NULL,
        flLatitud FLOAT NULL,
        flLongitud FLOAT NULL,
        vchImagenPortada NVARCHAR(500) NULL,
        vchCostoTexto NVARCHAR(100) NULL,
        vchOrganizador NVARCHAR(200) NULL,
        vchTelefono NVARCHAR(20) NULL,
        vchCorreo NVARCHAR(200) NULL,
        vchUrlOficial NVARCHAR(500) NULL,
        bDestacado BIT NOT NULL DEFAULT 0,
        bEstatus BIT NOT NULL DEFAULT 1,
        dtFechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_Eventos_CategoriaEvento FOREIGN KEY (iIdCategoriaEvento) REFERENCES SCIA.CategoriaEvento(iIdCategoriaEvento)
    );
END;
GO

-- ============================================================
-- Tabla: SCIA.ImagenesRegistro (genérica para todos los módulos)
-- Funciona para Eventos, Emprendimientos, Noticias, etc.
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ImagenesRegistro' AND schema_id = SCHEMA_ID('SCIA'))
BEGIN
    CREATE TABLE SCIA.ImagenesRegistro
    (
        iIdImagen INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
        vchTablaOrigen NVARCHAR(100) NOT NULL,
        iIdRegistro INT NOT NULL,
        vchUrlImagen NVARCHAR(500) NOT NULL,
        vchDescripcion NVARCHAR(200) NULL,
        iOrden INT NOT NULL DEFAULT 0,
        bActivo BIT NOT NULL DEFAULT 1,
        dtFechaRegistro DATETIME NOT NULL DEFAULT GETDATE()
    );
END;
GO

-- ============================================================
-- Inserts: Categorías de eventos iniciales
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM SCIA.CategoriaEvento WHERE vchNombre = 'Concierto')
BEGIN
    INSERT INTO SCIA.CategoriaEvento (vchNombre) VALUES
    ('Concierto'),
    ('Festival'),
    ('Deporte'),
    ('Cultural'),
    ('Gastronómico'),
    ('Feria'),
    ('Teatro'),
    ('Otro');
END;
GO

-- ============================================================
-- SP: SCIA.spGetCategoriaEventos
-- ============================================================
IF OBJECT_ID('SCIA.spGetCategoriaEventos') IS NULL
    EXEC ('CREATE PROCEDURE SCIA.spGetCategoriaEventos AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE SCIA.spGetCategoriaEventos
AS
/*
** Creador:      SantiagoConectaIA
** Propósito:    Obtener el catálogo de categorías de eventos.
** Última fecha: 2026-07-02
*/
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #Result
    (
        bResult BIT DEFAULT (1),
        vchMessage VARCHAR(500) DEFAULT (''),
        iIdCategoriaEvento INT DEFAULT (-1),
        vchNombre NVARCHAR(100) DEFAULT (''),
        vchIconoUrl NVARCHAR(500) DEFAULT (NULL)
    );

    BEGIN TRY
        INSERT INTO #Result
        (
            iIdCategoriaEvento, vchNombre, vchIconoUrl
        )
        SELECT
            CE.iIdCategoriaEvento, CE.vchNombre, CE.vchIconoUrl
        FROM
            SCIA.CategoriaEvento CE WITH(NOLOCK)
        ORDER BY
            CE.vchNombre;

        IF NOT EXISTS (SELECT 1 FROM #Result WHERE iIdCategoriaEvento > 0)
        BEGIN
            INSERT INTO #Result (bResult, vchMessage)
            VALUES (0, 'No se encontraron categorías de eventos.');
        END
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE()));
    END CATCH;

    SELECT * FROM #Result;
    DROP TABLE #Result;
END
GO

-- ============================================================
-- SP: SCIA.spSaveCategoriaEvento
-- ============================================================
IF OBJECT_ID('SCIA.spSaveCategoriaEvento') IS NULL
    EXEC ('CREATE PROCEDURE SCIA.spSaveCategoriaEvento AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE SCIA.spSaveCategoriaEvento
(
    @iIdCategoriaEvento INT,
    @vchNombre NVARCHAR(100),
    @vchIconoUrl NVARCHAR(500) = NULL
)
AS
/*
** Creador:      SantiagoConectaIA
** Propósito:    Crear o actualizar una categoría de evento.
** Última fecha: 2026-07-02
*/
BEGIN
    DECLARE @trancount INT = -1,
            @vchError VARCHAR(200) = '';

    DECLARE @Result AS TABLE (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdCategoriaEvento INT DEFAULT(-1)
    );

    SET NOCOUNT ON;

    BEGIN TRY
        SET XACT_ABORT ON;

        SET @trancount = @@TRANCOUNT;

        IF @trancount > 0
            SAVE TRANSACTION spSaveCategoriaEvento;
        ELSE
            BEGIN TRANSACTION;

        IF @iIdCategoriaEvento <= 0
        BEGIN
            INSERT INTO SCIA.CategoriaEvento (vchNombre, vchIconoUrl)
            VALUES (@vchNombre, @vchIconoUrl);

            SET @iIdCategoriaEvento = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            UPDATE SCIA.CategoriaEvento WITH(ROWLOCK)
            SET vchNombre = @vchNombre,
                vchIconoUrl = @vchIconoUrl
            WHERE iIdCategoriaEvento = @iIdCategoriaEvento;
        END

        IF @trancount = 0
            COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        SELECT @vchError = CONVERT(VARCHAR(200), CONCAT('spSaveCategoriaEvento: ', ERROR_MESSAGE(), ' in ', ERROR_PROCEDURE(), ' at line ', ERROR_LINE()));

        IF @trancount = 0
            ROLLBACK TRANSACTION;
        ELSE IF @trancount <> -1 AND XACT_STATE() <> -1
            ROLLBACK TRANSACTION spSaveCategoriaEvento;
    END CATCH;

_Fin:
    IF LEN(@vchError) > 0
    BEGIN
        INSERT INTO @Result (bResult, vchMessage)
        VALUES (0, @vchError);
    END
    ELSE
    BEGIN
        INSERT INTO @Result (bResult, vchMessage, iIdCategoriaEvento)
        VALUES (1, '', @iIdCategoriaEvento);
    END

    SELECT * FROM @Result;
    SET NOCOUNT OFF;
END
GO

-- ============================================================
-- SP: SCIA.spGetEventos
-- ============================================================
IF OBJECT_ID('SCIA.spGetEventos') IS NULL
    EXEC ('CREATE PROCEDURE SCIA.spGetEventos AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE SCIA.spGetEventos
(
    @iIdEvento INT = -1,
    @iIdCategoriaEvento INT = NULL,
    @bEstatus BIT = NULL,
    @bDestacado BIT = NULL
)
AS
/*
** Creador:      SantiagoConectaIA
** Propósito:    Obtener eventos filtrados por ID, categoría, estatus o destacado.
** Última fecha: 2026-07-02
*/
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #Result
    (
        bResult BIT DEFAULT (1),
        vchMessage VARCHAR(500) DEFAULT (''),
        iIdEvento INT DEFAULT (-1),
        iIdCategoriaEvento INT DEFAULT (-1),
        vchCategoriaNombre NVARCHAR(100) DEFAULT (''),
        vchNombre NVARCHAR(200) DEFAULT (''),
        nvchDescripcion NVARCHAR(MAX) DEFAULT (''),
        dtFechaInicio DATETIME DEFAULT (NULL),
        dtFechaFin DATETIME DEFAULT (NULL),
        vchLugar NVARCHAR(200) DEFAULT (''),
        vchDireccion NVARCHAR(500) DEFAULT (''),
        flLatitud FLOAT DEFAULT (0),
        flLongitud FLOAT DEFAULT (0),
        vchImagenPortada NVARCHAR(500) DEFAULT (''),
        vchCostoTexto NVARCHAR(100) DEFAULT (''),
        vchOrganizador NVARCHAR(200) DEFAULT (''),
        vchTelefono NVARCHAR(20) DEFAULT (''),
        vchCorreo NVARCHAR(200) DEFAULT (''),
        vchUrlOficial NVARCHAR(500) DEFAULT (''),
        bDestacado BIT DEFAULT (0),
        bEstatus BIT DEFAULT (1),
        dtFechaRegistro DATETIME DEFAULT (GETDATE())
    );

    BEGIN TRY
        INSERT INTO #Result
        (
            iIdEvento, iIdCategoriaEvento, vchCategoriaNombre,
            vchNombre, nvchDescripcion, dtFechaInicio, dtFechaFin,
            vchLugar, vchDireccion, flLatitud, flLongitud,
            vchImagenPortada, vchCostoTexto, vchOrganizador,
            vchTelefono, vchCorreo, vchUrlOficial,
            bDestacado, bEstatus, dtFechaRegistro
        )
        SELECT
            E.iIdEvento, E.iIdCategoriaEvento, ISNULL(C.vchNombre, ''),
            E.vchNombre, E.nvchDescripcion, E.dtFechaInicio, E.dtFechaFin,
            ISNULL(E.vchLugar, ''), ISNULL(E.vchDireccion, ''),
            ISNULL(E.flLatitud, 0), ISNULL(E.flLongitud, 0),
            ISNULL(E.vchImagenPortada, ''), ISNULL(E.vchCostoTexto, ''),
            ISNULL(E.vchOrganizador, ''), ISNULL(E.vchTelefono, ''),
            ISNULL(E.vchCorreo, ''), ISNULL(E.vchUrlOficial, ''),
            E.bDestacado, E.bEstatus, E.dtFechaRegistro
        FROM
            SCIA.Eventos E WITH(NOLOCK)
            LEFT JOIN SCIA.CategoriaEvento C WITH(NOLOCK) ON E.iIdCategoriaEvento = C.iIdCategoriaEvento
        WHERE
            (@iIdEvento = -1 OR E.iIdEvento = @iIdEvento)
            AND (@iIdCategoriaEvento IS NULL OR E.iIdCategoriaEvento = @iIdCategoriaEvento)
            AND (@bEstatus IS NULL OR E.bEstatus = @bEstatus)
            AND (@bDestacado IS NULL OR E.bDestacado = @bDestacado)
        ORDER BY
            E.bDestacado DESC, E.dtFechaInicio DESC;

        IF NOT EXISTS (SELECT 1 FROM #Result WHERE iIdEvento > 0)
        BEGIN
            INSERT INTO #Result (bResult, vchMessage)
            VALUES (0, 'No se encontraron eventos.');
        END
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE()));
    END CATCH;

    SELECT * FROM #Result;
    DROP TABLE #Result;
END
GO

-- ============================================================
-- SP: SCIA.spSaveEvento
-- ============================================================
IF OBJECT_ID('SCIA.spSaveEvento') IS NULL
    EXEC ('CREATE PROCEDURE SCIA.spSaveEvento AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE SCIA.spSaveEvento
(
    @iIdEvento INT,
    @iIdCategoriaEvento INT = NULL,
    @vchNombre NVARCHAR(200),
    @nvchDescripcion NVARCHAR(MAX) = NULL,
    @dtFechaInicio DATETIME,
    @dtFechaFin DATETIME = NULL,
    @vchLugar NVARCHAR(200) = NULL,
    @vchDireccion NVARCHAR(500) = NULL,
    @flLatitud FLOAT = NULL,
    @flLongitud FLOAT = NULL,
    @vchImagenPortada NVARCHAR(500) = NULL,
    @vchCostoTexto NVARCHAR(100) = NULL,
    @vchOrganizador NVARCHAR(200) = NULL,
    @vchTelefono NVARCHAR(20) = NULL,
    @vchCorreo NVARCHAR(200) = NULL,
    @vchUrlOficial NVARCHAR(500) = NULL,
    @bDestacado BIT = 0,
    @bEstatus BIT = 1
)
AS
/*
** Creador:      SantiagoConectaIA
** Propósito:    Crear o actualizar un evento.
** Última fecha: 2026-07-02
*/
BEGIN
    DECLARE @trancount INT = -1,
            @vchError VARCHAR(200) = '';

    DECLARE @Result AS TABLE (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdEvento INT DEFAULT(-1)
    );

    SET NOCOUNT ON;

    BEGIN TRY
        SET XACT_ABORT ON;

        SET @trancount = @@TRANCOUNT;

        IF @trancount > 0
            SAVE TRANSACTION spSaveEvento;
        ELSE
            BEGIN TRANSACTION;

        IF @iIdEvento <= 0
        BEGIN
            INSERT INTO SCIA.Eventos (
                iIdCategoriaEvento, vchNombre, nvchDescripcion, dtFechaInicio, dtFechaFin,
                vchLugar, vchDireccion, flLatitud, flLongitud, vchImagenPortada,
                vchCostoTexto, vchOrganizador, vchTelefono, vchCorreo, vchUrlOficial,
                bDestacado, bEstatus
            )
            VALUES (
                @iIdCategoriaEvento, @vchNombre, @nvchDescripcion, @dtFechaInicio, @dtFechaFin,
                @vchLugar, @vchDireccion, @flLatitud, @flLongitud, @vchImagenPortada,
                @vchCostoTexto, @vchOrganizador, @vchTelefono, @vchCorreo, @vchUrlOficial,
                @bDestacado, @bEstatus
            );

            SET @iIdEvento = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            UPDATE SCIA.Eventos WITH(ROWLOCK)
            SET iIdCategoriaEvento = @iIdCategoriaEvento,
                vchNombre = @vchNombre,
                nvchDescripcion = @nvchDescripcion,
                dtFechaInicio = @dtFechaInicio,
                dtFechaFin = @dtFechaFin,
                vchLugar = @vchLugar,
                vchDireccion = @vchDireccion,
                flLatitud = @flLatitud,
                flLongitud = @flLongitud,
                vchImagenPortada = ISNULL(@vchImagenPortada, vchImagenPortada),
                vchCostoTexto = @vchCostoTexto,
                vchOrganizador = @vchOrganizador,
                vchTelefono = @vchTelefono,
                vchCorreo = @vchCorreo,
                vchUrlOficial = @vchUrlOficial,
                bDestacado = @bDestacado,
                bEstatus = @bEstatus
            WHERE iIdEvento = @iIdEvento;
        END

        IF @trancount = 0
            COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        SELECT @vchError = CONVERT(VARCHAR(200), CONCAT('spSaveEvento: ', ERROR_MESSAGE(), ' in ', ERROR_PROCEDURE(), ' at line ', ERROR_LINE()));

        IF @trancount = 0
            ROLLBACK TRANSACTION;
        ELSE IF @trancount <> -1 AND XACT_STATE() <> -1
            ROLLBACK TRANSACTION spSaveEvento;
    END CATCH;

_Fin:
    IF LEN(@vchError) > 0
    BEGIN
        INSERT INTO @Result (bResult, vchMessage)
        VALUES (0, @vchError);
    END
    ELSE
    BEGIN
        INSERT INTO @Result (bResult, vchMessage, iIdEvento)
        VALUES (1, '', @iIdEvento);
    END

    SELECT * FROM @Result;
    SET NOCOUNT OFF;
END
GO

-- ============================================================
-- SP: SCIA.spGetImagenesRegistro (genérico)
-- ============================================================
IF OBJECT_ID('SCIA.spGetImagenesRegistro') IS NULL
    EXEC ('CREATE PROCEDURE SCIA.spGetImagenesRegistro AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE SCIA.spGetImagenesRegistro
(
    @vchTablaOrigen NVARCHAR(100),
    @iIdRegistro INT
)
AS
/*
** Creador:      SantiagoConectaIA
** Propósito:    Obtener imágenes activas de una tabla y registro específico (genérico).
** Última fecha: 2026-07-02
*/
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #Result
    (
        bResult BIT DEFAULT (1),
        vchMessage VARCHAR(500) DEFAULT (''),
        iIdImagen INT DEFAULT (-1),
        vchTablaOrigen NVARCHAR(100) DEFAULT (''),
        iIdRegistro INT DEFAULT (-1),
        vchUrlImagen NVARCHAR(500) DEFAULT (''),
        vchDescripcion NVARCHAR(200) DEFAULT (''),
        iOrden INT DEFAULT (0)
    );

    BEGIN TRY
        INSERT INTO #Result
        (
            iIdImagen, vchTablaOrigen, iIdRegistro,
            vchUrlImagen, vchDescripcion, iOrden
        )
        SELECT
            IR.iIdImagen, IR.vchTablaOrigen, IR.iIdRegistro,
            IR.vchUrlImagen, ISNULL(IR.vchDescripcion, ''), IR.iOrden
        FROM
            SCIA.ImagenesRegistro IR WITH(NOLOCK)
        WHERE
            IR.vchTablaOrigen = @vchTablaOrigen
            AND IR.iIdRegistro = @iIdRegistro
            AND IR.bActivo = 1
        ORDER BY
            IR.iOrden;

        IF NOT EXISTS (SELECT 1 FROM #Result WHERE iIdImagen > 0)
        BEGIN
            INSERT INTO #Result (bResult, vchMessage)
            VALUES (0, 'No se encontraron imágenes.');
        END
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE()));
    END CATCH;

    SELECT * FROM #Result;
    DROP TABLE #Result;
END
GO

-- ============================================================
-- SP: SCIA.spSaveImagenRegistro (genérico)
-- ============================================================
IF OBJECT_ID('SCIA.spSaveImagenRegistro') IS NULL
    EXEC ('CREATE PROCEDURE SCIA.spSaveImagenRegistro AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE SCIA.spSaveImagenRegistro
(
    @vchTablaOrigen NVARCHAR(100),
    @iIdRegistro INT,
    @vchUrlImagen NVARCHAR(500),
    @vchDescripcion NVARCHAR(200) = NULL,
    @iOrden INT = 0
)
AS
/*
** Creador:      SantiagoConectaIA
** Propósito:    Insertar una imagen para cualquier módulo (genérico).
** Última fecha: 2026-07-02
*/
BEGIN
    DECLARE @trancount INT = -1,
            @vchError VARCHAR(200) = '';

    DECLARE @Result AS TABLE (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdImagen INT DEFAULT(-1)
    );

    SET NOCOUNT ON;

    BEGIN TRY
        SET XACT_ABORT ON;

        SET @trancount = @@TRANCOUNT;

        IF @trancount > 0
            SAVE TRANSACTION spSaveImagenRegistro;
        ELSE
            BEGIN TRANSACTION;

        INSERT INTO SCIA.ImagenesRegistro (vchTablaOrigen, iIdRegistro, vchUrlImagen, vchDescripcion, iOrden)
        VALUES (@vchTablaOrigen, @iIdRegistro, @vchUrlImagen, @vchDescripcion, @iOrden);

        DECLARE @iIdImagen INT = SCOPE_IDENTITY();

        IF @trancount = 0
            COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        SELECT @vchError = CONVERT(VARCHAR(200), CONCAT('spSaveImagenRegistro: ', ERROR_MESSAGE(), ' in ', ERROR_PROCEDURE(), ' at line ', ERROR_LINE()));

        IF @trancount = 0
            ROLLBACK TRANSACTION;
        ELSE IF @trancount <> -1 AND XACT_STATE() <> -1
            ROLLBACK TRANSACTION spSaveImagenRegistro;
    END CATCH;

_Fin:
    IF LEN(@vchError) > 0
    BEGIN
        INSERT INTO @Result (bResult, vchMessage)
        VALUES (0, @vchError);
    END
    ELSE
    BEGIN
        INSERT INTO @Result (bResult, vchMessage, iIdImagen)
        VALUES (1, '', @iIdImagen);
    END

    SELECT * FROM @Result;
    SET NOCOUNT OFF;
END
GO

-- ============================================================
-- SP: SCIA.spDeleteImagenRegistro (genérico - soft delete)
-- ============================================================
IF OBJECT_ID('SCIA.spDeleteImagenRegistro') IS NULL
    EXEC ('CREATE PROCEDURE SCIA.spDeleteImagenRegistro AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE SCIA.spDeleteImagenRegistro
(
    @iIdImagen INT
)
AS
/*
** Creador:      SantiagoConectaIA
** Propósito:    Desactivar una imagen de forma lógica (genérico).
** Última fecha: 2026-07-02
*/
BEGIN
    DECLARE @trancount INT = -1,
            @vchError VARCHAR(200) = '';

    DECLARE @Result AS TABLE (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT('')
    );

    SET NOCOUNT ON;

    BEGIN TRY
        SET XACT_ABORT ON;

        SET @trancount = @@TRANCOUNT;

        IF @trancount > 0
            SAVE TRANSACTION spDeleteImagenRegistro;
        ELSE
            BEGIN TRANSACTION;

        UPDATE SCIA.ImagenesRegistro WITH(ROWLOCK)
        SET bActivo = 0
        WHERE iIdImagen = @iIdImagen;

        IF @trancount = 0
            COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        SELECT @vchError = CONVERT(VARCHAR(200), CONCAT('spDeleteImagenRegistro: ', ERROR_MESSAGE(), ' in ', ERROR_PROCEDURE(), ' at line ', ERROR_LINE()));

        IF @trancount = 0
            ROLLBACK TRANSACTION;
        ELSE IF @trancount <> -1 AND XACT_STATE() <> -1
            ROLLBACK TRANSACTION spDeleteImagenRegistro;
    END CATCH;

_Fin:
    IF LEN(@vchError) > 0
    BEGIN
        INSERT INTO @Result (bResult, vchMessage)
        VALUES (0, @vchError);
    END
    ELSE
    BEGIN
        INSERT INTO @Result (bResult, vchMessage)
        VALUES (1, 'Imagen eliminada correctamente.');
    END

    SELECT * FROM @Result;
    SET NOCOUNT OFF;
END
GO

-- ============================================================
-- SP: SCIA.spGetEventoDetalle
-- ============================================================
IF OBJECT_ID('SCIA.spGetEventoDetalle') IS NULL
    EXEC ('CREATE PROCEDURE SCIA.spGetEventoDetalle AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE SCIA.spGetEventoDetalle
(
    @iIdEvento INT
)
AS
/*
** Creador:      SantiagoConectaIA
** Propósito:    Obtener el detalle completo de un evento incluyendo todas sus imágenes.
** Última fecha: 2026-07-02
*/
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #Result
    (
        bResult BIT DEFAULT (1),
        vchMessage VARCHAR(500) DEFAULT (''),
        iIdEvento INT DEFAULT (-1),
        iIdCategoriaEvento INT DEFAULT (-1),
        vchCategoriaNombre NVARCHAR(100) DEFAULT (''),
        vchNombre NVARCHAR(200) DEFAULT (''),
        nvchDescripcion NVARCHAR(MAX) DEFAULT (''),
        dtFechaInicio DATETIME DEFAULT (NULL),
        dtFechaFin DATETIME DEFAULT (NULL),
        vchLugar NVARCHAR(200) DEFAULT (''),
        vchDireccion NVARCHAR(500) DEFAULT (''),
        flLatitud FLOAT DEFAULT (0),
        flLongitud FLOAT DEFAULT (0),
        vchImagenPortada NVARCHAR(500) DEFAULT (''),
        vchCostoTexto NVARCHAR(100) DEFAULT (''),
        vchOrganizador NVARCHAR(200) DEFAULT (''),
        vchTelefono NVARCHAR(20) DEFAULT (''),
        vchCorreo NVARCHAR(200) DEFAULT (''),
        vchUrlOficial NVARCHAR(500) DEFAULT (''),
        bDestacado BIT DEFAULT (0),
        bEstatus BIT DEFAULT (1),
        dtFechaRegistro DATETIME DEFAULT (GETDATE()),
        iIdImagen INT DEFAULT (-1),
        vchUrlImagen NVARCHAR(500) DEFAULT (''),
        vchDescripcionImagen NVARCHAR(200) DEFAULT (''),
        iOrden INT DEFAULT (0)
    );

    BEGIN TRY
        INSERT INTO #Result
        (
            iIdEvento, iIdCategoriaEvento, vchCategoriaNombre,
            vchNombre, nvchDescripcion, dtFechaInicio, dtFechaFin,
            vchLugar, vchDireccion, flLatitud, flLongitud,
            vchImagenPortada, vchCostoTexto, vchOrganizador,
            vchTelefono, vchCorreo, vchUrlOficial,
            bDestacado, bEstatus, dtFechaRegistro,
            iIdImagen, vchUrlImagen, vchDescripcionImagen, iOrden
        )
        SELECT
            E.iIdEvento, E.iIdCategoriaEvento, ISNULL(C.vchNombre, ''),
            E.vchNombre, E.nvchDescripcion, E.dtFechaInicio, E.dtFechaFin,
            ISNULL(E.vchLugar, ''), ISNULL(E.vchDireccion, ''),
            ISNULL(E.flLatitud, 0), ISNULL(E.flLongitud, 0),
            ISNULL(E.vchImagenPortada, ''), ISNULL(E.vchCostoTexto, ''),
            ISNULL(E.vchOrganizador, ''), ISNULL(E.vchTelefono, ''),
            ISNULL(E.vchCorreo, ''), ISNULL(E.vchUrlOficial, ''),
            E.bDestacado, E.bEstatus, E.dtFechaRegistro,
            ISNULL(IR.iIdImagen, -1), ISNULL(IR.vchUrlImagen, ''),
            ISNULL(IR.vchDescripcion, ''), ISNULL(IR.iOrden, 0)
        FROM
            SCIA.Eventos E WITH(NOLOCK)
            LEFT JOIN SCIA.CategoriaEvento C WITH(NOLOCK) ON E.iIdCategoriaEvento = C.iIdCategoriaEvento
            LEFT JOIN SCIA.ImagenesRegistro IR WITH(NOLOCK) ON IR.vchTablaOrigen = 'Eventos'
                AND IR.iIdRegistro = E.iIdEvento
                AND IR.bActivo = 1
        WHERE
            E.iIdEvento = @iIdEvento
        ORDER BY
            IR.iOrden;

        IF NOT EXISTS (SELECT 1 FROM #Result WHERE iIdEvento > 0)
        BEGIN
            INSERT INTO #Result (bResult, vchMessage)
            VALUES (0, 'No se encontró el evento.');
        END
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE()));
    END CATCH;

    SELECT * FROM #Result;
    DROP TABLE #Result;
END
GO

-- ============================================================
-- Tabla: SCIA.EventosSucursales
-- Sucursales/locales donde se llevará a cabo el evento y donde
-- se envían los boletos.
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EventosSucursales' AND schema_id = SCHEMA_ID('SCIA'))
BEGIN
    CREATE TABLE SCIA.EventosSucursales
    (
        iIdSucursalEvento INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
        iIdEvento INT NOT NULL,
        vchNombre NVARCHAR(200) NOT NULL,
        vchDireccion NVARCHAR(500) NULL,
        flLatitud FLOAT NULL,
        flLongitud FLOAT NULL,
        vchTelefono NVARCHAR(20) NULL,
        vchContacto NVARCHAR(200) NULL,
        vchHorario NVARCHAR(200) NULL,
        vchNotas NVARCHAR(500) NULL,
        bActivo BIT NOT NULL DEFAULT 1,
        dtFechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_EventosSucursales_Eventos FOREIGN KEY (iIdEvento) REFERENCES SCIA.Eventos(iIdEvento)
    );
END;
GO

-- ============================================================
-- SP: SCIA.spGetEventosSucursales
-- ============================================================
IF OBJECT_ID('SCIA.spGetEventosSucursales') IS NULL
    EXEC ('CREATE PROCEDURE SCIA.spGetEventosSucursales AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE SCIA.spGetEventosSucursales
(
    @iIdSucursalEvento INT = -1,
    @iIdEvento INT = NULL,
    @bActivo BIT = NULL
)
AS
/*
** Creador:      SantiagoConectaIA
** Propósito:    Obtener las sucursales/locales asociadas a un evento.
** Última fecha: 2026-07-02
*/
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #Result
    (
        bResult BIT DEFAULT (1),
        vchMessage VARCHAR(500) DEFAULT (''),
        iIdSucursalEvento INT DEFAULT (-1),
        iIdEvento INT DEFAULT (-1),
        vchNombre NVARCHAR(200) DEFAULT (''),
        vchDireccion NVARCHAR(500) DEFAULT (''),
        flLatitud FLOAT DEFAULT (0),
        flLongitud FLOAT DEFAULT (0),
        vchTelefono NVARCHAR(20) DEFAULT (''),
        vchContacto NVARCHAR(200) DEFAULT (''),
        vchHorario NVARCHAR(200) DEFAULT (''),
        vchNotas NVARCHAR(500) DEFAULT (''),
        bActivo BIT DEFAULT (1),
        dtFechaRegistro DATETIME DEFAULT (GETDATE())
    );

    BEGIN TRY
        INSERT INTO #Result
        (
            iIdSucursalEvento, iIdEvento, vchNombre, vchDireccion,
            flLatitud, flLongitud, vchTelefono, vchContacto,
            vchHorario,
            vchNotas, bActivo, dtFechaRegistro
        )
        SELECT
            ES.iIdSucursalEvento, ES.iIdEvento, ES.vchNombre, ISNULL(ES.vchDireccion, ''),
            ISNULL(ES.flLatitud, 0), ISNULL(ES.flLongitud, 0),
            ISNULL(ES.vchTelefono, ''), ISNULL(ES.vchContacto, ''),
            ISNULL(ES.vchHorario, ''), ISNULL(ES.vchNotas, ''),
            ES.bActivo, ES.dtFechaRegistro
        FROM
            SCIA.EventosSucursales ES WITH(NOLOCK)
        WHERE
            (@iIdSucursalEvento = -1 OR ES.iIdSucursalEvento = @iIdSucursalEvento)
            AND (@iIdEvento IS NULL OR ES.iIdEvento = @iIdEvento)
            AND (@bActivo IS NULL OR ES.bActivo = @bActivo)
        ORDER BY
            ES.vchNombre;

        IF NOT EXISTS (SELECT 1 FROM #Result WHERE iIdSucursalEvento > 0)
        BEGIN
            INSERT INTO #Result (bResult, vchMessage)
            VALUES (0, 'No se encontraron sucursales para este evento.');
        END
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE()));
    END CATCH;

    SELECT * FROM #Result;
    DROP TABLE #Result;
END
GO

-- ============================================================
-- SP: SCIA.spSaveSucursalEvento
-- ============================================================
IF OBJECT_ID('SCIA.spSaveSucursalEvento') IS NULL
    EXEC ('CREATE PROCEDURE SCIA.spSaveSucursalEvento AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE SCIA.spSaveSucursalEvento
(
    @iIdSucursalEvento INT,
    @iIdEvento INT,
    @vchNombre NVARCHAR(200),
    @vchDireccion NVARCHAR(500) = NULL,
    @flLatitud FLOAT = NULL,
    @flLongitud FLOAT = NULL,
    @vchTelefono NVARCHAR(20) = NULL,
    @vchContacto NVARCHAR(200) = NULL,
    @vchHorario NVARCHAR(200) = NULL,
    @vchNotas NVARCHAR(500) = NULL,
    @bActivo BIT = 1
)
AS
/*
** Creador:      SantiagoConectaIA
** Propósito:    Crear o actualizar una sucursal/local de evento.
** Última fecha: 2026-07-02
*/
BEGIN
    DECLARE @trancount INT = -1,
            @vchError VARCHAR(200) = '';

    DECLARE @Result AS TABLE (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdSucursalEvento INT DEFAULT(-1)
    );

    SET NOCOUNT ON;

    BEGIN TRY
        SET XACT_ABORT ON;

        SET @trancount = @@TRANCOUNT;

        IF @trancount > 0
            SAVE TRANSACTION spSaveSucursalEvento;
        ELSE
            BEGIN TRANSACTION;

        IF @iIdSucursalEvento <= 0
        BEGIN
            INSERT INTO SCIA.EventosSucursales (
                iIdEvento, vchNombre, vchDireccion, flLatitud, flLongitud,
                vchTelefono, vchContacto,
                vchHorario, vchNotas, bActivo
            )
            VALUES (
                @iIdEvento, @vchNombre, @vchDireccion, @flLatitud, @flLongitud,
                @vchTelefono, @vchContacto,
                @vchHorario, @vchNotas, @bActivo
            );

            SET @iIdSucursalEvento = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            UPDATE SCIA.EventosSucursales WITH(ROWLOCK)
            SET vchNombre = @vchNombre,
                vchDireccion = @vchDireccion,
                flLatitud = @flLatitud,
                flLongitud = @flLongitud,
                vchTelefono = @vchTelefono,
                vchContacto = @vchContacto,
                vchHorario = @vchHorario,
                vchNotas = @vchNotas,
                bActivo = @bActivo
            WHERE iIdSucursalEvento = @iIdSucursalEvento;
        END

        IF @trancount = 0
            COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        SELECT @vchError = CONVERT(VARCHAR(200), CONCAT('spSaveSucursalEvento: ', ERROR_MESSAGE(), ' in ', ERROR_PROCEDURE(), ' at line ', ERROR_LINE()));

        IF @trancount = 0
            ROLLBACK TRANSACTION;
        ELSE IF @trancount <> -1 AND XACT_STATE() <> -1
            ROLLBACK TRANSACTION spSaveSucursalEvento;
    END CATCH;

_Fin:
    IF LEN(@vchError) > 0
    BEGIN
        INSERT INTO @Result (bResult, vchMessage)
        VALUES (0, @vchError);
    END
    ELSE
    BEGIN
        INSERT INTO @Result (bResult, vchMessage, iIdSucursalEvento)
        VALUES (1, '', @iIdSucursalEvento);
    END

    SELECT * FROM @Result;
    SET NOCOUNT OFF;
END
GO
