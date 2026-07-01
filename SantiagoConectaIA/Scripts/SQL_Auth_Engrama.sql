USE Engrama -- Asegúrate de que sea la DB correcta
GO

-- =========================================================
-- TABLAS
-- =========================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Rol' AND schema_id = SCHEMA_ID('SCIA'))
BEGIN
    CREATE TABLE SCIA.Rol
    (
        iIdRol INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
        vchNombre VARCHAR(50) NOT NULL,
        bActivo BIT NOT NULL DEFAULT 1
    );

    INSERT INTO SCIA.Rol (vchNombre, bActivo) VALUES ('Admin', 1);
    INSERT INTO SCIA.Rol (vchNombre, bActivo) VALUES ('User', 1);
END;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Usuario' AND schema_id = SCHEMA_ID('SCIA'))
BEGIN
    CREATE TABLE SCIA.Usuario
    (
        iIdUsuario INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
        iIdRol INT NOT NULL,
        vchNombre VARCHAR(100) NOT NULL,
        vchUserName VARCHAR(100) NULL UNIQUE,
        vchEmail VARCHAR(150) NOT NULL UNIQUE,
        vchPassword VARCHAR(500) NOT NULL,
        dtFechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        bActivo BIT NOT NULL DEFAULT 1,
        CONSTRAINT FK_Usuario_Rol FOREIGN KEY (iIdRol) REFERENCES SCIA.Rol(iIdRol)
    );
END;
GO

-- =========================================================
-- STORED PROCEDURES: spSaveUsuario
-- =========================================================

