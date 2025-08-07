namespace NotaFiscal.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string CpfCnpj { get; set; } = string.Empty;
        public string NomeRazaoSocial { get; set; } = string.Empty;
        public string? NomeFantasia { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;

        public virtual ICollection<Endereco> Enderecos { get; set; } = new List<Endereco>();
        public virtual ICollection<Venda> Vendas { get; set; } = new List<Venda>();
    }
}
