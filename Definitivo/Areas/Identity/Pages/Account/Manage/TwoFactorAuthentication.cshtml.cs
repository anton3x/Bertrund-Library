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
    public class TwoFactorAuthenticationModel : PageModel
    {
        private readonly UserManager<Perfil> _userManager;
        private readonly SignInManager<Perfil> _signInManager;
        private readonly ILogger<TwoFactorAuthenticationModel> _logger;

        public TwoFactorAuthenticationModel(
            UserManager<Perfil> userManager, SignInManager<Perfil> signInManager, ILogger<TwoFactorAuthenticationModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        ///     Esta API suporta a infraestrutura padrão do ASP.NET Core Identity e não se destina a ser usada
        ///     diretamente do seu código. Esta API pode ser alterada ou removida em versões futuras.
        /// </summary>
        public bool HasAuthenticator { get; set; }

        /// <summary>
        ///     Esta API suporta a infraestrutura padrão do ASP.NET Core Identity e não se destina a ser usada
        ///     diretamente do seu código. Esta API pode ser alterada ou removida em versões futuras.
        /// </summary>
        public int RecoveryCodesLeft { get; set; }

        /// <summary>
        ///     Esta API suporta a infraestrutura padrão do ASP.NET Core Identity e não se destina a ser usada
        ///     diretamente do seu código. Esta API pode ser alterada ou removida em versões futuras.
        /// </summary>
        [BindProperty]
        public bool Is2faEnabled { get; set; }

        /// <summary>
        ///     Esta API suporta a infraestrutura padrão do ASP.NET Core Identity e não se destina a ser usada
        ///     diretamente do seu código. Esta API pode ser alterada ou removida em versões futuras.
        /// </summary>
        public bool IsMachineRemembered { get; set; }

        /// <summary>
        ///     Esta API suporta a infraestrutura padrão do ASP.NET Core Identity e não se destina a ser usada
        ///     diretamente do seu código. Esta API pode ser alterada ou removida em versões futuras.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Não foi possível encontrar o utilizador com o ID '{_userManager.GetUserId(User)}'.");
            }

            HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null;
            Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user);
            RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Não foi possível encontrar o utilizador com o ID '{_userManager.GetUserId(User)}'.");
            }

            await _signInManager.ForgetTwoFactorClientAsync();
            StatusMessage = "Este navegador foi esquecido. Quando iniciar sessão novamente neste navegador, será solicitado o seu código de autenticação 2FA.";
            return RedirectToPage();
        }
    }
}
