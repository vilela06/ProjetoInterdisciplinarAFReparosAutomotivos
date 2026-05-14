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

        public int clienteId
        {
            get => idCliente;
            set => idCliente = value;
        }

        public int funcionarioId
        {
            get => idFuncionario;
            set => idFuncionario = value;
        }

        [Display(Name = "Data de Criacao")]
        public DateTime dataCriacao { get; set; } = DateTime.Now;

        public DateTime data_criacao
        {
            get => dataCriacao;
            set => dataCriacao = value;
        }

        [Display(Name = "Data de Entrega")]
        public DateTime? dataEntrega { get; set; }

        public DateTime? data_entrega
        {
            get => dataEntrega;
            set => dataEntrega = value;
        }

        [Display(Name = "Status")]
        public int status { get; set; } = 1;

        public int statusOrc
        {
            get => status;
            set => status = value;
        }

        [Display(Name = "Total")]
        public decimal total { get; set; }

        [Display(Name = "Forma de Pagamento")]
        public string formaPagamento { get; set; } = string.Empty;

        public string? forma_pgto
        {
            get => formaPagamento;
            set => formaPagamento = value ?? string.Empty;
        }

        [Display(Name = "Parcelas")]
        public int parcelas { get; set; }

        [Display(Name = "Nome do Cliente")]
        public string nome { get; set; } = string.Empty;

        [Display(Name = "Nome do Funcionario")]
        public string nomeFunc { get; set; } = string.Empty;

        public int? idCli { get; set; }

        [Display(Name = "Documento do Cliente (CPF/CNPJ)")]
        public string DocumentoCli { get; set; } = string.Empty;

        [Display(Name = "Telefone")]
        public string TelefoneCli { get; set; } = string.Empty;

        [Display(Name = "Celular")]
        [Required(ErrorMessage = "Informe o celular.")]
        public string CelularCli { get; set; } = string.Empty;

        [Display(Name = "Endereco")]
        public string EnderecoCli { get; set; } = string.Empty;

        public int? idVeiculo { get; set; }

        public int veiculoId
        {
            get => idVeiculo ?? 0;
            set => idVeiculo = value;
        }

        [Display(Name = "Placa do Veiculo")]
        public string Placa { get; set; } = string.Empty;

        [Display(Name = "Marca")]
        public string Marca { get; set; } = string.Empty;

        [Display(Name = "Modelo")]
        public string Modelo { get; set; } = string.Empty;

        [Display(Name = "Servico")]
        public int IdServico { get; set; }

        [Display(Name = "Descricao")]
        public string Descricao { get; set; } = string.Empty;

        [Display(Name = "Preco Base")]
        public decimal? PrecoBase { get; set; }

        public List<ItemViewModel> ServicosAssociados { get; set; } = new();

        public IEnumerable<SelectListItem> ServicosDisponiveis { get; set; } = new List<SelectListItem>();

        public IEnumerable<SelectListItem> FuncionariosDisponiveis { get; set; } = new List<SelectListItem>();

        public IEnumerable<SelectListItem> PecasDisponiveis { get; set; } = new List<SelectListItem>();
    }
}
