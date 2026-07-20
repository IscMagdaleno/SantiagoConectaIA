-- ============================================================
-- Script: TablasWhatsApp.sql
-- Propósito: Crear las tablas para el módulo de WhatsApp
-- Autor: SantiagoConectaIA
-- Fecha: 2026-07-20
-- ============================================================

-- ============================================================
-- Tabla: WhatsAppUser
-- Propósito: Almacenar los datos de los usuarios de WhatsApp
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WhatsAppUser' AND schema_id = SCHEMA_ID('SCIA'))
BEGIN
    CREATE TABLE SCIA.WhatsAppUser
    (
        iIdWhatsAppUser INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
        nvchPhoneNumber NVARCHAR(20) NOT NULL,
        nvchName NVARCHAR(200) NOT NULL DEFAULT(''),
        dtFirstContact DATETIME NOT NULL DEFAULT(GETDATE()),
        dtLastContact DATETIME NOT NULL DEFAULT(GETDATE()),
        iTotalMessages INT NOT NULL DEFAULT(0),
        bActive BIT NOT NULL DEFAULT(1)
    );
END;
GO

-- Índice único por teléfono
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_WhatsAppUser_Phone' AND object_id = OBJECT_ID('SCIA.WhatsAppUser'))
BEGIN
    CREATE UNIQUE INDEX UQ_WhatsAppUser_Phone ON SCIA.WhatsAppUser(nvchPhoneNumber);
END;
GO

-- ============================================================
-- Tabla: WhatsAppConversation
-- Propósito: Almacenar las sesiones de conversación de WhatsApp
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WhatsAppConversation' AND schema_id = SCHEMA_ID('SCIA'))
BEGIN
    CREATE TABLE SCIA.WhatsAppConversation
    (
        iIdConversation INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
        iIdWhatsAppUser INT NOT NULL,
        dtStartTime DATETIME NOT NULL DEFAULT(GETDATE()),
        dtEndTime DATETIME NULL,
        iMessageCount INT NOT NULL DEFAULT(0),
        nvchStatus NVARCHAR(20) NOT NULL DEFAULT('active')
    );
END;
GO

-- Índice por usuario
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WhatsAppConversation_User' AND object_id = OBJECT_ID('SCIA.WhatsAppConversation'))
BEGIN
    CREATE INDEX IX_WhatsAppConversation_User ON SCIA.WhatsAppConversation(iIdWhatsAppUser);
END;
GO

-- ============================================================
-- Tabla: WhatsAppMessage
-- Propósito: Almacenar los mensajes individuales de WhatsApp
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WhatsAppMessage' AND schema_id = SCHEMA_ID('SCIA'))
BEGIN
    CREATE TABLE SCIA.WhatsAppMessage
    (
        iIdWhatsAppMessage INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
        iIdConversation INT NOT NULL,
        nvchWhatsAppMessageId NVARCHAR(100) NOT NULL DEFAULT(''),
        nvchDirection NVARCHAR(10) NOT NULL DEFAULT('inbound'),
        nvchMessageType NVARCHAR(20) NOT NULL DEFAULT('text'),
        nvchContent NVARCHAR(MAX) NOT NULL DEFAULT(''),
        dtTimestamp DATETIME NOT NULL DEFAULT(GETDATE())
    );
END;
GO

-- Índice por conversación
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WhatsAppMessage_Conversation' AND object_id = OBJECT_ID('SCIA.WhatsAppMessage'))
BEGIN
    CREATE INDEX IX_WhatsAppMessage_Conversation ON SCIA.WhatsAppMessage(iIdConversation);
END;
GO

-- Índice por timestamp para consultas de estadísticas
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WhatsAppMessage_Timestamp' AND object_id = OBJECT_ID('SCIA.WhatsAppMessage'))
BEGIN
    CREATE INDEX IX_WhatsAppMessage_Timestamp ON SCIA.WhatsAppMessage(dtTimestamp);
END;
GO
