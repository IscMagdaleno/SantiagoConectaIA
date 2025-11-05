IF OBJECT_ID( 'spGetOficinas' ) IS NULL
	EXEC ('CREATE PROCEDURE spGetOficinas AS SET NOCOUNT ON;') 
GO 
ALTER PROCEDURE spGetOficinas
(
    @iIdOficina INT = 0,
    @bActivo BIT = 0,
    @bIncluirContacto BIT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #Result
    (
        bResult BIT DEFAULT (1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdOficina INT DEFAULT(-1),
        iIdDependencia INT DEFAULT(-1),
        vchNombre VARCHAR(250) DEFAULT(''),
        vchDireccion VARCHAR(500) DEFAULT(''),
        vchTelefono VARCHAR(50) DEFAULT(''),
        vchEmail VARCHAR(150) DEFAULT(''),
        vchHorario NVARCHAR(250) DEFAULT(''),
        flLatitud FLOAT NULL,
        flLongitud FLOAT NULL,
        vchNotas NVARCHAR(MAX) DEFAULT(''),
        bActivo BIT DEFAULT(0),
        dtFechaCreacion DATETIME NULL
    );

    BEGIN TRY
        INSERT INTO #Result (iIdOficina, iIdDependencia, vchNombre, vchDireccion, vchTelefono, vchEmail, vchHorario, flLatitud, flLongitud, vchNotas, bActivo, dtFechaCreacion)
        SELECT
            O.iIdOficina,
            O.iIdDependencia,
            O.vchNombre,
            O.vchDireccion,
            CASE WHEN @bIncluirContacto = 1 THEN O.vchTelefono ELSE NULL END AS vchTelefono,
            CASE WHEN @bIncluirContacto = 1 THEN O.vchEmail ELSE NULL END AS vchEmail,
            O.vchHorario,
            O.flLatitud,
            O.flLongitud,
            O.vchNotas,
            O.bActivo,
            O.dtFechaCreacion
        FROM dbo.Oficina O WITH(NOLOCK)
        WHERE (@iIdOficina = 0 OR O.iIdOficina = @iIdOficina)
          AND (@bActivo = 0 OR O.bActivo = @bActivo)

        IF NOT EXISTS (SELECT 1 FROM #Result)
            INSERT INTO #Result (bResult, vchMessage) VALUES (0, 'No se encontraron oficinas.')
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE()));
    END CATCH

    SELECT * FROM #Result
    DROP TABLE #Result
END
GO



