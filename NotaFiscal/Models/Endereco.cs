using NotaFiscal.Models.Enums;

namespace NotaFiscal.Models
{
    public class Endereco
    {
        public int Id { get; set; }
        public int IdCliente { get; set; }
        public TipoEndereco TipoEndereco { get; set; }
        public string Logradouro { get; set; } = string.Empty;

        public virtual Cliente Cliente { get; set; } = null!;
        public virtual ICollection<Venda> Vendas { get; set; } = new List<Venda>();
    }
}
