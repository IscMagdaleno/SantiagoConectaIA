--------------------------------------------------------------------------------
-- Tabla: Dependencia
--------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.Dependencia') AND type = N'U')
BEGIN
CREATE TABLE dbo.Dependencia
(
    iIdDependencia INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    vchNombre VARCHAR(250) NOT NULL,
    nvchDescripcion NVARCHAR(MAX) NULL,
    vchUrlOficial VARCHAR(500) NULL,
    bActivo BIT NOT NULL DEFAULT(1),
    dtFechaCreacion DATETIME NOT NULL DEFAULT(GETDATE())
);
END;
GO

--------------------------------------------------------------------------------
-- Tabla: Oficina
-- Nota: incluye FK a Dependencia; coordenadas como FLOAT y validaciones CHECK.
--------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.Oficina') AND type = N'U')
BEGIN
CREATE TABLE dbo.Oficina
(
    iIdOficina INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    iIdDependencia INT NULL, -- FK a Dependencia (puede ser NULL si no aplica)
    vchNombre VARCHAR(250) NOT NULL,
    vchDireccion VARCHAR(500) NULL,
    vchTelefono VARCHAR(50) NULL,
    vchEmail VARCHAR(150) NULL,
    vchHorario NVARCHAR(250) NULL,
    flLatitud FLOAT NULL,
    flLongitud FLOAT NULL,
    vchNotas NVARCHAR(MAX) NULL,
    bActivo BIT NOT NULL DEFAULT(1),
    dtFechaCreacion DATETIME NOT NULL DEFAULT(GETDATE()),
    CONSTRAINT CK_Oficina_LatRange CHECK (flLatitud IS NULL OR (flLatitud >= -90 AND flLatitud <= 90)),
    CONSTRAINT CK_Oficina_LongRange CHECK (flLongitud IS NULL OR (flLongitud >= -180 AND flLongitud <= 180))
);
END;
GO

-- FK: Oficina.iIdDependencia -> Dependencia.iIdDependencia (no cascade delete)
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.Oficina') AND type = N'U')
    AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('dbo.Oficina') AND referenced_object_id = OBJECT_ID('dbo.Dependencia'))
BEGIN
ALTER TABLE dbo.Oficina
ADD CONSTRAINT FK_Oficina_Dependencia FOREIGN KEY (iIdDependencia)
REFERENCES dbo.Dependencia(iIdDependencia)
ON DELETE NO ACTION
ON UPDATE NO ACTION;
END;
GO

--------------------------------------------------------------------------------
-- Tabla: OficinaTramite (relación many-to-many Oficina <-> Tramite)
-- Se crea FK a Tramite solo si existe la tabla Tramite en la BD.
--------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.OficinaTramite') AND type = N'U')
BEGIN
CREATE TABLE dbo.OficinaTramite
(
    iIdOficinaTramite INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    iIdOficina INT NOT NULL,
    iIdTramite INT NOT NULL,
    vchObservacion NVARCHAR(500) NULL,
    bActivo BIT NOT NULL DEFAULT(1),
    dtFechaCreacion DATETIME NOT NULL DEFAULT(GETDATE())
);
END;
GO

-- FK: OficinaTramite.iIdOficina -> Oficina.iIdOficina
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.OficinaTramite') AND type = N'U')
    AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('dbo.OficinaTramite') AND referenced_object_id = OBJECT_ID('dbo.Oficina'))
BEGIN
ALTER TABLE dbo.OficinaTramite
ADD CONSTRAINT FK_OficinaTramite_Oficina FOREIGN KEY (iIdOficina)
REFERENCES dbo.Oficina(iIdOficina)
ON DELETE NO ACTION
ON UPDATE NO ACTION;
END;
GO

-- FK: OficinaTramite.iIdTramite -> Tramite.iIdTramite (solo si existe la tabla Tramite)
IF OBJECT_ID('dbo.Tramite', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE parent_object_id = OBJECT_ID('dbo.OficinaTramite') AND referenced_object_id = OBJECT_ID('dbo.Tramite')
    )
    BEGIN
        ALTER TABLE dbo.OficinaTramite
        ADD CONSTRAINT FK_OficinaTramite_Tramite FOREIGN KEY (iIdTramite)
        REFERENCES dbo.Tramite(iIdTramite)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION;
    END;
END;
GO

--------------------------------------------------------------------------------
-- Índices recomendados
--------------------------------------------------------------------------------

-- Índice por dependencia para filtros
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Oficina') AND name = 'IX_Oficina_iIdDependencia')
BEGIN
CREATE NONCLUSTERED INDEX IX_Oficina_iIdDependencia ON dbo.Oficina (iIdDependencia);
END;
GO

-- Índice en columnas de coordenadas (para consultas por rango / bounding box)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Oficina') AND name = 'IX_Oficina_LatLong')
BEGIN
CREATE NONCLUSTERED INDEX IX_Oficina_LatLong ON dbo.Oficina (flLatitud, flLongitud);
END;
GO

-- Índice en nombre para búsquedas simples (like '%texto%': no acelerará prefijo libre; considerar Full-Text)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Oficina') AND name = 'IX_Oficina_vchNombre')
BEGIN
CREATE NONCLUSTERED INDEX IX_Oficina_vchNombre ON dbo.Oficina (vchNombre);
END;
GO

--------------------------------------------------------------------------------
-- Opcional: Full-Text catalog & index (mejor búsquedas por contenido textual)
-- Ejecutar solo si tu instancia tiene Full-Text instalado y habilitado.
--------------------------------------------------------------------------------
/*
-- Crear catálogo FT (si no existe)
IF NOT EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'FTC_Santiago')
BEGIN
    CREATE FULLTEXT CATALOG FTC_Santiago;
END;

-- Crear índice full-text sobre vchNombre y vchDireccion
IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('dbo.Oficina'))
BEGIN
    CREATE FULLTEXT INDEX ON dbo.Oficina(vchNombre LANGUAGE 0, vchDireccion LANGUAGE 0)
    KEY INDEX PK__Oficina__ -- sustituir por el nombre real del índice PK si es necesario
    ON FTC_Santiago;
END;
*/
GO

--------------------------------------------------------------------------------
-- Opcional: columna geográfica y spatial index (mejor para consultas con distancias)
-- Si quieres usar spatial, crea una columna geography y un índice espacial.
--------------------------------------------------------------------------------
/*
-- Añadir columna geography
IF COL_LENGTH('dbo.Oficina', 'GeoUbicacion') IS NULL
BEGIN
    ALTER TABLE dbo.Oficina
    ADD GeoUbicacion geography NULL;
END;

-- Actualizar GeoUbicacion desde lat/long (ejecutar tras insertar datos)
UPDATE dbo.Oficina
SET GeoUbicacion = geography::Point(flLatitud, flLongitud, 4326)
WHERE flLatitud IS NOT NULL AND flLongitud IS NOT NULL AND GeoUbicacion IS NULL;

-- Crear índice espacial
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Oficina') AND name = 'SP_IDX_Oficina_Geo')
BEGIN
    CREATE SPATIAL INDEX SP_IDX_Oficina_Geo ON dbo.Oficina (GeoUbicacion)
    USING  GEOMETRY_AUTO_GRID
    WITH (BOUNDING_BOX = (-180, -90, 180, 90));
END;
*/
GO
