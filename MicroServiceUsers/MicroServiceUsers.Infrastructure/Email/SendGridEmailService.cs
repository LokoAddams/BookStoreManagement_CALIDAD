using MicroServiceUsers.Domain.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Logging;

namespace MicroServiceUsers.Infrastructure.Email
{
    public class SendGridEmailService : IEmailService
    {
        private readonly SendGridClient _client;
        private readonly SendGridOptions _options;
        private readonly ILogger<SendGridEmailService>? _logger;

        public SendGridEmailService(SendGridOptions options, ILogger<SendGridEmailService>? logger = null)
        {
            _options = options;
            _logger = logger;
            
            // Log para diagnóstico
            _logger?.LogInformation("SendGrid configurado - ApiKey length: {Length}, FromEmail: {FromEmail}", 
                _options.ApiKey?.Length ?? 0, _options.FromEmail);
            
            if (string.IsNullOrEmpty(_options.ApiKey))
            {
                _logger?.LogError("ADVERTENCIA: ApiKey de SendGrid está vacía!");
            }
            
            _client = new SendGridClient(_options.ApiKey);
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent, CancellationToken ct = default)
        {
            try
            {
                _logger?.LogInformation("Intentando enviar correo a {ToEmail} con asunto: {Subject}", toEmail, subject);
                
                var from = new EmailAddress(_options.FromEmail, _options.FromName);
                var to = new EmailAddress(toEmail);
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: null, htmlContent);
                
                var response = await _client.SendEmailAsync(msg, ct);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger?.LogInformation("Correo enviado exitosamente a {ToEmail}. Status: {StatusCode}", toEmail, response.StatusCode);
                    return true;
                }
                else
                {
                    var body = await response.Body.ReadAsStringAsync(ct);
                    _logger?.LogError("Error al enviar correo a {ToEmail}. Status: {StatusCode}, Body: {Body}", 
                        toEmail, response.StatusCode, body);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Excepción al enviar correo a {ToEmail}: {Message}", toEmail, ex.Message);
                return false;
            }
        }
    }
}
