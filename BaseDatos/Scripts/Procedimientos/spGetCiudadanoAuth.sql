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
