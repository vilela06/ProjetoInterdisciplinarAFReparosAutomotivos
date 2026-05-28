using Microsoft.AspNetCore.Mvc;
using AfReparosAutomotivos.Models;
using Microsoft.AspNetCore.Authorization;
using AfReparosAutomotivos.Interfaces;

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

    public async Task<IActionResult> Index(string? busca)
    {
        var clientes = string.IsNullOrWhiteSpace(busca)
            ? await _clienteRepository.GetAllAsync()
            : await _clienteRepository.Search(busca);

        return View(clientes.ToList());
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
        cliente.estado = (cliente.estado ?? string.Empty).ToUpperInvariant();

        if (!ModelState.IsValid)
        {
            return View("Edit", cliente);
        }

        try
        {
            await _clienteRepository.Update(cliente);
            return RedirectToAction("Index", "Clientes");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Não foi possível salvar o cliente: {ex.Message}");
            return View("Edit", cliente);
        }
    }
}
