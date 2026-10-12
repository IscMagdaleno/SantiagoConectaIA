-- seed_inserts.sql
SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- 1) Roles (idempotente)
IF NOT EXISTS (SELECT 1 FROM SCIA.Rol WHERE vchNombre = 'Admin')
BEGIN
    INSERT INTO SCIA.Rol (vchNombre, vchDescripcion, bActivo)
    VALUES ('Admin', N'Rol de administradores con permisos totales', 1);
END

IF NOT EXISTS (SELECT 1 FROM SCIA.Rol WHERE vchNombre = 'Ciudadano')
BEGIN
    INSERT INTO SCIA.Rol (vchNombre, vchDescripcion, bActivo)
    VALUES ('Ciudadano', N'Rol de usuarios ciudadanos (consulta)', 1);
END

-- 2) Usuarios (hash de password con SHA2_256) 
-- Ajusta si tu columna vchPass es VARBINARY o VARCHAR según tu esquema.
IF NOT EXISTS (SELECT 1 FROM SCIA.Usuario WHERE vchEmail = 'admin@santiagopapasquiaro.mx')
BEGIN
    INSERT INTO SCIA.Usuario (iIdRol, vchNombre, vchEmail, vchPass, vchNickName, dtFechaRegistro, bActivo)
    VALUES (
        (SELECT iIdRol FROM SCIA.Rol WHERE vchNombre = 'Admin'),
        'Administrador Principal',
        'admin@santiagopapasquiaro.mx',
        HASHBYTES('SHA2_256', 'Admin@123!'), -- si usas VARBINARY
        'admin',
        GETDATE(),
        1
    );
END

IF NOT EXISTS (SELECT 1 FROM SCIA.Usuario WHERE vchEmail = 'ciudadano@example.com')
BEGIN
    INSERT INTO SCIA.Usuario (iIdRol, vchNombre, vchEmail, vchPass, vchNickName, dtFechaRegistro, bActivo)
    VALUES (
        (SELECT iIdRol FROM SCIA.Rol WHERE vchNombre = 'Ciudadano'),
        'Usuario Ejemplo',
        'ciudadano@example.com',
        HASHBYTES('SHA2_256', 'Usuario123!'),
        'usuario1',
        GETDATE(),
        1
    );
END

-- 3) Dependencias
IF NOT EXISTS (SELECT 1 FROM SCIA.Dependencia WHERE vchNombre = 'Ayuntamiento de Santiago Papasquiaro')
BEGIN
    INSERT INTO SCIA.Dependencia (vchNombre, nvchDescripcion, vchUrlOficial, bActivo, dtFechaCreacion)
    VALUES (
        'Ayuntamiento de Santiago Papasquiaro',
        N'Oficina principal del municipio. Atiende trámites municipales.',
        'https://santiagopapasquiaro.gob.mx',
        1,
        GETDATE()
    );
END

IF NOT EXISTS (SELECT 1 FROM SCIA.Dependencia WHERE vchNombre = 'Registro Civil')
BEGIN
    INSERT INTO SCIA.Dependencia (vchNombre, nvchDescripcion, vchUrlOficial, bActivo, dtFechaCreacion)
    VALUES (
        'Registro Civil',
        N'Registro Civil - actas y certificaciones.',
        'https://santiagopapasquiaro.gob.mx/registro-civil',
        1,
        GETDATE()
    );
END

IF NOT EXISTS (SELECT 1 FROM SCIA.Dependencia WHERE vchNombre = 'Catastro')
BEGIN
    INSERT INTO SCIA.Dependencia (vchNombre, nvchDescripcion, vchUrlOficial, bActivo, dtFechaCreacion)
    VALUES (
        'Catastro',
        N'Departamento de catastro y predial.',
        'https://santiagopapasquiaro.gob.mx/catastro',
        1,
        GETDATE()
    );
END

