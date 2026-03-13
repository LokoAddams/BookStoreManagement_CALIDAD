using ServiceDistributors.Domain.Interfaces;

namespace MicroServiceWeb.Stubs;

public class StubDistributorService : IDistributorService
{
    private readonly List<ServiceDistributors.Domain.Models.Distributor> _dists = new();
    
    public IEnumerable<ServiceDistributors.Domain.Models.Distributor> GetAll() => _dists;
    public ServiceDistributors.Domain.Models.Distributor? Read(Guid id) => _dists.FirstOrDefault(d => d.Id == id);
    public void Update(ServiceDistributors.Domain.Models.Distributor distributor) { }
    public void Create(ServiceDistributors.Domain.Models.Distributor distributor) { distributor.Id = Guid.NewGuid(); _dists.Add(distributor); }
    public void Delete(Guid id) { _dists.RemoveAll(d => d.Id == id); }
}
