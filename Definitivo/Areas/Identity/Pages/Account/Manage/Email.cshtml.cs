// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Printing;
using System.Drawing;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Definitivo.Models;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using static System.Net.Mime.MediaTypeNames;
using System.Security.Policy;

namespace Definitivo.Areas.Identity.Pages.Account.Manage
{
    public class EmailModel : PageModel
    {
        private readonly UserManager<Perfil> _userManager;
        private readonly SignInManager<Perfil> _signInManager;
        private readonly IEmailSender _emailSender;

        public EmailModel(
            UserManager<Perfil> userManager,
            SignInManager<Perfil> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
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
        public bool IsEmailConfirmed { get; set; }

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
            [Required(ErrorMessage = "O email é obrigatório")]
            [EmailAddress(ErrorMessage = "Por favor, insira um email válido")]
            [Display(Name = "Novo Email")]
            [StringLength(256, ErrorMessage = "O email deve ter no máximo 256 caracteres")]
            public string NewEmail { get; set; }
        }

        private async Task LoadAsync(Perfil user)
        {
            var email = await _userManager.GetEmailAsync(user);
            Email = email;

            Input = new InputModel
            {
                NewEmail = email,
            };

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostChangeEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var email = await _userManager.GetEmailAsync(user);
            if (Input.NewEmail != email)
            {
                //verificar se o email já está a ser utilizado  
                var existingUser = await _userManager.FindByEmailAsync(Input.NewEmail);
                if (existingUser != null)
                {
                    StatusMessage = "Error: Este email já está a ser utilizado.";
                    return RedirectToPage();
                }

                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmailChange",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, email = Input.NewEmail, code = code },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(
    Input.NewEmail,
    "Confirme o seu email",
    $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
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
        <h2>Alteração de Email</h2>
        
        <p>Recebemos um pedido para alterar o seu endereço de email. Para confirmar esta alteração, por favor clique no botão abaixo.</p>
        
        <div style='text-align: center;'>
            <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' class='button'>Confirmar Alteração</a>
        </div>
        
        <p>Se o botão acima não funcionar, pode copiar e colar o seguinte link no seu navegador:</p>
        <p style='font-size: 12px; word-break: break-all;'>
            {HtmlEncoder.Default.Encode(callbackUrl)}
        </p>
        
        <p><strong>Nota:</strong> Este link é válido por 24 horas.</p>
        
        <p>Se não solicitou esta alteração, pode ignorar este email com segurança.</p>
    </div>
    <div class='footer'>
        <p>Este é um email automático. Por favor, não responda.</p>
        <p>&copy; {DateTime.Now.Year} Bertrund. Todos os direitos reservados.</p>
    </div>
</body>
</html>");




                StatusMessage = "Email de verificação enviado. Por favor, verifique o seu email.";
                return RedirectToPage();
            }

            StatusMessage = "Error: O seu email não foi alterado.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);
            await _emailSender.SendEmailAsync(
            Input.NewEmail,
            "Confirme o seu email",
            $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='utf-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
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
                <h2>Alteração de Email</h2>
        
                <p>Recebemos um pedido para alterar o seu endereço de email. Para confirmar esta alteração, por favor clique no botão abaixo.</p>
        
                <div style='text-align: center;'>
                    <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' class='button'>Confirmar Alteração</a>
                </div>
        
                <p>Se o botão acima não funcionar, pode copiar e colar o seguinte link no seu navegador:</p>
                <p style='font-size: 12px; word-break: break-all;'>
                    {HtmlEncoder.Default.Encode(callbackUrl)}
                </p>
        
                <p><strong>Nota:</strong> Este link é válido por 24 horas.</p>
        
                <p>Se não solicitou esta alteração, pode ignorar este email com segurança.</p>
            </div>
            <div class='footer'>
                <p>Este é um email automático. Por favor, não responda.</p>
                <p>&copy; {DateTime.Now.Year} Bertrund. Todos os direitos reservados.</p>
            </div>
        </body>
        </html>");



            StatusMessage = "Email de verificação enviado. Por favor, verifique o seu email.";
            return RedirectToPage();
        }
    }
}
