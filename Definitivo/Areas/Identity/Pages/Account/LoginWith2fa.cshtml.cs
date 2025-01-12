// Licenciado à .NET Foundation sob um ou mais acordos.
// A .NET Foundation licencia este ficheiro sob a licença MIT.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Definitivo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Definitivo.Areas.Identity.Pages.Account
{
    public class LoginWith2faModel : PageModel
    {
        private readonly SignInManager<Perfil> _signInManager;
        private readonly UserManager<Perfil> _userManager;
        private readonly ILogger<LoginWith2faModel> _logger;

        public LoginWith2faModel(
            SignInManager<Perfil> signInManager,
            UserManager<Perfil> userManager,
            ILogger<LoginWith2faModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        ///     Esta API suporta a infraestrutura padrão do ASP.NET Core Identity e não se destina a ser usada
        ///     diretamente do seu código. Esta API pode ser alterada ou removida em versões futuras.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     Esta API suporta a infraestrutura padrão do ASP.NET Core Identity e não se destina a ser usada
        ///     diretamente do seu código. Esta API pode ser alterada ou removida em versões futuras.
        /// </summary>
        public bool RememberMe { get; set; }

        /// <summary>
        ///     Esta API suporta a infraestrutura padrão do ASP.NET Core Identity e não se destina a ser usada
        ///     diretamente do seu código. Esta API pode ser alterada ou removida em versões futuras.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     Esta API suporta a infraestrutura padrão do ASP.NET Core Identity e não se destina a ser usada
        ///     diretamente do seu código. Esta API pode ser alterada ou removida em versões futuras.
        /// </summary>
        public class InputModel
        {
            [Required(ErrorMessage = "O código de autenticação é obrigatório")]
            [StringLength(7, ErrorMessage = "O {0} deve ter entre {2} e {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "Código de autenticação")]
            public string TwoFactorCode { get; set; }

            [Display(Name = "Memorizar este dispositivo")]
            public bool RememberMachine { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(bool rememberMe, string returnUrl = null)
        {
            // Garantir que o utilizador passou primeiro pelo ecrã de nome de utilizador e palavra-passe
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new InvalidOperationException($"Não foi possível carregar o utilizador para autenticação de dois fatores.");
            }

            ReturnUrl = returnUrl;
            RememberMe = rememberMe;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(bool rememberMe, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Não foi possível carregar o utilizador para autenticação de dois fatores.");
            }

            var authenticatorCode = Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, Input.RememberMachine);

            var userId = await _userManager.GetUserIdAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("Utilizador com ID '{UserId}' iniciou sessão com autenticação 2FA.", user.Id);
                return LocalRedirect(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("Conta do utilizador com ID '{UserId}' bloqueada.", user.Id);
                return RedirectToPage("./Lockout");
            }
            else
            {
                _logger.LogWarning("Código de autenticação inválido introduzido para o utilizador com ID '{UserId}'.", user.Id);
                ModelState.AddModelError(string.Empty, "Código de autenticação inválido.");
                return Page();
            }
        }
    }
}
