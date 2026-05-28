using System.ComponentModel.DataAnnotations;

namespace AfReparosAutomotivos.Models
{
    public class Clientes
    {
        [Display(Name = "ID")]
        public int id { get; set; }

        [Display(Name = "Nome")]
        [Required(ErrorMessage = "Informe o nome.")]
        [RegularExpression(@"^[^\d]+$", ErrorMessage = "O nome nao pode conter numeros.")]
        public string nome { get; set; } = string.Empty;

        [Display(Name = "Documento")]
        [Required(ErrorMessage = "Informe o documento.")]
        public string documento { get; set; } = string.Empty;

        [Display(Name = "Telefone")]
        [StringLength(14)]
        [RegularExpression(@"^[0-9()\-\s]*$", ErrorMessage = "O telefone nao pode conter letras.")]
        public string? telefone { get; set; }

        [Display(Name = "Celular")]
        [Required(ErrorMessage = "Informe o celular.")]
        [StringLength(15)]
        [RegularExpression(@"^[0-9()\-\s]+$", ErrorMessage = "O celular nao pode conter letras.")]
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
        [RegularExpression(@"^(\d+|s/n|S/N)$", ErrorMessage = "O numero deve conter apenas numeros ou s/n.")]
        public string numero { get; set; } = string.Empty;
        public string cidade { get; set; } = string.Empty;
        [RegularExpression(@"(?i)^(AC|AL|AP|AM|BA|CE|DF|ES|GO|MA|MT|MS|MG|PA|PB|PR|PE|PI|RJ|RN|RS|RO|RR|SC|SP|SE|TO)$", ErrorMessage = "Informe uma UF valida.")]
        public string estado { get; set; } = string.Empty;
        [RegularExpression(@"^[0-9\-]+$", ErrorMessage = "O CEP nao pode conter letras.")]
        public string cep { get; set; } = string.Empty;

        [Display(Name = "Tipo de Documento")]
        public char? tipo_doc { get; set; }
    }
}
