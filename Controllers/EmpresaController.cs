using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;

namespace AfReparosAutomotivos.Controllers;

[Authorize(AuthenticationSchemes = "Identity.Login")]
public class EmpresaController : Controller
{
    private readonly IEmpresaRepository _empresaRepository;

    public EmpresaController(IEmpresaRepository empresaRepository)
    {
        _empresaRepository = empresaRepository;
    }

    public async Task<IActionResult> Index()
    {
        var empresa = await _empresaRepository.GetDadosAsync();
        if (empresa == null)
        {
            return NotFound();
        }

        empresa.podeEditar = PodeEditar(empresa);
        return View(empresa);
    }

    public async Task<IActionResult> Edit()
    {
        var empresa = await _empresaRepository.GetDadosAsync();
        if (empresa == null)
        {
            return NotFound();
        }

        if (!PodeEditar(empresa))
        {
            TempData["EmpresaMensagem"] = "Somente o primeiro funcionario cadastrado pode editar os dados da empresa.";
            return RedirectToAction(nameof(Index));
        }

        return View(empresa);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EmpresaViewModel empresa)
    {
        var dadosAtuais = await _empresaRepository.GetDadosAsync();
        if (dadosAtuais == null)
        {
            return NotFound();
        }

        if (!PodeEditar(dadosAtuais))
        {
            TempData["EmpresaMensagem"] = "Somente o primeiro funcionario cadastrado pode editar os dados da empresa.";
            return RedirectToAction(nameof(Index));
        }

        empresa.idFuncionario = dadosAtuais.idFuncionario;
        empresa.estado = (empresa.estado ?? string.Empty).ToUpperInvariant();

        if (!ModelState.IsValid)
        {
            empresa.podeEditar = true;
            return View(empresa);
        }

        try
        {
            await _empresaRepository.UpdateAsync(empresa);
            return RedirectToAction(nameof(Index));
        }
        catch (SqlException ex)
        {
            ModelState.AddModelError(string.Empty, $"Nao foi possivel salvar os dados da empresa: {ex.Message}");
            empresa.podeEditar = true;
            return View(empresa);
        }
    }

    private bool PodeEditar(EmpresaViewModel empresa)
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return empresa.idFuncionario == 1 && int.TryParse(idClaim, out var idFuncionarioLogado) && idFuncionarioLogado == 1;
    }
}
