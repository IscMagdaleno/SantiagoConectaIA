--------------------------------------------------------------------------------
-- spLinkOficinaTramite
-- Inserta o actualiza registro en OficinaTramite
-- Parámetros: @iIdOficina INT, @iIdTramite INT, @vchObservacion NVARCHAR(500), @bActivo BIT
--------------------------------------------------------------------------------
IF OBJECT_ID('spLinkOficinaTramite') IS NOT NULL
    DROP PROCEDURE spLinkOficinaTramite;
GO

CREATE PROCEDURE spLinkOficinaTramite
(
    @iIdOficina INT,
    @iIdTramite INT,
    @vchObservacion NVARCHAR(500) = NULL,
    @bActivo BIT = 1
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @vchError VARCHAR(500) = '';
    DECLARE @iIdOficinaTramite INT = 0;

    DECLARE @Result TABLE (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdOficinaTramite INT DEFAULT(-1)
    );

    BEGIN TRY
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;

        -- Validaciones básicas
        IF NOT EXISTS (SELECT 1 FROM dbo.Oficina WHERE iIdOficina = @iIdOficina)
        BEGIN
            SET @vchError = 'Oficina no encontrada.';
            THROW 51020, @vchError, 1;
        END

        IF OBJECT_ID('dbo.Tramite', 'U') IS NULL OR NOT EXISTS (SELECT 1 FROM dbo.Tramite WHERE iIdTramite = @iIdTramite)
        BEGIN
            SET @vchError = 'Trámite no encontrado.';
            THROW 51021, @vchError, 1;
        END

        IF EXISTS (SELECT 1 FROM dbo.OficinaTramite WHERE iIdOficina = @iIdOficina AND iIdTramite = @iIdTramite)
        BEGIN
            UPDATE dbo.OficinaTramite
            SET vchObservacion = @vchObservacion, bActivo = @bActivo
            WHERE iIdOficina = @iIdOficina AND iIdTramite = @iIdTramite;

            SELECT @iIdOficinaTramite = iIdOficinaTramite FROM dbo.OficinaTramite WHERE iIdOficina = @iIdOficina AND iIdTramite = @iIdTramite;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.OficinaTramite (iIdOficina, iIdTramite, vchObservacion, bActivo, dtFechaCreacion)
            VALUES (@iIdOficina, @iIdTramite, @vchObservacion, @bActivo, GETDATE());

            SET @iIdOficinaTramite = SCOPE_IDENTITY();
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        IF LEN(@vchError) = 0
            SET @vchError = CONCAT(ERROR_PROCEDURE(), ': ', ERROR_MESSAGE(), ' - Línea ', ERROR_LINE());
    END CATCH

    IF LEN(@vchError) > 0
        INSERT INTO @Result (bResult, vchMessage) VALUES (0, @vchError);
    ELSE
        INSERT INTO @Result (bResult, vchMessage, iIdOficinaTramite) VALUES (1, '', ISNULL(@iIdOficinaTramite, (SELECT iIdOficinaTramite FROM dbo.OficinaTramite WHERE iIdOficina = @iIdOficina AND iIdTramite = @iIdTramite)));

    SELECT * FROM @Result;
END
GO
