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

        public async Task ImportarPlanilha(string caminhoDoArquivo)
        {
            // Define o contexto da licença do EPPlus
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var fileInfo = new FileInfo(caminhoDoArquivo);

            using (var package = new ExcelPackage(fileInfo))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++) // Começa da linha 2 para ignorar o cabeçalho
                {
                    var cpfCnpjLimpo = LimparNumeros(worksheet.Cells[row, 2].Value?.ToString());

                    if (string.IsNullOrWhiteSpace(cpfCnpjLimpo)) continue;

                    // 1. Procura se o cliente já existe
                    var cliente = await _context.Clientes
                                                .Include(c => c.Enderecos)
                                                .FirstOrDefaultAsync(c => c.CpfCnpj == cpfCnpjLimpo);

                    // 2. Se não existir, cria um novo cliente
                    if (cliente == null)
                    {
                        cliente = new Cliente
                        {
                            CpfCnpj = cpfCnpjLimpo,
                            NomeRazaoSocial = worksheet.Cells[row, 3].Value?.ToString()?.Trim(),
                            NomeFantasia = worksheet.Cells[row, 4].Value?.ToString()?.Trim(),
                            Email = worksheet.Cells[row, 5].Value?.ToString()?.Trim(),
                            Telefone = LimparNumeros(worksheet.Cells[row, 6].Value?.ToString())
                        };
                        _context.Clientes.Add(cliente);
                    }

                    // 3. Verifica o endereço
                    var logradouro = worksheet.Cells[row, 7].Value?.ToString()?.Trim();
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
                            // A coleção de endereços do cliente já está sendo rastreada pelo EF Core,
                            // então apenas adicionar a ela é suficiente.
                            cliente.Enderecos.Add(novoEndereco);
                        }
                    }

                    // 4. Cadastra a venda
                    var venda = new Venda
                    {
                        Id = int.Parse(worksheet.Cells[row, 9].Value.ToString()),
                        Data = DateTime.Parse(worksheet.Cells[row, 10].Value.ToString()),
                        ValorTotal = decimal.Parse(worksheet.Cells[row, 11].Value.ToString()),
                        FormaPagamento = ParseFormaPagamento(worksheet.Cells[row, 12].Value.ToString()),
                        Cliente = cliente // Associa a venda ao cliente (novo ou existente)
                    };
                    _context.Vendas.Add(venda);
                }

                // Salva todas as alterações de uma vez
                await _context.SaveChangesAsync();
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
                case "cartão":
                    return FormaPagamento.Cartao;
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