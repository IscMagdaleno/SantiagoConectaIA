ALTER PROCEDURE spGetModulo (
@iIdPlanTrabajo INT 
)
AS 
BEGIN 


CREATE TABLE #Result (
	bResult BIT DEFAULT (1),
	vchMessage VARCHAR(500) DEFAULT(''),
	 iIdModulo INT DEFAULT( -1 ),
	 iIdPlanTrabajo INT DEFAULT( -1 ),
	 vchTitulo VARCHAR (100)  DEFAULT( '' ),
	 nvchProposito NVARCHAR (2000)  DEFAULT( '' ),
);

SET NOCOUNT ON

	BEGIN TRY

	INSERT INTO  #Result
	 ( 

		iIdModulo, 			iIdPlanTrabajo, 			vchTitulo, 	
		nvchProposito 			)
		SELECT 
		 M.iIdModulo, 			 M.iIdPlanTrabajo, 			 M.vchTitulo, 	
				 M.nvchProposito 	FROM
		 dbo.Modulo M  WITH(NOLOCK)  
     where iIdPlanTrabajo =@iIdPlanTrabajo;


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
