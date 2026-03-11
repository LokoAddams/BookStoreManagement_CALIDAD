using System;
using System.Collections.Generic;

namespace ServiceDistributors.Domain.Models
{
    public class Distributor
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string? ContactEmail { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }
}

namespace ServiceDistributors.Domain.Interfaces
{
    using ServiceDistributors.Domain.Models;
    public interface IDistributorService
    {
        IEnumerable<Distributor> GetAll();
        Distributor? Read(Guid id);
        void Update(Distributor distributor);
        void Create(Distributor distributor);
        void Delete(Guid id);
    }
}

namespace ServiceDistributors.Domain.Validations
{
    using System.Collections.Generic;
    using ServiceCommon.Application.Services;
    using ServiceDistributors.Domain.Models;
    public static class DistributorValidation
    {
        public static IEnumerable<ValidationError> Validate(Distributor d)
        {
            if (string.IsNullOrWhiteSpace(d.Name)) yield return new ValidationError { Field = nameof(d.Name), Message = "Nombre requerido" };;
        }
    }
}
