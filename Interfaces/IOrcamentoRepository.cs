using AfReparosAutomotivos.Models;

namespace AfReparosAutomotivos.Interfaces
{
    public interface IOrcamentoRepository
    {
        Task<int> Add(Orcamentos orcamento);
        Task Delete(int id);
        Task<IEnumerable<OrcamentosViewModel>> GetByChaveCliente(string chaveAcesso);
        Task<IEnumerable<OrcamentosViewModel>> GetFilter(OrcamentosFilterViewModel filtros);
        Task<OrcamentosViewModel?> GetId(int id);
        Task Update(OrcamentosViewModel orcamento);
    }
}
