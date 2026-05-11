namespace AfReparosAutomotivos.Models
{
    public class Funcionarios
    {
        public int idFuncionario { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int permissao { get; set; }
        public string usuario { get; set; } = string.Empty;
        public int statusFunc { get; set; }
    }
}
