ALTER PROCEDURE spGetProyecto (
@iIdProyecto INT 
) 
AS 
BEGIN 


CREATE TABLE #Result (
	bResult BIT DEFAULT (1),
	vchMessage VARCHAR(500) DEFAULT(''),
	 iIdProyecto INT DEFAULT( -1 ),
	 nvchNombre NVARCHAR (510)  DEFAULT( '' ),
	 nvchDescripcion NVARCHAR (MAX)  DEFAULT( '' ),
	 dtCreadoEn DATETIME DEFAULT( '1900-01-01' ),
	 dtActualizadoEn DATETIME DEFAULT( '1900-01-01' ),
	 iIdPlanTrabajo INT DEFAULT( -1 ),
);

SET NOCOUNT ON

	BEGIN TRY

	INSERT INTO  #Result
	 ( 

		iIdProyecto, 			nvchNombre, 			nvchDescripcion, 	
		dtCreadoEn, 			dtActualizadoEn, 			iIdPlanTrabajo 	
		)
		SELECT 
		 P.iIdProyecto, 			 P.nvchNombre, 			 P.nvchDescripcion, 	
				 P.dtCreadoEn, 			 P.dtActualizadoEn, 			 P.iIdPlanTrabajo 	
		FROM
		 dbo.Proyecto P  WITH(NOLOCK)  
    WHERE P.iIdProyecto = @iIdProyecto OR @iIdProyecto <= 0

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
