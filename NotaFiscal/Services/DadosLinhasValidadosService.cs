using NotaFiscal.Models.Enums;
using OfficeOpenXml;

namespace NotaFiscal.Services
{
    public partial class DadosLinhaValidadosService
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

        /// <summary>
        /// Valida e converte os dados de uma linha da planilha
        /// </summary>
        public static DadosLinhaValidadosService ValidarCelulasLinha(ExcelWorksheet worksheet, int row, LogService logService, ValidacaoService validacaoService)
        {
            var dados = new DadosLinhaValidadosService();

            try
            {
                // Coluna 1: ID Cliente
                var idClienteStr = worksheet.Cells[row, 1].Value?.ToString()?.Trim() ?? "";
                if (string.IsNullOrEmpty(idClienteStr) || !int.TryParse(idClienteStr, out int clienteId))
                {
                    logService.RegistrarErroCampo(row, "ID Cliente", idClienteStr, "ID Cliente deve ser um número válido");
                    dados.TemErros = true;
                    return dados;
                }
                dados.ClienteId = clienteId;

                // Coluna 2: CPF/CNPJ
                var cpfCnpjStr = worksheet.Cells[row, 2].Value?.ToString() ?? "";
                try
                {
                    dados.CpfCnpj = validacaoService.LimparCpfCnpj(cpfCnpjStr);
                }
                catch (ArgumentException ex)
                {
                    logService.RegistrarErroCampo(row, "CPF/CNPJ", cpfCnpjStr, ex.Message);
                    dados.TemErros = true;
                }
                if (string.IsNullOrEmpty(dados.CpfCnpj))
                {
                    logService.RegistrarErroCampo(row, "CPF/CNPJ", cpfCnpjStr, "CPF/CNPJ é obrigatório");
                    dados.TemErros = true;
                }

                // Coluna 3: Nome/Razão Social
                dados.NomeRazaoSocial = worksheet.Cells[row, 3].Value?.ToString()?.Trim() ?? "";
                if (string.IsNullOrEmpty(dados.NomeRazaoSocial))
                {
                    logService.RegistrarErroCampo(row, "Nome/Razão Social", dados.NomeRazaoSocial, "Nome/Razão Social é obrigatório");
                    dados.TemErros = true;
                }

                // Coluna 4: Nome Fantasia (opcional)
                dados.NomeFantasia = worksheet.Cells[row, 4].Value?.ToString()?.Trim();

                // Coluna 5: Email
                dados.Email = worksheet.Cells[row, 5].Value?.ToString()?.Trim() ?? "";
                if (string.IsNullOrEmpty(dados.Email))
                {
                    logService.RegistrarErroCampo(row, "Email", dados.Email, "Email é obrigatório");
                    dados.TemErros = true;
                }

                // Coluna 6: Telefone
                var telefoneStr = worksheet.Cells[row, 6].Value?.ToString() ?? "";
                try
                {
                    dados.Telefone = validacaoService.LimparTelefone(telefoneStr);
                }
                catch (ArgumentException ex)
                {
                    logService.RegistrarErroCampo(row, "Telefone", telefoneStr, ex.Message);
                    dados.TemErros = true;
                }
                if (string.IsNullOrEmpty(dados.Telefone))
                {
                    logService.RegistrarErroCampo(row, "Telefone", telefoneStr, "Telefone é obrigatório");
                    dados.TemErros = true;
                }

                // Coluna 7: Logradouro (opcional)
                dados.Logradouro = worksheet.Cells[row, 7].Value?.ToString()?.Trim();

                // Coluna 8: Tipo Endereço
                var tipoEnderecoStr = worksheet.Cells[row, 8].Value?.ToString()?.Trim() ?? "";
                try
                {
                    dados.TipoEndereco = validacaoService.ParseTipoEndereco(tipoEnderecoStr);
                }
                catch (ArgumentException ex)
                {
                    logService.RegistrarErroCampo(row, "Tipo Endereço", tipoEnderecoStr, ex.Message);
                    dados.TemErros = true;
                }

                // Coluna 9: ID Venda
                var idVendaStr = worksheet.Cells[row, 9].Value?.ToString()?.Trim() ?? "";
                if (string.IsNullOrEmpty(idVendaStr) || !int.TryParse(idVendaStr, out int vendaId))
                {
                    logService.RegistrarErroCampo(row, "ID Venda", idVendaStr, "ID Venda deve ser um número válido");
                    dados.TemErros = true;
                    return dados;
                }
                dados.VendaId = vendaId;

                // Coluna 10: Data Venda
                var cellData = worksheet.Cells[row, 10].Value;
                if (validacaoService.TryParseExcelDate(cellData, out var dataVenda))
                {
                    dados.DataVenda = dataVenda.Date;
                }
                else
                {
                    var dataStr = worksheet.Cells[row, 10].Value?.ToString() ?? "";
                    logService.RegistrarErroCampo(row, "Data Venda", dataStr, "Data da venda deve ser uma data válida");
                    dados.TemErros = true;
                }

                // Coluna 11: Valor Total
                var valorCell = worksheet.Cells[row, 11].Value;
                if (validacaoService.TryParseDecimal(valorCell, out var valorTotal))
                {
                    dados.ValorTotal = valorTotal;
                }
                else
                {
                    var valorTotalStr = worksheet.Cells[row, 11].Value?.ToString()?.Trim() ?? "";
                    logService.RegistrarErroCampo(row, "Valor Total", valorTotalStr, "Valor Total deve ser um número válido");
                    dados.TemErros = true;
                }

                // Coluna 12: Forma Pagamento
                var formaPagamentoStr = worksheet.Cells[row, 12].Value?.ToString()?.Trim() ?? "";
                try
                {
                    dados.FormaPagamento = validacaoService.ParseFormaPagamento(formaPagamentoStr);
                }
                catch (ArgumentException ex)
                {
                    logService.RegistrarErroCampo(row, "Forma Pagamento", formaPagamentoStr, ex.Message);
                    dados.TemErros = true;
                }
            }
            catch (Exception ex)
            {
                logService.RegistrarErroCampo(row, "Erro geral", "", $"Erro inesperado ao validar linha: {ex.Message}");
                dados.TemErros = true;
            }

            return dados;
        }
    }
}