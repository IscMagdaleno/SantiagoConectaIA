IF OBJECT_ID('SCIA.spSaveCiudadanoCodigo') IS NULL
    EXEC('CREATE PROCEDURE SCIA.spSaveCiudadanoCodigo AS SET NOCOUNT ON;');
GO

ALTER PROCEDURE [SCIA].[spSaveCiudadanoCodigo]
(
    @vchTelefono VARCHAR(20),
    @vchCodigo VARCHAR(10)
)
AS
/*
** Propósito: Generar/guardar código OTP para registro ciudadano.
** Última fecha: 19/08/2026
*/
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

        IF LEN(@vchCodigo) <> 6 OR @vchCodigo LIKE '%[^0-9]%'
        BEGIN
            SET @vchError = 'El código debe tener 6 dígitos.';
            GOTO _Fin;
        END

        SET @Telefono = @vchTelefono;

        IF EXISTS (SELECT 1 FROM SCIA.Ciudadano C WITH (NOLOCK) WHERE C.vchTelefono = @Telefono)
        BEGIN
            SET @vchError = 'Este número ya está registrado.';
            GOTO _Fin;
        END

        IF (
            SELECT COUNT(1)
            FROM SCIA.CiudadanoVerificacion CV WITH (NOLOCK)
            WHERE CV.vchTelefono = @Telefono
              AND CV.dtFechaCreacion >= DATEADD(HOUR, -1, GETDATE())
        ) >= 3
        BEGIN
            SET @vchError = 'Límite de envíos alcanzado. Intenta de nuevo en unos minutos.';
            GOTO _Fin;
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

_Fin:
    IF LEN(@vchError) > 0
        INSERT INTO @Result (bResult, vchMessage) VALUES (0, @vchError);
    ELSE
        INSERT INTO @Result (bResult, vchMessage) VALUES (1, 'Código generado.');

    SELECT TOP 1 * FROM @Result ORDER BY bResult DESC;
END
GO
