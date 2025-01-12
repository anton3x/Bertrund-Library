// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Definitivo.Models;
using System.Net.Mail;
using System.Net;
using Definitivo.Models.Services;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Shared;

namespace Definitivo.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<Perfil> _signInManager;
        private readonly UserManager<Perfil> _userManager;
        private readonly IUserStore<Perfil> _userStore;
        private readonly IUserEmailStore<Perfil> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public RegisterModel(
            UserManager<Perfil> userManager,
            IUserStore<Perfil> userStore,
            SignInManager<Perfil> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
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
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            /// 

            [Required(ErrorMessage = "O 'Nome de Utilizador' é obrigatório")]
            [StringLength(21, MinimumLength = 6, ErrorMessage = "O 'Nome de Utilizador' deve ter entre 6 e 21 caracteres")]
            [Display(Name = "Nome de Utilizador")]
            public string UserName { get; set; }

            [Required(ErrorMessage = "O 'Email' é obrigatório")]
            [EmailAddress(ErrorMessage = "Por favor, insira um email válido")]
            [Display(Name = "Email")]
            [DataType(DataType.EmailAddress)]
            public string Email { get; set; }

            [Required(ErrorMessage = "A 'Password' é obrigatória")]
            [StringLength(100, ErrorMessage = "A 'Password' deve ter entre {2} e {1} caracteres", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar Password")]
            [Compare("Password", ErrorMessage = "As passwords não coincidem")]
            public string ConfirmPassword { get; set; }

            [MaxLength(255, ErrorMessage = "A 'Morada' não pode exceder 255 caracteres")]
            [Display(Name = "Morada")]
            public string Morada { get; set; }


            [Phone(ErrorMessage = "O 'Telemóvel' não é válido")]
            [Display(Name = "Telemóvel")]
            public string? PhoneNumber { get; set; }

            [Required(ErrorMessage = "Por favor, selecione um cargo")]
            [Display(Name = "Cargo")]
            public string SelectedRole { get; set; }

        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
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
                ModelState.AddModelError(string.Empty, "Captcha validation failed");
                return Page();
            }

            if (ModelState.IsValid)
            {
                //o email ja esta usado
                if (await _userManager.FindByEmailAsync(Input.Email) != null)
                {
                    ModelState.AddModelError("Input.Email", "Este email já está a ser usado");
                    return Page();
                }

                //o username ja esta a ser usado
                if (await _userManager.FindByNameAsync(Input.UserName) != null)
                {
                    ModelState.AddModelError("Input.UserName", "Este nome de utilizador já está a ser usado");
                    return Page();
                }

                if (Input.PhoneNumber != null)
                {
                    // Verifica se já existe um user com o número de telemovel fornecido
                    var userWithThatNumber = await _userManager.Users
                        .FirstOrDefaultAsync(u => u.PhoneNumber == Input.PhoneNumber);

                    if (userWithThatNumber != null)
                    {
                        ModelState.AddModelError("Input.PhoneNumber", "Este número de telemóvel já está em uso.");
                        return Page();
                    }
                }


                //cria o perfil e mete os dados dentro dele, que correspondem ao utilizador a registar-se
                var user = new Perfil();

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
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
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


                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
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
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
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
    }
}
