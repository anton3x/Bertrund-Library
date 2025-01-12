// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Definitivo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Definitivo.Areas.Identity.Pages.Account.Manage
{
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<Perfil> _userManager;
        private readonly SignInManager<Perfil> _signInManager;
        private readonly ILogger<ChangePasswordModel> _logger;

        public ChangePasswordModel(
            UserManager<Perfil> userManager,
            SignInManager<Perfil> signInManager,
            ILogger<ChangePasswordModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {

            [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
            [DataType(DataType.Password)]
            [Display(Name = "Password Atual")]
            public string OldPassword { get; set; }

            [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
            [StringLength(100, ErrorMessage = "A '{0}' deve ter entre {2} e {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&\-._#*/|])[A-Za-z\d@$!%*?&\-._#*/|]{6,}$",
                ErrorMessage = "A Password deve conter pelo menos uma letra maiúscula, uma minúscula, um número, um caractere especial e ter no minimo 6 caracteres.")]
            [Display(Name = "Nova Password")]
            public string NewPassword { get; set; }

            [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
            [StringLength(100, ErrorMessage = "A '{0}' deve ter entre {2} e {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&\-._#*/|])[A-Za-z\d@$!%*?&\-._#*/|]{6,}$",
                ErrorMessage = "A Password deve conter pelo menos uma letra maiúscula, uma minúscula, um número, um caractere especial e ter no minimo 6 caracteres.")]
            [Display(Name = "Confirmar Nova Password")]
            [Compare("NewPassword", ErrorMessage = "A nova Password e a confirmação não correspondem.")]
            public string ConfirmPassword { get; set; }

        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Não foi possível encontrar o utilizador com o ID '{_userManager.GetUserId(User)}'.");

            }

            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                return RedirectToPage("./SetPassword");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Não foi possível encontrar o utilizador com o ID '{_userManager.GetUserId(User)}'.");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("O utilizador alterou a palavra-passe com sucesso.");
            StatusMessage = "A sua palavra-passe foi alterada com sucesso.";

            return RedirectToPage();
        }
    }
}
