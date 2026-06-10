ALTER PROCEDURE spGetPaso (
@iIdFase INT 
) 
AS 
BEGIN 


CREATE TABLE #Result (
	bResult BIT DEFAULT (1),
	vchMessage VARCHAR(500) DEFAULT(''),
	 iIdPaso INT DEFAULT( -1 ),
	 iIdFase INT DEFAULT( -1 ),
	 smNumeroSecuencia SMALLINT DEFAULT( -1 ),
	 nvchDescripcion NVARCHAR (MAX)  DEFAULT( '' ),
	 nvchProposito NVARCHAR (MAX)  DEFAULT( '' ),
	 nvchCaracteristicas NVARCHAR (MAX)  DEFAULT( '' ),
	 nvchEnfoque NVARCHAR (MAX)  DEFAULT( '' ),
	 dtCreadoEn DATETIME DEFAULT( '1900-01-01' ),
	 dtActualizadoEn DATETIME DEFAULT( '1900-01-01' ),
);

SET NOCOUNT ON

	BEGIN TRY

	INSERT INTO  #Result
	 ( 

		iIdPaso, 			iIdFase, 			smNumeroSecuencia, 	
		nvchDescripcion, 			nvchProposito, 			nvchCaracteristicas, 	
		nvchEnfoque, 			dtCreadoEn, 			dtActualizadoEn 	
		)
		SELECT 
		 P.iIdPaso, 			 P.iIdFase, 			 P.smNumeroSecuencia, 	
				 P.nvchDescripcion, 			 P.nvchProposito, 			 P.nvchCaracteristicas, 	
				 P.nvchEnfoque, 			 P.dtCreadoEn, 			 P.dtActualizadoEn 	
		FROM
		 dbo.Paso P  WITH(NOLOCK)  
WHERE iIdFase =@iIdFase

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
