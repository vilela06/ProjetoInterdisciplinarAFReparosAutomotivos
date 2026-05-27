using AfReparosAutomotivos.Models;

namespace AfReparosAutomotivos.Interfaces
{
    public interface IOrcamentoRepository
    {
        Task<int> Add(Orcamentos orcamento);
        Task<IEnumerable<OrcamentosViewModel>> GetByChaveCliente(string chaveAcesso);
        Task<IEnumerable<OrcamentosViewModel>> GetFilter(OrcamentosFilterViewModel filtros);
        Task<OrcamentosViewModel?> GetId(int id);
        Task Delete(int id);
        Task<bool> UpdateStatusByChaveCliente(int id, string chaveAcesso, int status);
        Task Update(OrcamentosViewModel orcamento);
    }
}
