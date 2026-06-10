ALTER PROCEDURE spGetFuncionalidad (
@iIdModulo INT 
) 
AS 
BEGIN 


CREATE TABLE #Result (
	bResult BIT DEFAULT (1),
	vchMessage VARCHAR(500) DEFAULT(''),
	 iIdFuncionalidad INT DEFAULT( -1 ),
	 iIdModulo INT DEFAULT( -1 ),
	 nvchDescripcion NVARCHAR (2000)  DEFAULT( '' ),
	 nvchEntidades NVARCHAR (800)  DEFAULT( '' ),
	 nvchInteracciones NVARCHAR (2000)  DEFAULT( '' ),
	 nvchTecnico NVARCHAR (4000)  DEFAULT( '' ),
	 nvchConsideraciones NVARCHAR (4000)  DEFAULT( '' ),
);

SET NOCOUNT ON

	BEGIN TRY

	INSERT INTO  #Result
	 ( 

		iIdFuncionalidad, 			iIdModulo, 			nvchDescripcion, 	
		nvchEntidades, 			nvchInteracciones, 			nvchTecnico, 	
		nvchConsideraciones 			)
		SELECT 
		 F.iIdFuncionalidad, 			 F.iIdModulo, 			 F.nvchDescripcion, 	
				 F.nvchEntidades, 			 F.nvchInteracciones, 			 F.nvchTecnico, 	
				 F.nvchConsideraciones 	FROM
		 dbo.Funcionalidad F  WITH(NOLOCK)  
WHERE F.iIdModulo= @iIdModulo

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
