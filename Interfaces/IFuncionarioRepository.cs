using AfReparosAutomotivos.Models;

namespace AfReparosAutomotivos.Interfaces
{
    public interface IFuncionarioRepository
    {
        Task<IEnumerable<Funcionarios>> GetAtivos();
    }
}
