using NotaFiscal.Models.Enums;

namespace NotaFiscal.Models
{
    public class Venda
    {
        public int Id { get; set; }
        public int IdEndereco { get; set; }
        public int IdCliente { get; set; }
        public FormaPagamento FormaPagamento { get; set; }
        public DateTime Data { get; set; }
        public decimal ValorTotal { get; set; }

        public Cliente Cliente { get; set; } = null!;
        public Endereco Endereco { get; set; } = null!;
    }
}
