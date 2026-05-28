using AfReparosAutomotivos.Models;

namespace AfReparosAutomotivos.Interfaces
{
    public interface IServicoRepository
    {
        Task<int> Add(Servicos servico);
        Task<IEnumerable<Servicos>> Get();
        Task<IEnumerable<Servicos>> Search(string termo);
        Task<Servicos?> GetId(int id);
        Task<decimal> GetPrecoBaseByIdAsync(int id);
        Task DeleteCreated(int id);
    }
}
