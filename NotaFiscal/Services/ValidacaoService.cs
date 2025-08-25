using NotaFiscal.Models.Enums;
using System.Globalization;
using System.Text.RegularExpressions;

namespace NotaFiscal.Services
{
    /// <summary>
    /// Serviço responsável por validações e transformações de dados da planilha
    /// </summary>
    public class ValidacaoService
    {
        
        public string LimparTelefone(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return string.Empty;
            
            // Primeiro verifica se contém apenas números e caracteres de formatação válidos (+ - ( ) espaços)
            if (!Regex.IsMatch(valor, @"^[\d\+\-\(\)\s]+$"))
            {
                throw new ArgumentException($"Telefone contém caracteres inválidos: '{valor}'. Apenas números e caracteres de formatação (+ - ( )) são permitidos.");
            }
            
            var numeroLimpo = Regex.Replace(valor, @"[^\d]", "");
            
            // Validação: telefone deve ter pelo menos 10 dígitos (permite código do país)
            if (numeroLimpo.Length > 0 && numeroLimpo.Length < 10)
            {
                throw new ArgumentException($"Telefone inválido após limpeza: '{valor}' -> '{numeroLimpo}'. Deve ter pelo menos 10 dígitos.");
            }
            
            return numeroLimpo;
        }

        public string LimparCpfCnpj(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return string.Empty;
            
            // Primeiro verifica se contém apenas números e caracteres de formatação válidos (. - / espaços)
            if (!Regex.IsMatch(valor, @"^[\d\.\-\/\s]+$"))
            {
                throw new ArgumentException($"CPF/CNPJ contém caracteres inválidos: '{valor}'. Apenas números e caracteres de formatação (. - /) são permitidos.");
            }
            
            var numeroLimpo = Regex.Replace(valor, @"[^\d]", "");
            
            // Validação: CPF deve ter 11 dígitos, CNPJ deve ter 14 dígitos
            if (numeroLimpo.Length > 0 && numeroLimpo.Length != 11 && numeroLimpo.Length != 14)
            {
                throw new ArgumentException($"CPF/CNPJ inválido após limpeza: '{valor}' -> '{numeroLimpo}'. CPF deve ter 11 dígitos ou CNPJ deve ter 14 dígitos.");
            }
            
            return numeroLimpo;
        }

        public FormaPagamento ParseFormaPagamento(string formaPagamentoStr)
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

        public TipoEndereco ParseTipoEndereco(string tipoEnderecoStr)
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

        public bool TryParseExcelDate(object? value, out DateTime date)
        {
            date = default;
            if (value is null) return false;

            try
            {
                switch (value)
                {
                    case DateTime dt:
                        date = dt.Date; return true;
                    case double d:
                        date = DateTime.FromOADate(d).Date; return true;
                    case float f:
                        date = DateTime.FromOADate(f).Date; return true;
                    case int i:
                        date = DateTime.FromOADate(i).Date; return true;
                    case long l:
                        date = DateTime.FromOADate(l).Date; return true;
                    case decimal m:
                        date = DateTime.FromOADate((double)m).Date; return true;
                    case string s:
                        var text = s.Trim();
                        if (string.IsNullOrEmpty(text)) return false;

                        // 1) Tenta como número OADate (excel serial)
                        if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var dn))
                        {
                            date = DateTime.FromOADate(dn).Date; return true;
                        }

                        // 2) Tenta com culturas e formatos comuns
                        var cultures = new[] { new CultureInfo("pt-BR"), CultureInfo.CurrentCulture, CultureInfo.InvariantCulture };
                        var formats = new[]
                        {
                            "dd/MM/yyyy","d/M/yyyy","dd-MM-yyyy","d-M-yyyy",
                            "yyyy-MM-dd","yyyy/MM/dd","M/d/yyyy","MM/dd/yyyy",
                            "ddMMyyyy","yyyyMMdd"
                        };

                        foreach (var culture in cultures)
                        {
                            if (DateTime.TryParse(text, culture, DateTimeStyles.None, out var dtParsed))
                            {
                                date = dtParsed.Date; return true;
                            }
                            if (DateTime.TryParseExact(text, formats, culture, DateTimeStyles.None, out var dtExact))
                            {
                                date = dtExact.Date; return true;
                            }
                        }
                        break;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        public bool TryParseDecimal(object? value, out decimal result)
        {
            result = default;
            if (value is null) return false;

            switch (value)
            {
                case decimal dec:
                    result = dec; return true;
                case double d:
                    result = Convert.ToDecimal(d, CultureInfo.InvariantCulture); return true;
                case float f:
                    result = Convert.ToDecimal(f, CultureInfo.InvariantCulture); return true;
                case int i:
                    result = i; return true;
                case long l:
                    result = l; return true;
                case string s:
                    var text = s.Trim();
                    if (string.IsNullOrEmpty(text)) return false;
                    // Tenta nas principais culturas
                    var cultures = new[] { new CultureInfo("pt-BR"), CultureInfo.CurrentCulture, CultureInfo.InvariantCulture };
                    foreach (var culture in cultures)
                    {
                        if (decimal.TryParse(text, NumberStyles.Number | NumberStyles.AllowCurrencySymbol, culture, out var parsed))
                        {
                            result = parsed; return true;
                        }
                    }
                    return false;
                default:
                    return false;
            }
        }
    }
}
