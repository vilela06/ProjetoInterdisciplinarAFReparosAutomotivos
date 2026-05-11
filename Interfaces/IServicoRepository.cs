using AfReparosAutomotivos.Models;

namespace AfReparosAutomotivos.Interfaces
{
    public interface IServicoRepository
    {
        Task<int> Add(Servicos servico);
        Task Delete(int id);
        Task<IEnumerable<Servicos>> Get();
        Task<Servicos?> GetId(int id);
        Task<Servicos?> Update(int id);
        Task Update(Servicos servico);
        Task<decimal> GetPrecoBaseByIdAsync(int id);
    }
}
