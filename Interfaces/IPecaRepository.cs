using AfReparosAutomotivos.Models;

namespace AfReparosAutomotivos.Interfaces
{
    public interface IPecaRepository
    {
        Task<int> Add(Pecas peca);
        Task<IEnumerable<Pecas>> GetDisponiveis();
        Task<IEnumerable<Pecas>> GetAll();
        Task<IEnumerable<Pecas>> Search(string termo);
        Task<Pecas?> GetId(int id);
        Task Update(Pecas peca);
    }
}
