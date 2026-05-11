namespace AfReparosAutomotivos.Models
{
    public class OrcamentosFilterViewModel
    {
        public int? statusId { get; set; }

        public string? cpf { get; set; }

        public string? nome { get; set; }

        public DateTime? dataCriacao { get; set; }

        public DateTime? dataEntrega { get; set; }

        public string? formaPagamento { get; set; }

        public int? parcelas { get; set; }

        public decimal? preco { get; set; }
    }
}