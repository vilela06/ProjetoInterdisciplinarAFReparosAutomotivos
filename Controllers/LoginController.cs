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
        /// <summary>
        /// Reserva espaço para, no construtor, receber e guardar uma instância do repositório de login.
        /// </summary>
        private readonly ILoginRepository _loginRepository;

        /// <summary>
        /// Atribui a instância do repositório de login ao espaço reservado.
        /// </summary>
        public LoginController(ILoginRepository loginRepository)
        {
            _loginRepository = loginRepository;
        }

        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return Json(new { message = "Usuário já logado." });
            }
            return View();
        }

        public IActionResult Erro()
        {
            return View();
        }

        /// <summary>
        /// Garante que somente requisições POST possam acessar este método.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Logar(string username, string senha)
        {
            /// Retorna o funcionário com base nas credenciais fornecidas. Retorna null se não encontrar.
            var funcionario = await _loginRepository.GetFuncionarioByCredentialsAsync(username, senha);

            if (funcionario != null)
            {
                /// Cria uma lista com informações (claims) do usuário autenticado.
                List<Claim> direitosAcesso = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, funcionario.idFuncionario.ToString()),
                    new Claim(ClaimTypes.Name, funcionario.Nome)
                };

                /// Cria o cartão de identidade do usuário(com todos os claims) e o principal (usuário).
                var identity = new ClaimsIdentity(direitosAcesso, "Identity.Login");
                var user = new ClaimsPrincipal(new[] { identity });

                /// Loga o usuário na aplicação. E define o cookie como não persistente.
                await HttpContext.SignInAsync(user, new AuthenticationProperties
                {
                    IsPersistent = false
                });
                return RedirectToAction("Index", "Orcamentos");
            }
            var erro = new Modal
            {
                Title = "Credenciais inválidas",
                Mensagem = "O usuário ou senha fornecidos são inválidos."
            };
            /// TempData é uum dicionário temporário para armazenar dados entre requisições. JsonSerializer converte o objeto em string JSON. A view pode acessar TempData["Mensagem"] e desserializar o JSON de volta para um objeto Modal.
            TempData["Mensagem"] = JsonSerializer.Serialize(erro);
            return View("Index");
        }
        
        public async Task<IActionResult> Logout()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                /// Remove o cookie de autenticação.
                await HttpContext.SignOutAsync();
            }
            return RedirectToAction("Index", "Home");
        }
    }
}