using System.ComponentModel.DataAnnotations;

namespace AfReparosAutomotivos.Models
{
    public class ItemViewModel
    {
        public int idItem { get; set; }

        [Required]
        [Display(Name = "Serviço")]
        public int idServico { get; set; }
        
        [Required]
        [Display(Name = "Quantidade")]
        [Range(1, int.MaxValue)]
        public int qtd { get; set; }

        public DateTime? data_entrega { get; set; } 

        [Display(Name = "Preço Base")]
        public decimal preco { get; set; }

        public string? descricao { get; set; }

        public string? observacao { get; set; }

        [Display(Name = "Taxa")]
        public decimal taxa { get; set; }

        [Display(Name = "Desconto")]
        public decimal desconto { get; set; }
    }
}