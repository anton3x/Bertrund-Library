// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Definitivo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Mail;
using System.Net;
using System.Text.Encodings.Web;

namespace Definitivo.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        private readonly UserManager<Perfil> _userManager;
        private readonly IEmailSender _sender;

        public RegisterConfirmationModel(UserManager<Perfil> userManager, IEmailSender sender)
        {
            _userManager = userManager;
            _sender = sender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public bool DisplayConfirmAccountLink { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string EmailConfirmationUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            if (email == null)
            {
                return RedirectToPage("/Index");
            }
            returnUrl = returnUrl ?? Url.Content("~/");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"Unable to load user with email '{email}'.");
            }

            Email = email;

            ViewData["Email"] = email;
            ViewData["ConfirmationLink"] = returnUrl;

            return Page();
        }

        // AccountController.cs
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendConfirmationEmail(string email, string callbackUrl)
        {

            // Email template
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

            // Send email
            await _sender.SendEmailAsync(email, subject, emailTemplate);

            return RedirectToPage("RegisterConfirmation", new { email });


        }
        public async Task<IActionResult> OnPostResendConfirmationEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email não fornecido.");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"Não foi possível encontrar o usuário com o email '{email}'.");
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);

            // Use seu template de email personalizado aqui
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

            await _sender.SendEmailAsync(
                email,
                "Confirmação de Email - Bertrund",
                emailTemplate);

            TempData["SuccessMessage"] = "Email de confirmação reenviado com sucesso!";
            return RedirectToPage("RegisterConfirmation", new { email });
        }

    }
}
