using AfReparosAutomotivos.Models;

namespace AfReparosAutomotivos.Interfaces
{
    public interface IClienteRepository
    {
        Task<int> Add(Clientes cliente);
        Task<IEnumerable<Clientes>> GetAllAsync();
        Task<IEnumerable<Clientes>> Search(string termo);
        Task<Clientes?> GetId(int id);
        Task Update(Clientes cliente);
        Task DeleteCreated(int id);
    }
}
