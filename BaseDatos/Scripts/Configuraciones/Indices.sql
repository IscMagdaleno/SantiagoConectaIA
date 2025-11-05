-- Índice en Tramite.vchNombre (búsqueda por nombre)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Tramite') AND name = 'IX_Tramite_vchNombre')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Tramite_vchNombre ON dbo.Tramite (vchNombre);
END;
GO

-- Índice en Tramite.dtFechaCreacion (usado en ORDER BY reciente)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Tramite') AND name = 'IX_Tramite_dtFechaCreacion')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Tramite_dtFechaCreacion ON dbo.Tramite (dtFechaCreacion DESC);
END;
GO

-- Índice en Oficina.vchNombre y vchDireccion
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Oficina') AND name = 'IX_Oficina_vchNombre')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Oficina_vchNombre ON dbo.Oficina (vchNombre);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Oficina') AND name = 'IX_Oficina_vchDireccion')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Oficina_vchDireccion ON dbo.Oficina (vchDireccion);
END;
GO




