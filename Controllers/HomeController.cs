using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AfReparosAutomotivos.Models;
using AfReparosAutomotivos.Interfaces;

namespace AfReparosAutomotivos.Controllers;

/// <summary>
/// Classe padrão do Framework.
/// </summary>
public class HomeController : Controller
{
    /// <summary>
    /// Reserva espaço para armazenar o logger.
    /// </summary>
    private readonly ILogger<HomeController> _logger;
    private readonly IOrcamentoRepository _orcamentoRepository;

    /// <summary>
    /// Atribui o logger ao espaço reservado.
    /// </summary>
    public HomeController(ILogger<HomeController> logger, IOrcamentoRepository orcamentoRepository)
    {
        _logger = logger;
        _orcamentoRepository = orcamentoRepository;
    }
  
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ClienteOrcamentos()
    {
        return View(new ClienteOrcamentosViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClienteOrcamentos(ClienteOrcamentosViewModel model)
    {
        model.PesquisaRealizada = true;
        model.Orcamentos = (await _orcamentoRepository.GetByChaveCliente(model.ChaveAcesso)).ToList();
        return View(model);
    }

    /// <summary>
    /// Armazena em cache por 0 segundos, e em nenhum local, não armazenando nada.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
