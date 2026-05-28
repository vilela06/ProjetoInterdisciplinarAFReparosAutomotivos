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
    private static readonly HashSet<string> UfsValidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG",
        "PA", "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    };

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
    public async Task<IActionResult> Create(OrcamentosViewModel orcamentoViewModel)
    {
        var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        var idFuncionario = idClaim != null && int.TryParse(idClaim.Value, out int id) ? id : 1;
        orcamentoViewModel.DocumentoCli = ApenasNumeros(orcamentoViewModel.DocumentoCli);
        ValidarEnderecoClienteFormulario(orcamentoViewModel);

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

        int? clienteCriadoId = null;
        int? veiculoCriadoId = null;
        int? orcamentoCriadoId = null;
        var servicosCriados = new List<int>();
        var baixasEstoque = new List<(int pecaId, int quantidade)>();

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
                clienteCriadoId = idCliente;
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
                veiculoCriadoId = idVeiculo;
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
                    servicosCriados.Add(item.idServico);
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
                var valorServico = precoBase * item.qtd;
                var valorItemFinal = Math.Max(0, valorServico + (valorPeca * item.qtdPeca) + item.taxa - item.desconto);
                item.preco = valorItemFinal;

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
            orcamentoCriadoId = idOrcamento;

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

                    baixasEstoque.Add((item.pecaId!.Value, item.qtdPeca));
                }
            }

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

            return RedirectToAction("Details", "Orcamentos", new { id = idOrcamento });
        }
        catch (Exception ex)
        {
            await DesfazerCriacaoOrcamentoAsync(orcamentoCriadoId, baixasEstoque, servicosCriados, veiculoCriadoId, clienteCriadoId);

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

    private async Task DesfazerCriacaoOrcamentoAsync(
        int? orcamentoId,
        List<(int pecaId, int quantidade)> baixasEstoque,
        List<int> servicosCriados,
        int? veiculoId,
        int? clienteId)
    {
        foreach (var baixa in baixasEstoque)
        {
            await _pecaRepository.ReporEstoque(baixa.pecaId, baixa.quantidade);
        }

        if (orcamentoId.HasValue)
        {
            await _orcamentoRepository.Delete(orcamentoId.Value);
        }

        foreach (var servicoId in servicosCriados.Distinct().Reverse())
        {
            await _servicoRepository.DeleteCreated(servicoId);
        }

        if (veiculoId.HasValue)
        {
            await _veiculoRepository.DeleteCreated(veiculoId.Value);
        }

        if (clienteId.HasValue)
        {
            await _clienteRepository.DeleteCreated(clienteId.Value);
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
        cliente.estado = (partes.ElementAtOrDefault(3) ?? string.Empty).ToUpperInvariant();
        cliente.cep = partes.ElementAtOrDefault(4) ?? string.Empty;
    }

    private void ValidarEnderecoClienteFormulario(OrcamentosViewModel orcamentoViewModel)
    {
        var partes = (orcamentoViewModel.EnderecoCli ?? string.Empty)
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        var numero = partes.ElementAtOrDefault(1) ?? string.Empty;
        var uf = partes.ElementAtOrDefault(3) ?? string.Empty;
        var cep = partes.ElementAtOrDefault(4) ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(numero) &&
            !System.Text.RegularExpressions.Regex.IsMatch(numero, @"^(\d+|s/n|S/N)$"))
        {
            ModelState.AddModelError(nameof(orcamentoViewModel.EnderecoCli), "O numero deve conter apenas numeros ou s/n.");
        }

        if (!string.IsNullOrWhiteSpace(cep) &&
            !System.Text.RegularExpressions.Regex.IsMatch(cep, @"^[0-9\-]+$"))
        {
            ModelState.AddModelError(nameof(orcamentoViewModel.EnderecoCli), "O CEP nao pode conter letras.");
        }

        if (string.IsNullOrWhiteSpace(uf) || !UfsValidas.Contains(uf))
        {
            ModelState.AddModelError(nameof(orcamentoViewModel.EnderecoCli), "Informe uma UF valida.");
        }
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

        if (orcamentoViewModel.status is 4 or 5 && !orcamentoViewModel.dataEntrega.HasValue)
        {
            ModelState.AddModelError(nameof(orcamentoViewModel.dataEntrega), "Informe a data de entrega para colocar o orcamento em execucao ou finalizado.");
            PreencherDadosExibicaoEdicao(orcamentoViewModel, orcamentoAtual);
            return View("Edit", orcamentoViewModel);
        }

        var todosOsItensCalculados = new List<ItemViewModel>();
        decimal totalGeral = 0;
        var itensAtuais = (await _itemRepository.GetByOrcamento(orcamentoViewModel.idOrcamento)).ToList();

        var itensDoOrcamento = orcamentoViewModel.ServicosAssociados
            .Where(item => item != null && item.idServico > 0 && item.qtd > 0)
            .ToList();

        if (orcamentoViewModel.ServicosAssociados.Any(item => item.qtd <= 0 || item.preco <= 0))
        {
            ModelState.AddModelError(string.Empty, "A quantidade e o valor dos servicos devem ser maiores que zero.");
            PreencherDadosExibicaoEdicao(orcamentoViewModel, orcamentoAtual);
            return View("Edit", orcamentoViewModel);
        }

        var pecasSolicitadas = itensDoOrcamento
            .Where(item => item.pecaId.HasValue)
            .GroupBy(item => item.pecaId!.Value)
            .Select(grupo => new
            {
                PecaId = grupo.Key,
                Quantidade = grupo.Sum(item => Math.Max(1, item.qtdPeca))
            })
            .ToList();

        foreach (var solicitacao in pecasSolicitadas)
        {
            var peca = await _pecaRepository.GetId(solicitacao.PecaId);
            var reservadoNesteOrcamento = itensAtuais
                .Where(item => item.pecaId == solicitacao.PecaId)
                .Sum(item => item.qtdPeca);

            var disponivel = (peca?.qtdEsto ?? 0) + reservadoNesteOrcamento;
            if (peca == null || solicitacao.Quantidade > disponivel)
            {
                ModelState.AddModelError(string.Empty, $"Estoque insuficiente para a peca {peca?.nome ?? solicitacao.PecaId.ToString()}.");
                PreencherDadosExibicaoEdicao(orcamentoViewModel, orcamentoAtual);
                return View("Edit", orcamentoViewModel);
            }
        }
                
        foreach (var item in itensDoOrcamento)
        {
            var valorPeca = 0m;
            if (item.pecaId.HasValue)
            {
                var peca = await _pecaRepository.GetId(item.pecaId.Value);
                valorPeca = (peca?.valor ?? 0m) * Math.Max(1, item.qtdPeca);
            }

            var custoTotal = ((item.preco * item.qtd) + valorPeca) * (1 + item.taxa);
            var valorItemFinal = custoTotal - item.desconto;
            item.preco = valorItemFinal;

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
                    idVeiculo = orcamentoViewModel.idVeiculo.GetValueOrDefault(orcamentoAtual.idVeiculo.GetValueOrDefault()),
                    idServico = itemViewModel.idServico,
                    funcionarioId = itemViewModel.funcionarioId > 0 ? itemViewModel.funcionarioId : orcamentoViewModel.idFuncionario,
                    qtd = itemViewModel.qtd,
                    pecaId = itemViewModel.pecaId,
                    qtdPeca = itemViewModel.qtdPeca,
                    data_entrega = itemViewModel.data_entrega,
                    preco = itemViewModel.preco,
                    descricao = itemViewModel.observacao,
                    taxa = 0m,
                    desconto = 0m
                };
                
                listaEntidadesItens.Add(itemEntidade);
            }

            await _itemRepository.Update(listaEntidadesItens);

            foreach (var item in itensAtuais.Where(item => item.pecaId.HasValue && item.qtdPeca > 0))
            {
                await _pecaRepository.ReporEstoque(item.pecaId!.Value, item.qtdPeca);
            }

            foreach (var item in listaEntidadesItens.Where(item => item.pecaId.HasValue && item.qtdPeca > 0))
            {
                var baixouEstoque = await _pecaRepository.BaixarEstoque(item.pecaId!.Value, item.qtdPeca);
                if (!baixouEstoque)
                {
                    throw new InvalidOperationException("Nao foi possivel baixar o estoque de uma das pecas do orcamento.");
                }
            }
        }

        orcamentoViewModel.total = totalGeral;

        await _orcamentoRepository.Update(orcamentoViewModel);
        return RedirectToAction("Details", "Orcamentos", new { id = orcamentoViewModel.idOrcamento });
    }

    private static void PreencherDadosExibicaoEdicao(OrcamentosViewModel destino, OrcamentosViewModel origem)
    {
        destino.nomeFunc = origem.nomeFunc;
        destino.nome = origem.nome;
        destino.DocumentoCli = origem.DocumentoCli;
        destino.TelefoneCli = origem.TelefoneCli;
        destino.EnderecoCli = origem.EnderecoCli;
        destino.Placa = string.IsNullOrWhiteSpace(destino.Placa) ? origem.Placa : destino.Placa;
        destino.Marca = string.IsNullOrWhiteSpace(destino.Marca) ? origem.Marca : destino.Marca;
        destino.Modelo = string.IsNullOrWhiteSpace(destino.Modelo) ? origem.Modelo : destino.Modelo;
        destino.Cor = string.IsNullOrWhiteSpace(destino.Cor) ? origem.Cor : destino.Cor;
        destino.Ano ??= origem.Ano;
        destino.dataCriacao = origem.dataCriacao;
        destino.total = origem.total;
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
