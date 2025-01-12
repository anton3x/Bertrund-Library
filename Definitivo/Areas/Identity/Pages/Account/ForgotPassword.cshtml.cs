// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Definitivo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Definitivo.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<Perfil> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<Perfil> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
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
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "O 'Email' é obrigatório")]
            [EmailAddress(ErrorMessage = "Por favor, insira um 'Email' válido")]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                string subject = "Redefinir a sua palavra-passe - Bertrund";

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
                            <div class='header' style='background-color: #f4f4f4; padding: 20px; text-align: center;'>
                                <h1 style='font-family: Arial, sans-serif; color: #333;'>Bertrund</h1>
                            </div>
                            <div class='content' style='font-family: Arial, sans-serif; margin: 20px; line-height: 1.6; color: #333;'>
                                <h2 style='color: #555;'>Redefinir a sua palavra-passe</h2>
                                
                                <p>Recebemos um pedido para redefinir a palavra-passe associada à sua conta na nossa plataforma.</p>
                                
                                <p>Se fez este pedido, clique no botão abaixo para criar uma nova palavra-passe:</p>
                                
                                <div style='text-align: center; margin: 20px 0;'>
                                    <a href='{{HtmlEncoder.Default.Encode(callbackUrl)}}' 
                                       style='background-color: #007BFF; color: #fff; text-decoration: none; padding: 10px 20px; border-radius: 5px; font-size: 16px;'>
                                       Redefinir Palavra-passe
                                    </a>
                                </div>
                                
                                <p>Se o botão acima não funcionar, copie e cole o seguinte link no seu navegador:</p>
                                <p style='font-size: 12px; word-break: break-word; background-color: #f8f8f8; padding: 10px; border: 1px solid #ddd; border-radius: 5px;'>
                                    {{HtmlEncoder.Default.Encode(callbackUrl)}}
                                </p>
                                
                                <p><strong>Nota:</strong> Este link é válido por 24 horas. Após esse período, será necessário solicitar uma nova redefinição de palavra-passe.</p>
                                
                                <p>Se não solicitou a redefinição de palavra-passe, pode ignorar este email. A sua palavra-passe atual permanecerá inalterada.</p>
                            </div>
                            <div class='footer' style='background-color: #f4f4f4; padding: 20px; text-align: center; font-size: 12px; color: #777;'>
                                <p>Este é um email automático. Por favor, não responda.</p>
                                <p>&copy; {{DateTime.Now.Year}} Bertrund. Todos os direitos reservados.</p>
                            </div>
                        </body>
                        </html>";

                await _emailSender.SendEmailAsync(Input.Email, subject,
                    emailTemplate);

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
