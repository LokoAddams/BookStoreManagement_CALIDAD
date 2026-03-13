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

            string resultado = "";

            // Procesar bloques de magnitud
            resultado = AgregarSeccion(resultado, ref numero, 1000000, "Un Millón", "Millones");
            resultado = AgregarSeccion(resultado, ref numero, 1000, "Mil", "Mil");
            resultado = ProcesarResto(resultado, numero);

            return resultado.Trim();
        }

        private string AgregarSeccion(string texto, ref int numero, int divisor, string singular, string plural)
        {
            if (numero < divisor) return texto;

            int cantidad = numero / divisor;
            string prefijo = (cantidad == 1 && divisor >= 1000) ? singular : $"{ConvertirNumeroATexto(cantidad)} {plural}";

            numero %= divisor;
            return $"{texto} {prefijo}".TrimStart();
        }

        private string ProcesarResto(string texto, int numero)
        {
            if (numero <= 0) return texto;

            string sufijo = "";
            if (numero >= 100) sufijo = ProcesarCentenas(ref numero);
            else if (numero >= 20) sufijo = ProcesarDecenas(ref numero);
            else sufijo = ProcesarEspeciales(numero);

            return $"{texto} {sufijo}".TrimStart();
        }

        private string ProcesarCentenas(ref int numero)
        {
            if (numero == 100) return "Cien";
            string[] nombres = { "", "Ciento", "Doscientos", "Trescientos", "Cuatrocientos", "Quinientos", "Seiscientos", "Setecientos", "Ochocientos", "Novecientos" };
            int c = numero / 100;
            numero %= 100;
            return nombres[c];
        }

        private string ProcesarDecenas(ref int numero)
        {
            string[] decenas = { "", "", "Veinte", "Treinta", "Cuarenta", "Cincuenta", "Sesenta", "Setenta", "Ochenta", "Noventa" };
            int d = numero / 10;
            int u = numero % 10;
            numero = 0; // Finalizar proceso
            return u > 0 ? $"{decenas[d]} y {ConvertirUnidades(u)}" : decenas[d];
        }

        private string ProcesarEspeciales(int numero)
        {
            string[] especiales = { "Diez", "Once", "Doce", "Trece", "Catorce", "Quince", "Dieciséis", "Diecisiete", "Dieciocho", "Diecinueve" };
            return numero >= 10 ? especiales[numero - 10] : ConvertirUnidades(numero);
        }

        private string ConvertirUnidades(int numero)
        {
            string[] unidades = { "", "Un", "Dos", "Tres", "Cuatro", "Cinco", 
                "Seis", "Siete", "Ocho", "Nueve" };
            return unidades[numero];
        }
    }
}
