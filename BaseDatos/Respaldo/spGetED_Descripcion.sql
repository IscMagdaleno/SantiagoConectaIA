ALTER PROCEDURE spGetED_Descripcion(
	@iIdED_Proceso INT
)
AS
BEGIN
	CREATE TABLE #Result (
		bResult              BIT DEFAULT (1),
		vchMessage           VARCHAR( 500 ) DEFAULT(''),
		iIdED_Descripcion    INT DEFAULT( -1 ),
		iIdED_Proceso        INT DEFAULT( -1 ),
		vchTitulo            VARCHAR ( 200 ) DEFAULT( '' ),
		vchDescripcion       VARCHAR ( 3000 ) DEFAULT( '' ),
		vchDescripcionIngles VARCHAR ( 3000 ) DEFAULT( '' ),
		iIdED_Imagenes       INT DEFAULT ( -1 ),
		vchImagen            VARCHAR ( 500 ) DEFAULT ( '' ),
		vchImagenDescripcion VARCHAR ( 500 ) DEFAULT ( '' ),
		vchAlt               VARCHAR ( 250 ) DEFAULT ( '' ),
		iNumeration    INT DEFAULT ( 0 ),
	);

	SET NOCOUNT ON

	BEGIN TRY
		INSERT INTO #Result
		(
			iIdED_Descripcion,iIdED_Proceso,vchTitulo,
			vchDescripcion,vchDescripcionIngles,iIdED_Imagenes,
			vchImagen,vchImagenDescripcion,vchAlt,iNumeration
		)
		SELECT EDD.iIdED_Descripcion,EDD.iIdED_Proceso,EDD.vchTitulo,
			   EDD.vchDescripcion,EDD.vchDescripcionIngles,EDD.iIdED_Imagenes,
			   EDI.vchImagen,EDI.vchDescripcion,EDI.vchAlt, ROW_NUMBER() OVER (ORDER BY EDD.iIdED_Descripcion) AS iNumeration
		FROM
			dbo.ED_Descripcion EDD WITH(NOLOCK)
			INNER JOIN ED_Imagenes EDI ON EDD.iIdED_Imagenes = EDI.iIdED_Imagenes
		WHERE
			iIdED_Proceso = @iIdED_Proceso;

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
