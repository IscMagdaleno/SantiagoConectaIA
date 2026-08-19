/*
** Migration: Citizen (Ciudadano) phone registration
** Unique 10-digit phone + alias + hashed PIN.
** Date: 2026-08-18
*/

SET NOCOUNT ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'SCIA')
    EXEC('CREATE SCHEMA SCIA');
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables
    WHERE name = N'Ciudadano' AND schema_id = SCHEMA_ID(N'SCIA')
)
BEGIN
    CREATE TABLE SCIA.Ciudadano
    (
        iIdCiudadano INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
        vchAlias NVARCHAR(80) NOT NULL,
        vchTelefono CHAR(10) NOT NULL,
        vchPassword VARCHAR(500) NOT NULL,
        dtFechaCreacion DATETIME NOT NULL CONSTRAINT DF_Ciudadano_dtFechaCreacion DEFAULT (GETDATE()),
        bActivo BIT NOT NULL CONSTRAINT DF_Ciudadano_bActivo DEFAULT (1),
        CONSTRAINT UQ_Ciudadano_Telefono UNIQUE (vchTelefono)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables
    WHERE name = N'CiudadanoVerificacion' AND schema_id = SCHEMA_ID(N'SCIA')
)
BEGIN
    CREATE TABLE SCIA.CiudadanoVerificacion
    (
        iIdCiudadanoVerificacion INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
        vchTelefono CHAR(10) NOT NULL,
        vchCodigoHash VARCHAR(500) NOT NULL,
        dtFechaCreacion DATETIME NOT NULL CONSTRAINT DF_CiudadanoVerificacion_dtFechaCreacion DEFAULT (GETDATE()),
        dtExpiracion DATETIME NOT NULL,
        iIntentos INT NOT NULL CONSTRAINT DF_CiudadanoVerificacion_iIntentos DEFAULT (0),
        bValidado BIT NOT NULL CONSTRAINT DF_CiudadanoVerificacion_bValidado DEFAULT (0)
    );
END
GO

IF OBJECT_ID('SCIA.spSaveCiudadano') IS NULL
    EXEC('CREATE PROCEDURE SCIA.spSaveCiudadano AS SET NOCOUNT ON;');
GO

ALTER PROCEDURE [SCIA].[spSaveCiudadano]
(
    @vchAlias NVARCHAR(80),
    @vchTelefono VARCHAR(20),
    @vchPassword VARCHAR(500)
)
AS
/*
** Propósito: Registrar un ciudadano. El teléfono (10 dígitos) es único.
** Última fecha: 18/08/2026
*/
BEGIN
    DECLARE @vchError VARCHAR(200) = '';
    DECLARE @iIdCiudadano INT = -1;
    DECLARE @Telefono CHAR(10);

    DECLARE @Result AS TABLE
    (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdCiudadano INT DEFAULT(-1),
        vchAlias NVARCHAR(80) DEFAULT(''),
        vchTelefono VARCHAR(10) DEFAULT('')
    );

    SET NOCOUNT ON;

    BEGIN TRY
        SET @vchAlias = LTRIM(RTRIM(ISNULL(@vchAlias, N'')));
        SET @vchTelefono = REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(ISNULL(@vchTelefono, ''))), ' ', ''), '-', ''), '(', '');
        SET @vchTelefono = REPLACE(@vchTelefono, ')', '');

        IF @vchAlias = N''
        BEGIN
            SET @vchError = 'El nombre o alias es obligatorio.';
            GOTO _Fin;
        END

        IF LEN(@vchTelefono) <> 10 OR @vchTelefono LIKE '%[^0-9]%'
        BEGIN
            SET @vchError = 'El teléfono debe tener exactamente 10 dígitos.';
            GOTO _Fin;
        END

        IF LEFT(@vchTelefono, 1) IN ('0', '1')
        BEGIN
            SET @vchError = 'El teléfono debe iniciar con un dígito entre 2 y 9.';
            GOTO _Fin;
        END

        IF @vchTelefono = REPLICATE(LEFT(@vchTelefono, 1), 10)
        BEGIN
            SET @vchError = 'El teléfono no puede tener todos los dígitos iguales.';
            GOTO _Fin;
        END

        IF LEN(LTRIM(RTRIM(ISNULL(@vchPassword, '')))) < 4
        BEGIN
            SET @vchError = 'El PIN debe tener al menos 4 caracteres.';
            GOTO _Fin;
        END

        SET @Telefono = @vchTelefono;

        IF EXISTS (SELECT 1 FROM SCIA.Ciudadano C WITH (NOLOCK) WHERE C.vchTelefono = @Telefono)
        BEGIN
            SET @vchError = 'Este número ya está registrado.';
            GOTO _Fin;
        END

        IF NOT EXISTS
        (
            SELECT 1
            FROM SCIA.CiudadanoVerificacion CV WITH (NOLOCK)
            WHERE CV.vchTelefono = @Telefono
              AND CV.bValidado = 1
              AND CV.dtExpiracion >= GETDATE()
              AND CV.dtFechaCreacion >= DATEADD(MINUTE, -30, GETDATE())
        )
        BEGIN
            SET @vchError = 'Primero valida tu código de WhatsApp.';
            GOTO _Fin;
        END

        INSERT INTO SCIA.Ciudadano (vchAlias, vchTelefono, vchPassword, dtFechaCreacion, bActivo)
        VALUES
        (
            @vchAlias,
            @Telefono,
            CONVERT(VARCHAR(500), HASHBYTES('SHA2_256', @vchPassword), 2),
            GETDATE(),
            1
        );

        DELETE FROM SCIA.CiudadanoVerificacion WHERE vchTelefono = @Telefono;

        SET @iIdCiudadano = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        SET @vchError = CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE());
    END CATCH

