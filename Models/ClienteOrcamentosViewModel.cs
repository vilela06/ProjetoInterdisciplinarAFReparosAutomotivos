namespace AfReparosAutomotivos.Models
{
    public class ClienteOrcamentosViewModel
    {
        public string ChaveAcesso { get; set; } = string.Empty;
        public List<OrcamentosViewModel> Orcamentos { get; set; } = new();
        public bool PesquisaRealizada { get; set; }
    }
}
