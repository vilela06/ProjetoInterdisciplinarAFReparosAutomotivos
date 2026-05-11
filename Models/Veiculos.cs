namespace AfReparosAutomotivos.Models
{
    public class Veiculos
    {
        public int? id { get; set; }
        public int? idVeiculo
        {
            get => id;
            set => id = value;
        }
        public int clienteId { get; set; }
        public string placa { get; set; } = string.Empty;
        public string marca { get; set; } = string.Empty;
        public string modelo { get; set; } = string.Empty;
    }
}
