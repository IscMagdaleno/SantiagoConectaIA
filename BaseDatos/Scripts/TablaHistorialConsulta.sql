-- Script idempotente: Crear tabla HistorialConsulta
-- Sigue convenciones Engrama (prefijos i/vch/nvch/dt, PK iId..., NOT NULL donde aplica).
-- Ejecutar en la base de datos de staging/desarrollo.

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.HistorialConsulta') AND type = N'U')
BEGIN
    CREATE TABLE dbo.HistorialConsulta
    (
        iIdHistorial INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
        iIdUsuario INT NULL,                             -- puede ser NULL para consultas anónimas
        nvchPregunta NVARCHAR(MAX) NOT NULL,             -- texto completo de la pregunta del usuario
        nvchRespuesta NVARCHAR(MAX) NOT NULL,            -- texto completo de la respuesta (LLM / función)
        vchOrigen VARCHAR(50) NULL,                      -- ejemplo: 'web', 'mobile', 'api'
        vchSessionId VARCHAR(100) NULL,                  -- opcional: identificar sesión/convocatoria
        vchIntent VARCHAR(150) NULL,                     -- opcional: intent detectado/función ejecutada
        bAnonimizado BIT NOT NULL DEFAULT 0,             -- si la entrada fue anonimizadá por privacidad
        dtFecha DATETIME NOT NULL DEFAULT(GETDATE())     -- fecha/hora de la consulta
    );

    -- Índices recomendados para consultas por usuario/fecha y para limpieza/retención
    CREATE NONCLUSTERED INDEX IX_HistorialConsulta_iIdUsuario ON dbo.HistorialConsulta (iIdUsuario);
    CREATE NONCLUSTERED INDEX IX_HistorialConsulta_dtFecha ON dbo.HistorialConsulta (dtFecha);
END;
GO

-- Verificación básica (ejemplo)
-- SELECT TOP 10 * FROM dbo.HistorialConsulta ORDER BY dtFecha DESC;
