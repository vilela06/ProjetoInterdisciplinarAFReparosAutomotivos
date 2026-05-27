using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AfReparosAutomotivos.Controllers;

[Authorize(AuthenticationSchemes = "Identity.Login")]
public class PecasController : Controller
{
    private readonly IPecaRepository _pecaRepository;

    public PecasController(IPecaRepository pecaRepository)
    {
        _pecaRepository = pecaRepository;
    }

    public async Task<IActionResult> Index(string? pesquisa)
    {
        var pecas = string.IsNullOrWhiteSpace(pesquisa)
            ? await _pecaRepository.GetAll()
            : await _pecaRepository.Search(pesquisa);
        ViewBag.Pesquisa = pesquisa ?? string.Empty;
        return View(pecas);
    }

    public async Task<IActionResult> Details(int id)
    {
        var peca = await _pecaRepository.GetId(id);
        return peca == null ? NotFound() : View(peca);
    }

    [Authorize(AuthenticationSchemes = "Identity.Login", Roles = "1")]
    public IActionResult Create()
    {
        return View(new Pecas());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = "Identity.Login", Roles = "1")]
    public async Task<IActionResult> Create(Pecas peca)
    {
        if (!ModelState.IsValid)
        {
            return View(peca);
        }

        await _pecaRepository.Add(peca);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(AuthenticationSchemes = "Identity.Login", Roles = "1")]
    public async Task<IActionResult> Edit(int id)
    {
        var peca = await _pecaRepository.GetId(id);
        return peca == null ? NotFound() : View(peca);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = "Identity.Login", Roles = "1")]
    public async Task<IActionResult> Edit(Pecas peca)
    {
        if (!ModelState.IsValid)
        {
            return View(peca);
        }

        await _pecaRepository.Update(peca);
        return RedirectToAction(nameof(Index));
    }
}
