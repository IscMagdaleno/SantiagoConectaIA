USE Engrama -- Asegúrate de que sea la DB correcta
GO

-- =========================================================
-- 1. ACTUALIZACIÓN DE LA TABLA SCIA.Ciudadano
-- =========================================================

-- Asegurar que las columnas para login social existan en SCIA.Ciudadano
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SCIA.Ciudadano') AND name = 'vchEmail')
BEGIN
    ALTER TABLE SCIA.Ciudadano ADD vchEmail VARCHAR(150) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SCIA.Ciudadano') AND name = 'vchProveedorAuth')
BEGIN
    ALTER TABLE SCIA.Ciudadano ADD vchProveedorAuth VARCHAR(50) NOT NULL CONSTRAINT DF_Ciudadano_Proveedor DEFAULT 'Local';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SCIA.Ciudadano') AND name = 'vchIdProveedor')
BEGIN
    ALTER TABLE SCIA.Ciudadano ADD vchIdProveedor VARCHAR(250) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SCIA.Ciudadano') AND name = 'vchAvatarUrl')
BEGIN
    ALTER TABLE SCIA.Ciudadano ADD vchAvatarUrl VARCHAR(500) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SCIA.Ciudadano') AND name = 'bCuentaVerificada')
BEGIN
    ALTER TABLE SCIA.Ciudadano ADD bCuentaVerificada BIT NOT NULL CONSTRAINT DF_Ciudadano_Verificada DEFAULT 1;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SCIA.Ciudadano') AND name = 'dtUltimoLogin')
BEGIN
    ALTER TABLE SCIA.Ciudadano ADD dtUltimoLogin DATETIME NULL;
END
GO

-- Permitir que vchTelefono y vchPassword sean NULL si el registro es por Google/Facebook
IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('SCIA.Ciudadano') AND name = 'vchTelefono' AND is_nullable = 0
)
BEGIN
    ALTER TABLE SCIA.Ciudadano ALTER COLUMN vchTelefono VARCHAR(20) NULL;
END
GO

IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('SCIA.Ciudadano') AND name = 'vchPassword' AND is_nullable = 0
)
BEGIN
    ALTER TABLE SCIA.Ciudadano ALTER COLUMN vchPassword VARCHAR(500) NULL;
END
GO

-- =========================================================
-- 2. PROCEDIMIENTO ALMACENADO: SCIA.spSaveCiudadanoExternalLogin
-- =========================================================

