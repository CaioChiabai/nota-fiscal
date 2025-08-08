-- Verifica se o banco existe e cria se necessário
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'db_nota_fiscal')
BEGIN
    CREATE DATABASE [db_nota_fiscal];
    PRINT '✓ Banco de dados [db_nota_fiscal] criado com sucesso!';
END
ELSE
BEGIN
    PRINT '⚠ Banco de dados [db_nota_fiscal] já existe.';
END
GO

-- Usa o banco de dados
USE [db_nota_fiscal];
GO

-- ========================================
-- CRIAÇÃO DAS TABELAS
-- ========================================

-- 1. Tabela de Clientes
-- ========================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Clientes' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Clientes] (
        [Id] INT NOT NULL PRIMARY KEY,
        [CpfCnpj] NVARCHAR(14) NOT NULL,
        [NomeRazaoSocial] NVARCHAR(255) NOT NULL,
        [NomeFantasia] NVARCHAR(150) NULL,
        [Email] NVARCHAR(255) NOT NULL,
        [Telefone] NVARCHAR(15) NOT NULL
    );
    
    PRINT '✓ Tabela [Clientes] criada com sucesso!';
END
ELSE
BEGIN
    PRINT '⚠ Tabela [Clientes] já existe.';
END
GO

-- 2. Tabela de Endereços
-- ========================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Enderecos' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Enderecos] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [IdCliente] INT NOT NULL,
        [TipoEndereco] INT NOT NULL, -- 1=Cobrança, 2=Entrega
        [Logradouro] NVARCHAR(255) NOT NULL,
        
        -- Foreign Key
        CONSTRAINT [FK_Enderecos_Clientes] 
            FOREIGN KEY ([IdCliente]) REFERENCES [dbo].[Clientes] ([Id])
            ON DELETE CASCADE
    );
    
    PRINT '✓ Tabela [Enderecos] criada com sucesso!';
END
ELSE
BEGIN
    PRINT '⚠ Tabela [Enderecos] já existe.';
END
GO

-- 3. Tabela de Vendas
-- ========================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Vendas' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Vendas] (
        [Id] INT NOT NULL PRIMARY KEY,
        [IdCliente] INT NOT NULL,
        [IdEndereco] INT NOT NULL,
        [Data] DATE NOT NULL,
        [ValorTotal] DECIMAL(18,2) NOT NULL,
        [FormaPagamento] INT NOT NULL, -- 1=Dinheiro, 2=Cartão, 3=Pix, 4=Transferência, 5=Boleto
        
        -- Foreign Keys
        CONSTRAINT [FK_Vendas_Clientes] 
            FOREIGN KEY ([IdCliente]) REFERENCES [dbo].[Clientes] ([Id])
            ON DELETE NO ACTION,
            
        CONSTRAINT [FK_Vendas_Enderecos] 
            FOREIGN KEY ([IdEndereco]) REFERENCES [dbo].[Enderecos] ([Id])
            ON DELETE NO ACTION
    );
    
    PRINT '✓ Tabela [Vendas] criada com sucesso!';
END
ELSE
BEGIN
    PRINT '⚠ Tabela [Vendas] já existe.';
END
GO

-- ========================================
-- CRIAÇÃO DE ÍNDICES PARA PERFORMANCE
-- ========================================

-- Índices na tabela Clientes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Clientes_CpfCnpj')
BEGIN
    CREATE INDEX [IX_Clientes_CpfCnpj] ON [dbo].[Clientes] ([CpfCnpj]);
    PRINT '✓ Índice [IX_Clientes_CpfCnpj] criado!';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Clientes_NomeRazaoSocial')
BEGIN
    CREATE INDEX [IX_Clientes_NomeRazaoSocial] ON [dbo].[Clientes] ([NomeRazaoSocial]);
    PRINT '✓ Índice [IX_Clientes_NomeRazaoSocial] criado!';
END

-- Índices na tabela Enderecos
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Enderecos_IdCliente')
BEGIN
    CREATE INDEX [IX_Enderecos_IdCliente] ON [dbo].[Enderecos] ([IdCliente]);
    PRINT '✓ Índice [IX_Enderecos_IdCliente] criado!';
END

-- Índices na tabela Vendas
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Vendas_IdCliente')
BEGIN
    CREATE INDEX [IX_Vendas_IdCliente] ON [dbo].[Vendas] ([IdCliente]);
    PRINT '✓ Índice [IX_Vendas_IdCliente] criado!';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Vendas_IdEndereco')
BEGIN
    CREATE INDEX [IX_Vendas_IdEndereco] ON [dbo].[Vendas] ([IdEndereco]);
    PRINT '✓ Índice [IX_Vendas_IdEndereco] criado!';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Vendas_Data')
BEGIN
    CREATE INDEX [IX_Vendas_Data] ON [dbo].[Vendas] ([Data]);
    PRINT '✓ Índice [IX_Vendas_Data] criado!';
END
GO