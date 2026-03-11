using System;
using System.Collections.Generic;

namespace ServiceClients.Domain.Models
{
    public class Client
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }
}

namespace ServiceClients.Domain.Interfaces
{
    using ServiceClients.Domain.Models;
    public interface IClientService
    {
        IEnumerable<Client> GetAll();
        Client? Read(Guid id);
        void Update(Client client);
        void Create(Client client);
        void Delete(Guid id);
    }
}

namespace ServiceClients.Domain.Validations
{
    using System.Collections.Generic;
    using ServiceCommon.Application.Services;
    using ServiceClients.Domain.Models;
    public static class ClientValidation
    {
        public static IEnumerable<ValidationError> Validate(Client c)
        {
            if (string.IsNullOrWhiteSpace(c.FirstName)) yield return new ValidationError { Field = nameof(c.FirstName), Message = "Nombre requerido" };;
            if (string.IsNullOrWhiteSpace(c.LastName)) yield return new ValidationError { Field = nameof(c.LastName), Message = "Apellido requerido" };;
            if (!string.IsNullOrWhiteSpace(c.Email) && !c.Email.Contains('@')) yield return new ValidationError { Field = nameof(c.Email), Message = "Email inválido" };;
        }
    }
}
