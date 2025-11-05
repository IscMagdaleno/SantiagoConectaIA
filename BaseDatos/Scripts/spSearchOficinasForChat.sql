IF OBJECT_ID('spSearchOficinasForChat') IS NULL
    EXEC ('CREATE PROCEDURE spSearchOficinasForChat AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE spSearchOficinasForChat
(
    @vchTexto NVARCHAR(500) ,
    @iLimit INT = 5
)
AS
/*
** Propósito:    Busca oficinas activas por texto, devolviendo toda la información de la tabla Oficina.
** Última fecha: 11/04/2025
*/
BEGIN
    SET NOCOUNT ON;

    -- Modificación: Incluye todas las columnas de la tabla Oficina.
    CREATE TABLE #Result
    (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdOficina INT DEFAULT(-1),
        iIdDependencia INT NULL,
        vchNombre VARCHAR(250) DEFAULT(''),
        vchDireccion VARCHAR(500) NULL,
        vchTelefono VARCHAR(50) NULL,
        vchEmail VARCHAR(150) NULL,
        vchHorario NVARCHAR(250) NULL,
        flLatitud FLOAT NULL,
        flLongitud FLOAT NULL,
        vchNotas NVARCHAR(MAX) NULL,
        bActivo BIT NULL,
        dtFechaCreacion DATETIME NULL
    );

    BEGIN TRY
        -- Modificación: Inserta TODAS las columnas de la tabla Oficina
        INSERT INTO #Result (
            iIdOficina, iIdDependencia, vchNombre, vchDireccion,
            vchTelefono, vchEmail, vchHorario, flLatitud, flLongitud,
            vchNotas, bActivo, dtFechaCreacion
        )
        SELECT TOP (@iLimit)
            O.iIdOficina,
            O.iIdDependencia,
            O.vchNombre,
            O.vchDireccion,
            O.vchTelefono,
            O.vchEmail,
            O.vchHorario,
            O.flLatitud,
            O.flLongitud,
            O.vchNotas,
            O.bActivo,
            O.dtFechaCreacion
        FROM dbo.Oficina O WITH(NOLOCK)
        WHERE O.bActivo = 1
          AND (
                @vchTexto IS NULL
                OR O.vchNombre LIKE '%' + @vchTexto + '%'
                OR O.vchDireccion LIKE '%' + @vchTexto + '%'
              )
        ORDER BY
            CASE WHEN @vchTexto IS NOT NULL AND O.vchNombre LIKE '%' + @vchTexto + '%' THEN 0 ELSE 1 END,
            O.dtFechaCreacion DESC;

        IF NOT EXISTS (SELECT 1 FROM #Result WHERE iIdOficina <> -1)
            INSERT INTO #Result (bResult, vchMessage) VALUES (0, 'No se encontraron oficinas relevantes.');
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE()));
    END CATCH

    SELECT * FROM #Result
    DROP TABLE #Result
END
GO