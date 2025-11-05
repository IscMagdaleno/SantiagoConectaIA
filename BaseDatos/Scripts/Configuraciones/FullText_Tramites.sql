/*
SELECT name 
FROM sys.indexes 
WHERE object_id = OBJECT_ID('dbo.Tramite') 
  AND is_primary_key = 1;
*/
-- Paso 1: Crear Catálogo de Full-Text (si no existe)
IF NOT EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'FTC_Tramites')
BEGIN
    CREATE FULLTEXT CATALOG FTC_Tramites;
END;
GO

-- Paso 2: Crear Índice de Full-Text en la tabla Tramite
-- Primero, elimina si existe para evitar errores al intentar crearlo de nuevo
IF EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('dbo.Tramite'))
BEGIN
    DROP FULLTEXT INDEX ON dbo.Tramite;
END;

BEGIN
    -- ATENCIÓN: Sustituir [Nombre REAL del Índice Primario] con el nombre obtenido del query anterior
    CREATE FULLTEXT INDEX ON dbo.Tramite(
        vchNombre LANGUAGE 'Spanish', 
        nvchDescripcion LANGUAGE 'Spanish'
    )
    KEY INDEX [PK__Tramite__5EB01175642DB1DB] 
    ON FTC_Tramites
    WITH CHANGE_TRACKING AUTO;
END;
GO