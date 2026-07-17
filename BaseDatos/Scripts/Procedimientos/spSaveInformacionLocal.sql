IF OBJECT_ID('SCIA.spSaveInformacionLocal') IS NULL
    EXEC ('CREATE PROCEDURE SCIA.spSaveInformacionLocal AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE SCIA.spSaveInformacionLocal
(
    @iIdInformacionLocal INT = 0,
    @nvchCategoria NVARCHAR(100),
    @nvchTitulo NVARCHAR(255),
    @nvchPalabrasClave NVARCHAR(500) = NULL,
    @nvchDescripcionCorta NVARCHAR(1000),
    @nvchContenidoDetallado NVARCHAR(MAX) = NULL,
    @nvchUbicacion_LatLong NVARCHAR(100) = NULL,
    @bActivo BIT = 1
)
AS
/*
** Proposito:    Guarda (inserta o actualiza) un registro de informacion local
** Ultima fecha: 17/07/2026
*/
BEGIN
    SET NOCOUNT ON;

    DECLARE @bResult BIT = 0;
    DECLARE @vchMessage VARCHAR(500) = '';
    DECLARE @iNewId INT = 0;

    BEGIN TRY
        IF @iIdInformacionLocal = 0
        BEGIN
            INSERT INTO Engrama.SCIA.InformacionLocal
            (
                nvchCategoria, nvchTitulo, nvchPalabrasClave,
                nvchDescripcionCorta, nvchContenidoDetallado, nvchUbicacion_LatLong,
                bActivo
            )
            VALUES
            (
                @nvchCategoria, @nvchTitulo, @nvchPalabrasClave,
                @nvchDescripcionCorta, @nvchContenidoDetallado, @nvchUbicacion_LatLong,
                @bActivo
            );

            SET @iNewId = SCOPE_IDENTITY();
            SET @vchMessage = 'Informacion local creada correctamente.';
        END
        ELSE
        BEGIN
            UPDATE Engrama.SCIA.InformacionLocal
            SET
                nvchCategoria = @nvchCategoria,
                nvchTitulo = @nvchTitulo,
                nvchPalabrasClave = @nvchPalabrasClave,
                nvchDescripcionCorta = @nvchDescripcionCorta,
                nvchContenidoDetallado = @nvchContenidoDetallado,
                nvchUbicacion_LatLong = @nvchUbicacion_LatLong,
                bActivo = @bActivo
            WHERE iIdInformacionLocal = @iIdInformacionLocal;

            SET @iNewId = @iIdInformacionLocal;
            SET @vchMessage = 'Informacion local actualizada correctamente.';
        END

        SET @bResult = 1;
    END TRY
    BEGIN CATCH
        SET @bResult = 0;
        SET @vchMessage = CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Linea ', ERROR_LINE());
    END CATCH

    SELECT @bResult AS bResult, @vchMessage AS vchMessage, @iNewId AS iIdInformacionLocal;
END
GO
