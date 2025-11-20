IF OBJECT_ID( 'spSaveDocumento' ) IS NULL
	EXEC ('CREATE PROCEDURE spSaveDocumento AS SET NOCOUNT ON;') 
GO 
ALTER PROCEDURE spSaveDocumento (
@iIdDocumento INT, 
@iIdTramite INT, 
@vchNombre VARCHAR (250) , 
@vchUrlDocumento VARCHAR (500) , 
@bActivo BIT 
) 
AS 
BEGIN 

DECLARE @vchError VARCHAR(200) = '';

DECLARE @Result AS TABLE (
	bResult BIT DEFAULT(1),
	vchMessage VARCHAR(500) DEFAULT(''),
	iIdDocumento INT DEFAULT( -1 ) 
	);

SET NOCOUNT ON
SET XACT_ABORT ON;

BEGIN TRY

BEGIN TRANSACTION

IF  ( @iIdDocumento <= 0) 
BEGIN 
	INSERT INTO dbo.Documento
	 ( 

		iIdTramite, 			vchNombre, 			vchUrl, 	
		bActivo 	
	)
	VALUES 
	(
		@iIdTramite,		@vchNombre,		@vchUrlDocumento,
		@bActivo
	)
		 SET @iIdDocumento = @@IDENTITY
END
ELSE
BEGIN
	UPDATE  dbo.Documento WITH(ROWLOCK) SET
		 iIdTramite = @iIdTramite, 
		 vchNombre = @vchNombre, 
		 vchUrl = @vchUrlDocumento, 
		 bActivo = @bActivo 

	 WHERE  iIdDocumento  = @iIdDocumento;

END
		COMMIT TRANSACTION ;
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION ;

		SELECT @vchError = CONCAT( 'spSaveDocumento: ', ERROR_MESSAGE( ), ' ', ERROR_PROCEDURE( ), ' ', ERROR_LINE( ) );
		PRINT @vchError;
	END CATCH

	IF LEN( @vchError ) > 0
	BEGIN
		INSERT INTO @Result
		(
			bResult,vchMessage
		)
		VALUES
		(
			0,@vchError
		);
	END
	ELSE
	BEGIN
		INSERT INTO @Result
		(
			bResult,vchMessage,iIdDocumento
		)
		VALUES
		(
			1,'',@iIdDocumento
		);
	END;

	SELECT *
	FROM
		@Result;
	SET NOCOUNT OFF;
END;
GO 


