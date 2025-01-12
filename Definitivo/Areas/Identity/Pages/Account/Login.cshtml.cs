// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Definitivo.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Definitivo.Data;
using Microsoft.EntityFrameworkCore;
using Definitivo.Models.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Definitivo.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly UserManager<Perfil> _userManager;
        private readonly SignInManager<Perfil> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public LoginModel(SignInManager<Perfil> signInManager, ILogger<LoginModel> logger, UserManager<Perfil> userManager, ApplicationDbContext applicationDbContext, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = applicationDbContext;
            _configuration = configuration;
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
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            
            /// <summary>
            ///     Login credentials for user authentication.
            /// </summary>
            [Required(ErrorMessage = "O 'Email' é obrigatório")]
            [EmailAddress(ErrorMessage = "Por favor, insira um Email válido")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     User password for authentication.
            /// </summary>
            [Required(ErrorMessage = "A 'Password' é obrigatória")]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     Option to remember user login.
            /// </summary>
            [Display(Name = "Lembrar-me?")]
            public bool RememberMe { get; set; }

            [BindProperty(Name = "cf-turnstile-response")]
            public string TurnstileResponse { get; set; }

        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            var turnstileResponse = Request.Form["cf-turnstile-response"].ToString();

            if (string.IsNullOrEmpty(turnstileResponse))
            {
                ModelState.AddModelError(string.Empty, "Por favor, complete o captcha");
                return Page();
            }

            using var client = new HttpClient();
            var response = await client.PostAsync(
                "https://challenges.cloudflare.com/turnstile/v0/siteverify",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
            { "secret", _configuration["Turnstile:SecretKey"] },
            { "response", turnstileResponse }
                })
            );

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result1 = JsonSerializer.Deserialize<TurnstileResponse>(jsonResponse);

            if (!result1.Success)
            {
                ModelState.AddModelError(string.Empty, "Falha na validação do Captcha.");
                return Page();
            }

            if (ModelState.IsValid)
            {
                // Encontra o usuário pelo email fornecido
                var user = await _userManager.FindByEmailAsync(Input.Email);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Tentativa de login inválida.");
                    return Page();
                }

                // Obtém os roles do usuário
                var roles = await _userManager.GetRolesAsync(user);
                var userRole = roles.FirstOrDefault();

                if (userRole == "Bibliotecario")
                {
                    // Verifica se o bibliotecário foi aprovado
                    var aprovacao = await _context.AprovaBibliotecario
                        .FirstOrDefaultAsync(a => a.ID_PerfilBibliotecario == user.Id);

                    if (aprovacao == null)
                    {
                        ModelState.AddModelError(string.Empty, "A sua conta ainda não foi aprovada por um administrador.");
                        return Page();
                    }
                }

                // Verifica se o usuário está bloqueado (para ambos os tipos de usuário)
                var bloqueio = await _context.BloqueiaUser
                    .FirstOrDefaultAsync(b => b.ID_PerfilUser == user.Id);

                if (bloqueio != null)
                {
                    ModelState.AddModelError(string.Empty, $"A sua conta foi bloqueada em {bloqueio.dataBloqueio}. Entre em contato com o administrador.");
                    return Page();
                }

                // Faz o login usando o UserName do usuário encontrado e a senha
                var result = await _signInManager.PasswordSignInAsync(user.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                if (result.IsNotAllowed)
                {
                    return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                }

                ModelState.AddModelError(string.Empty, "Tentativa de login inválida.");
                return Page();
            }

            return Page();
        }

    }
    public class TurnstileResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }
}