_Fin:
    IF LEN(@vchError) > 0
        INSERT INTO @Result (bResult, vchMessage) VALUES (0, @vchError);
    ELSE
        INSERT INTO @Result (bResult, vchMessage, iIdCiudadano, vchAlias, vchTelefono)
        VALUES (1, 'Bienvenido a Santiago Conecta.', @iIdCiudadano, @vchAlias, RTRIM(@Telefono));

    SELECT * FROM @Result;
END
GO

IF OBJECT_ID('SCIA.spGetCiudadanoAuth') IS NULL
    EXEC('CREATE PROCEDURE SCIA.spGetCiudadanoAuth AS SET NOCOUNT ON;');
GO

ALTER PROCEDURE [SCIA].[spGetCiudadanoAuth]
(
    @vchTelefono VARCHAR(20),
    @vchPassword VARCHAR(500)
)
AS
/*
** Propósito: Validar teléfono + PIN de un ciudadano activo.
** Última fecha: 18/08/2026
*/
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #Result
    (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdCiudadano INT DEFAULT(-1),
        vchAlias NVARCHAR(80) DEFAULT(''),
        vchTelefono VARCHAR(10) DEFAULT('')
    );

    BEGIN TRY
        SET @vchTelefono = REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(ISNULL(@vchTelefono, ''))), ' ', ''), '-', ''), '(', '');
        SET @vchTelefono = REPLACE(@vchTelefono, ')', '');

        IF LEN(@vchTelefono) <> 10 OR @vchTelefono LIKE '%[^0-9]%'
        BEGIN
            INSERT INTO #Result (bResult, vchMessage)
            VALUES (0, 'El teléfono debe tener exactamente 10 dígitos.');
        END
        ELSE IF LEFT(@vchTelefono, 1) IN ('0', '1')
        BEGIN
            INSERT INTO #Result (bResult, vchMessage)
            VALUES (0, 'El teléfono debe iniciar con un dígito entre 2 y 9.');
        END
        ELSE IF @vchTelefono = REPLICATE(LEFT(@vchTelefono, 1), 10)
        BEGIN
            INSERT INTO #Result (bResult, vchMessage)
            VALUES (0, 'El teléfono no puede tener todos los dígitos iguales.');
        END
        ELSE
        BEGIN
            INSERT INTO #Result (iIdCiudadano, vchAlias, vchTelefono)
            SELECT C.iIdCiudadano, C.vchAlias, RTRIM(C.vchTelefono)
            FROM SCIA.Ciudadano C WITH (NOLOCK)
            WHERE C.vchTelefono = @vchTelefono
              AND C.vchPassword = CONVERT(VARCHAR(500), HASHBYTES('SHA2_256', @vchPassword), 2)
              AND C.bActivo = 1;

            IF NOT EXISTS (SELECT 1 FROM #Result WHERE iIdCiudadano > 0)
            BEGIN
                INSERT INTO #Result (bResult, vchMessage)
                VALUES (0, 'Teléfono o PIN incorrectos.');
            END
        END
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE()));
    END CATCH

    SELECT TOP 1 * FROM #Result ORDER BY bResult DESC;
    DROP TABLE #Result;
