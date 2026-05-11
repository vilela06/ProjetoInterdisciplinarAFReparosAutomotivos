using Microsoft.AspNetCore.Mvc;
using AfReparosAutomotivos.Models;
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

    [HttpGet]
    public IActionResult Create()
    {
        return View(new Servicos());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Servicos servico)
    {
        await _servicoRepository.Add(servico);   
        return RedirectToAction("Index", "Servicos");
    }

    [HttpGet, ActionName("Edit")]
    public async Task<IActionResult> Update(int id)
    {
        var servico = await _servicoRepository.Update(id);
        return View(servico);
    }

    [HttpPost, ActionName("Edit")]
    public async Task<IActionResult> Update(Servicos servico)
    {
        await _servicoRepository.Update(servico);
        return RedirectToAction("Index", "Servicos");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _servicoRepository.Delete(id);
        return RedirectToAction("Index", "Servicos");
    }
}