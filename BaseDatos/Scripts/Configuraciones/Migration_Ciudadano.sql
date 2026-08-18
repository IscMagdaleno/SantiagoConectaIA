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

        INSERT INTO SCIA.Ciudadano (vchAlias, vchTelefono, vchPassword, dtFechaCreacion, bActivo)
        VALUES
        (
            @vchAlias,
            @Telefono,
            CONVERT(VARCHAR(500), HASHBYTES('SHA2_256', @vchPassword), 2),
            GETDATE(),
            1
        );

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