-- 4) Oficinas (referenciar dependencia)
-- Oficina principal del ayuntamiento
IF NOT EXISTS (SELECT 1 FROM SCIA.Oficina WHERE vchNombre = 'Oficina Central - Ayuntamiento')
BEGIN
    INSERT INTO SCIA.Oficina (iIdDependencia, vchNombre, vchDireccion, vchTelefono, vchEmail, vchHorario, flLatitud, flLongitud, vchNotas, bActivo, dtFechaCreacion)
    VALUES (
        (SELECT iIdDependencia FROM SCIA.Dependencia WHERE vchNombre = 'Ayuntamiento de Santiago Papasquiaro'),
        'Oficina Central - Ayuntamiento',
        'Plaza Principal s/n, Centro, Santiago Papasquiaro, Durango',
        '618-000-0000',
        'contacto@santiagopapasquiaro.gob.mx',
        N'Lunes a Viernes 09:00 - 15:00',
        25.9880, -105.1160,
        N'Atención general y ventanilla única',
        1,
        GETDATE()
    );
END

-- Registro Civil
IF NOT EXISTS (SELECT 1 FROM SCIA.Oficina WHERE vchNombre = 'Oficina Registro Civil - Centro')
BEGIN
    INSERT INTO SCIA.Oficina (iIdDependencia, vchNombre, vchDireccion, vchTelefono, vchEmail, vchHorario, flLatitud, flLongitud, vchNotas, bActivo, dtFechaCreacion)
    VALUES (
        (SELECT iIdDependencia FROM SCIA.Dependencia WHERE vchNombre = 'Registro Civil'),
        'Oficina Registro Civil - Centro',
        'Calle Libertad 123, Centro, Santiago Papasquiaro',
        '618-000-0001',
        'registrocivil@santiagopapasquiaro.gob.mx',
        N'Lunes a Viernes 09:00 - 14:00',
        25.9875, -105.1165,
        N'Actas, certificaciones y citas.',
        1,
        GETDATE()
    );
END

-- Catastro
IF NOT EXISTS (SELECT 1 FROM SCIA.Oficina WHERE vchNombre = 'Oficina Catastro')
BEGIN
    INSERT INTO SCIA.Oficina (iIdDependencia, vchNombre, vchDireccion, vchTelefono, vchEmail, vchHorario, flLatitud, flLongitud, vchNotas, bActivo, dtFechaCreacion)
    VALUES (
        (SELECT iIdDependencia FROM SCIA.Dependencia WHERE vchNombre = 'Catastro'),
        'Oficina Catastro',
        'Calle Reforma 45, Santiago Papasquiaro',
        '618-000-0002',
        'catastro@santiagopapasquiaro.gob.mx',
        N'Lunes a Viernes 09:00 - 14:00',
        25.9868, -105.1170,
        N'Trámites prediales y consulta catastral.',
        1,
        GETDATE()
    );
END

-- 5) Trámites (ejemplos)
IF NOT EXISTS (SELECT 1 FROM SCIA.Tramite WHERE vchNombre = 'Expedición de acta de nacimiento')
BEGIN
    INSERT INTO SCIA.Tramite (vchNombre, nvchDescripcion, iIdCategoria, bModalidadEnLinea, mCosto, iIdOficina, dtFechaCreacion, bActivo)
    VALUES (
        'Expedición de acta de nacimiento',
        N'Obtención de copia certificada de acta de nacimiento. Requisitos: identificación oficial, CURP, pago de derechos.',
        1,
        0,
        100.00,
        (SELECT TOP 1 iIdOficina FROM SCIA.Oficina WHERE vchNombre LIKE '%Registro Civil%'),
        GETDATE(),
        1
    );
END

IF NOT EXISTS (SELECT 1 FROM SCIA.Tramite WHERE vchNombre = 'Pago de impuesto predial')
BEGIN
    INSERT INTO SCIA.Tramite (vchNombre, nvchDescripcion, iIdCategoria, bModalidadEnLinea, mCosto, iIdOficina, dtFechaCreacion, bActivo)
    VALUES (
        'Pago de impuesto predial',
        N'Pago anual de impuesto predial. Requisitos: identificación y número de cuenta predial.',
        2,
        1,
        0.00,
        (SELECT TOP 1 iIdOficina FROM SCIA.Oficina WHERE vchNombre LIKE '%Catastro%'),
        GETDATE(),
        1
    );
END

-- 6) Requisitos (para los trámites insertados)
DECLARE @IdTramActa INT = (SELECT TOP 1 iIdTramite FROM SCIA.Tramite WHERE vchNombre = 'Expedición de acta de nacimiento');
IF @IdTramActa IS NOT NULL AND NOT EXISTS (SELECT 1 FROM SCIA.Requisito WHERE iIdTramite = @IdTramActa AND vchNombre = 'Identificación oficial')
BEGIN
    INSERT INTO SCIA.Requisito (iIdTramite, vchNombre, nvchDetalle, bObligatorio, bActivo)
    VALUES (@IdTramActa, 'Identificación oficial', N'INE o pasaporte vigente', 1, 1);
