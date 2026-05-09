ALTER PROCEDURE spGetPlanTrabajo (
@iIdPlanTrabajo INT 
) 
AS 
BEGIN 


CREATE TABLE #Result (
	bResult BIT DEFAULT (1),
	vchMessage VARCHAR(500) DEFAULT(''),
	 iIdPlanTrabajo INT DEFAULT( -1 ),
	 vchNombre VARCHAR (100)  DEFAULT( '' ),
	 nvchDescripcion NVARCHAR (4000)  DEFAULT( '' ),
	 vchEstatus VARCHAR (50)  DEFAULT( '' ),
);

SET NOCOUNT ON

	BEGIN TRY

	INSERT INTO  #Result
	 ( 

		iIdPlanTrabajo, 			vchNombre, 			nvchDescripcion, 	
		vchEstatus 			)
		SELECT 
		 PT.iIdPlanTrabajo, 			 PT.vchNombre, 			 PT.nvchDescripcion, 	
				 PT.vchEstatus 	FROM
		 dbo.PlanTrabajo PT  WITH(NOLOCK)  
     WHERE @iIdPlanTrabajo = iIdPlanTrabajo or @iIdPlanTrabajo <=  0

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
