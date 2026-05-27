using Microsoft.AspNetCore.Mvc;
using AfReparosAutomotivos.Models;
using Microsoft.AspNetCore.Authorization;
using AfReparosAutomotivos.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using AfReparosAutomotivos.Models.ViewModels;
using System.Text.Json;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AfReparosAutomotivos.Controllers;

[Authorize(AuthenticationSchemes = "Identity.Login")]
public class OrcamentosController : Controller
{
    private readonly IOrcamentoRepository _orcamentoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IItemRepository _itemRepository;
    private readonly IServicoRepository _servicoRepository;
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IFuncionarioRepository _funcionarioRepository;
    private readonly IPecaRepository _pecaRepository;
    
    /// <summary>
    /// Construtor injetando todos os repositórios necessários.
    /// </summary>
    public OrcamentosController
    (
        IOrcamentoRepository orcamentoRepository,
        IClienteRepository clienteRepository,
        IItemRepository itemRepository,
        IServicoRepository servicoRepository,
        IVeiculoRepository veiculoRepository,
        IFuncionarioRepository funcionarioRepository,
        IPecaRepository pecaRepository
    )
    {
        QuestPDF.Settings.License = LicenseType.Community;

        _orcamentoRepository = orcamentoRepository;
        _clienteRepository = clienteRepository;
        _itemRepository = itemRepository;
        _servicoRepository = servicoRepository;
        _veiculoRepository = veiculoRepository;
        _funcionarioRepository = funcionarioRepository;
        _pecaRepository = pecaRepository;
    }

