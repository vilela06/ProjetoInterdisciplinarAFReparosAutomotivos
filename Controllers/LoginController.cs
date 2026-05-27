using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models.ViewModels;
using System.Text.Json;

namespace AfReparosAutomotivos.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILoginRepository _loginRepository;

        public LoginController(ILoginRepository loginRepository)
        {
            _loginRepository = loginRepository;
        }

        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Orcamentos");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logar(string username, string senha)
        {
            var funcionario = await _loginRepository.GetFuncionarioByCredentialsAsync(username, senha);

            if (funcionario != null)
            {
                var direitosAcesso = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, funcionario.idFuncionario.ToString()),
                    new Claim(ClaimTypes.Name, funcionario.Nome),
                    new Claim(ClaimTypes.Role, funcionario.permissao.ToString())
                };

                var identity = new ClaimsIdentity(direitosAcesso, "Identity.Login");
                var user = new ClaimsPrincipal(new[] { identity });

                await HttpContext.SignInAsync("Identity.Login", user, new AuthenticationProperties
                {
                    IsPersistent = false
                });

                return RedirectToAction("Index", "Orcamentos");
            }

            var erro = new Modal
            {
                Title = "Credenciais invalidas",
                Mensagem = "O usuario ou senha fornecidos sao invalidos."
            };
            TempData["Mensagem"] = JsonSerializer.Serialize(erro);
            return View("Index");
        }
        
        public async Task<IActionResult> Logout()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                await HttpContext.SignOutAsync("Identity.Login");
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
