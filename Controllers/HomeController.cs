using System.Diagnostics;
using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.AspNetCore.Mvc;

namespace AfReparosAutomotivos.Controllers;

public class HomeController : Controller
{
    private readonly IOrcamentoRepository _orcamentoRepository;

    public HomeController(IOrcamentoRepository orcamentoRepository)
    {
        _orcamentoRepository = orcamentoRepository;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ClienteOrcamentos()
    {
        return View("~/Views/ClienteOrcamento/Index.cshtml", new ClienteOrcamentosViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClienteOrcamentos(ClienteOrcamentosViewModel model)
    {
        model.PesquisaRealizada = true;
        model.Orcamentos = (await _orcamentoRepository.GetByChaveCliente(model.ChaveAcesso)).ToList();
        return View("~/Views/ClienteOrcamento/Index.cshtml", model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
