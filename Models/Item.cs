using System.ComponentModel.DataAnnotations;

namespace AfReparosAutomotivos.Models
{
    public class Item
    {
        [Required]
        public int orcamentoId { get; set; }

        [Required]
        public int servicoId { get; set; }

        [Required]
        public int funcionarioId { get; set; }

        public int? pecaId { get; set; }

        [Required]
        [Display(Name = "Preco")]
        public decimal preco { get; set; }

        public decimal? desconto { get; set; }

        [Display(Name = "Data de Entrega")]
        public DateTime? dataEntrega { get; set; }

        public int idOrcamento
        {
            get => orcamentoId;
            set => orcamentoId = value;
        }

        public int idServico
        {
            get => servicoId;
            set => servicoId = value;
        }

        public DateTime? data_entrega
        {
            get => dataEntrega;
            set => dataEntrega = value;
        }

        public int idItem { get; set; }
        public int idVeiculo { get; set; }
        public int qtd { get; set; } = 1;
        public string? descricao { get; set; }
        public decimal? taxa { get; set; } = 0m;
    }
}
