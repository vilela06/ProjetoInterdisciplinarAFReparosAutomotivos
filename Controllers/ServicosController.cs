using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AfReparosAutomotivos.Interfaces;

namespace AfReparosAutomotivos.Controllers;

[Authorize(AuthenticationSchemes = "Identity.Login")]
public class ServicosController : Controller
{
    private readonly IServicoRepository _servicoRepository;

    public ServicosController(IServicoRepository servicoRepository)
    {
        _servicoRepository = servicoRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var servicos = await _servicoRepository.Get();
        return View(servicos);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var servico = await _servicoRepository.GetId(id);
        return View(servico);
    }

}
