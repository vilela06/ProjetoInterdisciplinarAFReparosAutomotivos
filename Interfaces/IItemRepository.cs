using AfReparosAutomotivos.Models;

namespace AfReparosAutomotivos.Interfaces
{
    public interface IItemRepository
    {
        Task Add(IEnumerable<Item> itens);
        Task DeleteByOrcamento(int orcamentoId);
        Task<IEnumerable<Item>> GetByOrcamento(int orcamentoId);
        Task Update(IEnumerable<Item> itens);
    }
}
