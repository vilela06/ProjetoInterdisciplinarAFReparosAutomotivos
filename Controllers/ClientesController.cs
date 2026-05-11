using Microsoft.AspNetCore.Mvc;
using AfReparosAutomotivos.Models;
using Microsoft.AspNetCore.Authorization;
using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models.ViewModels;

namespace AfReparosAutomotivos.Controllers;

[Authorize(AuthenticationSchemes = "Identity.Login")]
public class ClientesController : Controller
{
    private readonly IClienteRepository _clienteRepository;

    public ClientesController
    (
        IClienteRepository clienteRepository
    )
    {
        _clienteRepository = clienteRepository;
    }

    public async Task<IActionResult> Index()
    {
        var clientes = await _clienteRepository.GetAllAsync();
        return View(clientes);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var cliente = await _clienteRepository.GetId(id);
        return View(cliente);
    }

    [HttpGet, ActionName("Edit")]
    public async Task<IActionResult> Update(int id)
    {
        var cliente = await _clienteRepository.GetId(id);
        return View(cliente);
    }

    [HttpPost, ActionName("Edit")]
    public async Task<IActionResult> Update(Clientes cliente)
    {
        await _clienteRepository.Update(cliente);
        return RedirectToAction("Index", "Clientes");
    }
}