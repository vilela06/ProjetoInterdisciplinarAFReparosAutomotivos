using System.ComponentModel.DataAnnotations;

namespace AfReparosAutomotivos.Models
{
    public class Orcamentos
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

        public int veiculoId { get; set; }

        [Display(Name = "Data de Criacao")]
        public DateTime dataCriacao { get; set; }

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
        [Range(1, 5, ErrorMessage = "Informe um status valido.")]
        public int status { get; set; }

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

        [Display(Name = "Documento")]
        public string documento { get; set; } = string.Empty;

        [Display(Name = "ID do Servico")]
        public int? idServico { get; set; }

        [Display(Name = "ID do Veiculo")]
        public int? idVeiculo { get; set; }

        [Display(Name = "Placa")]
        public string placa { get; set; } = string.Empty;

        [Display(Name = "Marca")]
        public string marca { get; set; } = string.Empty;

        [Display(Name = "Modelo")]
        public string modelo { get; set; } = string.Empty;

        public ICollection<Item> Itens { get; set; } = new List<Item>();
    }
}