IF OBJECT_ID('SCIA.spSaveUsuario') IS NULL
    EXEC ('CREATE PROCEDURE SCIA.spSaveUsuario AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE SCIA.spSaveUsuario
(
    @iIdUsuario INT,
    @iIdRol INT,
    @vchNombre VARCHAR(100),
    @vchUserName VARCHAR(100) = NULL,
    @vchEmail VARCHAR(150),
    @vchPassword VARCHAR(500),
    @bActivo BIT
)
AS 
/*
** Creador:      Engrama Bot
** Propósito:    Guardar (Insert/Update) un Usuario aplicando hash a la contraseña.
** Actualizado por: Engrama Bot
** Última fecha: 30/06/2026
*/
BEGIN
    DECLARE @trancount INT = -1,
            @vchError VARCHAR(200) = '';

    DECLARE @Result AS TABLE (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdUsuario INT DEFAULT(-1)
    );

    SET NOCOUNT ON;

    BEGIN TRY
        SET XACT_ABORT ON;

        -- Validar si el email o username ya existe en otro usuario
        IF EXISTS (SELECT 1 FROM SCIA.Usuario U WHERE (U.vchEmail = @vchEmail OR (U.vchUserName = @vchUserName AND @vchUserName IS NOT NULL)) AND U.iIdUsuario <> @iIdUsuario)
        BEGIN
            SET @vchError = 'El email o nombre de usuario ya se encuentra registrado.'
            GOTO _Fin
        END;

        SET @trancount = @@TRANCOUNT;

        IF @trancount > 0
            SAVE TRANSACTION spSaveUsuario;
        ELSE
            BEGIN TRANSACTION;

        IF @iIdUsuario <= 0 
        BEGIN
            -- INSERT
            INSERT INTO SCIA.Usuario
            (
                iIdRol, vchNombre, vchUserName, vchEmail, vchPassword, dtFechaCreacion, bActivo
            )
            VALUES
            (
                @iIdRol, @vchNombre, @vchUserName, @vchEmail, CONVERT(VARCHAR(500), HASHBYTES('SHA2_256', @vchPassword), 2), GETDATE(), 1
            );
            
            SET @iIdUsuario = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            -- UPDATE (Si vchPassword no está vacío, actualizamos el Hash)
            IF LEN(@vchPassword) > 0
            BEGIN
                UPDATE SCIA.Usuario WITH(ROWLOCK)
                SET
                    iIdRol = @iIdRol,
                    vchNombre = @vchNombre,
                    vchUserName = @vchUserName,
                    vchEmail = @vchEmail,
                    vchPassword = CONVERT(VARCHAR(500), HASHBYTES('SHA2_256', @vchPassword), 2),
                    bActivo = @bActivo
                WHERE iIdUsuario = @iIdUsuario;
            END
            ELSE
            BEGIN
                UPDATE SCIA.Usuario WITH(ROWLOCK)
                SET
                    iIdRol = @iIdRol,
                    vchNombre = @vchNombre,
                    vchUserName = @vchUserName,
                    vchEmail = @vchEmail,
                    bActivo = @bActivo
                WHERE iIdUsuario = @iIdUsuario;
            END
        END

        IF @trancount = 0
            COMMIT TRANSACTION;
        
    END TRY
    BEGIN CATCH
        SELECT @vchError = CONVERT(VARCHAR(200), CONCAT('spSaveUsuario: ', ERROR_MESSAGE(), ' en línea ', ERROR_LINE()));
        PRINT @vchError;
        
        IF @trancount = 0
            ROLLBACK TRANSACTION;
        ELSE IF @trancount <> -1 AND XACT_STATE() <> -1
            ROLLBACK TRANSACTION spSaveUsuario;
    END CATCH;

_Fin:
    IF LEN(@vchError) > 0
    BEGIN
        INSERT INTO @Result (bResult, vchMessage) VALUES (0, @vchError);
    END
    ELSE
    BEGIN
        INSERT INTO @Result (bResult, vchMessage, iIdUsuario) VALUES (1, 'Usuario guardado correctamente.', @iIdUsuario);
    END

    SELECT * FROM @Result;
    SET NOCOUNT OFF;
END
GO


-- =========================================================
-- STORED PROCEDURES: spGetUsuarioAuth
-- =========================================================

IF OBJECT_ID('SCIA.spGetUsuarioAuth') IS NULL
    EXEC ('CREATE PROCEDURE SCIA.spGetUsuarioAuth AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE SCIA.spGetUsuarioAuth
(
    @vchUserOrEmail VARCHAR(150),
    @vchPassword VARCHAR(500)
)
AS
/*
** Creador:      Engrama Bot
** Propósito:    Validar credenciales para Auth. Retorna datos del usuario si son correctos.
** Actualizado por: Engrama Bot
** Última fecha: 30/06/2026
*/
BEGIN
    CREATE TABLE #Result
    (
        bResult BIT DEFAULT (1),
        vchMessage VARCHAR(500) DEFAULT (''),
        iIdUsuario INT DEFAULT (-1),
        vchNombre VARCHAR(100) DEFAULT (''),
        vchUserName VARCHAR(100) DEFAULT (''),
        vchEmail VARCHAR(150) DEFAULT (''),
        vchRol VARCHAR(50) DEFAULT ('')
    );

    SET NOCOUNT ON;

    BEGIN TRY
        DECLARE @vchPasswordHash VARBINARY(8000) = HASHBYTES('SHA2_256', @vchPassword);

        INSERT INTO #Result
        (
            iIdUsuario, vchNombre, vchUserName, vchEmail, vchRol
        )
        SELECT
            U.iIdUsuario, U.vchNombre, U.vchUserName, U.vchEmail, R.vchNombre
        FROM
            SCIA.Usuario U WITH(NOLOCK)
        INNER JOIN SCIA.Rol R WITH(NOLOCK) ON U.iIdRol = R.iIdRol
        WHERE
            (U.vchEmail = @vchUserOrEmail OR U.vchUserName = @vchUserOrEmail)
            AND U.vchPassword = CONVERT(VARCHAR(500), @vchPasswordHash, 2)
            AND U.bActivo = 1;

        IF NOT EXISTS (SELECT 1 FROM #Result WHERE iIdUsuario > 0)
        BEGIN
            INSERT INTO #Result (bResult, vchMessage)
            VALUES (0, 'Credenciales incorrectas o usuario inactivo.');
        END
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE()));
    END CATCH;

    SELECT TOP 1 * FROM #Result ORDER BY bResult DESC;
    DROP TABLE #Result;
END
GO
