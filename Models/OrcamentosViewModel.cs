using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AfReparosAutomotivos.Models
{
    public class OrcamentosViewModel
    {
        [Display(Name = "ID do Orcamento")]
        public int idOrcamento { get; set; }

        [Display(Name = "ID do Funcionario")]
        public int idFuncionario { get; set; }

        [Display(Name = "ID do Cliente")]
        public int idCliente { get; set; }

        [Display(Name = "Data de Criacao")]
        public DateTime dataCriacao { get; set; } = DateTime.Now;

        [Display(Name = "Data de Entrega")]
        public DateTime? dataEntrega { get; set; }

        [Display(Name = "Status")]
        [Range(1, 5, ErrorMessage = "Informe um status valido.")]
        public int status { get; set; } = 1;

        [Display(Name = "Total")]
        public decimal total { get; set; }

        [Display(Name = "Forma de Pagamento")]
        public string formaPagamento { get; set; } = string.Empty;

        [Display(Name = "Parcelas")]
        public int parcelas { get; set; }

        [Display(Name = "Nome do Cliente")]
        [Required(ErrorMessage = "Informe o nome do cliente.")]
        [RegularExpression(@"^[^\d]+$", ErrorMessage = "O nome nao pode conter numeros.")]
        public string nome { get; set; } = string.Empty;

        [Display(Name = "Nome do Funcionario")]
        public string nomeFunc { get; set; } = string.Empty;

        public int? idCli { get; set; }

        [Display(Name = "Documento do Cliente (CPF/CNPJ)")]
        [Required(ErrorMessage = "Informe o documento do cliente.")]
        public string DocumentoCli { get; set; } = string.Empty;

        [Display(Name = "Telefone")]
        [RegularExpression(@"^[0-9()\-\s]*$", ErrorMessage = "O telefone nao pode conter letras.")]
        public string? TelefoneCli { get; set; }

        [Display(Name = "Celular")]
        [Required(ErrorMessage = "Informe o celular.")]
        [RegularExpression(@"^[0-9()\-\s]+$", ErrorMessage = "O celular nao pode conter letras.")]
        public string CelularCli { get; set; } = string.Empty;

        [Display(Name = "E-mail")]
        [Required(ErrorMessage = "Informe o e-mail.")]
        [EmailAddress(ErrorMessage = "Informe um e-mail valido.")]
        public string EmailCli { get; set; } = string.Empty;

        [Display(Name = "Endereco")]
        [Required(ErrorMessage = "Informe o endereco do cliente.")]
        public string EnderecoCli { get; set; } = string.Empty;

        public int? idVeiculo { get; set; }

        [Display(Name = "Placa do Veiculo")]
        [Required(ErrorMessage = "Informe a placa do veiculo.")]
        public string Placa { get; set; } = string.Empty;

        [Display(Name = "Marca")]
        [Required(ErrorMessage = "Informe a marca do veiculo.")]
        public string Marca { get; set; } = string.Empty;

        [Display(Name = "Modelo")]
        [Required(ErrorMessage = "Informe o modelo do veiculo.")]
        public string Modelo { get; set; } = string.Empty;

        [Display(Name = "Cor")]
        [Required(ErrorMessage = "Informe a cor do veiculo.")]
        public string Cor { get; set; } = string.Empty;

        [Display(Name = "Ano")]
        [Required(ErrorMessage = "Informe o ano do veiculo.")]
        [Range(1886, 9999, ErrorMessage = "Informe um ano valido.")]
        public int? Ano { get; set; }

        public List<ItemViewModel> ServicosAssociados { get; set; } = new();

        public IEnumerable<SelectListItem> ServicosDisponiveis { get; set; } = new List<SelectListItem>();

        public IEnumerable<SelectListItem> FuncionariosDisponiveis { get; set; } = new List<SelectListItem>();

        public IEnumerable<SelectListItem> PecasDisponiveis { get; set; } = new List<SelectListItem>();
    }
}
