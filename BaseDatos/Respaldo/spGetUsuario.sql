ALTER PROCEDURE spGetUsuario 
AS 
BEGIN 


CREATE TABLE #Result (
	bResult BIT DEFAULT (1),
	vchMessage VARCHAR(500) DEFAULT(''),
	 iIdUsuario INT DEFAULT( -1 ),
	 nvchNombre NVARCHAR (200)  DEFAULT( '' ),
	 nvchCorreo NVARCHAR (510)  DEFAULT( '' ),
	 vchTelefono VARCHAR (15)  DEFAULT( '' ),
	 dtFechaRegistro DATETIME DEFAULT( '1900-01-01' ),
	 bActivo BIT DEFAULT( 0 ),
);

SET NOCOUNT ON

	BEGIN TRY

	INSERT INTO  #Result
	 ( 

		iIdUsuario, 			nvchNombre, 			nvchCorreo, 	
		 			vchTelefono, 			dtFechaRegistro, 	
		bActivo 			)
		SELECT 
		 U.iIdUsuario, 			 U.nvchNombre, 			 U.nvchCorreo, 	
							 U.vchTelefono, 			 U.dtFechaRegistro, 	
				 U.bActivo 	FROM
		 dbo.Usuario U  WITH(NOLOCK)  


		IF NOT EXISTS (SELECT 1 FROM #Result)
			BEGIN
				INSERT INTO #Result (bResult, vchMessage)
				SELECT 0, 'No register found.';
			END
	END TRY
	BEGIN CATCH
		INSERT INTO #Result (bResult, vchMessage)
		SELECT 0, CONCAT(ERROR_PROCEDURE(), ' : ', ERROR_MESSAGE(), ' - ', ERROR_LINE());
		PRINT CONCAT(ERROR_PROCEDURE(), ' : ', ERROR_MESSAGE(), ' - ', ERROR_LINE());
	END CATCH
	SELECT * FROM #Result;
	DROP TABLE #Result;
	END
