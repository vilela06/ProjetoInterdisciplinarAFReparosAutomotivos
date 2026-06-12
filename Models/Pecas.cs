using System.ComponentModel.DataAnnotations;

namespace AfReparosAutomotivos.Models
{
    public class Pecas
    {
        public int idPeca { get; set; }

        public int funcionarioId { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Peça")]
        public string nome { get; set; } = string.Empty;

        [Range(0, 99999999.99)]
        [Display(Name = "Valor Base")]
        public decimal valor { get; set; }

        [Range(0, int.MaxValue)]
        [Display(Name = "Estoque")]
        public int qtdEsto { get; set; }

        [Display(Name = "FuncionÃ¡rio")]
        public string nomeFuncionario { get; set; } = string.Empty;
    }
}
