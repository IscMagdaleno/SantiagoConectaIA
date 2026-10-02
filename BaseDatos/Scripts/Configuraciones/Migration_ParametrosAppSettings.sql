/*
** Migration: configuración de appsettings.json a SCIA.Parametros
** Date: 2026-10-01
**
** Alias                               Valor1                                   Valor2
** ----------------------------------  ---------------------------------------  -----------------------------------
** make.facebook.webhook               URL del webhook de Make (Facebook)       -
** publicaciones.sitio                 URL del sitio                            URL base de noticias
** publicaciones.auto.noticias         Hora diaria (0-23)                       -
** publicaciones.auto.emprendimientos  Hora diaria (0-23)                       ID inicial de la rotación
** azure.blob.storage                  Cadena de conexión de Azure Blob         -
** jwt.secret                          Secreto para firmar JWT                  -
** jwt.config                          Issuer                                   Audience
**
** Solo inserta los alias que no existan; no sobrescribe valores ya capturados.
** La API guarda los parámetros en caché 5 minutos; el secreto JWT y el Issuer/Audience
** también se toman de la caché, así que un cambio aplica a más tardar en 5 minutos.
*/
SET NOCOUNT ON;
GO

DECLARE @Nuevos TABLE
(
    nvchAlias NVARCHAR(200) NOT NULL,
    nvchNombre NVARCHAR(200) NOT NULL,
    nvchNombreEN NVARCHAR(200) NOT NULL,
    nvchDescripcion NVARCHAR(1000) NULL,
    nvchDescripcionEN NVARCHAR(1000) NULL,
    iSecuencia INT NOT NULL,
    nvchValor1 NVARCHAR(MAX) NULL,
    nvchValor2 NVARCHAR(MAX) NULL
);

INSERT INTO @Nuevos (nvchAlias, nvchNombre, nvchNombreEN, nvchDescripcion, nvchDescripcionEN, iSecuencia, nvchValor1, nvchValor2)
VALUES
(N'make.facebook.webhook', N'Webhook de Make para Facebook', N'Make webhook for Facebook',
 N'URL del webhook de Make que publica en Facebook (Valor1)', N'Make webhook URL that publishes to Facebook (Valor1)', 1,
 N'https://hook.us2.make.com/rtkntpqxns7ni0vsbtmmvjxmjh4bv794', NULL),

(N'publicaciones.sitio', N'Sitio de Santiago Conecta', N'Santiago Conecta website',
 N'URL del sitio (Valor1) y URL base de noticias (Valor2) para los enlaces de las publicaciones', N'Website URL (Valor1) and news base URL (Valor2) for post links', 1,
 N'https://www.santiagopapasquiaro.com.mx', N'https://www.santiagopapasquiaro.com.mx/noticias'),

(N'publicaciones.auto.noticias', N'Publicación automática de noticias', N'Automatic news posting',
 N'Hora diaria 0-23, hora de Ciudad de México (Valor1)', N'Daily hour 0-23, Mexico City time (Valor1)', 2,
 N'11', NULL),

(N'publicaciones.auto.emprendimientos', N'Publicación automática de emprendimientos', N'Automatic business posting',
 N'Hora diaria 0-23, hora de Ciudad de México (Valor1) e ID del emprendimiento inicial de la rotación (Valor2)', N'Daily hour 0-23, Mexico City time (Valor1) and starting business ID of the rotation (Valor2)', 3,
 N'13', N'24'),

(N'azure.blob.storage', N'Azure Blob Storage', N'Azure Blob Storage',
 N'Cadena de conexión de Azure Blob Storage (Valor1)', N'Azure Blob Storage connection string (Valor1)', 1,
 N'DefaultEndpointsProtocol=https;AccountName=santiagoconectaia;AccountKey=XQ5YsugDu7uEKdOUZDwwGSC1/4+WmgjSElB2hhEf/4VyhwKksRMFItMI20bF0SIQ6ZfyQoVpyyq5+ASthbfO0g==;EndpointSuffix=core.windows.net', NULL),

(N'jwt.secret', N'Secreto JWT', N'JWT secret',
 N'Secreto para firmar y validar los JWT (Valor1)', N'Secret used to sign and validate JWTs (Valor1)', 1,
 N'SuperSecretKeyForSantiagoConectaIA123!@#', NULL),

(N'jwt.config', N'Configuración JWT', N'JWT configuration',
 N'Issuer (Valor1) y Audience (Valor2) de los JWT', N'JWT Issuer (Valor1) and Audience (Valor2)', 2,
 N'SantiagoConectaIA', N'SantiagoConectaIA');

BEGIN TRAN;

IF COLUMNPROPERTY(OBJECT_ID('SCIA.Parametros'), 'iIdParametro', 'IsIdentity') = 1
BEGIN
    INSERT INTO SCIA.Parametros (iIdParametroPadre, nvchAlias, nvchNombre, nvchNombreEN, nvchDescripcion, nvchDescripcionEN, iSecuencia, bTieneValores, nvchValor1, nvchValor2, bHabilitado)
    SELECT NULL, N.nvchAlias, N.nvchNombre, N.nvchNombreEN, N.nvchDescripcion, N.nvchDescripcionEN, N.iSecuencia, 1, N.nvchValor1, N.nvchValor2, 1
    FROM @Nuevos N
    WHERE NOT EXISTS (SELECT 1 FROM SCIA.Parametros P WITH (UPDLOCK, HOLDLOCK) WHERE P.nvchAlias = N.nvchAlias);
END
ELSE
BEGIN
    DECLARE @MaxId INT = (SELECT ISNULL(MAX(iIdParametro), 0) FROM SCIA.Parametros WITH (UPDLOCK, HOLDLOCK));

    INSERT INTO SCIA.Parametros (iIdParametro, iIdParametroPadre, nvchAlias, nvchNombre, nvchNombreEN, nvchDescripcion, nvchDescripcionEN, iSecuencia, bTieneValores, nvchValor1, nvchValor2, bHabilitado)
    SELECT @MaxId + ROW_NUMBER() OVER (ORDER BY N.nvchAlias), NULL, N.nvchAlias, N.nvchNombre, N.nvchNombreEN, N.nvchDescripcion, N.nvchDescripcionEN, N.iSecuencia, 1, N.nvchValor1, N.nvchValor2, 1
    FROM @Nuevos N
    WHERE NOT EXISTS (SELECT 1 FROM SCIA.Parametros P WHERE P.nvchAlias = N.nvchAlias);
END

COMMIT TRAN;
GO

SELECT iIdParametro, nvchAlias, nvchNombre, bHabilitado
FROM SCIA.Parametros
WHERE nvchAlias IN (N'make.facebook.webhook', N'publicaciones.sitio', N'publicaciones.auto.noticias',
                    N'publicaciones.auto.emprendimientos', N'azure.blob.storage', N'jwt.secret', N'jwt.config');
GO
