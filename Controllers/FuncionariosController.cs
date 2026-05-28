using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace AfReparosAutomotivos.Controllers;

[Authorize(AuthenticationSchemes = "Identity.Login", Roles = "1")]
public class FuncionariosController : Controller
{
    private readonly IFuncionarioRepository _funcionarioRepository;

    public FuncionariosController(IFuncionarioRepository funcionarioRepository)
    {
        _funcionarioRepository = funcionarioRepository;
    }

    public async Task<IActionResult> Index(string? pesquisa)
    {
        var funcionarios = string.IsNullOrWhiteSpace(pesquisa)
            ? await _funcionarioRepository.GetAll()
            : await _funcionarioRepository.Search(pesquisa);

        ViewBag.Pesquisa = pesquisa ?? string.Empty;
        return View(funcionarios);
    }

    public async Task<IActionResult> Details(int id)
    {
        var funcionario = await _funcionarioRepository.GetId(id);
        return funcionario == null ? NotFound() : View(funcionario);
    }

    public IActionResult Create()
    {
        return View(new Funcionarios
        {
            permissao = 3,
            statusFunc = 1,
            tipo_doc = "F"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Funcionarios funcionario)
    {
        if (string.IsNullOrWhiteSpace(funcionario.senha))
        {
            ModelState.AddModelError(nameof(Funcionarios.senha), "Informe a senha.");
        }

        if (!ModelState.IsValid)
        {
            return View(funcionario);
        }

        try
        {
            await _funcionarioRepository.Add(funcionario);
            return RedirectToAction(nameof(Index));
        }
        catch (SqlException)
        {
            ModelState.AddModelError(string.Empty, "Nao foi possivel salvar. Verifique se documento ou usuario ja estao cadastrados.");
            return View(funcionario);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var funcionario = await _funcionarioRepository.GetId(id);
        return funcionario == null ? NotFound() : View(funcionario);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Funcionarios funcionario)
    {
        if (!ModelState.IsValid)
        {
            return View(funcionario);
        }

        try
        {
            await _funcionarioRepository.Update(funcionario);
            return RedirectToAction(nameof(Index));
        }
        catch (SqlException)
        {
            ModelState.AddModelError(string.Empty, "Nao foi possivel salvar. Verifique se documento ou usuario ja estao cadastrados.");
            return View(funcionario);
        }
    }
}
