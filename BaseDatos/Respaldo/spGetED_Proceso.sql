ALTER PROCEDURE spGetED_Proceso
AS
BEGIN
	CREATE TABLE #Result (
		bResult              BIT DEFAULT (1),
		vchMessage           VARCHAR( 500 ) DEFAULT(''),
		iIdED_Proceso        INT DEFAULT( -1 ),
		vchTitle             VARCHAR ( 50 ) DEFAULT( '' ),
		vchDescripcion       VARCHAR ( 500 ) DEFAULT( '' ),
		vchDescripcionIngles VARCHAR ( 500 ) DEFAULT( '' ),
		vchImage VARCHAR ( 500 ) DEFAULT( '' )
	);

	SET NOCOUNT ON

	BEGIN TRY
		INSERT INTO #Result
		(
			iIdED_Proceso,vchTitle,vchDescripcion,
			vchDescripcionIngles,vchImage
		)
		SELECT iIdED_Proceso,EDP.vchTitle,EDP.vchDescripcion,
			   EDP.vchDescripcionIngles,EDP.vchImage
		FROM
			dbo.ED_Proceso EDP WITH(NOLOCK)

		IF NOT EXISTS ( SELECT 1
						FROM
							#Result )
		BEGIN
			INSERT INTO #Result
			(
				bResult,vchMessage
			)
			SELECT 0,'No register found.';

		END

	END TRY
	BEGIN CATCH
		INSERT INTO #Result
		(
			bResult,vchMessage
		)
		SELECT 0,CONCAT( ERROR_PROCEDURE( ), ' : ', ERROR_MESSAGE( ), ' - ', ERROR_LINE( ) );

		PRINT CONCAT( ERROR_PROCEDURE( ), ' : ', ERROR_MESSAGE( ), ' - ', ERROR_LINE( ) );

	END CATCH

	SELECT *
	FROM
		#Result;

	DROP TABLE #Result;

END
