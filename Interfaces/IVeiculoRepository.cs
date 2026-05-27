using AfReparosAutomotivos.Models;

namespace AfReparosAutomotivos.Interfaces
{
    public interface IVeiculoRepository
    {
        Task<int> Add(Veiculos veiculo);
        Task<Veiculos?> GetId(int id);
        Task<IEnumerable<Veiculos>> SearchByCliente(int clienteId, string termo);
    }
}
