IF OBJECT_ID('SCIA.spValidarCiudadanoCodigo') IS NULL
    EXEC('CREATE PROCEDURE SCIA.spValidarCiudadanoCodigo AS SET NOCOUNT ON;');
GO

ALTER PROCEDURE [SCIA].[spValidarCiudadanoCodigo]
(
    @vchTelefono VARCHAR(20),
    @vchCodigo VARCHAR(10)
)
AS
/*
** Propósito: Validar código OTP de ciudadano.
** Última fecha: 19/08/2026
*/
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
            GOTO _Fin;
        END

        IF LEN(@vchCodigo) <> 6 OR @vchCodigo LIKE '%[^0-9]%'
        BEGIN
            SET @vchError = 'El código debe tener 6 dígitos.';
            GOTO _Fin;
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
            GOTO _Fin;
        END

        IF @dtExpiracion < GETDATE()
        BEGIN
            SET @vchError = 'El código expiró. Solicita uno nuevo.';
            GOTO _Fin;
        END

        IF @iIntentos >= 5
        BEGIN
            SET @vchError = 'Demasiados intentos. Solicita un código nuevo.';
            GOTO _Fin;
        END

        IF @vchCodigoHash <> CONVERT(VARCHAR(500), HASHBYTES('SHA2_256', @vchCodigo), 2)
        BEGIN
            UPDATE SCIA.CiudadanoVerificacion
            SET iIntentos = iIntentos + 1
            WHERE iIdCiudadanoVerificacion = @iIdCiudadanoVerificacion;

            SET @vchError = 'Código incorrecto.';
            GOTO _Fin;
        END

        UPDATE SCIA.CiudadanoVerificacion
        SET bValidado = 1
        WHERE iIdCiudadanoVerificacion = @iIdCiudadanoVerificacion;
    END TRY
    BEGIN CATCH
        SET @vchError = CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE());
    END CATCH

_Fin:
    IF LEN(@vchError) > 0
        INSERT INTO @Result (bResult, vchMessage) VALUES (0, @vchError);
    ELSE
        INSERT INTO @Result (bResult, vchMessage) VALUES (1, 'Código validado.');

    SELECT TOP 1 * FROM @Result ORDER BY bResult DESC;
END
GO