END
GO

IF OBJECT_ID('SCIA.spSaveCiudadanoCodigo') IS NULL
    EXEC('CREATE PROCEDURE SCIA.spSaveCiudadanoCodigo AS SET NOCOUNT ON;');
GO

ALTER PROCEDURE [SCIA].[spSaveCiudadanoCodigo]
(
    @vchTelefono VARCHAR(20),
    @vchCodigo VARCHAR(10)
)
AS
BEGIN
    DECLARE @vchError VARCHAR(200) = '';
    DECLARE @Telefono CHAR(10);

    DECLARE @Result AS TABLE
    (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT('')
    );

    BEGIN TRY
        SET @vchTelefono = REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(ISNULL(@vchTelefono, ''))), ' ', ''), '-', ''), '(', '');
        SET @vchTelefono = REPLACE(@vchTelefono, ')', '');
        SET @vchCodigo = LTRIM(RTRIM(ISNULL(@vchCodigo, '')));

        IF LEN(@vchTelefono) <> 10 OR @vchTelefono LIKE '%[^0-9]%'
        BEGIN
            SET @vchError = 'El teléfono debe tener exactamente 10 dígitos.';
            GOTO _FinCodigo;
        END

        IF LEFT(@vchTelefono, 1) IN ('0', '1')
        BEGIN
            SET @vchError = 'El teléfono debe iniciar con un dígito entre 2 y 9.';
            GOTO _FinCodigo;
        END

        IF @vchTelefono = REPLICATE(LEFT(@vchTelefono, 1), 10)
        BEGIN
            SET @vchError = 'El teléfono no puede tener todos los dígitos iguales.';
            GOTO _FinCodigo;
        END

        IF LEN(@vchCodigo) <> 6 OR @vchCodigo LIKE '%[^0-9]%'
        BEGIN
            SET @vchError = 'El código debe tener 6 dígitos.';
            GOTO _FinCodigo;
        END

        SET @Telefono = @vchTelefono;

        IF EXISTS (SELECT 1 FROM SCIA.Ciudadano C WITH (NOLOCK) WHERE C.vchTelefono = @Telefono)
        BEGIN
            SET @vchError = 'Este número ya está registrado.';
            GOTO _FinCodigo;
        END

        IF (
            SELECT COUNT(1)
            FROM SCIA.CiudadanoVerificacion CV WITH (NOLOCK)
            WHERE CV.vchTelefono = @Telefono
              AND CV.dtFechaCreacion >= DATEADD(HOUR, -1, GETDATE())
        ) >= 3
        BEGIN
            SET @vchError = 'Límite de envíos alcanzado. Intenta de nuevo en unos minutos.';
            GOTO _FinCodigo;
        END

        UPDATE SCIA.CiudadanoVerificacion
        SET dtExpiracion = DATEADD(MINUTE, -1, GETDATE())
        WHERE vchTelefono = @Telefono
          AND bValidado = 0
          AND dtExpiracion >= GETDATE();

        INSERT INTO SCIA.CiudadanoVerificacion
        (
            vchTelefono,
            vchCodigoHash,
            dtFechaCreacion,
            dtExpiracion,
            iIntentos,
            bValidado
        )
        VALUES
        (
            @Telefono,
            CONVERT(VARCHAR(500), HASHBYTES('SHA2_256', @vchCodigo), 2),
            GETDATE(),
            DATEADD(MINUTE, 10, GETDATE()),
            0,
            0
        );
    END TRY
    BEGIN CATCH
        SET @vchError = CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE());
    END CATCH

