-- ===============================================
-- INSERTAR DATOS EN TABLA Tramite
-- ===============================================
INSERT INTO Tramite (vchNombre, nvchDescripcion, iIdCategoria, bModalidadEnLinea, mCosto, iIdOficina)
VALUES
('Licencia de Conducir', 'Trámite para obtener o renovar licencia de conducir.', 1, 0, 450.00, 1),
('Pago de Predial', 'Pago anual del impuesto predial.', 2, 1, 0.00, 2),
('Registro de Propiedad', 'Inscripción de una propiedad ante el registro público.', 3, 0, 1200.00, 3),
('Permiso de Construcción', 'Solicitud de permiso para construcción o remodelación.', 4, 0, 800.00, 4),
('Constancia de Residencia', 'Documento que acredita el domicilio de una persona.', 5, 1, 50.00, 5),
('Cambio de Propietario Vehicular', 'Trámite para cambiar el propietario de un vehículo.', 6, 0, 600.00, 6),
('Acta de Nacimiento', 'Obtención o copia certificada de acta de nacimiento.', 7, 1, 150.00, 7),
('Carta de No Antecedentes Penales', 'Certificación de no tener antecedentes penales.', 8, 1, 200.00, 8),
('Pago de Tenencia', 'Pago anual de la tenencia vehicular.', 9, 1, 0.00, 9),
('Licencia de Funcionamiento', 'Permiso necesario para operar un negocio.', 10, 0, 950.00, 10),
('Registro de Comercio', 'Registro de una empresa ante la autoridad local.', 11, 0, 700.00, 11),
('Permiso para Evento Público', 'Autorización para realizar eventos masivos.', 12, 0, 400.00, 12),
('Baja Vehicular', 'Trámite para dar de baja un vehículo.', 13, 0, 350.00, 13),
('Cambio de Domicilio Fiscal', 'Actualización de domicilio ante la autoridad fiscal.', 14, 1, 0.00, 14),
('Certificado de Libertad de Gravamen', 'Documento que certifica que una propiedad no tiene adeudos.', 15, 0, 500.00, 15);


-- ===============================================
-- INSERTAR DATOS EN TABLA Requisito
-- ===============================================
INSERT INTO Requisito (iIdTramite, vchNombre, nvchDetalle, bObligatorio)
VALUES
(1, 'Identificación Oficial', 'INE o Pasaporte vigente.', 1),
(1, 'Comprobante de domicilio', 'Recibo de luz, agua o teléfono reciente.', 1),
(4, 'Número de cuenta predial', 'Debe estar vigente.', 1),
(4, 'Escritura pública', 'Documento que acredita la propiedad.', 1),
(4, 'Pago de derechos', 'Recibo del pago correspondiente.', 1),
(4, 'Plano de obra', 'Plano autorizado por un arquitecto o ingeniero.', 1),
(5, 'Comprobante de domicilio', 'Debe estar a nombre del solicitante.', 1),
(6, 'Factura del vehículo', 'Documento original de propiedad.', 1),
(7, 'CURP', 'Clave Única de Registro de Población.', 1),
(8, 'Comprobante de pago', 'Recibo de pago del trámite.', 1),
(9, 'Tarjeta de circulación', 'Documento vigente del vehículo.', 1),
(10, 'Constancia de uso de suelo', 'Documento que acredita el tipo de actividad comercial.', 1),
(11, 'RFC de la empresa', 'Registro Federal de Contribuyentes.', 1),
(12, 'Carta de solicitud', 'Documento firmado por el responsable del evento.', 1),
(13, 'Placas del vehículo', 'Debe coincidir con la factura.', 1),
(14, 'RFC actualizado', 'Documento emitido por el SAT.', 1);


