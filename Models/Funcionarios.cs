using System.ComponentModel.DataAnnotations;

namespace AfReparosAutomotivos.Models
{
    public class Funcionarios
    {
        public int idFuncionario { get; set; }

        [Required(ErrorMessage = "Informe o nome.")]
        [StringLength(50)]
        [Display(Name = "Funcionario")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o celular.")]
        [StringLength(15)]
        [RegularExpression(@"^[0-9()\-\s]+$", ErrorMessage = "O celular deve conter apenas numeros.")]
        public string celular { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o documento.")]
        [StringLength(18)]
        public string documento { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o tipo do documento.")]
        [RegularExpression("F|J", ErrorMessage = "Selecione um tipo de documento valido.")]
        [Display(Name = "Tipo documento")]
        public string tipo_doc { get; set; } = "F";

        [Range(1, 3, ErrorMessage = "Selecione uma permissao valida.")]
        [Display(Name = "Permissao")]
        public int permissao { get; set; }

        [Required(ErrorMessage = "Informe o usuario.")]
        [StringLength(16)]
        [Display(Name = "Usuario")]
        public string usuario { get; set; } = string.Empty;

        [StringLength(15)]
        [Display(Name = "Senha")]
        public string senha { get; set; } = string.Empty;

        [Range(1, 2, ErrorMessage = "Selecione um status valido.")]
        [Display(Name = "Status")]
        public int statusFunc { get; set; }
    }
}