END

IF @IdTramActa IS NOT NULL AND NOT EXISTS (SELECT 1 FROM SCIA.Requisito WHERE iIdTramite = @IdTramActa AND vchNombre = 'CURP')
BEGIN
    INSERT INTO SCIA.Requisito (iIdTramite, vchNombre, nvchDetalle, bObligatorio, bActivo)
    VALUES (@IdTramActa, 'CURP', N'Presentar CURP impreso o digital', 1, 1);
END

-- 7) Documentos (ejemplo enlaces)
IF @IdTramActa IS NOT NULL AND NOT EXISTS (SELECT 1 FROM SCIA.Documento WHERE iIdTramite = @IdTramActa AND vchNombre = 'Formato de Solicitud')
BEGIN
    INSERT INTO SCIA.Documento (iIdTramite, vchNombre, vchUrl, bActivo)
    VALUES (@IdTramActa, 'Formato de Solicitud', 'https://santiagopapasquiaro.gob.mx/docs/solicitud-acta.pdf', 1);
END

-- 8) OficinaTramite (link trámites <-> oficinas)
-- Asociar Acta de Nacimiento a Oficina Registro Civil
DECLARE @IdOfiRegistro INT = (SELECT TOP 1 iIdOficina FROM SCIA.Oficina WHERE vchNombre LIKE '%Registro Civil%');
IF @IdTramActa IS NOT NULL AND @IdOfiRegistro IS NOT NULL AND NOT EXISTS (SELECT 1 FROM SCIA.OficinaTramite WHERE iIdOficina = @IdOfiRegistro AND iIdTramite = @IdTramActa)
BEGIN
    INSERT INTO SCIA.OficinaTramite (iIdOficina, iIdTramite, vchObservacion, bActivo, dtFechaCreacion)
    VALUES (@IdOfiRegistro, @IdTramActa, N'Oficina para trámites de registro civil', 1, GETDATE());
END

-- Asociar Predial a Oficina Catastro
DECLARE @IdTramPredial INT = (SELECT TOP 1 iIdTramite FROM SCIA.Tramite WHERE vchNombre = 'Pago de impuesto predial');
DECLARE @IdOfiCatastro INT = (SELECT TOP 1 iIdOficina FROM SCIA.Oficina WHERE vchNombre LIKE '%Catastro%');
IF @IdTramPredial IS NOT NULL AND @IdOfiCatastro IS NOT NULL AND NOT EXISTS (SELECT 1 FROM SCIA.OficinaTramite WHERE iIdOficina = @IdOfiCatastro AND iIdTramite = @IdTramPredial)
BEGIN
    INSERT INTO SCIA.OficinaTramite (iIdOficina, iIdTramite, vchObservacion, bActivo, dtFechaCreacion)
    VALUES (@IdOfiCatastro, @IdTramPredial, N'Pago de predial en Catastro', 1, GETDATE());
END

-- 9) Avisos / FAQ
IF NOT EXISTS (SELECT 1 FROM SCIA.Aviso WHERE vchTitulo = 'Cierre por mantenimiento')
BEGIN
    INSERT INTO SCIA.Aviso (vchTitulo, nvchContenido, dtFechaPublicacion, bActivo)
    VALUES ('Cierre por mantenimiento', N'El portal municipal estará en mantenimiento el próximo lunes de 08:00 a 12:00.', GETDATE(), 1);
END

IF NOT EXISTS (SELECT 1 FROM SCIA.FAQ WHERE vchPregunta LIKE '%¿Cómo obtengo mi acta de nacimiento?%')
BEGIN
    INSERT INTO SCIA.FAQ (vchPregunta, nvchRespuesta, bActivo)
    VALUES ('¿Cómo obtengo mi acta de nacimiento?', N'Acude a la oficina del Registro Civil o consulta el formato en línea y lleva identificación oficial.', 1);
END

