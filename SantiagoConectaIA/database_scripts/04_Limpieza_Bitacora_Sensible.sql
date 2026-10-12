-- ==========================================================================================
-- 04 - LIMPIEZA DE DATOS SENSIBLES EN LA BITACORA DE LA API ([SCIA].[ApiCallLogs])
--
-- Hasta ahora la bitacora guardaba el cuerpo completo de cada peticion y respuesta, incluidos
-- contrasenas de inicio de sesion, tokens JWT y parametros secretos. El API ya no los guarda
-- (ApiLoggingMiddleware). Este script borra lo que ya estaba guardado.
--
-- NO se ejecuta solo: revisa primero el SELECT y luego corre el UPDATE.
-- ==========================================================================================

-- 1. Revision: cuantas filas se van a limpiar.
SELECT COUNT(*) AS FilasPorLimpiar
FROM [SCIA].[ApiCallLogs]
WHERE vchEndpoint LIKE '/api/Auth/%'
   OR vchEndpoint LIKE '/api/Ciudadano/%'
   OR vchEndpoint LIKE '/api/Catalogos/PostGetParametro%'
   OR vchEndpoint LIKE '/api/Publicaciones/%'
   OR vchEndpoint LIKE '/api/WhatsApp/webhook%'
   OR nvchResponseBody LIKE '%eyJ%';        -- cualquier respuesta que contenga un JWT

-- 2. Limpieza: se conserva la fila (endpoint, fecha, duracion, exito) y se quita solo el contenido.
UPDATE [SCIA].[ApiCallLogs]
SET nvchRequestBody  = N'[omitido por seguridad]',
    nvchResponseBody = N'[omitido por seguridad]'
WHERE vchEndpoint LIKE '/api/Auth/%'
   OR vchEndpoint LIKE '/api/Ciudadano/%'
   OR vchEndpoint LIKE '/api/Catalogos/PostGetParametro%'
   OR vchEndpoint LIKE '/api/Publicaciones/%'
   OR vchEndpoint LIKE '/api/WhatsApp/webhook%'
   OR nvchResponseBody LIKE '%eyJ%';

-- 3. Opcional: las contrasenas ya estuvieron guardadas. Si alguien con acceso a la base pudo verlas,
--    conviene cambiar la contrasena de los usuarios administradores.
