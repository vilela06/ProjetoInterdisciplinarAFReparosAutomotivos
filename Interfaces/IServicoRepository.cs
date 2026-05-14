using AfReparosAutomotivos.Models;

namespace AfReparosAutomotivos.Interfaces
{
    public interface IServicoRepository
    {
        Task<int> Add(Servicos servico);
        Task<IEnumerable<Servicos>> Get();
        Task<Servicos?> GetId(int id);
        Task<decimal> GetPrecoBaseByIdAsync(int id);
    }
}
