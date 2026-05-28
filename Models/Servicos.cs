using System.ComponentModel.DataAnnotations;

namespace AfReparosAutomotivos.Models
{
    public class Servicos
    {
        [Display(Name = "ID do Servico")]
        public int IdServico { get; set; }

        [Display(Name = "ID do Orcamento")]
        public int? IdOrcamento { get; set; }

        [Display(Name = "Descricao")]
        [StringLength(50)]
        public string Descricao { get; set; } = string.Empty;

        [Display(Name = "Preco Base")]
        [Range(0, 99999999.99, ErrorMessage = "O preco deve estar entre 0 e 99.999.999,99")]
        public decimal PrecoBase { get; set; }

        public int idServico
        {
            get => IdServico;
            set => IdServico = value;
        }

        public string descricao
        {
            get => Descricao;
            set => Descricao = value;
        }

        public decimal valorBase
        {
            get => PrecoBase;
            set => PrecoBase = value;
        }

        [Display(Name = "Funcionário responsável")]
        public string FuncionarioResponsavel { get; set; } = "Sem responsável";

        [Display(Name = "Status")]
        public string Status { get; set; } = "Catálogo";
    }
}
