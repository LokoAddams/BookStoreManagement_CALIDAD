using ServiceClients.Domain.Interfaces;

namespace MicroServiceWeb.Stubs;

public class StubClientService : IClientService
{
    private readonly List<ServiceClients.Domain.Models.Client> _clients = new();
    
    public IEnumerable<ServiceClients.Domain.Models.Client> GetAll() => _clients;
    public ServiceClients.Domain.Models.Client? Read(Guid id) => _clients.FirstOrDefault(c => c.Id == id);
    public void Update(ServiceClients.Domain.Models.Client client) { }
    public void Create(ServiceClients.Domain.Models.Client client) { client.Id = Guid.NewGuid(); _clients.Add(client); }
    public void Delete(Guid id) { _clients.RemoveAll(c => c.Id == id); }
}
