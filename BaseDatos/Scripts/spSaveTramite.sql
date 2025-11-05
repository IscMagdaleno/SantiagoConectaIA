IF OBJECT_ID('spSaveTramite') IS NULL
    EXEC('CREATE PROCEDURE spSaveTramite AS SET NOCOUNT ON;');
GO

ALTER PROCEDURE spSaveTramite
(
    @iIdTramite INT,
    @vchNombre VARCHAR(250),
    @nvchDescripcion NVARCHAR(MAX),
    @iIdCategoria INT,
    @bModalidadEnLinea BIT,
    @mCosto MONEY,
    @iIdOficina INT,
    @bActivo BIT
)
AS
BEGIN
    DECLARE @trancount INT = -1,
            @vchError VARCHAR(500) = '';

    DECLARE @Result TABLE (
        bResult BIT DEFAULT(1),
        vchMessage VARCHAR(500) DEFAULT(''),
        iIdTramite INT DEFAULT(-1)
    );

    SET NOCOUNT ON;
    BEGIN TRY
        SET XACT_ABORT ON;
        SET @trancount = @@TRANCOUNT;
        IF @trancount > 0
            SAVE TRANSACTION spSaveTramite;
        ELSE
            BEGIN TRANSACTION;

        IF @iIdTramite IS NULL OR @iIdTramite <= 0
        BEGIN
            INSERT INTO dbo.Tramite (vchNombre, nvchDescripcion, iIdCategoria, bModalidadEnLinea, mCosto, iIdOficina, dtFechaCreacion, bActivo)
            VALUES (@vchNombre, @nvchDescripcion, @iIdCategoria, @bModalidadEnLinea, @mCosto, @iIdOficina, GETDATE(), @bActivo);

            SET @iIdTramite = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            UPDATE dbo.Tramite WITH(ROWLOCK)
            SET vchNombre = @vchNombre,
                nvchDescripcion = @nvchDescripcion,
                iIdCategoria = @iIdCategoria,
                bModalidadEnLinea = @bModalidadEnLinea,
                mCosto = @mCosto,
                iIdOficina = @iIdOficina,
                dtFechaActualizacion = GETDATE(),
                bActivo = @bActivo
            WHERE iIdTramite = @iIdTramite;
        END

        IF @trancount = 0
            COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        SELECT @vchError = CONVERT(VARCHAR(500), CONCAT('spSaveTramite: ', ERROR_MESSAGE(), ' in ', ERROR_PROCEDURE(), ' at line ', ERROR_LINE()));
        IF @trancount = 0
            ROLLBACK TRANSACTION;
    END CATCH

    IF LEN(@vchError) > 0
    BEGIN
        INSERT INTO @Result (bResult, vchMessage) VALUES (0, @vchError);
    END
    ELSE
    BEGIN
        INSERT INTO @Result (bResult, vchMessage, iIdTramite) VALUES (1, '', @iIdTramite);
    END

    SELECT * FROM @Result;
END
GO
