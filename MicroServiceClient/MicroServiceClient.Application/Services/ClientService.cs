using MicroServiceClient.Domain.Interfaces;
using MicroServiceClient.Domain.Models;
using MicroServiceClient.Domain.Validations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MicroServiceClient.Application.Services
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _repository;

        public ClientService(IClientRepository repository)
        {
            _repository = repository;
        }

        public List<Client> GetAll() => _repository.GetAll();

        public Task<PagedResult<Client>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default) 
            => _repository.GetPagedAsync(page, pageSize, ct);

        public Client? Read(Guid id) => _repository.Read(id);

        public Task<Client?> GetByCiAsync(string ci, CancellationToken ct = default)
        {
            var normalized = TextRules.NormalizeCi(ci);
            return _repository.GetByCiAsync(normalized, ct);
        }

        public void Create(Client client)
        {
            var errors = ClientValidation.Validate(client).ToList();

            if (_repository.ExistsByCi(client.Ci))
                errors.Add(new ValidationError(nameof(client.Ci), "El CI ya está registrado."));

            if (errors.Any())
                throw new ValidationException(errors);

            ClientValidation.Normalize(client);
            _repository.Create(client);
        }

        public void Update(Client client)
        {
            var errors = ClientValidation.Validate(client).ToList();

            if (_repository.ExistsByCi(client.Ci, client.Id))
                errors.Add(new ValidationError(nameof(client.Ci), "El CI ya está registrado."));

            if (errors.Any())
                throw new ValidationException(errors);

            ClientValidation.Normalize(client);
            _repository.Update(client);
        }

        public void Delete(Guid id) => _repository.Delete(id);
    }
}
