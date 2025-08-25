using Microsoft.EntityFrameworkCore;
using NotaFiscal.Data;
using NotaFiscal.Models;
using NotaFiscal.Models.Enums;
using NotaFiscal.Services;
using OfficeOpenXml;

namespace NotaFiscal.Controllers
{
    public class PlanilhaController
    {
        private readonly AppDbContext _context;
        private readonly LogService _logService;
        private readonly ValidacaoService _validacaoService;
        
        public PlanilhaController(AppDbContext context, LogService logService, ValidacaoService validacaoService)
        {
            _context = context;
            _logService = logService;
            _validacaoService = validacaoService;
        }     

        public async Task ImportarPlanilha(string caminhoDoArquivo)
        {
            ExcelPackage.License.SetNonCommercialPersonal("Teste");
            var fileInfo = new FileInfo(caminhoDoArquivo);

            // Inicializa o log
            var logPath = _logService.InicializarLog(Path.GetFileName(caminhoDoArquivo));

            // Informa onde o log foi criado
            if (!string.IsNullOrEmpty(logPath))
            {
                // Log salvo apenas em arquivo
            }

            int linhasProcessadas = 0;
            int linhasPuladas = 0;
            int linhasInseridas = 0;
            int linhasComErro = 0;

            _logService.RegistrarLog("Iniciando importação da planilha");

            using (var package = new ExcelPackage(fileInfo))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension.Rows;

                _logService.RegistrarLog($"Total de linhas encontradas: {rowCount - 1} (ignorando cabeçalho)");

                for (int row = 2; row <= rowCount; row++) // Começa da linha 2 para ignorar o cabeçalho
                {
                    linhasProcessadas++;
                    
                    // Valida todos os campos da linha ANTES de tentar criar objetos
                    var dadosValidados = DadosLinhaValidadosService.ValidarCelulasLinha(worksheet, row, _logService, _validacaoService);
                    
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
                    _logService.RegistrarLog(linha);
                }

                _logService.RegistrarLog("Importação finalizada");
            }
        }
    }
}