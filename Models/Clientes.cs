using System.ComponentModel.DataAnnotations;

namespace AfReparosAutomotivos.Models
{
    public class Clientes
    {
        // Dados do Cliente
        [Display(Name = "ID")]
        public int id { get; set; }
        [Display(Name = "Nome")]
        public string nome { get; set; } = string.Empty;
        [Display(Name = "Documento")]
        public string documento { get; set; } = string.Empty;
        [Display(Name = "Telefone")]
        public string telefone { get; set; } = string.Empty;
        [Display(Name = "Endereço")]
        public string endereco { get; set; } = string.Empty;
        [Display(Name = "Tipo de Documento")]
        public char? tipo_doc { get; set; }
    }
}
