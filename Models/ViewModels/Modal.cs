namespace AfReparosAutomotivos.Models.ViewModels
{
    /// <summary>
    /// Modelo para modais genéricos.
    /// </summary>
    public class Modal
    {
        public string Title { get; set; } = "";
        public string Mensagem { get; set; } = "";
        public string Css { get; set; } = "";
        public string Id { get; set; } = "generico";
    }
}