    /// <summary>
    /// Lista os serviços disponíveis e os adiciona ao ViewModel para preenchimento do dropdown na view.
    /// </summary>
    private async Task CarregarServicosNoViewModel(OrcamentosViewModel orcamentoViewModel)
    {
        var servicos = await _servicoRepository.Get();
        orcamentoViewModel.ServicosDisponiveis = servicos.Select(s => new SelectListItem
        {
            Value = s.IdServico.ToString(),
            Text = $"{s.Descricao} (R$ {s.PrecoBase:N2})"
        }).ToList();

        var funcionarios = await _funcionarioRepository.GetAtivos();
        orcamentoViewModel.FuncionariosDisponiveis = funcionarios.Select(f => new SelectListItem
        {
            Value = f.idFuncionario.ToString(),
            Text = f.Nome
        }).ToList();

        var pecas = await _pecaRepository.GetDisponiveis();
        orcamentoViewModel.PecasDisponiveis = pecas.Select(p => new SelectListItem
        {
            Value = p.idPeca.ToString(),
            Text = $"{p.nome} (R$ {p.valor:N2}, estoque {p.qtdEsto})"
        }).ToList();
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] OrcamentosFilterViewModel filtros)
    {
        /// Busca a lista de orçamentos no repositório e a passa para a view.
        var orcamentos = await _orcamentoRepository.GetFilter(filtros);
        ViewBag.filtrosAplicados = filtros;
        return View(orcamentos);
    }

    /// <summary>
    /// Retorna os detalhes do orçamento do ID.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var orcamento = await _orcamentoRepository.GetId(id);
        return View(orcamento);
    }

    /// <summary>
    /// Gera o PDF (O Layout ficou no "OrcamentoPdfDocument.cs")
    /// </summary>
    public async Task<IActionResult> GerarPdf(int id)
    {
        var orcamento = await _orcamentoRepository.GetId(id);
        if (orcamento == null)
            return NotFound("Orçamento não encontrado.");

        var cliente = await _clienteRepository.GetId(orcamento.idCliente);
        if (cliente == null)
            return NotFound("Cliente do orcamento nao encontrado.");

        var itens = await _itemRepository.GetByOrcamento(id);

        var idsVeiculosUnicos = itens
            .Where(item => item != null)
            .Select(item => item.idVeiculo)
            .Distinct()
            .ToList();
            
        List<Veiculos> veiculos = new List<Veiculos>();

        foreach (var idVeiculo in idsVeiculosUnicos)
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

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var orcamentoViewModel = new OrcamentosViewModel 
        {
            ServicosAssociados = new List<ItemViewModel> { new ItemViewModel { qtd = 1 } } 
        };
        await CarregarServicosNoViewModel(orcamentoViewModel); 
        return View(orcamentoViewModel);
    }

    /// <summary>
    /// Ação post para criação de Orçamento e seus Itens relacionados.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrcamentosViewModel orcamentoViewModel, string acao)
    {
        var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        var idFuncionario = idClaim != null && int.TryParse(idClaim.Value, out int id) ? id : 1;
        orcamentoViewModel.DocumentoCli = ApenasNumeros(orcamentoViewModel.DocumentoCli);

        if (!ModelState.IsValid)
        {
            AdicionarMensagemErrosValidacao();
            await CarregarServicosNoViewModel(orcamentoViewModel); 
            return View(orcamentoViewModel);
        }
        
        if (orcamentoViewModel.DocumentoCli != null && orcamentoViewModel.DocumentoCli.Length != 11 && orcamentoViewModel.DocumentoCli.Length != 14)
        {
            var erro = new Modal
            {
                Title = "Formato de documento inválido",
                Mensagem = "O documento deve ser um CPF (11 dígitos) ou CNPJ (14 dígitos)."
            };
            TempData["Mensagem"] = JsonSerializer.Serialize(erro);
            await CarregarServicosNoViewModel(orcamentoViewModel); 
            return View(orcamentoViewModel);
        }

        try
        {
            int idCliente = orcamentoViewModel.idCli.GetValueOrDefault();
            if (idCliente <= 0)
            {
                Clientes cliente = new Clientes
                {
                    nome = orcamentoViewModel.nome,
                    telefone = string.IsNullOrWhiteSpace(orcamentoViewModel.TelefoneCli) ? null : orcamentoViewModel.TelefoneCli,
                    celular = orcamentoViewModel.CelularCli,
                    email = orcamentoViewModel.EmailCli,
                    endereco = orcamentoViewModel.EnderecoCli,
                    documento = orcamentoViewModel.DocumentoCli ?? string.Empty
                };
                PreencherEnderecoCliente(cliente, orcamentoViewModel.EnderecoCli);
                idCliente = await _clienteRepository.Add(cliente);
            }

            var todosOsItensCalculados = new List<ItemViewModel>();
            decimal totalGeral = 0;

            int idVeiculo = orcamentoViewModel.idVeiculo.GetValueOrDefault();
            if (idVeiculo <= 0)
            {
                Veiculos veiculoEntidade = new Veiculos
                {
                    clienteId = idCliente,
                    placa = orcamentoViewModel.Placa,
                    marca = orcamentoViewModel.Marca,
                    modelo = orcamentoViewModel.Modelo,
                    cor = orcamentoViewModel.Cor,
                    ano = orcamentoViewModel.Ano.GetValueOrDefault()
                };

                idVeiculo = await _veiculoRepository.Add(veiculoEntidade);
            }

            var itensDoOrcamento = orcamentoViewModel.ServicosAssociados
                .Where(item => item != null && item.qtd > 0 && item.funcionarioId > 0 &&
                    (item.idServico > 0 || (!string.IsNullOrWhiteSpace(item.novoServicoDescricao) && item.preco > 0)))
                .ToList();

            var servicosRepetidos = itensDoOrcamento
                .Where(item => item.idServico > 0)
                .GroupBy(item => item.idServico)
                .Where(grupo => grupo.Count() > 1)
                .Select(grupo => grupo.Key)
                .ToList();

            if (servicosRepetidos.Any())
            {
                throw new InvalidOperationException("Nao repita o mesmo servico no orcamento. O banco permite cada servico apenas uma vez por orcamento.");
            }
                    
            foreach (var item in itensDoOrcamento)
            {
                if (item.idServico <= 0)
                {
                    item.idServico = await _servicoRepository.Add(new Servicos
                    {
                        Descricao = item.novoServicoDescricao ?? string.Empty,
                        PrecoBase = item.preco
                    });
                }

                var precoBase = item.idServico <= 0
                    ? item.preco
                    : await _servicoRepository.GetPrecoBaseByIdAsync(item.idServico);

                var valorPeca = 0m;
                if (item.pecaId.HasValue)
                {
                    var peca = await _pecaRepository.GetId(item.pecaId.Value);
                    if (peca == null)
                    {
                        throw new InvalidOperationException("A peca selecionada nao foi encontrada.");
                    }

                    if (peca.qtdEsto < item.qtdPeca)
                    {
                        throw new InvalidOperationException($"Estoque insuficiente para a peca {peca.nome}.");
                    }

                    valorPeca = peca.valor;
                }

                item.valorPeca = valorPeca;
                item.preco = (precoBase * item.qtd) + (valorPeca * item.qtdPeca);

                var custoTotal = item.preco * (1 + item.taxa);

                var valorItemFinal = custoTotal - item.desconto;

                totalGeral += valorItemFinal;

                todosOsItensCalculados.Add(item);
            }

            if (!todosOsItensCalculados.Any())
            {
                throw new InvalidOperationException("Nenhum item de serviço válido foi fornecido para o orçamento.");
            }

            orcamentoViewModel.total = totalGeral;
            orcamentoViewModel.status = 1;

            Orcamentos orcamento = new Orcamentos
            {
                idFuncionario = idFuncionario,
                idCliente = idCliente,
                veiculoId = idVeiculo,
                dataCriacao = DateTime.Now,
                dataEntrega = orcamentoViewModel.dataEntrega,
                status = orcamentoViewModel.status,
                total = orcamentoViewModel.total,
                formaPagamento = orcamentoViewModel.formaPagamento,
                parcelas = orcamentoViewModel.parcelas
            };
            int idOrcamento = await _orcamentoRepository.Add(orcamento);

            if (todosOsItensCalculados.Any()) 
            {
                var listaEntidadesItens = new List<Item>();

                foreach (var itemTuple in todosOsItensCalculados)
                {
                    ItemViewModel itemViewModel = itemTuple;

                    var itemEntidade = new Item
                    {
                        idOrcamento = idOrcamento, 
                        idVeiculo = idVeiculo,
                        idServico = itemViewModel.idServico,
                        funcionarioId = itemViewModel.funcionarioId,
                        pecaId = itemViewModel.pecaId,
                        qtd = itemViewModel.qtd,
                        qtdPeca = itemViewModel.qtdPeca,
                        data_entrega = itemViewModel.data_entrega, 
                        preco = itemViewModel.preco,
                        descricao = itemViewModel.observacao,
                        taxa = itemViewModel.taxa,
                        desconto = itemViewModel.desconto
                    };
                    
                    listaEntidadesItens.Add(itemEntidade);
                }

                await _itemRepository.Add(listaEntidadesItens);

                foreach (var item in listaEntidadesItens.Where(item => item.pecaId.HasValue))
                {
                    var baixouEstoque = await _pecaRepository.BaixarEstoque(item.pecaId!.Value, item.qtdPeca);
                    if (!baixouEstoque)
                    {
                        throw new InvalidOperationException("Nao foi possivel baixar o estoque de uma das pecas do orcamento.");
                    }
                }
            }

            if (string.Equals(acao, "gerar", StringComparison.OrdinalIgnoreCase))
            {
                var clienteAviso = await _clienteRepository.GetId(idCliente);
                var mensagemAviso = CriarMensagemAvisoCliente(clienteAviso, idOrcamento);
                var avisoUrl = CriarLinkAvisoCliente(clienteAviso?.celular ?? orcamentoViewModel.CelularCli, mensagemAviso);
                var emailAviso = string.IsNullOrWhiteSpace(clienteAviso?.email) ? orcamentoViewModel.EmailCli : clienteAviso.email;
                var avisoEmailUrl = CriarLinkEmailCliente(emailAviso, mensagemAviso);

                if (avisoUrl == null)
                {
                    TempData["AvisoClienteMensagem"] = "Orcamento salvo, mas o cliente nao possui celular cadastrado para envio do aviso.";
                }
                else
                {
                    TempData["AvisoClienteUrl"] = avisoUrl;
                    TempData["AvisoClienteMensagem"] = "Orcamento salvo. O aviso ao cliente foi preparado para envio pelo celular e e-mail.";
                }

                if (avisoEmailUrl != null)
                {
                    TempData["AvisoClienteEmailUrl"] = avisoEmailUrl;
                }
            }
            else
            {
                TempData["AvisoClienteMensagem"] = "Orcamento salvo como rascunho.";
            }

            return RedirectToAction("Details", "Orcamentos", new { id = idOrcamento });
        }
        catch (Exception ex)
        {
            var erro = new Modal
            {
                Title = "Erro na criação",
                Mensagem = $"Ocorreu um erro ao salvar: {ex.Message}"
            };
            TempData["Mensagem"] = JsonSerializer.Serialize(erro);
            await CarregarServicosNoViewModel(orcamentoViewModel); 
            return View(orcamentoViewModel);
        }
    }

    private string CriarMensagemAvisoCliente(Clientes? cliente, int idOrcamento)
    {
        var linkConsulta = Url.Action("ClienteOrcamentos", "Home", null, Request.Scheme) ?? string.Empty;
        var chave = string.IsNullOrWhiteSpace(cliente?.chaveCli) ? "nao cadastrada" : cliente.chaveCli;

        return $"Ola! Voce possui um novo orcamento para analisar. Orcamento: {idOrcamento}. Sua chave de acesso: {chave}. Acesse: {linkConsulta}";
    }

    private string? CriarLinkAvisoCliente(string? celular, string mensagem)
    {
        var numeros = new string((celular ?? string.Empty).Where(char.IsDigit).ToArray());
        if (string.IsNullOrWhiteSpace(numeros))
        {
            return null;
        }

        if (!numeros.StartsWith("55", StringComparison.Ordinal))
        {
            numeros = $"55{numeros}";
        }

        return $"https://wa.me/{numeros}?text={Uri.EscapeDataString(mensagem)}";
    }

    private static string? CriarLinkEmailCliente(string? email, string mensagem)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var assunto = Uri.EscapeDataString("Novo orcamento para analisar");
        var corpo = Uri.EscapeDataString(mensagem);
        return $"mailto:{email}?subject={assunto}&body={corpo}";
    }

    private static string ApenasNumeros(string? valor)
    {
        return new string((valor ?? string.Empty).Where(char.IsDigit).ToArray());
    }

    private static void PreencherEnderecoCliente(Clientes cliente, string endereco)
    {
        var partes = (endereco ?? string.Empty)
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        cliente.logradouro = partes.ElementAtOrDefault(0) ?? string.Empty;
        cliente.numero = partes.ElementAtOrDefault(1) ?? string.Empty;
        cliente.cidade = partes.ElementAtOrDefault(2) ?? string.Empty;
        cliente.estado = partes.ElementAtOrDefault(3) ?? string.Empty;
        cliente.cep = partes.ElementAtOrDefault(4) ?? string.Empty;
    }

    private void AdicionarMensagemErrosValidacao()
    {
        var mensagens = ModelState
            .Where(campo => campo.Value?.Errors.Any() == true)
            .SelectMany(campo => campo.Value!.Errors.Select(erro => erro.ErrorMessage))
            .Where(mensagem => !string.IsNullOrWhiteSpace(mensagem))
            .Distinct()
            .ToList();

        if (!mensagens.Any())
        {
            return;
        }

        var erro = new Modal
        {
            Title = "Campos obrigatorios",
            Mensagem = string.Join(" ", mensagens)
        };

        TempData["Mensagem"] = JsonSerializer.Serialize(erro);
    }

    [HttpGet]
    public async Task<IActionResult> BuscarClientes(string termo)
    {
        if (string.IsNullOrWhiteSpace(termo))
        {
            return Json(Array.Empty<object>());
        }

        var clientes = await _clienteRepository.Search(termo);
        return Json(clientes.Take(10).Select(cliente => new
        {
            id = cliente.id,
            nome = cliente.nome,
            documento = cliente.documento,
            telefone = cliente.telefone,
            celular = cliente.celular,
            email = cliente.email,
            endereco = cliente.endereco,
            logradouro = cliente.logradouro,
            numero = cliente.numero,
            cidade = cliente.cidade,
            estado = cliente.estado,
            cep = cliente.cep
        }));
    }

    [HttpGet]
    public async Task<IActionResult> BuscarVeiculosCliente(int clienteId, string termo)
    {
        if (clienteId <= 0 || string.IsNullOrWhiteSpace(termo))
        {
            return Json(Array.Empty<object>());
        }

        var veiculos = await _veiculoRepository.SearchByCliente(clienteId, termo);
        return Json(veiculos.Take(10).Select(veiculo => new
        {
            id = veiculo.id,
            placa = veiculo.placa,
            marca = veiculo.marca,
            modelo = veiculo.modelo,
            cor = veiculo.cor,
            ano = veiculo.ano
        }));
    }

    /// <summary>
    /// Retorna o orçamento para edição.
    /// </summary>
    [HttpGet, ActionName("Edit")]
    public async Task<IActionResult> Update(int id)
    {
        var orcamento = await _orcamentoRepository.GetId(id);
        if (orcamento == null)
        {
            return NotFound();
        }

        if (orcamento.status == 5)
        {
            TempData["OrcamentoMensagem"] = "Orcamento finalizado nao pode ser alterado.";
            return RedirectToAction("Index");
        }

        return View(orcamento);
    }

    /// <summary>
    /// Realiza a atualização do orçamento.
    /// </summary>
    [HttpPost, ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(OrcamentosViewModel orcamentoViewModel)
    {
        var orcamentoAtual = await _orcamentoRepository.GetId(orcamentoViewModel.idOrcamento);
        if (orcamentoAtual == null)
        {
            return NotFound();
        }

        if (orcamentoAtual.status == 5)
        {
            TempData["OrcamentoMensagem"] = "Orcamento finalizado nao pode ser alterado.";
            return RedirectToAction("Index");
        }

        var todosOsItensCalculados = new List<ItemViewModel>();
        decimal totalGeral = 0;

        var itensDoOrcamento = orcamentoViewModel.ServicosAssociados
            .Where(item => item != null && item.idServico > 0 && item.qtd > 0)
            .ToList();
                
        foreach (var item in itensDoOrcamento)
        {
            var precoBase = await _servicoRepository.GetPrecoBaseByIdAsync(item.idServico); 

            item.preco = precoBase;

            var custoTotal = (item.preco * item.qtd) * (1 + item.taxa);

            var valorItemFinal = custoTotal - item.desconto;

            totalGeral += valorItemFinal;

            todosOsItensCalculados.Add(item);
        }

        if (!todosOsItensCalculados.Any())
        {
            throw new InvalidOperationException("Nenhum item de serviço válido foi fornecido para o orçamento.");
        }
        else 
        {
            var listaEntidadesItens = new List<Item>();

            foreach (var itemTuple in todosOsItensCalculados)
            {
                ItemViewModel itemViewModel = itemTuple;

                var itemEntidade = new Item
                {
                    idItem = itemViewModel.idItem,
                    idOrcamento = orcamentoViewModel.idOrcamento, 
                    idVeiculo = orcamentoViewModel.idVeiculo.GetValueOrDefault(),
                    idServico = itemViewModel.idServico,
                    funcionarioId = orcamentoViewModel.idFuncionario,
                    qtd = itemViewModel.qtd,
                    data_entrega = itemViewModel.data_entrega,
                    preco = itemViewModel.preco,
                    descricao = itemViewModel.observacao,
                    taxa = itemViewModel.taxa,
                    desconto = itemViewModel.desconto
                };
                
                listaEntidadesItens.Add(itemEntidade);
            }

            await _itemRepository.Update(listaEntidadesItens);
        }

        orcamentoViewModel.total = totalGeral;

        await _orcamentoRepository.Update(orcamentoViewModel);
        return RedirectToAction("Details", "Orcamentos", new { id = orcamentoViewModel.idOrcamento });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var orcamento = await _orcamentoRepository.GetId(id);
        if (orcamento == null)
        {
            return NotFound();
        }

        if (orcamento.status is 2 or 4 or 5)
        {
            TempData["OrcamentoMensagem"] = "Este orcamento nao pode ser excluido por estar aprovado, em execucao ou finalizado.";
            return RedirectToAction("Index");
        }

        var itens = await _itemRepository.GetByOrcamento(id);
        if (orcamento.status == 1)
        {
            foreach (var item in itens.Where(item => item.pecaId.HasValue && item.qtdPeca > 0))
            {
                await _pecaRepository.ReporEstoque(item.pecaId!.Value, item.qtdPeca);
            }
        }

        await _orcamentoRepository.Delete(id);
        TempData["OrcamentoMensagem"] = "Orcamento excluido com sucesso.";
        return RedirectToAction("Index");
    }

}
