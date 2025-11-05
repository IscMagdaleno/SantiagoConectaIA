IF OBJECT_ID('spSearchOficinasForChat') IS NOT NULL
    DROP PROCEDURE spSearchOficinasForChat;
GO

CREATE PROCEDURE spSearchOficinasForChat
(
    @vchTexto NVARCHAR(500) = NULL,
    @iLimit INT = 5
)
AS
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #Result
    (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdOficina INT DEFAULT(-1),
        vchTitle VARCHAR(250) DEFAULT(''),
        nvchShortText NVARCHAR(2000) DEFAULT(''), -- combinación de dirección/horario
        vchSource VARCHAR(100) DEFAULT('Oficina'),
        dtFecha DATETIME NULL
    );

    BEGIN TRY
        INSERT INTO #Result (iIdOficina, vchTitle, nvchShortText, vchSource, dtFecha)
        SELECT TOP (@iLimit)
            O.iIdOficina,
            O.vchNombre,
            LEFT(
                COALESCE(O.vchDireccion, N'') + N' | Horario: ' + COALESCE(O.vchHorario, N''),
                1500
            ),
            'Oficina',
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

        IF NOT EXISTS (SELECT 1 FROM #Result)
            INSERT INTO #Result (bResult, vchMessage) VALUES (0, 'No se encontraron oficinas relevantes.');
    END TRY
    BEGIN CATCH
        INSERT INTO #Result (bResult, vchMessage)
        VALUES (0, CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE()));
    END CATCH

    SELECT * FROM #Result
    DROP TABLE #Result
END;