-- ===============================================
-- INSERTAR DATOS EN TABLA Documento
-- ===============================================
INSERT INTO Documento (iIdTramite, vchNombre, vchUrl)
VALUES
(1, 'Formato de Solicitud de Licencia', 'https://miportal.gob/licencia_solicitud.pdf'),
(5, 'Comprobante de Pago Predial', 'https://miportal.gob/predial_pago.pdf'),
(5, 'Formato de Registro de Propiedad', 'https://miportal.gob/registro_propiedad.pdf'),
(5, 'Solicitud de Permiso de Construcción', 'https://miportal.gob/permiso_construccion.pdf'),
(5, 'Constancia de Residencia Formato', 'https://miportal.gob/residencia_formato.pdf'),
(6, 'Formato de Cambio de Propietario', 'https://miportal.gob/cambio_propietario.pdf'),
(7, 'Solicitud de Acta de Nacimiento', 'https://miportal.gob/acta_nacimiento.pdf'),
(8, 'Formato de No Antecedentes Penales', 'https://miportal.gob/no_antecedentes.pdf'),
(9, 'Formato de Pago de Tenencia', 'https://miportal.gob/tenencia_formato.pdf'),
(10, 'Licencia de Funcionamiento Formato', 'https://miportal.gob/licencia_funcionamiento.pdf'),
(11, 'Solicitud de Registro de Comercio', 'https://miportal.gob/registro_comercio.pdf'),
(12, 'Formato para Evento Público', 'https://miportal.gob/evento_publico.pdf'),
(13, 'Baja Vehicular Formato', 'https://miportal.gob/baja_vehicular.pdf'),
(14, 'Cambio de Domicilio Fiscal', 'https://miportal.gob/cambio_domicilio.pdf'),
(15, 'Certificado de Libertad de Gravamen', 'https://miportal.gob/libertad_gravamen.pdf');


--------------------------------------------------------------------------------
-- INSERTS EN TABLA: Dependencia
--------------------------------------------------------------------------------
INSERT INTO Dependencia (vchNombre, nvchDescripcion, vchUrlOficial)
VALUES
('Secretaría de Finanzas', 'Encargada de la recaudación de impuestos estatales.', 'https://finanzas.dgo.gob.mx'),
('Secretaría de Seguridad Pública', 'Gestión de seguridad y tránsito.', 'https://ssp.dgo.gob.mx'),
('Dirección de Obras Públicas', 'Atiende temas relacionados con infraestructura y obras.', 'https://obraspublicas.dgo.gob.mx'),
('Catastro Municipal', 'Encargada del registro de propiedades y avalúos.', 'https://catastro.dgo.gob.mx'),
('Desarrollo Urbano', 'Control urbano, permisos y licencias de construcción.', 'https://desarrollourbano.dgo.gob.mx'),
('Registro Civil', 'Actas y certificados oficiales del estado civil.', 'https://registrocivil.dgo.gob.mx'),
('Transporte y Movilidad', 'Regulación del transporte público y privado.', 'https://transporte.dgo.gob.mx'),
('Ecología y Medio Ambiente', 'Permisos ambientales y gestión ecológica.', 'https://ecologia.dgo.gob.mx'),
('Protección Civil', 'Atiende emergencias y riesgos municipales.', 'https://proteccioncivil.dgo.gob.mx'),
('Comercio y Servicios', 'Trámites de giros comerciales y licencias.', 'https://comercio.dgo.gob.mx'),
('Secretaría de Salud', 'Regulación sanitaria y permisos de establecimientos.', 'https://salud.dgo.gob.mx'),
('Turismo Municipal', 'Promoción y control de actividades turísticas.', 'https://turismo.dgo.gob.mx'),
('Educación Municipal', 'Gestión de educación local y certificados.', 'https://educacion.dgo.gob.mx'),
('Servicios Públicos', 'Encargada de mantenimiento urbano y recolección.', 'https://servicios.dgo.gob.mx'),
('Oficialía Mayor', 'Administración general y recursos humanos.', 'https://oficialiamayor.dgo.gob.mx');
GO