-- 10) HistorialConsulta (ejemplo de consulta conversacional)
IF NOT EXISTS (SELECT 1 FROM SCIA.HistorialConsulta WHERE nvchPregunta LIKE '%¿Dónde puedo tramitar mi acta?%' AND iIdUsuario IS NOT NULL)
BEGIN
    INSERT INTO SCIA.HistorialConsulta (iIdUsuario, nvchPregunta, nvchRespuesta, dtFecha, vchOrigen)
    VALUES (
        (SELECT iIdUsuario FROM SCIA.Usuario WHERE vchEmail = 'ciudadano@example.com'),
        N'¿Dónde puedo tramitar mi acta?',
        N'En la Oficina Registro Civil - Centro. Horario: Lunes a Viernes 09:00 - 14:00.',
        GETDATE(),
        'web'
    );
END

-- 11) LogCambio (registro de ejemplo de una acción administrativa)
IF NOT EXISTS (SELECT 1 FROM SCIA.LogCambio WHERE vchEntidad = 'Oficina' AND vchAccion = 'Insert' AND vchDetalle LIKE '%Oficina Central%')
BEGIN
    INSERT INTO SCIA.LogCambio (iIdUsuario, vchEntidad, iIdRegistro, vchAccion, nvchDetalle, dtFecha)
    VALUES (
        (SELECT iIdUsuario FROM SCIA.Usuario WHERE vchEmail = 'admin@santiagopapasquiaro.mx'),
        'Oficina',
        (SELECT iIdOficina FROM SCIA.Oficina WHERE vchNombre = 'Oficina Central - Ayuntamiento'),
        'Insert',
        N'Creación de Oficina Central - Ayuntamiento durante seed inicial.',
        GETDATE()
    );
END

COMMIT TRANSACTION;

-- seed_rollback.sql
SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- Borrar logs y historiales de seed
DELETE FROM SCIA.LogCambio
WHERE nvchDetalle LIKE '%seed inicial%' OR (vchEntidad = 'Oficina' AND vchAccion = 'Insert' AND dtFecha >= DATEADD(day,-30,GETDATE()));

DELETE FROM SCIA.HistorialConsulta
WHERE nvchPregunta LIKE '%¿Dónde puedo tramitar mi acta?%' OR dtFecha >= DATEADD(day,-30,GETDATE());

-- Borrar avisos y FAQ insertados
DELETE FROM SCIA.Aviso WHERE vchTitulo = 'Cierre por mantenimiento';
DELETE FROM SCIA.FAQ WHERE vchPregunta LIKE '%¿Cómo obtengo mi acta de nacimiento?%';

-- Borrar relaciones OficinaTramite
DELETE OT
FROM SCIA.OficinaTramite OT
INNER JOIN SCIA.Oficina O ON OT.iIdOficina = O.iIdOficina
WHERE O.vchNombre IN ('Oficina Central - Ayuntamiento', 'Oficina Registro Civil - Centro', 'Oficina Catastro');

-- Borrar Documentos y Requisitos por trámites de ejemplo
DELETE FROM SCIA.Documento WHERE vchUrl LIKE '%solicitud-acta.pdf%';
DELETE FROM SCIA.Requisito WHERE vchNombre IN ('Identificación oficial', 'CURP');

-- Borrar Trámites de ejemplo
DELETE FROM SCIA.Tramite WHERE vchNombre IN ('Expedición de acta de nacimiento', 'Pago de impuesto predial');

-- Borrar Oficinas
DELETE FROM SCIA.Oficina WHERE vchNombre IN ('Oficina Central - Ayuntamiento', 'Oficina Registro Civil - Centro', 'Oficina Catastro');

-- Borrar Dependencias
DELETE FROM SCIA.Dependencia WHERE vchNombre IN ('Ayuntamiento de Santiago Papasquiaro', 'Registro Civil', 'Catastro');

-- Borrar Usuarios seed (usa emails únicos)
DELETE FROM SCIA.Usuario WHERE vchEmail IN ('admin@santiagopapasquiaro.mx','ciudadano@example.com');

-- Borrar Roles seed si no están en uso por otros usuarios
DELETE FROM SCIA.Rol WHERE vchNombre IN ('Admin','Ciudadano') AND NOT EXISTS (SELECT 1 FROM SCIA.Usuario U WHERE U.iIdRol = SCIA.Rol.iIdRol);

COMMIT TRANSACTION;




