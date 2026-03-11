namespace MicroServiceUsers.Domain.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent, CancellationToken ct = default);
    }
}
