IF OBJECT_ID('spSaveOficina') IS NOT NULL
    DROP PROCEDURE spSaveOficina;
GO

CREATE PROCEDURE spSaveOficina
(
    @iIdOficina INT = 0,
    @iIdDependencia INT = NULL,
    @vchNombre VARCHAR(250),
    @vchDireccion VARCHAR(500) = NULL,
    @vchTelefono VARCHAR(50) = NULL,
    @vchEmail VARCHAR(150) = NULL,
    @vchHorario NVARCHAR(250) = NULL,
    @flLatitud FLOAT = NULL,
    @flLongitud FLOAT = NULL,
    @vchNotas NVARCHAR(MAX) = NULL,
    @bActivo BIT = 1
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @vchError VARCHAR(500) = '';

    DECLARE @Result TABLE (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdOficina INT DEFAULT(-1)
    );

    BEGIN TRY
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;

        --------------------------------------------------------------------------------
        -- VALIDACIONES DE NEGOCIO (trasladadas desde C#)
        --------------------------------------------------------------------------------

        IF @vchNombre IS NULL OR LTRIM(RTRIM(@vchNombre)) = ''
        BEGIN
            SET @vchError = 'El nombre de la oficina es obligatorio.';
            THROW 51000, @vchError, 1;
        END

        IF @flLatitud IS NOT NULL AND (@flLatitud < -90 OR @flLatitud > 90)
        BEGIN
            SET @vchError = 'Latitud fuera de rango (-90..90).';
            THROW 51001, @vchError, 1;
        END

        IF @flLongitud IS NOT NULL AND (@flLongitud < -180 OR @flLongitud > 180)
        BEGIN
            SET @vchError = 'Longitud fuera de rango (-180..180).';
            THROW 51002, @vchError, 1;
        END

        IF @vchEmail IS NOT NULL AND LTRIM(RTRIM(@vchEmail)) <> ''
        BEGIN
            IF @vchEmail NOT LIKE '_%@_%._%'
            BEGIN
                SET @vchError = 'Email inválido.';
                THROW 51003, @vchError, 1;
            END
        END

        IF @vchTelefono IS NOT NULL AND LEN(REPLACE(@vchTelefono, ' ', '')) < 7
        BEGIN
            SET @vchError = 'Teléfono inválido.';
            THROW 51004, @vchError, 1;
        END

        -- Validar dependencia
        IF @iIdDependencia IS NOT NULL AND @iIdDependencia > 0
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.Dependencia WHERE iIdDependencia = @iIdDependencia)
            BEGIN
                SET @vchError = 'Dependencia no encontrada.';
                THROW 51005, @vchError, 1;
            END
        END

        -- Validar duplicado: mismo nombre + dependencia
        IF EXISTS (
            SELECT 1
            FROM dbo.Oficina
            WHERE vchNombre = @vchNombre
              AND ISNULL(iIdDependencia, 0) = ISNULL(@iIdDependencia, 0)
              AND (@iIdOficina <= 0 OR iIdOficina <> @iIdOficina)
        )
        BEGIN
            SET @vchError = 'Ya existe una oficina con el mismo nombre en la dependencia seleccionada.';
            THROW 51006, @vchError, 1;
        END

        --------------------------------------------------------------------------------
        -- INSERCIÓN / ACTUALIZACIÓN
        --------------------------------------------------------------------------------

        IF @iIdOficina IS NULL OR @iIdOficina <= 0
        BEGIN
            INSERT INTO dbo.Oficina (
                iIdDependencia, vchNombre, vchDireccion, vchTelefono, vchEmail,
                vchHorario, flLatitud, flLongitud, vchNotas, bActivo, dtFechaCreacion
            )
            VALUES (
                @iIdDependencia, @vchNombre, @vchDireccion, @vchTelefono, @vchEmail,
                @vchHorario, @flLatitud, @flLongitud, @vchNotas, @bActivo, GETDATE()
            );

            SET @iIdOficina = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            UPDATE dbo.Oficina
            SET
                iIdDependencia = @iIdDependencia,
                vchNombre = @vchNombre,
                vchDireccion = @vchDireccion,
                vchTelefono = @vchTelefono,
                vchEmail = @vchEmail,
                vchHorario = @vchHorario,
                flLatitud = @flLatitud,
                flLongitud = @flLongitud,
                vchNotas = @vchNotas,
                bActivo = @bActivo
            WHERE iIdOficina = @iIdOficina;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        IF LEN(@vchError) = 0
            SET @vchError = CONCAT(ERROR_MESSAGE(), ' (línea ', ERROR_LINE(), ')');
    END CATCH

    IF LEN(@vchError) > 0
        INSERT INTO @Result (bResult, vchMessage) VALUES (0, @vchError);
    ELSE
        INSERT INTO @Result (bResult, vchMessage, iIdOficina) VALUES (1, '', @iIdOficina);

    SELECT * FROM @Result;
END
GO
