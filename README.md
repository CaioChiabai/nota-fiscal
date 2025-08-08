# 📋 Sistema de Importação de Nota Fiscal

![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![Windows Forms](https://img.shields.io/badge/Windows%20Forms-Application-green)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Database-red)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework-Core-orange)

Sistema desktop desenvolvido em C# com Windows Forms para importação e gerenciamento de dados de notas fiscais a partir de planilhas Excel.

## 🚀 Funcionalidades

- ✅ **Importação de Planilhas Excel**: Importa dados de clientes, endereços e vendas
- ✅ **Verificação Automática de Conexão**: Testa conectividade com banco de dados na inicialização
- ✅ **Logs em Tempo Real**: Acompanhe o progresso da importação com feedback detalhado
- ✅ **Gerenciamento de Duplicatas**: Identifica e pula registros duplicados automaticamente
- ✅ **Validação de Dados**: Tratamento robusto de dados inválidos ou ausentes
- ✅ **Interface Amigável**: Interface simples e intuitiva para o usuário

## 🛠️ Tecnologias Utilizadas

- **Framework**: .NET 8.0 (Windows Forms)
- **Banco de Dados**: SQL Server / SQL Server Express
- **ORM**: Entity Framework Core 9.0.8
- **Planilhas**: EPPlus 8.0.8
- **Arquitetura**: MVC (Model-View-Controller) com Injeção de Dependência
- **Padrões**: Repository Pattern, SOLID Principles

## 📋 Pré-requisitos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) ou [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-editions/sql-server-editions-express)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) ou [Visual Studio Code](https://code.visualstudio.com/)
- Windows 10 ou superior

## 🎯 Instalação e Configuração

### 1. Clone o Repositório
```bash
git clone https://github.com/CaioChiabai/nota_fiscal.git
cd nota_fiscal
```

### 2. Configure o Banco de Dados

#### Opção A: Usando Entity Framework (Recomendado)
```bash
# Restaurar pacotes
dotnet restore

# Aplicar migrações (cria o banco automaticamente)
dotnet ef database update
```

#### Opção B: Usando Script SQL Manual
Execute o script `database_setup.sql` no seu SQL Server Management Studio ou utilize:
```bash
sqlcmd -S SERVIDOR -d master -i database_setup.sql
```

### 3. Configure a String de Conexão

Edite o arquivo `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SEU_SERVIDOR;Database=db_nota_fiscal;Trusted_Connection=true;MultipleActiveResultSets=true;Encrypt=False"
  }
}
```

### 4. Execute a Aplicação
```bash
dotnet run
```

## 📊 Estrutura da Planilha Excel

A planilha deve seguir o formato abaixo (primeira linha é o cabeçalho):

| Cliente ID | CPF/CNPJ | Nome/Razão Social | Nome Fantasia | Email | Telefone | Logradouro | Tipo Endereço | Venda ID | Data | Valor Total | Forma Pagamento |
|------------|----------|-------------------|---------------|-------|----------|------------|---------------|----------|------|-------------|-----------------|
| 1 | 12345678901 | João Silva | | joao@email.com | 11999999999 | Rua A, 123 | Entrega | 1001 | 2024-01-15 | 150.50 | Pix |

### Campos Obrigatórios:
- **Cliente ID**: Número único do cliente
- **Nome/Razão Social**: Nome completo ou razão social
- **Venda ID**: Número único da venda
- **Data**: Data da venda (formato: YYYY-MM-DD)
- **Valor Total**: Valor da venda (formato numérico)

### Valores Aceitos:
- **Tipo Endereço**: "Entrega", "Cobrança"
- **Forma Pagamento**: "Dinheiro", "Cartao", "Pix", "Transferencia", "Boleto"

## 🏗️ Estrutura do Projeto

```
NotaFiscal/
├── Controllers/           # Lógica de negócio
│   └── PlanilhaController.cs
├── Data/                 # Contexto do banco de dados
│   ├── AppDbContext.cs
│   └── AppDbContextFactory.cs
├── Models/               # Modelos de dados
│   ├── Cliente.cs
│   ├── Endereco.cs
│   ├── Venda.cs
│   └── Enums/
├── Views/                # Interface do usuário
│   └── FormImportador.cs
├── Migrations/           # Migrações do Entity Framework
└── appsettings.json     # Configurações da aplicação
```

## 🎮 Como Usar

1. **Inicie a Aplicação**: Execute o programa
2. **Verificação de Conexão**: Observe os logs iniciais que mostram o status da conexão
3. **Selecione a Planilha**: Clique em "Selecionar Planilha" e escolha seu arquivo Excel
4. **Importe os Dados**: Clique em "Importar" e acompanhe o progresso nos logs
5. **Verifique os Resultados**: Observe o resumo final com estatísticas da importação

### Exemplo de Log de Importação:
```
[10:30:15] Iniciando aplicação...
[10:30:15] ✓ Conexão com banco de dados estabelecida com sucesso!
[10:30:15] ✓ Registros encontrados: 25 clientes, 150 vendas
[10:30:45] Arquivo selecionado: vendas_janeiro.xlsx
[10:30:45] Iniciando importação da planilha...
[10:30:46] Total de linhas encontradas: 100 (ignorando cabeçalho)
[10:30:50] Processadas 50 de 100 linhas...
[10:30:55] Salvando dados no banco...
[10:30:56] Resumo da importação:
[10:30:56] - Total de linhas processadas: 100
[10:30:56] - Linhas inseridas com sucesso: 98
[10:30:56] - Linhas puladas (duplicadas): 2
[10:30:56] Importação finalizada com sucesso.
```

## 🗄️ Estrutura do Banco de Dados

### Tabelas:

#### `Clientes`
- `Id` (int, PK) - Identificador único
- `CpfCnpj` (nvarchar) - CPF ou CNPJ
- `NomeRazaoSocial` (nvarchar) - Nome ou razão social
- `NomeFantasia` (nvarchar, nullable) - Nome fantasia
- `Email` (nvarchar) - E-mail
- `Telefone` (nvarchar) - Telefone

#### `Enderecos`
- `Id` (int, PK, Identity) - Identificador único
- `IdCliente` (int, FK) - Referência ao cliente
- `TipoEndereco` (int) - Tipo do endereço (1=Cobrança, 2=Entrega)
- `Logradouro` (nvarchar) - Endereço completo

#### `Vendas`
- `Id` (int, PK) - Identificador único
- `IdCliente` (int, FK) - Referência ao cliente
- `IdEndereco` (int, FK) - Referência ao endereço
- `Data` (date) - Data da venda
- `ValorTotal` (decimal) - Valor total da venda
- `FormaPagamento` (int) - Forma de pagamento (1-5)

## 👨‍💻 Desenvolvimento

### Executar em Modo Debug
```bash
dotnet run --configuration Debug
```

### Gerar Nova Migração
```bash
dotnet ef migrations add NomeDaMigracao
dotnet ef database update
```

### Compilar Release
```bash
dotnet publish -c Release -r win-x64 --self-contained
```

## 🤝 Contribuição

1. Faça um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está licenciado sob a MIT License - veja o arquivo [LICENSE](LICENSE) para detalhes.

---

⭐ **Se este projeto foi útil para você, não esqueça de dar uma estrela!**

**Desenvolvido por [Caio Chiabai](https://github.com/CaioChiabai)**
