using Microsoft.EntityFrameworkCore;
using NotaFiscal.Data;
using NotaFiscal.Models;
using NotaFiscal.Models.Enums;
using NotaFiscal.Services;
using OfficeOpenXml;
using System.Globalization;
using System.Text.RegularExpressions;

namespace NotaFiscal.Controllers
{
    public class PlanilhaController
    {
        private readonly AppDbContext _context;
        private readonly LogService _logService;
        private readonly ValidacaoService _validacaoService;

        public PlanilhaController(AppDbContext context)
        {
            _context = context;
            _logService = new LogService();
            _validacaoService = new ValidacaoService();
        }

        /// <summary>
        /// Classe para armazenar dados validados de uma linha
        /// </summary>
        private class DadosLinhaValidados
        {
            public bool TemErros { get; set; }
            public int ClienteId { get; set; }
            public string CpfCnpj { get; set; } = "";
            public string NomeRazaoSocial { get; set; } = "";
            public string? NomeFantasia { get; set; }
            public string Email { get; set; } = "";
            public string Telefone { get; set; } = "";
            public string? Logradouro { get; set; }
            public TipoEndereco TipoEndereco { get; set; }
            public int VendaId { get; set; }
            public DateTime DataVenda { get; set; }
            public decimal ValorTotal { get; set; }
            public FormaPagamento FormaPagamento { get; set; }
        }

        /// <summary>
        /// Valida e converte os dados de uma linha da planilha
        /// </summary>
        private DadosLinhaValidados ValidarCelulasLinha(ExcelWorksheet worksheet, int row)
        {
            var dados = new DadosLinhaValidados();

            try
            {
                // Coluna 1: ID Cliente
                var idClienteStr = worksheet.Cells[row, 1].Value?.ToString()?.Trim() ?? "";
                if (string.IsNullOrEmpty(idClienteStr) || !int.TryParse(idClienteStr, out int clienteId))
                {
                    _logService.RegistrarErroCampo(row, "ID Cliente", idClienteStr, "ID Cliente deve ser um número válido");
                    dados.TemErros = true;
                    return dados;
                }
                dados.ClienteId = clienteId;

                // Coluna 2: CPF/CNPJ
                var cpfCnpjStr = worksheet.Cells[row, 2].Value?.ToString() ?? "";
                dados.CpfCnpj = _validacaoService.LimparCpfCnpj(cpfCnpjStr);
                if (string.IsNullOrEmpty(dados.CpfCnpj))
                {
                    _logService.RegistrarErroCampo(row, "CPF/CNPJ", cpfCnpjStr, "CPF/CNPJ é obrigatório");
                    dados.TemErros = true;
                }

                // Coluna 3: Nome/Razão Social
                dados.NomeRazaoSocial = worksheet.Cells[row, 3].Value?.ToString()?.Trim() ?? "";
                if (string.IsNullOrEmpty(dados.NomeRazaoSocial))
                {
                    _logService.RegistrarErroCampo(row, "Nome/Razão Social", dados.NomeRazaoSocial, "Nome/Razão Social é obrigatório");
                    dados.TemErros = true;
                }

                // Coluna 4: Nome Fantasia (opcional)
                dados.NomeFantasia = worksheet.Cells[row, 4].Value?.ToString()?.Trim();

                // Coluna 5: Email
                dados.Email = worksheet.Cells[row, 5].Value?.ToString()?.Trim() ?? "";
                if (string.IsNullOrEmpty(dados.Email))
                {
                    _logService.RegistrarErroCampo(row, "Email", dados.Email, "Email é obrigatório");
                    dados.TemErros = true;
                }

                // Coluna 6: Telefone
                var telefoneStr = worksheet.Cells[row, 6].Value?.ToString() ?? "";
                dados.Telefone = _validacaoService.LimparTelefone(telefoneStr);
                if (string.IsNullOrEmpty(dados.Telefone))
                {
                    _logService.RegistrarErroCampo(row, "Telefone", telefoneStr, "Telefone é obrigatório");
                    dados.TemErros = true;
                }

                // Coluna 7: Logradouro (opcional)
                dados.Logradouro = worksheet.Cells[row, 7].Value?.ToString()?.Trim();

                // Coluna 8: Tipo Endereço
                var tipoEnderecoStr = worksheet.Cells[row, 8].Value?.ToString()?.Trim() ?? "";
                try
                {
                    dados.TipoEndereco = _validacaoService.ParseTipoEndereco(tipoEnderecoStr);
                }
                catch
                {
                    _logService.RegistrarErroCampo(row, "Tipo Endereço", tipoEnderecoStr, "Tipo de endereço inválido (use: Entrega, Cobranca ou Ambos)");
                    dados.TemErros = true;
                }

                // Coluna 9: ID Venda
                var idVendaStr = worksheet.Cells[row, 9].Value?.ToString()?.Trim() ?? "";
                if (string.IsNullOrEmpty(idVendaStr) || !int.TryParse(idVendaStr, out int vendaId))
                {
                    _logService.RegistrarErroCampo(row, "ID Venda", idVendaStr, "ID Venda deve ser um número válido");
                    dados.TemErros = true;
                    return dados;
                }
                dados.VendaId = vendaId;

                // Coluna 10: Data Venda
                try
                {
                    dados.DataVenda = worksheet.Cells[row, 10].GetValue<DateTime>().Date;
                }
                catch
                {
                    var dataStr = worksheet.Cells[row, 10].Value?.ToString() ?? "";
                    _logService.RegistrarErroCampo(row, "Data Venda", dataStr, "Data da venda deve ser uma data válida");
                    dados.TemErros = true;
                }

                // Coluna 11: Valor Total
                var valorTotalStr = worksheet.Cells[row, 11].Value?.ToString()?.Trim() ?? "";
                if (string.IsNullOrEmpty(valorTotalStr) || !decimal.TryParse(valorTotalStr, out decimal valorTotal))
                {
                    _logService.RegistrarErroCampo(row, "Valor Total", valorTotalStr, "Valor Total deve ser um número válido");
                    dados.TemErros = true;
                }
                else
                {
                    dados.ValorTotal = valorTotal;
                }

                // Coluna 12: Forma Pagamento
                var formaPagamentoStr = worksheet.Cells[row, 12].Value?.ToString()?.Trim() ?? "";
                try
                {
                    dados.FormaPagamento = _validacaoService.ParseFormaPagamento(formaPagamentoStr);
                }
                catch
                {
                    _logService.RegistrarErroCampo(row, "Forma Pagamento", formaPagamentoStr, "Forma de pagamento inválida");
                    dados.TemErros = true;
                }
            }
            catch (Exception ex)
            {
                _logService.RegistrarErroCampo(row, "Erro geral", "", $"Erro inesperado ao validar linha: {ex.Message}");
                dados.TemErros = true;
            }

            return dados;
        }