IF OBJECT_ID('SCIA.spSaveCiudadanoExternalLogin') IS NULL
    EXEC ('CREATE PROCEDURE SCIA.spSaveCiudadanoExternalLogin AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE SCIA.spSaveCiudadanoExternalLogin
(
    @vchProveedor       VARCHAR(50),          -- 'Google', 'Facebook'
    @vchIdProveedor     VARCHAR(250),         -- ID único de Google o Facebook
    @vchEmail           VARCHAR(150) = NULL,
    @vchAlias           VARCHAR(100) = NULL,
    @vchAvatarUrl       VARCHAR(500) = NULL
)
AS 
/*
** Propósito: Registrar o iniciar sesión de un ciudadano a través de Google o Facebook.
**            Si el usuario ya existe por Proveedor+IdProveedor o por Email, se actualiza su último login.
**            Si no existe, se crea un nuevo ciudadano verificado.
*/
BEGIN
    DECLARE @trancount INT = -1,
            @vchError VARCHAR(200) = '',
            @iIdCiudadanoExistente INT = 0;

    DECLARE @Result AS TABLE (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdCiudadano INT DEFAULT(-1),
        vchAlias VARCHAR(100) DEFAULT(''),
        vchTelefono VARCHAR(20) DEFAULT(''),
        vchEmail VARCHAR(150) DEFAULT(''),
        vchAvatarUrl VARCHAR(500) DEFAULT(''),
        bCuentaVerificada BIT DEFAULT(1)
    );

    SET NOCOUNT ON;

    BEGIN TRY
        SET XACT_ABORT ON;

        -- 1. Validar parámetros requeridos
        IF ISNULL(@vchProveedor, '') = '' OR ISNULL(@vchIdProveedor, '') = ''
        BEGIN
            SET @vchError = 'El proveedor y el identificador externo son requeridos.';
            GOTO _Fin;
        END

        -- 2. Buscar si ya existe por (vchProveedorAuth y vchIdProveedor)
        SELECT TOP 1 @iIdCiudadanoExistente = C.iIdCiudadano
        FROM SCIA.Ciudadano C
        WHERE C.vchProveedorAuth = @vchProveedor AND C.vchIdProveedor = @vchIdProveedor;

        -- Si no se encontró por ID externo, buscar por correo (si fue provisto)
        IF @iIdCiudadanoExistente <= 0 AND ISNULL(@vchEmail, '') <> ''
        BEGIN
            SELECT TOP 1 @iIdCiudadanoExistente = C.iIdCiudadano
            FROM SCIA.Ciudadano C
            WHERE C.vchEmail = @vchEmail;
        END

        SET @trancount = @@TRANCOUNT;
        IF @trancount > 0
            SAVE TRANSACTION spSaveCiudadanoExternalLogin;
        ELSE
            BEGIN TRANSACTION;

        IF @iIdCiudadanoExistente > 0
        BEGIN
            -- ACTUALIZAR usuario existente
            UPDATE SCIA.Ciudadano
            SET 
                dtUltimoLogin = GETDATE(),
                vchAvatarUrl = ISNULL(NULLIF(@vchAvatarUrl, ''), vchAvatarUrl),
                vchEmail = ISNULL(NULLIF(@vchEmail, ''), vchEmail),
                vchIdProveedor = ISNULL(NULLIF(@vchIdProveedor, ''), vchIdProveedor),
                vchProveedorAuth = CASE WHEN vchProveedorAuth = 'Local' AND ISNULL(@vchProveedor, '') <> '' THEN @vchProveedor ELSE vchProveedorAuth END,
                bCuentaVerificada = 1
            WHERE iIdCiudadano = @iIdCiudadanoExistente;

            INSERT INTO @Result (bResult, vchMessage, iIdCiudadano, vchAlias, vchTelefono, vchEmail, vchAvatarUrl, bCuentaVerificada)
            SELECT 
                1, 
                'Inicio de sesión exitoso.', 
                C.iIdCiudadano, 
                C.vchAlias, 
                ISNULL(C.vchTelefono, ''), 
                ISNULL(C.vchEmail, ''), 
                ISNULL(C.vchAvatarUrl, ''), 
                C.bCuentaVerificada
            FROM SCIA.Ciudadano C
            WHERE C.iIdCiudadano = @iIdCiudadanoExistente;
        END
        ELSE
        BEGIN
            -- INSERTAR nuevo ciudadano
            DECLARE @AliasFinal VARCHAR(100) = ISNULL(NULLIF(@vchAlias, ''), 'Usuario ' + SUBSTRING(@vchProveedor, 1, 1) + SUBSTRING(CAST(NEWID() AS VARCHAR(36)), 1, 6));

            INSERT INTO SCIA.Ciudadano
            (
                vchAlias,
                vchTelefono,
                vchEmail,
                vchPassword,
                vchProveedorAuth,
                vchIdProveedor,
                vchAvatarUrl,
                bCuentaVerificada,
                bActivo,
                dtFechaCreacion,
                dtUltimoLogin
            )
            VALUES
            (
                @AliasFinal,
                NULL,
                @vchEmail,
                NULL,
                @vchProveedor,
                @vchIdProveedor,
                @vchAvatarUrl,
                1,
                1,
                GETDATE(),
                GETDATE()
            );

            SET @iIdCiudadanoExistente = SCOPE_IDENTITY();

            INSERT INTO @Result (bResult, vchMessage, iIdCiudadano, vchAlias, vchTelefono, vchEmail, vchAvatarUrl, bCuentaVerificada)
            VALUES (
                1,
                'Registro completado exitosamente.',
                @iIdCiudadanoExistente,
                @AliasFinal,
                '',
                ISNULL(@vchEmail, ''),
                ISNULL(@vchAvatarUrl, ''),
                1
            );
        END

        IF @trancount = 0
            COMMIT TRANSACTION;

    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
        BEGIN
            IF @trancount = 0
                ROLLBACK TRANSACTION;
            ELSE IF @trancount > 0
                ROLLBACK TRANSACTION spSaveCiudadanoExternalLogin;
        END

        SET @vchError = ERROR_MESSAGE();
    END CATCH

_Fin:
    IF @vchError <> ''
    BEGIN
        DELETE FROM @Result;
        INSERT INTO @Result (bResult, vchMessage, iIdCiudadano)
        VALUES (0, @vchError, -1);
    END

    SELECT * FROM @Result;
END;
GO
