using System.ComponentModel.DataAnnotations;

namespace AfReparosAutomotivos.Models
{
    public class ItemViewModel
    {
        public int idItem { get; set; }

        [Required]
        [Display(Name = "Servico")]
        public int idServico { get; set; }

        public string? novoServicoDescricao { get; set; }

        [Required]
        [Display(Name = "Funcionario")]
        public int funcionarioId { get; set; }

        [Required]
        [Display(Name = "Quantidade")]
        [Range(1, int.MaxValue)]
        public int qtd { get; set; }

        public int? pecaId { get; set; }

        [Range(1, int.MaxValue)]
        public int qtdPeca { get; set; } = 1;

        public decimal valorPeca { get; set; }

        public DateTime? data_entrega { get; set; }

        [Display(Name = "Preco Base")]
        public decimal preco { get; set; }

        public string? descricao { get; set; }

        public string? observacao { get; set; }

        [Display(Name = "Taxa")]
        public decimal taxa { get; set; }

        [Display(Name = "Desconto")]
        public decimal desconto { get; set; }
    }
}