        public async Task ImportarPlanilha(string caminhoDoArquivo, IProgress<string>? progress = null)
        {
            ExcelPackage.License.SetNonCommercialPersonal("Teste");
            var fileInfo = new FileInfo(caminhoDoArquivo);

            // Inicializa o log
            var logPath = _logService.InicializarLog(Path.GetFileName(caminhoDoArquivo));

            // Informa onde o log foi criado
            if (!string.IsNullOrEmpty(logPath))
            {
                progress?.Report($"Log de importação será salvo em: {logPath}");
            }

            int linhasProcessadas = 0;
            int linhasPuladas = 0;
            int linhasInseridas = 0;
            int linhasComErro = 0;

            progress?.Report("Iniciando importação da planilha...");
            _logService.RegistrarLog("Iniciando importação da planilha");

            using (var package = new ExcelPackage(fileInfo))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension.Rows;

                progress?.Report($"Total de linhas encontradas: {rowCount - 1} (ignorando cabeçalho)");
                _logService.RegistrarLog($"Total de linhas encontradas: {rowCount - 1} (ignorando cabeçalho)");

                for (int row = 2; row <= rowCount; row++) // Começa da linha 2 para ignorar o cabeçalho
                {
                    linhasProcessadas++;
                    
                    // Valida todos os campos da linha ANTES de tentar criar objetos
                    var dadosValidados = ValidarCelulasLinha(worksheet, row);
                    
                    if (dadosValidados.TemErros)
                    {
                        linhasComErro++;
                        continue; // Pula para a próxima linha se houver erros de validação
                    }
                    
                    // Inicia uma nova transação para cada linha
                    using (var transaction = await _context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            // 1. Procura se o cliente já existe no banco de dados
                            var cliente = await _context.Clientes
                                    .Include(c => c.Enderecos)
                                    .FirstOrDefaultAsync(c => c.Id == dadosValidados.ClienteId);

                            // 2. Se não existir, cria um novo cliente
                            if (cliente == null)
                            {
                                cliente = new Cliente
                                {
                                    Id = dadosValidados.ClienteId,
                                    CpfCnpj = dadosValidados.CpfCnpj,
                                    NomeRazaoSocial = dadosValidados.NomeRazaoSocial,
                                    NomeFantasia = dadosValidados.NomeFantasia,
                                    Email = dadosValidados.Email,
                                    Telefone = dadosValidados.Telefone
                                };
                                _context.Clientes.Add(cliente);
                            }

                            // 3. Verifica o endereço
                            Endereco enderecoParaVenda;

                            if (!string.IsNullOrWhiteSpace(dadosValidados.Logradouro))
                            {
                                var enderecoExistente = cliente.Enderecos.FirstOrDefault(e => e.Logradouro.Equals(dadosValidados.Logradouro, StringComparison.OrdinalIgnoreCase));

                                if (enderecoExistente == null)
                                {
                                    var novoEndereco = new Endereco
                                    {
                                        TipoEndereco = dadosValidados.TipoEndereco,
                                        Logradouro = dadosValidados.Logradouro,
                                        Cliente = cliente
                                    };
                                    cliente.Enderecos.Add(novoEndereco);
                                    enderecoParaVenda = novoEndereco;
                                }
                                else
                                {
                                    enderecoParaVenda = enderecoExistente;
                                }
                            }
                            else
                            {
                                // Se não há logradouro, usa o primeiro endereço existente do cliente
                                var enderecoExistente = cliente.Enderecos.FirstOrDefault();
                                if (enderecoExistente != null)
                                {
                                    enderecoParaVenda = enderecoExistente;
                                }
                                else
                                {
                                    // Cria endereço padrão quando não há endereço informado
                                    var enderecoDefault = new Endereco
                                    {
                                        TipoEndereco = TipoEndereco.Entrega,
                                        Logradouro = "Endereço não informado",
                                        Cliente = cliente
                                    };
                                    cliente.Enderecos.Add(enderecoDefault);
                                    enderecoParaVenda = enderecoDefault;
                                }
                            }
                            
                            // 4. Verifica se a venda já existe
                            var vendaExistente = await _context.Vendas.AsNoTracking().FirstOrDefaultAsync(v => v.Id == dadosValidados.VendaId);

                            if (vendaExistente != null)
                            {
                                var mensagem = $"Linha {row}: Venda ID {dadosValidados.VendaId} já existe - pulando";
                                progress?.Report(mensagem);
                                _logService.RegistrarLog(mensagem);
                                linhasPuladas++;
                                
                                // Faz rollback da transação para não salvar cliente/endereço desnecessários
                                await transaction.RollbackAsync();
                                continue;
                            }

                            // 5. Cadastra a venda
                            var venda = new Venda
                            {
                                Id = dadosValidados.VendaId,
                                Data = dadosValidados.DataVenda,
                                ValorTotal = dadosValidados.ValorTotal,
                                FormaPagamento = dadosValidados.FormaPagamento,
                                Cliente = cliente,
                                Endereco = enderecoParaVenda
                            };

                            _context.Vendas.Add(venda);

                            // Salva as alterações da linha atual
                            await _context.SaveChangesAsync();
                            
                            // Confirma a transação apenas se tudo deu certo
                            await transaction.CommitAsync();
                            
                            linhasInseridas++;

                            // Log de progresso a cada 10 linhas
                            if (linhasProcessadas % 10 == 0)
                            {
                                var progressMessage = $"Processadas {linhasProcessadas} de {rowCount - 1} linhas...";
                                progress?.Report(progressMessage);
                                _logService.RegistrarLog(progressMessage);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Faz rollback da transação em caso de erro
                            await transaction.RollbackAsync();
                            linhasComErro++;
                            
                            // Como já validamos os campos, este erro deve ser de banco de dados ou lógica
                            _logService.RegistrarErroCampo(row, "Erro de banco/lógica", "", ex.Message);
                            
                            var errorMessage = $"ERRO na linha {row}: {ex.Message} - Rollback executado";
                            progress?.Report(errorMessage);
                            
                            // IMPORTANTE: Limpa o contexto para evitar problemas nas próximas linhas
                            _context.ChangeTracker.Clear();
                            
                            // Continua processando as próximas linhas mesmo com erro nesta linha
                            continue;
                        }
                    }
                }

                // Log final com resumo
                var resumo = new[]
                {
                    "Resumo da importação:",
                    $"- Total de linhas processadas: {linhasProcessadas}",
                    $"- Linhas inseridas com sucesso: {linhasInseridas}",
                    $"- Linhas puladas (duplicadas): {linhasPuladas}",
                    $"- Linhas com erro: {linhasComErro}"
                };

                foreach (var linha in resumo)
                {
                    progress?.Report(linha);
                    _logService.RegistrarLog(linha);
                }

                _logService.RegistrarLog("Importação finalizada");
            }
        }
    }
}