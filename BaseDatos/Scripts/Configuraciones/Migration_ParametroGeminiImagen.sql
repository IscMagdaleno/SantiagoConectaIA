/*
** Migration: modelo de Gemini para editar imágenes (dbo.Parametros)
** Date: 2026-10-01
**
** Alias           Valor1                                   Valor2
** --------------  ---------------------------------------  ------
** gemini.imagen   Modelo de Gemini para editar imágenes    -
**
** La API key se toma de key.gemini (Valor2). Si el alias no existe, la API usa gemini-3.1-flash-image.
** Solo inserta el alias si no existe; no sobrescribe un valor ya capturado.
*/
SET NOCOUNT ON;
GO

BEGIN TRAN;

IF NOT EXISTS (SELECT 1 FROM dbo.Parametros WITH (UPDLOCK, HOLDLOCK) WHERE nvchAlias = N'gemini.imagen')
BEGIN
    IF COLUMNPROPERTY(OBJECT_ID('dbo.Parametros'), 'iIdParametro', 'IsIdentity') = 1
    BEGIN
        INSERT INTO dbo.Parametros (iIdParametroPadre, nvchAlias, nvchNombre, nvchNombreEN, nvchDescripcion, nvchDescripcionEN, iSecuencia, bTieneValores, nvchValor1, nvchValor2, bHabilitado)
        VALUES (NULL, N'gemini.imagen', N'Modelo de Gemini para imágenes', N'Gemini image model',
                N'Modelo de Gemini que edita las imágenes de las publicaciones (Valor1)', N'Gemini model used to edit post images (Valor1)',
                2, 1, N'gemini-3.1-flash-image', NULL, 1);
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Parametros (iIdParametro, iIdParametroPadre, nvchAlias, nvchNombre, nvchNombreEN, nvchDescripcion, nvchDescripcionEN, iSecuencia, bTieneValores, nvchValor1, nvchValor2, bHabilitado)
        SELECT ISNULL(MAX(iIdParametro), 0) + 1, NULL, N'gemini.imagen', N'Modelo de Gemini para imágenes', N'Gemini image model',
               N'Modelo de Gemini que edita las imágenes de las publicaciones (Valor1)', N'Gemini model used to edit post images (Valor1)',
               2, 1, N'gemini-3.1-flash-image', NULL, 1
        FROM dbo.Parametros WITH (UPDLOCK, HOLDLOCK);
    END
END

COMMIT TRAN;
GO

SELECT iIdParametro, nvchAlias, nvchValor1, bHabilitado
FROM dbo.Parametros
WHERE nvchAlias = N'gemini.imagen';
GO
