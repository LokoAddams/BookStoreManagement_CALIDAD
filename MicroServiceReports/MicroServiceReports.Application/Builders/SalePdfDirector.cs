using System.Text.Json;

namespace MicroServiceReports.Application.Builders
{
    /// <summary>
    /// Director que orquesta la construcción del PDF usando el Builder
    /// </summary>
    public class SalePdfDirector
    {
        private readonly ISalePdfBuilder _builder;

        public SalePdfDirector(ISalePdfBuilder builder)
        {
            _builder = builder;
        }

        /// <summary>
        /// Construye un PDF completo a partir del JSON de la venta
        /// </summary>
        public byte[] ConstructSalePdf(string saleJsonPayload, DateTime receivedAt)
        {
            _builder.Reset();

            // Parsear JSON
            var saleData = JsonDocument.Parse(saleJsonPayload);
            var root = saleData.RootElement;

            // Extraer fecha de venta
            DateTime saleDate = ExtractSaleDate(root);

            // Extraer información del usuario
            string user = ExtractProperty(root, "User", "user", "N/A");

            // Extraer CI
            string ci = ExtractProperty(root, "Ci", "ci", "N/A");

            // Extraer cliente
            string client = ExtractProperty(root, "Client", "client", "N/A");

            // Extraer total
            decimal total = ExtractDecimal(root, "Total", "total");

            // Configurar información de la venta
            _builder.SetSaleInfo(saleDate, client, ci);
            _builder.SetUserInfo(user);
            _builder.SetTotal(total);
            _builder.SetReceivedAt(receivedAt);

            // Agregar productos
            AddProductsFromJson(root);

            // Construir y retornar PDF
            return _builder.Build();
        }

        private DateTime ExtractSaleDate(JsonElement root)
        {
            if (root.TryGetProperty("Date", out var dateProp))
            {
                return dateProp.GetDateTime();
            }
            else if (root.TryGetProperty("date", out var dateLower))
            {
                return dateLower.GetDateTime();
            }
            return DateTime.UtcNow;
        }

        private string ExtractProperty(JsonElement root, string propertyUpper, string propertyLower, string defaultValue)
        {
            if (root.TryGetProperty(propertyUpper, out var prop))
            {
                return prop.GetString() ?? defaultValue;
            }
            else if (root.TryGetProperty(propertyLower, out var propLower))
            {
                return propLower.GetString() ?? defaultValue;
            }
            return defaultValue;
        }

        private decimal ExtractDecimal(JsonElement root, string propertyUpper, string propertyLower)
        {
            if (root.TryGetProperty(propertyUpper, out var prop))
            {
                return prop.GetDecimal();
            }
            else if (root.TryGetProperty(propertyLower, out var propLower))
            {
                return propLower.GetDecimal();
            }
            return 0;
        }

        private int ExtractInt(JsonElement element, string propertyUpper, string propertyLower)
        {
            if (element.TryGetProperty(propertyUpper, out var prop))
            {
                return prop.GetInt32();
            }
            else if (element.TryGetProperty(propertyLower, out var propLower))
            {
                return propLower.GetInt32();
            }
            return 0;
        }

        private void AddProductsFromJson(JsonElement root)
        {
            JsonElement productsArray;
            bool hasProducts = root.TryGetProperty("Products", out productsArray) || 
                               root.TryGetProperty("products", out productsArray);

            if (hasProducts)
            {
                foreach (var product in productsArray.EnumerateArray())
                {
                    string name = ExtractProperty(product, "Name", "name", "Sin nombre");
                    int quantity = ExtractInt(product, "Quantity", "quantity");
                    decimal unitPrice = ExtractDecimal(product, "UnitPrice", "unitPrice");

                    _builder.AddProduct(quantity, name, unitPrice);
                }
            }
        }
    }
}
