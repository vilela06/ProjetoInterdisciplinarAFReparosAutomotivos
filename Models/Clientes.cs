using System.ComponentModel.DataAnnotations;

namespace AfReparosAutomotivos.Models
{
    public class Clientes
    {
        [Display(Name = "ID")]
        public int id { get; set; }

        [Display(Name = "Nome")]
        [Required(ErrorMessage = "Informe o nome.")]
        public string nome { get; set; } = string.Empty;

        [Display(Name = "Documento")]
        [Required(ErrorMessage = "Informe o documento.")]
        public string documento { get; set; } = string.Empty;

        [Display(Name = "Telefone")]
        [StringLength(14)]
        public string telefone { get; set; } = string.Empty;

        [Display(Name = "Celular")]
        [Required(ErrorMessage = "Informe o celular.")]
        [StringLength(15)]
        public string celular { get; set; } = string.Empty;

        [Display(Name = "E-mail")]
        [Required(ErrorMessage = "Informe o e-mail.")]
        [StringLength(50)]
        public string email { get; set; } = string.Empty;

        public int statusCli { get; set; } = 1;
        public string chaveCli { get; set; } = string.Empty;

        [Display(Name = "Endereco")]
        public string endereco { get; set; } = string.Empty;

        public string logradouro { get; set; } = string.Empty;
        public string numero { get; set; } = string.Empty;
        public string cidade { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string cep { get; set; } = string.Empty;

        [Display(Name = "Tipo de Documento")]
        public char? tipo_doc { get; set; }
    }
}