_FinCodigo:
    IF LEN(@vchError) > 0
        INSERT INTO @Result (bResult, vchMessage) VALUES (0, @vchError);
    ELSE
        INSERT INTO @Result (bResult, vchMessage) VALUES (1, 'Código generado.');

    SELECT TOP 1 * FROM @Result ORDER BY bResult DESC;
END
GO

IF OBJECT_ID('SCIA.spValidarCiudadanoCodigo') IS NULL
    EXEC('CREATE PROCEDURE SCIA.spValidarCiudadanoCodigo AS SET NOCOUNT ON;');
GO

ALTER PROCEDURE [SCIA].[spValidarCiudadanoCodigo]
(
    @vchTelefono VARCHAR(20),
    @vchCodigo VARCHAR(10)
)
AS
BEGIN
    DECLARE @vchError VARCHAR(200) = '';
    DECLARE @Telefono CHAR(10);
    DECLARE @iIdCiudadanoVerificacion INT = 0;
    DECLARE @vchCodigoHash VARCHAR(500) = '';
    DECLARE @dtExpiracion DATETIME;
    DECLARE @iIntentos INT = 0;

    DECLARE @Result AS TABLE
    (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT('')
    );

    BEGIN TRY
        SET @vchTelefono = REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(ISNULL(@vchTelefono, ''))), ' ', ''), '-', ''), '(', '');
        SET @vchTelefono = REPLACE(@vchTelefono, ')', '');
        SET @vchCodigo = LTRIM(RTRIM(ISNULL(@vchCodigo, '')));

        IF LEN(@vchTelefono) <> 10 OR @vchTelefono LIKE '%[^0-9]%'
        BEGIN
            SET @vchError = 'El teléfono debe tener exactamente 10 dígitos.';
            GOTO _FinValidar;
        END

        IF LEN(@vchCodigo) <> 6 OR @vchCodigo LIKE '%[^0-9]%'
        BEGIN
            SET @vchError = 'El código debe tener 6 dígitos.';
            GOTO _FinValidar;
        END

        SET @Telefono = @vchTelefono;

        SELECT TOP 1
            @iIdCiudadanoVerificacion = CV.iIdCiudadanoVerificacion,
            @vchCodigoHash = CV.vchCodigoHash,
            @dtExpiracion = CV.dtExpiracion,
            @iIntentos = CV.iIntentos
        FROM SCIA.CiudadanoVerificacion CV WITH (NOLOCK)
        WHERE CV.vchTelefono = @Telefono
          AND CV.bValidado = 0
        ORDER BY CV.iIdCiudadanoVerificacion DESC;

        IF @iIdCiudadanoVerificacion <= 0
        BEGIN
            SET @vchError = 'Primero solicita un código.';
            GOTO _FinValidar;
        END

        IF @dtExpiracion < GETDATE()
        BEGIN
            SET @vchError = 'El código expiró. Solicita uno nuevo.';
            GOTO _FinValidar;
        END

        IF @iIntentos >= 5
        BEGIN
            SET @vchError = 'Demasiados intentos. Solicita un código nuevo.';
            GOTO _FinValidar;
        END

        IF @vchCodigoHash <> CONVERT(VARCHAR(500), HASHBYTES('SHA2_256', @vchCodigo), 2)
        BEGIN
            UPDATE SCIA.CiudadanoVerificacion
            SET iIntentos = iIntentos + 1
            WHERE iIdCiudadanoVerificacion = @iIdCiudadanoVerificacion;

            SET @vchError = 'Código incorrecto.';
            GOTO _FinValidar;
        END

        UPDATE SCIA.CiudadanoVerificacion
        SET bValidado = 1
        WHERE iIdCiudadanoVerificacion = @iIdCiudadanoVerificacion;
    END TRY
    BEGIN CATCH
        SET @vchError = CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE());
    END CATCH

_FinValidar:
    IF LEN(@vchError) > 0
        INSERT INTO @Result (bResult, vchMessage) VALUES (0, @vchError);
    ELSE
        INSERT INTO @Result (bResult, vchMessage) VALUES (1, 'Código validado.');

    SELECT TOP 1 * FROM @Result ORDER BY bResult DESC;
END
GO
