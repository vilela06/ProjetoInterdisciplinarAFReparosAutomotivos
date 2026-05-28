using AfReparosAutomotivos.Models;

namespace AfReparosAutomotivos.Interfaces
{
    public interface IFuncionarioRepository
    {
        Task<IEnumerable<Funcionarios>> GetAll();
        Task<IEnumerable<Funcionarios>> GetAtivos();
        Task<IEnumerable<Funcionarios>> Search(string pesquisa);
        Task<Funcionarios?> GetId(int id);
        Task Add(Funcionarios funcionario);
        Task Update(Funcionarios funcionario);
    }
}
