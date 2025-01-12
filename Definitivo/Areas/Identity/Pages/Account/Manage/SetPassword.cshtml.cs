 // Licenciado à .NET Foundation sob um ou mais acordos.
// A .NET Foundation licencia este ficheiro para si sob a licença MIT.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Definitivo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Definitivo.Areas.Identity.Pages.Account.Manage
{
    public class SetPasswordModel : PageModel
    {
        private readonly UserManager<Perfil> _userManager;
        private readonly SignInManager<Perfil> _signInManager;

        public SetPasswordModel(
            UserManager<Perfil> userManager,
            SignInManager<Perfil> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "O campo {0} é obrigatório")]
            [StringLength(100, ErrorMessage = "A {0} deve ter entre {2} e {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&\-._#*/|])[A-Za-z\d@$!%*?&\-._#*/|]{6,}$",
                ErrorMessage = "A Password deve conter pelo menos uma letra maiúscula, uma minúscula, um número, um caractere especial e ter no minimo 6 caracteres.")]
            [Display(Name = "Nova Password")]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar Password")]
            [Compare("NewPassword", ErrorMessage = "A nova Password e a confirmação não correspondem.")]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&\-._#*/|])[A-Za-z\d@$!%*?&\-._#*/|]{6,}$",
                ErrorMessage = "A Password deve conter pelo menos uma letra maiúscula, uma minúscula, um número, um caractere especial e ter no minimo 6 caracteres.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Não foi possível carregar o utilizador com ID '{_userManager.GetUserId(User)}'.");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);

            if (hasPassword)
            {
                return RedirectToPage("./ChangePassword");
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
                return NotFound($"Não foi possível carregar o utilizador com ID '{_userManager.GetUserId(User)}'.");
            }

            var addPasswordResult = await _userManager.AddPasswordAsync(user, Input.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Erro ao alterar a password.");
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "A sua palavra-passe foi definida.";

            return RedirectToPage();
        }
    }
}
