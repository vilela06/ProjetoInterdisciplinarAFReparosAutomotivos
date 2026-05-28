using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using AfReparosAutomotivos.Models;

public class OrcamentoPdfDocument : IDocument
{
    private readonly OrcamentosViewModel Orcamento;
    private readonly Clientes Cliente;
    private readonly List<Veiculos> Veiculos;
    private readonly IEnumerable<Item> Itens;

    public OrcamentoPdfDocument(
        OrcamentosViewModel orcamento,
        Clientes cliente,
        List<Veiculos> veiculos,
        IEnumerable<Item> itens)
    {
        Orcamento = orcamento;
        Cliente = cliente;
        Veiculos = veiculos;
        Itens = itens;
    }

    public DocumentMetadata GetMetadata() => new DocumentMetadata();

    public DocumentSettings GetSettings() => new DocumentSettings();

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(30);

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().AlignCenter().Text("Obrigado pela preferência!");
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("ORÇAMENTO DE SERVIÇO")
                    .FontSize(20).Bold().AlignCenter();
                col.Item().Text("\n");
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Element(ComposeCliente);
            col.Item().PaddingTop(10).Element(ComposeVeiculo);
            col.Item().PaddingTop(10).Element(ComposeItens);
            col.Item().PaddingTop(20).Element(ComposeTotal);
        });
    }

    private void ComposeCliente(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Text("Dados do Cliente")
                .FontSize(14).Bold();

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(1);
                    c.RelativeColumn(3);
                });

                table.Cell().Text("Nome").Bold();
                table.Cell().Text(Cliente.nome);

                table.Cell().Text("Documento").Bold();
                table.Cell().Text(Cliente.documento != null ? FormatarCPF(Cliente.documento) : "");

                table.Cell().Text("Telefone").Bold();
                table.Cell().Text(Cliente.telefone != null ? FormatarTelefone(Cliente.telefone) : "");

                table.Cell().Text("Endereço").Bold();
                table.Cell().Text(Cliente.endereco ?? "");
            });
        });
    }

    private string FormatarCPF(string cpf)
    {
        if (string.IsNullOrEmpty(cpf) || cpf.Length != 11)
            return cpf ?? "";

        return $"{cpf.Substring(0,3)}.{cpf.Substring(3,3)}.{cpf.Substring(6,3)}-{cpf.Substring(9,2)}";
    }

    private string FormatarTelefone(string tel)
    {
        if (string.IsNullOrEmpty(tel) || tel.Length != 11)
            return tel ?? "";

        return $"({tel.Substring(0,2)}) {tel.Substring(2,5)}-{tel.Substring(7,4)}";
    }

    private void ComposeVeiculo(IContainer container)
    {
        container.Column(veiculosCol =>
        {
           veiculosCol.Item().PaddingBottom(5).Text("Dados do(s) Veículo(s)").Bold().FontSize(14);

            for (var i = 0; i < Veiculos.Count; i++)
            {
                var Veiculo = Veiculos[i];
                var marca = Veiculo?.marca ?? "-";
                var modelo = Veiculo?.modelo ?? "-";
                var placa = Veiculo?.placa ?? "-";

                if (i > 0)
                {
                    veiculosCol.Item().PaddingTop(10); 
                }

                veiculosCol.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(1);
                        c.RelativeColumn(3);
                    });

                    table.Cell().Text("Marca").Bold();
                    table.Cell().Text(marca);

                    table.Cell().Text("Modelo").Bold();
                    table.Cell().Text(modelo);

                    table.Cell().Text("Placa").Bold();
                    table.Cell().Text(placa);

                    table.Cell().Text("Cor").Bold();
                    table.Cell().Text(Veiculo?.cor ?? "-");

                    table.Cell().Text("Ano").Bold();
                    table.Cell().Text(Veiculo?.ano > 0 ? Veiculo.ano.ToString() : "-");
                });
            }
        });
    }

    private void ComposeItens(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Text("Serviços / Itens")
                .Bold().FontSize(14);

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(3); // Descrição
                    c.RelativeColumn(0.8f); // Qtd
                    c.RelativeColumn(1.5f); // Valor Unit.
                    c.RelativeColumn(1.2f); // Taxa
                    c.RelativeColumn(1.2f); // Desconto
                    c.RelativeColumn(1.5f); // Subtotal
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Element(CellHeader).Text("Descrição");
                    header.Cell().Element(CellHeader).Text("Qtd");
                    header.Cell().Element(CellHeader).Text("Valor Unit.");
                    header.Cell().Element(CellHeader).Text("Taxa (%)");
                    header.Cell().Element(CellHeader).Text("Desconto");
                    header.Cell().Element(CellHeader).Text("Subtotal");
                });

                foreach (var item in Itens)
                {
                    decimal taxaAplicada = item.taxa ?? 0m;
                    decimal desconto = item.desconto ?? 0m;

                    table.Cell().Element(CellDefault).Text(item.descricao);
                    table.Cell().Element(CellDefault).Text(item.qtd.ToString());
                    table.Cell().Element(CellDefault).Text(item.preco.ToString("C"));
                    table.Cell().Element(CellDefault).Text($"{taxaAplicada * 100:N0}%");
                    table.Cell().Element(CellDefault).Text(desconto.ToString("C"));
                    table.Cell().Element(CellDefault).Text(item.preco.ToString("C"));
                }
            });
        });
    }

    private static IContainer CellHeader(IContainer container) =>
        container.Padding(5).Background("#EEE").Border(0.5f).BorderColor("#999");

    private static IContainer CellDefault(IContainer container) =>
        container.Padding(5).BorderBottom(0.5f).BorderColor("#DDD");

    private void ComposeTotal(IContainer container)
    {
        decimal total = Itens.Sum(i => i.preco);

        container.AlignRight().Text($"TOTAL: {total:C}")
            .FontSize(16).Bold();
    }
}
