using AfReparosAutomotivos.Models;

namespace AfReparosAutomotivos.Interfaces
{
    public interface ILoginRepository
    {
        Task<Funcionarios?> GetFuncionarioByCredentialsAsync(string usuario, string senha);
    }
}