--------------------------------------------------------------------------------
-- INSERTS EN TABLA: Oficina
--------------------------------------------------------------------------------
INSERT INTO Oficina (iIdDependencia, vchNombre, vchDireccion, vchTelefono, vchEmail, vchHorario, flLatitud, flLongitud, vchNotas)
VALUES
(4, 'Oficina Central de Finanzas', 'Calle Juárez #101, Centro', '6181230001', 'finanzas@dgo.gob.mx', 'L-V 9:00-15:00', 24.0321, -104.6721, 'Atención general de impuestos.'),
(4, 'Dirección de Seguridad Pública', 'Av. Universidad #230', '6181230002', 'seguridad@dgo.gob.mx', 'L-V 8:00-16:00', 24.0372, -104.6702, 'Atiende trámites de tránsito.'),
(4, 'Obras Públicas Norte', 'Blvd. Dolores del Río #450', '6181230003', 'obras@dgo.gob.mx', 'L-V 8:00-15:00', 24.0412, -104.6783, NULL),
(4, 'Oficina de Catastro', 'Av. Hidalgo #56', '6181230004', 'catastro@dgo.gob.mx', 'L-V 9:00-14:00', 24.0392, -104.6744, NULL),
(5, 'Urbanismo Zona Centro', 'Calle Victoria #90', '6181230005', 'urbano@dgo.gob.mx', 'L-V 9:00-15:00', 24.0431, -104.6755, 'Atiende licencias de construcción.'),
(6, 'Registro Civil #1', 'Calle Allende #23', '6181230006', 'registro1@dgo.gob.mx', 'L-V 9:00-15:00', 24.0401, -104.6731, NULL),
(7, 'Delegación Transporte', 'Carretera México #102', '6181230007', 'transporte@dgo.gob.mx', 'L-V 8:00-16:00', 24.0452, -104.6802, NULL),
(8, 'Oficina de Ecología', 'Av. Las Rosas #300', '6181230008', 'ecologia@dgo.gob.mx', 'L-V 8:00-14:00', 24.0482, -104.6822, NULL),
(9, 'Protección Civil Municipal', 'Calle Reforma #32', '6181230009', 'proteccion@dgo.gob.mx', 'L-D 24 HRS', 24.0501, -104.6791, 'Atiende emergencias las 24 horas.'),
(10, 'Licencias Comerciales', 'Calle Negrete #25', '6181230010', 'comercio@dgo.gob.mx', 'L-V 9:00-14:00', 24.0523, -104.6777, NULL),
(11, 'Jurisdicción Sanitaria', 'Blvd. Durango #150', '6181230011', 'salud@dgo.gob.mx', 'L-V 8:00-15:00', 24.0531, -104.6799, NULL),
(12, 'Oficina de Turismo', 'Av. 20 de Noviembre #305', '6181230012', 'turismo@dgo.gob.mx', 'L-V 9:00-15:00', 24.0544, -104.6732, 'Información turística y permisos.'),
(13, 'Educación Municipal', 'Calle Independencia #99', '6181230013', 'educacion@dgo.gob.mx', 'L-V 8:00-14:00', 24.0565, -104.6715, NULL),
(14, 'Oficina de Servicios Públicos', 'Calle Bravo #65', '6181230014', 'servicios@dgo.gob.mx', 'L-V 8:00-16:00', 24.0573, -104.6741, NULL),
(15, 'Oficialía Mayor', 'Calle Constitución #200', '6181230015', 'oficialia@dgo.gob.mx', 'L-V 9:00-15:00', 24.0582, -104.6763, NULL);


--------------------------------------------------------------------------------
-- INSERTS EN TABLA: OficinaTramite
-- * Evita iIdTramite = 2 y 3
-- * Se vincula aleatoriamente oficinas con distintos trámites válidos
--------------------------------------------------------------------------------
INSERT INTO OficinaTramite (iIdOficina, iIdTramite, vchObservacion)
VALUES
(4, 1, 'Atiende emisión de licencias.'),
(4, 4, 'Permisos de construcción en zona norte.'),
(4, 5, 'Constancias de residencia.'),
(4, 6, 'Cambio de propietario vehicular.'),
(6, 7, 'Expedición de actas de nacimiento.'),
(6, 8, 'Trámite de carta de no antecedentes.'),
(7, 9, 'Recepción de pagos de tenencia.'),
(8, 10, 'Licencia de funcionamiento para negocios.'),
(9, 11, 'Registro de comercios.'),
(10, 12, 'Autorización para eventos públicos.'),
(11, 13, 'Trámites de baja vehicular.'),
(12, 14, 'Actualización de domicilio fiscal.'),
(13, 15, 'Certificados de libertad de gravamen.'),
(14, 1, 'Renovación de licencias.'),
(15, 4, 'Revisión de permisos de obra civil.');


