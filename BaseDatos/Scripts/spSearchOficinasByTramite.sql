IF OBJECT_ID('spSearchOficinasByTramite') IS NULL
    EXEC ('CREATE PROCEDURE spSearchOficinasByTramite AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE spSearchOficinasByTramite
(
    @iIdTramite INT
)
AS
/*
** Propósito:    Busca todas las oficinas asociadas a un trámite específico, utilizando la tabla OficinaTramite (relación N:M).
** Última fecha: 11/04/2025
*/
BEGIN
    SET NOCOUNT ON;

    -- Se utiliza la misma estructura de resultado que el spSearchOficinasForChat
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
        -- Consulta la tabla de unión (OficinaTramite) y JOIN a Oficina
        INSERT INTO #Result (
            iIdOficina, iIdDependencia, vchNombre, vchDireccion,
            vchTelefono, vchEmail, vchHorario, flLatitud, flLongitud,
            vchNotas, bActivo, dtFechaCreacion
        )
        SELECT 
            O.iIdOficina, O.iIdDependencia, O.vchNombre, O.vchDireccion,
            O.vchTelefono, O.vchEmail, O.vchHorario, O.flLatitud, O.flLongitud,
            O.vchNotas, O.bActivo, O.dtFechaCreacion
        FROM dbo.Oficina O WITH(NOLOCK)
        INNER JOIN dbo.OficinaTramite OT WITH(NOLOCK)
            ON O.iIdOficina = OT.iIdOficina
        WHERE OT.iIdTramite = @iIdTramite
          AND O.bActivo = 1 
          AND OT.bActivo = 1;

        -- Validar si se encontraron oficinas
        IF NOT EXISTS (SELECT 1 FROM #Result)
            INSERT INTO #Result (bResult, vchMessage) 
            VALUES (0, CONCAT('No se encontró ninguna oficina activa para el Trámite ID ', @iIdTramite, '.'));

    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE()));
    END CATCH

    -- Devolver resultados
    SELECT * FROM #Result;
    DROP TABLE #Result;
END
GO