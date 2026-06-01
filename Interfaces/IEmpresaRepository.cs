using AfReparosAutomotivos.Models;

namespace AfReparosAutomotivos.Interfaces
{
    public interface IEmpresaRepository
    {
        Task<EmpresaViewModel?> GetDadosAsync();
        Task UpdateAsync(EmpresaViewModel empresa);
    }
}
