--------------------------------------------------------------------------------
-- spGetOficinasPorTramite
-- Parámetros:
--   @iIdTramite INT
--   @vchTexto VARCHAR(500) = NULL  -> filtro sobre nombre/dirección
--   @bIncluirContacto BIT = 0
-- Retorna oficinas asociadas al trámite (JOIN con OficinaTramite)
--------------------------------------------------------------------------------
IF OBJECT_ID('spGetOficinasPorTramite') IS NOT NULL
    DROP PROCEDURE spGetOficinasPorTramite;
GO

CREATE PROCEDURE spGetOficinasPorTramite
(
    @iIdTramite INT,
    @vchTexto VARCHAR(500) = NULL,
    @bIncluirContacto BIT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #Result
    (
        bResult BIT DEFAULT (1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdOficinaTramite INT DEFAULT(-1),
        iIdOficina INT DEFAULT(-1),
        iIdTramite INT DEFAULT(-1),
        vchNombre VARCHAR(250) DEFAULT(''),
        vchDireccion VARCHAR(500) DEFAULT(''),
        vchHorario NVARCHAR(250) DEFAULT(''),
        flLatitud FLOAT NULL,
        flLongitud FLOAT NULL,
        vchTelefono VARCHAR(50) DEFAULT(''),
        vchEmail VARCHAR(150) DEFAULT(''),
        vchObservacion NVARCHAR(500) DEFAULT(''),
        bActivo BIT DEFAULT(0),
        dtFechaCreacion DATETIME NULL
    );

    BEGIN TRY
        INSERT INTO #Result (iIdOficinaTramite, iIdOficina, iIdTramite, vchNombre, vchDireccion, vchHorario, flLatitud, flLongitud, vchTelefono, vchEmail, vchObservacion, bActivo, dtFechaCreacion)
        SELECT
            OT.iIdOficinaTramite,
            O.iIdOficina,
            OT.iIdTramite,
            O.vchNombre,
            O.vchDireccion,
            O.vchHorario,
            O.flLatitud,
            O.flLongitud,
            CASE WHEN @bIncluirContacto = 1 THEN O.vchTelefono ELSE NULL END,
            CASE WHEN @bIncluirContacto = 1 THEN O.vchEmail ELSE NULL END,
            OT.vchObservacion,
            OT.bActivo,
            OT.dtFechaCreacion
        FROM dbo.OficinaTramite OT WITH(NOLOCK)
        INNER JOIN dbo.Oficina O WITH(NOLOCK) ON OT.iIdOficina = O.iIdOficina
        WHERE OT.iIdTramite = @iIdTramite
          AND (
                @vchTexto IS NULL
                OR O.vchNombre LIKE '%' + @vchTexto + '%'
                OR O.vchDireccion LIKE '%' + @vchTexto + '%'
              )

        IF NOT EXISTS (SELECT 1 FROM #Result)
            INSERT INTO #Result (bResult, vchMessage) VALUES (0, 'No se encontraron oficinas para el trámite.')
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE()));
    END CATCH

    SELECT * FROM #Result
    DROP TABLE #Result
END
GO
