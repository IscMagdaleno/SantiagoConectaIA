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
