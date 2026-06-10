ALTER PROCEDURE spGetFase (
@iIdProyecto INT 
) 
AS 
BEGIN 


CREATE TABLE #Result (
	bResult BIT DEFAULT (1),
	vchMessage VARCHAR(500) DEFAULT(''),
	 iIdFase INT DEFAULT( -1 ),
	 iIdProyecto INT DEFAULT( -1 ),
	 smNumeroSecuencia SMALLINT DEFAULT( -1 ),
	 nvchTitulo NVARCHAR (510)  DEFAULT( '' ),
	 nvchDescripcion NVARCHAR (MAX)  DEFAULT( '' ),
	 dtCreadoEn DATETIME DEFAULT( '1900-01-01' ),
	 dtActualizadoEn DATETIME DEFAULT( '1900-01-01' ),
);

SET NOCOUNT ON

	BEGIN TRY

	INSERT INTO  #Result
	 ( 

		iIdFase, 			iIdProyecto, 			smNumeroSecuencia, 	
		nvchTitulo, 			nvchDescripcion, 			dtCreadoEn, 	
		dtActualizadoEn 			)
		SELECT 
		 F.iIdFase, 			 F.iIdProyecto, 			 F.smNumeroSecuencia, 	
				 F.nvchTitulo, 			 F.nvchDescripcion, 			 F.dtCreadoEn, 	
				 F.dtActualizadoEn 	FROM
		 dbo.Fase F  WITH(NOLOCK)  
WHERE iIdProyecto = @iIdProyecto

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
