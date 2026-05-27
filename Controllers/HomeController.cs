using System.Diagnostics;
using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AfReparosAutomotivos.Controllers;

public class HomeController : Controller
{
    private readonly IOrcamentoRepository _orcamentoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IItemRepository _itemRepository;
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IPecaRepository _pecaRepository;

    public HomeController(
        IOrcamentoRepository orcamentoRepository,
        IClienteRepository clienteRepository,
        IItemRepository itemRepository,
        IVeiculoRepository veiculoRepository,
        IPecaRepository pecaRepository)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        _orcamentoRepository = orcamentoRepository;
        _clienteRepository = clienteRepository;
        _itemRepository = itemRepository;
        _veiculoRepository = veiculoRepository;
        _pecaRepository = pecaRepository;
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

    [HttpGet]
    public async Task<IActionResult> ClienteOrcamentoDetalhes(int id, string chave)
    {
        var orcamento = await ObterOrcamentoDoCliente(id, chave);
        if (orcamento == null)
        {
            return NotFound();
        }

        ViewBag.ChaveAcesso = chave;
        return View("~/Views/ClienteOrcamento/Details.cshtml", orcamento);
    }

    [HttpGet]
    public async Task<IActionResult> ClienteOrcamentoPdf(int id, string chave)
    {
        var orcamento = await ObterOrcamentoDoCliente(id, chave);
        if (orcamento == null)
        {
            return NotFound();
        }

        var cliente = await _clienteRepository.GetId(orcamento.idCliente);
        if (cliente == null)
        {
            return NotFound();
        }

        var itens = (await _itemRepository.GetByOrcamento(id)).ToList();
        var veiculos = new List<Veiculos>();
        foreach (var idVeiculo in itens.Select(item => item.idVeiculo).Distinct())
        {
            var veiculo = await _veiculoRepository.GetId(idVeiculo);
            if (veiculo != null)
            {
                veiculos.Add(veiculo);
            }
        }

        var document = new OrcamentoPdfDocument(orcamento, cliente, veiculos, itens);
        var pdf = document.GeneratePdf();

        return File(pdf, "application/pdf", $"orcamento_{id}.pdf");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClienteOrcamentoStatus(int id, string chave, string decisao)
    {
        var status = string.Equals(decisao, "aprovar", StringComparison.OrdinalIgnoreCase) ? 2 : 3;
        var itens = status == 3
            ? (await _itemRepository.GetByOrcamento(id)).Where(item => item.pecaId.HasValue).ToList()
            : new List<Item>();
        var atualizado = await _orcamentoRepository.UpdateStatusByChaveCliente(id, chave, status);

        if (atualizado && status == 3)
        {
            foreach (var item in itens)
            {
                await _pecaRepository.ReporEstoque(item.pecaId!.Value, item.qtdPeca);
            }
        }

        var model = new ClienteOrcamentosViewModel
        {
            ChaveAcesso = chave,
            PesquisaRealizada = true,
            Orcamentos = (await _orcamentoRepository.GetByChaveCliente(chave)).ToList()
        };

        TempData["ClienteOrcamentoMensagem"] = atualizado
            ? (status == 2 ? "Orcamento aprovado com sucesso." : "Orcamento recusado com sucesso.")
            : "Nao foi possivel atualizar este orcamento. Ele pode ja ter sido aprovado ou recusado.";

        return View("~/Views/ClienteOrcamento/Index.cshtml", model);
    }

    private async Task<OrcamentosViewModel?> ObterOrcamentoDoCliente(int id, string chave)
    {
        var orcamentos = await _orcamentoRepository.GetByChaveCliente(chave);
        if (!orcamentos.Any(orcamento => orcamento.idOrcamento == id))
        {
            return null;
        }

        return await _orcamentoRepository.GetId(id);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
