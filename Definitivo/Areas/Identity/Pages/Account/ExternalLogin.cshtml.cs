// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Definitivo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Definitivo.Data;

namespace Definitivo.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<Perfil> _signInManager;
        private readonly UserManager<Perfil> _userManager;
        private readonly IUserStore<Perfil> _userStore;
        private readonly IUserEmailStore<Perfil> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;
        private readonly ApplicationDbContext _context;

        public ExternalLoginModel(
            SignInManager<Perfil> signInManager,
            UserManager<Perfil> userManager,
            IUserStore<Perfil> userStore,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender, ApplicationDbContext applicationDbContext)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _emailSender = emailSender;
            _context = applicationDbContext;
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
        public string ProviderDisplayName { get; set; }

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
            [Required(ErrorMessage = "O nome de utilizador é obrigatório")]
            [StringLength(21, MinimumLength = 6, ErrorMessage = "O nome de utilizador deve ter entre 6 e 21 caracteres")]
            [Display(Name = "UserName")]
            public string UserName { get; set; }

            [Required(ErrorMessage = "O email é obrigatório")]
            [EmailAddress(ErrorMessage = "Por favor, insira um email válido")]
            [StringLength(256, ErrorMessage = "O email deve ter no máximo 256 caracteres")]
            [Display(Name = "Email")]
            [DataType(DataType.EmailAddress)]
            public string Email { get; set; }

            [Required(ErrorMessage = "A password é obrigatória")]
            [Display(Name = "Password")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [MaxLength(255, ErrorMessage = "A morada não pode exceder 255 caracteres")]
            [Display(Name = "Morada")]
            public string Morada { get; set; }


            [Phone(ErrorMessage = "O número de telemóvel não é válido")]
            [Display(Name = "Telemóvel")]
            public string? PhoneNumber { get; set; }

            [Required(ErrorMessage = "Por favor, selecione um cargo")]
            [Display(Name = "Cargo")]
            public string SelectedRole { get; set; }
        }
        
        public IActionResult OnGet() => RedirectToPage("./Login");

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Obter o email do provedor externo
            var _email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (_email != null)
            {
                // Verificar se existe um usuário com este email
                var user = await _userManager.FindByEmailAsync(_email);
                if (user != null)
                {
                    if (user.EstadoAtivacao == "Bloqueado")
                    {
                        //procura pelo bloqueio na tabela BloqueiaUser
                        var bloqueio = await _context.BloqueiaUser
                    .FirstOrDefaultAsync(b => b.ID_PerfilUser == user.Id);

                        ErrorMessage =  $"A sua conta foi bloqueada em {bloqueio.dataBloqueio}. Entre em contato com o administrador.";

                        return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                    }

                    if(user.EmailConfirmed == false)
                    {
                        //ErrorMessage = "Confirme o email da sua conta.";
                        return RedirectToPage("/Account/RegisterConfirmation", new { area = "Identity", Email = _email });

                    }
                }
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: false);
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return RedirectToPage("./");
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl});
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return RedirectToPage("./Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ReturnUrl = returnUrl;
                ProviderDisplayName = info.ProviderDisplayName;
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    Input = new InputModel
                    {
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                    };
                }
                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                // Verificar se o nome de usuário já está em uso
                var existingUserByUsername = await _userManager.FindByNameAsync(Input.UserName);
                if (existingUserByUsername != null)
                {
                    ModelState.AddModelError(string.Empty, "O nome de utilizador já está em uso.");
                    return Page();
                }

                // Verificar se o email já está em uso
                var existingUserByEmail = await _userManager.FindByEmailAsync(Input.Email);
                if (existingUserByEmail != null)
                {
                    ModelState.AddModelError(string.Empty, "O email já está em uso.");
                    return Page();
                }

                if (Input.PhoneNumber != null)
                {
                    // Verifica se já existe um user com o número de telemovel fornecido
                    var userWithThatNumber = await _userManager.Users
                        .FirstOrDefaultAsync(u => u.PhoneNumber == Input.PhoneNumber);

                    if (userWithThatNumber != null)
                    {
                        ModelState.AddModelError(string.Empty, "Este número de telemóvel já está em uso.");
                        return Page();
                    }
                }

                var user = CreateUser();

                user.UserName = Input.UserName;
                user.Email = Input.Email;
                user.Morada = Input.Morada;
                user.PhoneNumber = Input.PhoneNumber;

                //se o utilizador escolher bibliotecario, o estado de ativacao é "PorAtivar", pois tem que ser ativado pelo admin
                if (Input.SelectedRole == "Bibliotecario")
                {
                    user.EstadoAtivacao = "PorAtivar";
                }
                else
                {
                    user.EstadoAtivacao = "Ativo"; //se nao, ta simplesmente ativo
                }

                await _userStore.SetUserNameAsync(user, Input.UserName, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code },
                            protocol: Request.Scheme);

                        await _userManager.AddToRoleAsync(user, Input.SelectedRole); //adiciona a role a ele

                        string subject = "Confirmação da Conta - Bertrund";

                        string emailTemplate = $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <meta charset='utf-8'>
                            <style>
                                body {{
                                    font-family: Arial, sans-serif;
                                    line-height: 1.6;
                                    color: #333333;
                                    max-width: 600px;
                                    margin: 0 auto;
                                    padding: 20px;
                                }}
                                .header {{
                                    background-color: #2c3e50;
                                    color: white;
                                    padding: 20px;
                                    text-align: center;
                                    border-radius: 5px 5px 0 0;
                                }}
                                .content {{
                                    background-color: #ffffff;
                                    padding: 30px;
                                    border: 1px solid #dedede;
                                    border-radius: 0 0 5px 5px;
                                }}
                                .button {{
                                    display: inline-block;
                                    padding: 12px 24px;
                                    background-color: #3498db;
                                    color: #ffffff !important;
                                    text-decoration: none;
                                    border-radius: 5px;
                                    margin: 20px 0;
                                    transition: background-color 0.3s;
                                }}
                                .button:hover {{
                                    background-color: #2980b9;
                                    color: #ffffff !important;
                                }}
                                .button:visited {{
                                    color: #ffffff !important;
                                }}
                                .button:active {{
                                    color: #ffffff !important;
                                }}
                                .footer {{
                                    margin-top: 20px;
                                    text-align: center;
                                    font-size: 12px;
                                    color: #666666;
                                }}
                            </style>
                        </head>
                        <body>
                            <div class='header'>
                                <h1>Bertrund</h1>
                            </div>
                            <div class='content'>
                                <h2>Bem-vindo(a) à Bertrund!</h2>
            
                                <p>Obrigado por se registar na nossa plataforma. Para começar a utilizar a sua conta, precisamos de confirmar o seu endereço de email.</p>
            
                                <div style='text-align: center;'>
                                    <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' class='button'>Confirmar Conta</a>
                                </div>
            
                                <p>Se o botão acima não funcionar, pode copiar e colar o seguinte link no seu navegador:</p>
                                <p style='font-size: 12px; word-break: break-all;'>
                                    {HtmlEncoder.Default.Encode(callbackUrl)}
                                </p>
            
                                <p><strong>Nota:</strong> Este link é válido por 24 horas.</p>
            
                                <p>Se não solicitou este registo, pode ignorar este email.</p>
                            </div>
                            <div class='footer'>
                                <p>Este é um email automático. Por favor, não responda.</p>
                                <p>&copy; {DateTime.Now.Year} Bertrund. Todos os direitos reservados.</p>
                            </div>
                        </body>
                        </html>";

                        await _emailSender.SendEmailAsync(Input.Email, subject,
                            emailTemplate);

                        // If account confirmation is required, we need to show the link if we don't have a real email sender
                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
                        }

                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }

        private Perfil CreateUser()
        {
            try
            {
                return Activator.CreateInstance<Perfil>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(Perfil)}'. " +
                    $"Ensure that '{nameof(Perfil)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
            }
        }

        private IUserEmailStore<Perfil> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<Perfil>)_userStore;
        }

        public async Task<IActionResult> OnPostLinkExistingAccountAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            // Obter as informações do login externo
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Erro ao carregar as informações do login externo.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Verificar se o nome de utilizador e senha fornecidos correspondem a uma conta local existente
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user != null)
            {
                var passwordCheck = await _signInManager.CheckPasswordSignInAsync(user, Input.Password, lockoutOnFailure: false);
                if (passwordCheck.Succeeded)
                {
                    // Associar o login externo à conta local
                    var result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        // Fazer sign-in do utilizador
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Credenciais inválidas.");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Conta não encontrada.");
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }

    }
}
