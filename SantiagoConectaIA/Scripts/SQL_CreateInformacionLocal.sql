CREATE TABLE [dbo].[InformacionLocal] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Categoria] NVARCHAR(100) NOT NULL,
    [Titulo] NVARCHAR(255) NOT NULL,
    [PalabrasClave] NVARCHAR(500) NULL,
    [DescripcionCorta] NVARCHAR(1000) NOT NULL,
    [ContenidoDetallado] NVARCHAR(MAX) NULL,
    [Ubicacion_LatLong] NVARCHAR(100) NULL,
    [FechaCreacion] DATETIME NOT NULL DEFAULT GETDATE(),
    [Activo] BIT NOT NULL DEFAULT 1
);
GO

-- Opcional: Índice para mejorar búsquedas de texto si usas LIKE o CONTAINS más adelante
CREATE NONCLUSTERED INDEX [IX_InformacionLocal_Categoria] ON [dbo].[InformacionLocal] ([Categoria]);
GO
