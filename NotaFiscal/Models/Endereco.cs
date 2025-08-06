namespace NotaFiscal.Models
{
    public class Endereco
    {
        public int Id { get; set; }
        public int IdCliente { get; set; }
        public string TipoEndereco { get; set; } = string.Empty;
        public string Logradouro { get; set; } = string.Empty;

        public virtual Cliente Cliente { get; set; } = null!;
    }
}
