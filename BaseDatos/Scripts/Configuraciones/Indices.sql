-- Índice en Tramite.vchNombre (búsqueda por nombre)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('SCIA.Tramite') AND name = 'IX_Tramite_vchNombre')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Tramite_vchNombre ON SCIA.Tramite (vchNombre);
END;
GO

-- Índice en Tramite.dtFechaCreacion (usado en ORDER BY reciente)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('SCIA.Tramite') AND name = 'IX_Tramite_dtFechaCreacion')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Tramite_dtFechaCreacion ON SCIA.Tramite (dtFechaCreacion DESC);
END;
GO

-- Índice en Oficina.vchNombre y vchDireccion
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('SCIA.Oficina') AND name = 'IX_Oficina_vchNombre')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Oficina_vchNombre ON SCIA.Oficina (vchNombre);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('SCIA.Oficina') AND name = 'IX_Oficina_vchDireccion')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Oficina_vchDireccion ON SCIA.Oficina (vchDireccion);
END;
GO




