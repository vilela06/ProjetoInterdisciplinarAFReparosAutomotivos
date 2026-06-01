using System.ComponentModel.DataAnnotations;

namespace AfReparosAutomotivos.Models
{
    public class EmpresaViewModel
    {
        public int idFuncionario { get; set; }

        [Required(ErrorMessage = "Informe o nome da empresa.")]
        [StringLength(50)]
        [Display(Name = "Nome da empresa")]
        public string nomeEmpresa { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o CNPJ.")]
        [StringLength(18)]
        [Display(Name = "CNPJ")]
        public string cnpj { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o e-mail.")]
        [StringLength(50)]
        [EmailAddress(ErrorMessage = "Informe um e-mail valido.")]
        [Display(Name = "E-mail")]
        public string email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o celular.")]
        [StringLength(15)]
        [RegularExpression(@"^[0-9()\-\s]+$", ErrorMessage = "O celular deve conter apenas numeros.")]
        public string celular { get; set; } = string.Empty;

        [StringLength(150)]
        [Display(Name = "Foto")]
        public string fotoUrl { get; set; } = "/images/logo-af-reparos.png";

        [Required(ErrorMessage = "Informe a rua.")]
        [StringLength(150)]
        public string logradouro { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o numero.")]
        [StringLength(5)]
        [RegularExpression(@"^(\d+|s/n|S/N)$", ErrorMessage = "Use apenas numeros ou s/n.")]
        public string numero { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a cidade.")]
        [StringLength(100)]
        public string cidade { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a UF.")]
        [StringLength(2)]
        public string estado { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o CEP.")]
        [StringLength(9)]
        [RegularExpression(@"^[0-9\-]+$", ErrorMessage = "O CEP nao pode conter letras.")]
        public string cep { get; set; } = string.Empty;

        public bool podeEditar { get; set; }

        public string enderecoCompleto =>
            $"{logradouro}, {numero}, {cidade} - {estado}, {cep}";
    }
}
