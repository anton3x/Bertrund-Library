// Licenciado à .NET Foundation sob um ou mais acordos.
// A .NET Foundation licencia este ficheiro sob a licença MIT.
#nullable disable

using System;
using System.Threading.Tasks;
using Definitivo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Definitivo.Areas.Identity.Pages.Account.Manage
{
    public class Disable2faModel : PageModel
    {
        private readonly UserManager<Perfil> _userManager;
        private readonly ILogger<Disable2faModel> _logger;

        public Disable2faModel(
            UserManager<Perfil> userManager,
            ILogger<Disable2faModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        ///     Esta API suporta a infraestrutura padrão do ASP.NET Core Identity e não se destina a ser usada
        ///     diretamente do seu código. Esta API pode ser alterada ou removida em versões futuras.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Não foi possível encontrar o utilizador com o ID '{_userManager.GetUserId(User)}'.");
            }

            if (!await _userManager.GetTwoFactorEnabledAsync(user))
            {
                throw new InvalidOperationException($"Não é possível desativar a autenticação 2FA para o utilizador pois não está atualmente ativa.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Não foi possível encontrar o utilizador com o ID '{_userManager.GetUserId(User)}'.");
            }

            var disable2faResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!disable2faResult.Succeeded)
            {
                throw new InvalidOperationException($"Ocorreu um erro inesperado ao desativar a autenticação 2FA.");
            }

            _logger.LogInformation("O utilizador com ID '{UserId}' desativou a autenticação 2FA.", _userManager.GetUserId(User));
            StatusMessage = "A autenticação 2FA foi desativada. Pode reativá-la quando configurar uma aplicação de autenticação";
            return RedirectToPage("./TwoFactorAuthentication");
        }
    }
}
