using Microsoft.EntityFrameworkCore;
using NotaFiscal.Data;
using NotaFiscal.Models;
using NotaFiscal.Models.Enums;
using OfficeOpenXml;
using System.Globalization;
using System.Text.RegularExpressions;

namespace NotaFiscal.Controllers
{
    public class PlanilhaController
    {
        private readonly AppDbContext _context;

        public PlanilhaController(AppDbContext context)
        {
            _context = context;
        }

        public async Task ImportarPlanilha(string caminhoDoArquivo, IProgress<string>? progress = null)
        {
            ExcelPackage.License.SetNonCommercialPersonal("Teste");
            var fileInfo = new FileInfo(caminhoDoArquivo);

            int linhasProcessadas = 0;
            int linhasPuladas = 0;
            int linhasInseridas = 0;

            try
            {
                progress?.Report("Iniciando importação da planilha...");

                using (var package = new ExcelPackage(fileInfo))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension.Rows;

                    progress?.Report($"Total de linhas encontradas: {rowCount - 1} (ignorando cabeçalho)");

                    for (int row = 2; row <= rowCount; row++) // Começa da linha 2 para ignorar o cabeçalho
                    {
                        linhasProcessadas++;
                        
                        var clienteId = int.Parse(worksheet.Cells[row, 1].Value?.ToString() ?? "0");

                        // 1. Procura se o cliente já existe (primeiro no rastreamento local, depois no banco)
                        var cliente = _context.Clientes.Local.FirstOrDefault(c => c.Id == clienteId)
                                        ?? await _context.Clientes
                                                .Include(c => c.Enderecos)
                                                .FirstOrDefaultAsync(c => c.Id == clienteId);

                        // 2. Se não existir, cria um novo cliente
                        if (cliente == null)
                        {
                            cliente = new Cliente
                            {
                                Id = clienteId,
                                CpfCnpj = LimparNumeros(worksheet.Cells[row, 2].Value?.ToString() ?? ""),
                                NomeRazaoSocial = worksheet.Cells[row, 3].Value?.ToString()?.Trim() ?? "",
                                NomeFantasia = worksheet.Cells[row, 4].Value?.ToString()?.Trim(),
                                Email = worksheet.Cells[row, 5].Value?.ToString()?.Trim() ?? "",
                                Telefone = LimparNumeros(worksheet.Cells[row, 6].Value?.ToString() ?? "")
                            };
                            _context.Clientes.Add(cliente);
                        }

                        // 3. Verifica o endereço
                        var logradouro = worksheet.Cells[row, 7].Value?.ToString()?.Trim();
                        Endereco enderecoParaVenda = null;

                        if (!string.IsNullOrWhiteSpace(logradouro))
                        {
                            var enderecoExistente = cliente.Enderecos.FirstOrDefault(e => e.Logradouro.Equals(logradouro, StringComparison.OrdinalIgnoreCase));

                        if (enderecoExistente == null)
                        {
                            var novoEndereco = new Endereco
                            {
                                TipoEndereco = ParseTipoEndereco(worksheet.Cells[row, 8].Value.ToString()),
                                Logradouro = logradouro,
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

                        // 4. Cadastra a venda
                        var vendaId = int.Parse(worksheet.Cells[row, 9].Value?.ToString() ?? "0");
                        var venda = _context.Vendas.Local.FirstOrDefault(v => v.Id == vendaId)
                                        ?? await _context.Vendas.AsNoTracking().FirstOrDefaultAsync(v => v.Id == vendaId);

                        if (venda != null)
                        {
                            progress?.Report($"Linha {row}: Venda ID {vendaId} já existe - pulando");
                            linhasPuladas++;
                            continue;
                        }

                        venda = new Venda
                        {
                            Id = vendaId,
                            Data = worksheet.Cells[row, 10].GetValue<DateTime>().Date,
                            ValorTotal = decimal.Parse(worksheet.Cells[row, 11].Value?.ToString() ?? "0"),
                            FormaPagamento = ParseFormaPagamento(worksheet.Cells[row, 12].Value?.ToString() ?? ""),
                            Cliente = cliente,
                            Endereco = enderecoParaVenda
                        };

                        _context.Vendas.Add(venda);
                        linhasInseridas++;

                        // Log de progresso a cada 10 linhas
                        if (linhasProcessadas % 10 == 0)
                        {
                            progress?.Report($"Processadas {linhasProcessadas} de {rowCount - 1} linhas...");
                        }
                    }

                    // Salva todas as alterações de uma vez
                    progress?.Report("Salvando dados no banco...");
                    await _context.SaveChangesAsync();

                    // Log final com resumo
                    progress?.Report($"Resumo da importação:");
                    progress?.Report($"- Total de linhas processadas: {linhasProcessadas}");
                    progress?.Report($"- Linhas inseridas com sucesso: {linhasInseridas}");
                    progress?.Report($"- Linhas puladas (duplicadas): {linhasPuladas}");
                }
            }
            catch (Exception ex)
            {
                progress?.Report($"ERRO durante a importação: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Remove todos os caracteres não numéricos de uma string.
        /// </summary>
        private string LimparNumeros(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return string.Empty;
            return Regex.Replace(valor, @"[^\d]", "");
        }

        /// <summary>
        /// Converte a string da forma de pagamento para o enum correspondente.
        /// </summary>
        private FormaPagamento ParseFormaPagamento(string formaPagamentoStr)
        {
            if (string.IsNullOrWhiteSpace(formaPagamentoStr))
                return default;

            // Remove acentos e espaços para uma comparação mais robusta
            var textoNormalizado = string.Concat(formaPagamentoStr.Normalize(System.Text.NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)).Replace(" ", "").ToLower();

            switch (textoNormalizado)
            {
                case "cartao":
                    return FormaPagamento.Cartao;
                case "transferencia":
                    return FormaPagamento.Transferencia;
                case "pix":
                    return FormaPagamento.Pix;
                case "boleto":
                    return FormaPagamento.Boleto;
                case "dinheiro":
                    return FormaPagamento.Dinheiro;
                default:
                    // Tenta fazer o parse direto, caso o valor na planilha seja o nome exato do enum
                    if (Enum.TryParse<FormaPagamento>(formaPagamentoStr, true, out var resultado))
                    {
                        return resultado;
                    }
                    throw new ArgumentException($"Forma de pagamento desconhecida: {formaPagamentoStr}");
            }
        }

        /// <summary>
        /// Converte a string do tipo de endereço para o enum correspondente.
        /// </summary>
        private TipoEndereco ParseTipoEndereco(string tipoEnderecoStr)
        {
            if (string.IsNullOrWhiteSpace(tipoEnderecoStr))
                return default;

            // Remove acentos e espaços para uma comparação mais robusta
            var textoNormalizado = string.Concat(tipoEnderecoStr.Normalize(System.Text.NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)).Replace(" ", "").ToLower();

            switch (textoNormalizado)
            {
                case "cobranca":
                    return TipoEndereco.Cobranca;
                case "cobrança":
                    return TipoEndereco.Cobranca;
                case "entrega":
                    return TipoEndereco.Entrega;
                default:
                    // Tenta fazer o parse direto, caso o valor na planilha seja o nome exato do enum
                    if (Enum.TryParse<TipoEndereco>(tipoEnderecoStr, true, out var resultado))
                    {
                        return resultado;
                    }
                    throw new ArgumentException($"Tipo de endereço desconhecido: {tipoEnderecoStr}");
            }
        }
    }
}