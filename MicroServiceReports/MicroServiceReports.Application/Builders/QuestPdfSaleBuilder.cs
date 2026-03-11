using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MicroServiceReports.Application.Builders
{
    /// <summary>
    /// Builder concreto que construye PDFs de ventas usando QuestPDF
    /// </summary>
    public class QuestPdfSaleBuilder : ISalePdfBuilder
    {
        private DateTime _saleDate;
        private string _client = string.Empty;
        private string _ci = string.Empty;
        private string _userName = string.Empty;
        private decimal _total;
        private DateTime _receivedAt;
        private List<ProductLineItem> _products = new();

        public QuestPdfSaleBuilder()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            Reset();
        }

        public void Reset()
        {
            _saleDate = DateTime.UtcNow;
            _client = "N/A";
            _ci = "N/A";
            _userName = "N/A";
            _total = 0;
            _receivedAt = DateTime.UtcNow;
            _products = new List<ProductLineItem>();
        }

        public ISalePdfBuilder SetSaleInfo(DateTime saleDate, string client, string ci)
        {
            _saleDate = saleDate;
            _client = client ?? "N/A";
            _ci = ci ?? "N/A";
            return this;
        }

        public ISalePdfBuilder SetUserInfo(string userName)
        {
            _userName = userName ?? "N/A";
            return this;
        }

        public ISalePdfBuilder AddProduct(int quantity, string description, decimal unitPrice)
        {
            _products.Add(new ProductLineItem
            {
                Quantity = quantity,
                Description = description ?? "Sin nombre",
                UnitPrice = unitPrice,
                Total = quantity * unitPrice
            });
            return this;
        }

        public ISalePdfBuilder SetTotal(decimal total)
        {
            _total = total;
            return this;
        }

        public ISalePdfBuilder SetReceivedAt(DateTime receivedAt)
        {
            _receivedAt = receivedAt;
            return this;
        }

        public byte[] Build()
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }

        private void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(10);

                // Logo y título
                column.Item().BorderBottom(1).PaddingBottom(10).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Border(1).Width(80).Height(60)
                            .AlignCenter().AlignMiddle()
                            .Text("Logo").FontSize(14).Bold();
                    });

                    row.RelativeItem().AlignRight().PaddingLeft(20)
                        .Text("COMPROBANTE DE VENTA")
                        .FontSize(22).Bold();
                });

                // Fecha y datos del cliente
                column.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Text($"Fecha: {_saleDate:dd/MM/yyyy}").FontSize(11);
                });

                column.Item().Row(row =>
                {
                    row.RelativeItem().Text($"CI/NIT: {_ci}").FontSize(11);
                    row.RelativeItem().Text($"Razón Social: {_client.ToUpper()}").FontSize(11);
                });
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingTop(20).Column(column =>
            {
                // Encabezado de tabla
                column.Item().Border(1).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1.5f); // Cantidad
                        columns.RelativeColumn(5);    // Descripción
                        columns.RelativeColumn(2);    // Precio Unitario
                        columns.RelativeColumn(2);    // Importe
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten3)
                            .Padding(5).AlignCenter()
                            .Text("Cantidad").Bold();

                        header.Cell().Background(Colors.Grey.Lighten3)
                            .Padding(5).AlignCenter()
                            .Text("Descripción").Bold();

                        header.Cell().Background(Colors.Grey.Lighten3)
                            .Padding(5).AlignCenter()
                            .Text("Precio Unitario Bs.").Bold();

                        header.Cell().Background(Colors.Grey.Lighten3)
                            .Padding(5).AlignCenter()
                            .Text("Importe Bs.").Bold();
                    });

                    // Productos
                    foreach (var product in _products)
                    {
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                            .Padding(5).AlignCenter()
                            .Text(product.Quantity.ToString());

                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                            .Padding(5)
                            .Text(product.Description);

                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                            .Padding(5).AlignRight()
                            .Text($"{product.UnitPrice:F2}");

                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                            .Padding(5).AlignRight()
                            .Text($"{product.Total:F2}");
                    }
                });

                // Total
                column.Item().PaddingTop(10).AlignRight()
                    .Text($"Total Bs: {_total:F2}")
                    .FontSize(14).Bold();

                // Convertir a texto
                int parteEntera = (int)_total;
                int centavos = (int)((_total - parteEntera) * 100);
                
                column.Item().PaddingTop(5)
                    .Text($"Son {ConvertirNumeroATexto(parteEntera)} {centavos:D2}/100 Bolivianos")
                    .FontSize(10);
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.AlignBottom().Column(column =>
            {
                column.Spacing(5);
                
                column.Item().PaddingTop(20).AlignRight()
                    .Text($"{_receivedAt:dd/MM/yyyy - HH:mm:ss} - {_userName}")
                    .FontSize(9).Italic();
            });
        }

        private string ConvertirNumeroATexto(int numero)
        {
            if (numero == 0) return "Cero";
            if (numero < 0) return "Menos " + ConvertirNumeroATexto(-numero);

            string texto = "";

            // Millones
            if (numero >= 1000000)
            {
                int millones = numero / 1000000;
                texto += (millones == 1 ? "Un Millón" : ConvertirNumeroATexto(millones) + " Millones");
                numero %= 1000000;
                if (numero > 0) texto += " ";
            }

            // Miles
            if (numero >= 1000)
            {
                int miles = numero / 1000;
                texto += (miles == 1 ? "Mil" : ConvertirNumeroATexto(miles) + " Mil");
                numero %= 1000;
                if (numero > 0) texto += " ";
            }

            // Centenas
            if (numero >= 100)
            {
                int centenas = numero / 100;
                string[] nombresCentenas = { "", "Ciento", "Doscientos", "Trescientos", "Cuatrocientos", 
                    "Quinientos", "Seiscientos", "Setecientos", "Ochocientos", "Novecientos" };
                
                if (numero == 100)
                    texto += "Cien";
                else
                    texto += nombresCentenas[centenas];
                
                numero %= 100;
                if (numero > 0) texto += " ";
            }

            // Decenas y unidades
            if (numero >= 20)
            {
                string[] decenas = { "", "", "Veinte", "Treinta", "Cuarenta", "Cincuenta", 
                    "Sesenta", "Setenta", "Ochenta", "Noventa" };
                int d = numero / 10;
                int u = numero % 10;
                
                texto += decenas[d];
                if (u > 0)
                {
                    texto += " y " + ConvertirUnidades(u);
                }
            }
            else if (numero >= 10)
            {
                string[] especiales = { "Diez", "Once", "Doce", "Trece", "Catorce", "Quince", 
                    "Dieciséis", "Diecisiete", "Dieciocho", "Diecinueve" };
                texto += especiales[numero - 10];
            }
            else if (numero > 0)
            {
                texto += ConvertirUnidades(numero);
            }

            return texto.Trim();
        }

        private string ConvertirUnidades(int numero)
        {
            string[] unidades = { "", "Un", "Dos", "Tres", "Cuatro", "Cinco", 
                "Seis", "Siete", "Ocho", "Nueve" };
            return unidades[numero];
        }
    }
}
