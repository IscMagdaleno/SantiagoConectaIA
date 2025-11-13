ALTER PROCEDURE spGetTramitesCard
(
    @iIdTramite INT,
    @bActivo BIT
)
AS
/*
** Propósito:    Obtener el listado de trámites con información de sus oficinas.
** Metodología:  Engrama
*/
BEGIN
    -- Crear tabla temporal para los resultados estándar de Engrama
    CREATE TABLE #Result
    (
        bResult BIT DEFAULT (1),
        vchMessage VARCHAR(500) DEFAULT (''),
        -- Campos específicos solicitados
        iIdTramite INT DEFAULT(0),
        iIdOficina INT DEFAULT(0),
        vchNombreTramite VARCHAR(200) DEFAULT(''),
        vchDescripcionTramite VARCHAR(MAX) DEFAULT(''),
        vchNombreOficina VARCHAR(200) DEFAULT(''),
        vchDireccionOficina VARCHAR(500) DEFAULT(''),
        vchTelefonoOficina VARCHAR(50) DEFAULT(''),
        bModalidadEnLinea BIT DEFAULT(0),
        dCosto DECIMAL(18,2) DEFAULT(0.0),
        vchHorarioOficina VARCHAR (50) DEFAULT('')
    );

    SET NOCOUNT ON;

    BEGIN TRY
        -- Insertar resultados haciendo JOIN entre Tramite y Oficina
        INSERT INTO #Result
        (
            iIdTramite, 
            iIdOficina, 
            vchNombreTramite, 
            vchDescripcionTramite, 
            vchNombreOficina, 
            vchDireccionOficina, 
            vchTelefonoOficina, 
            bModalidadEnLinea, 
            dCosto,
            vchHorarioOficina
        )
        SELECT 
            T.iIdTramite,
            T.iIdOficina,
            T.vchNombre,      -- Alias vchNombreTramite
            T.nvchDescripcion, -- Alias vchDescripcionTramite
            O.vchNombre,      -- Alias vchNombreOficina
            O.vchDireccion,   -- Alias vchDireccionOficina
            O.vchTelefono,    -- Alias vchTelefonoOficina
            T.bModalidadEnLinea,
            T.mCosto,          -- Alias dCosto
            O.vchHorario
        FROM 
            dbo.Tramite T WITH(NOLOCK)
        INNER JOIN 
            dbo.Oficina O WITH(NOLOCK) ON T.iIdOficina = O.iIdOficina
        WHERE 
            (T.iIdTramite = @iIdTramite OR @iIdTramite = 0) -- Si es 0 trae todos
            AND (T.bActivo = @bActivo);

        -- Validar si no se encontraron registros
        IF NOT EXISTS (SELECT 1 FROM #Result)
        BEGIN
            INSERT INTO #Result (bResult, vchMessage)
            VALUES (0, 'No se encontraron trámites con los criterios especificados.');
        END

    END TRY
    BEGIN CATCH
        -- Capturar error estándar Engrama
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE()));
    END CATCH;

    -- Devolver resultados
    SELECT * FROM #Result;

    -- Eliminar tabla temporal
    DROP TABLE #Result;
END