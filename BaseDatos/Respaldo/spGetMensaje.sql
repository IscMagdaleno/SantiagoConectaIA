ALTER PROCEDURE spGetMensaje (
@iIdChat INT 
) 
AS 
BEGIN 


CREATE TABLE #Result (
	bResult BIT DEFAULT (1),
	vchMessage VARCHAR(500) DEFAULT(''),
	 iIdMensaje INT DEFAULT( -1 ),
	 iIdChat INT DEFAULT( -1 ),
	 iOrden INT DEFAULT( -1 ),	 
   iIdFase INT DEFAULT( -1 ),
	 nvchRol NVARCHAR (40)  DEFAULT( '' ),
	 nvchContenido NVARCHAR (MAX)  DEFAULT( '' ),
	 dtFecha DATETIME DEFAULT( '1900-01-01' ),
);

SET NOCOUNT ON

	BEGIN TRY

	INSERT INTO  #Result
	 ( 

		iIdMensaje, 			iIdChat, 			iOrden, 	iIdFase,
		nvchRol, 			nvchContenido, 			dtFecha 	
		)
		SELECT 
		 M.iIdMensaje, 			 M.iIdChat, 			 M.iOrden, 	M.iIdFase,
				 M.nvchRol, 			 M.nvchContenido, 			 M.dtFecha 	
		FROM
		 dbo.Mensaje M  WITH(NOLOCK)  
     WHERE M.iIdChat =@iIdChat


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
