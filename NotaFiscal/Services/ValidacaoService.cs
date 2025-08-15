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
        /// <summary>
        /// Remove todos os caracteres não numéricos de uma string e valida se o telefone está no formato correto.
        /// </summary>
        /// <param name="valor">Valor do telefone a ser limpo e validado</param>
        /// <returns>Telefone limpo contendo apenas números</returns>
        /// <exception cref="ArgumentException">Lançada quando o telefone não atende aos critérios mínimos</exception>
        public string LimparTelefone(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return string.Empty;
            
            var numeroLimpo = Regex.Replace(valor, @"[^\d]", "");
            
            // Validação: telefone deve ter pelo menos 10 dígitos (permite código do país)
            if (numeroLimpo.Length > 0 && numeroLimpo.Length < 10)
            {
                throw new ArgumentException($"Telefone inválido após limpeza: '{valor}' -> '{numeroLimpo}'. Deve ter pelo menos 10 dígitos.");
            }
            
            return numeroLimpo;
        }

        /// <summary>
        /// Remove todos os caracteres não numéricos de uma string e valida se o CPF/CNPJ está no formato correto.
        /// </summary>
        /// <param name="valor">Valor do CPF/CNPJ a ser limpo e validado</param>
        /// <returns>CPF/CNPJ limpo contendo apenas números</returns>
        /// <exception cref="ArgumentException">Lançada quando o CPF/CNPJ não atende aos critérios de tamanho</exception>
        public string LimparCpfCnpj(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return string.Empty;
            
            var numeroLimpo = Regex.Replace(valor, @"[^\d]", "");
            
            // Validação: CPF deve ter 11 dígitos, CNPJ deve ter 14 dígitos
            if (numeroLimpo.Length > 0 && numeroLimpo.Length != 11 && numeroLimpo.Length != 14)
            {
                throw new ArgumentException($"CPF/CNPJ inválido após limpeza: '{valor}' -> '{numeroLimpo}'. CPF deve ter 11 dígitos ou CNPJ deve ter 14 dígitos.");
            }
            
            return numeroLimpo;
        }

        /// <summary>
        /// Converte a string da forma de pagamento para o enum correspondente.
        /// </summary>
        /// <param name="formaPagamentoStr">String representando a forma de pagamento</param>
        /// <returns>Enum FormaPagamento correspondente</returns>
        /// <exception cref="ArgumentException">Lançada quando a forma de pagamento é desconhecida</exception>
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

        /// <summary>
        /// Converte a string do tipo de endereço para o enum correspondente.
        /// </summary>
        /// <param name="tipoEnderecoStr">String representando o tipo de endereço</param>
        /// <returns>Enum TipoEndereco correspondente</returns>
        /// <exception cref="ArgumentException">Lançada quando o tipo de endereço é desconhecido</exception>
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
