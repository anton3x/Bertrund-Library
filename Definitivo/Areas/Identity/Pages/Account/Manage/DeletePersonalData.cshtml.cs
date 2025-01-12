// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Definitivo.Data;
using Definitivo.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Definitivo.Areas.Identity.Pages.Account.Manage
{
    public class DeletePersonalDataModel : PageModel
    {
        private readonly UserManager<Perfil> _userManager;
        private readonly SignInManager<Perfil> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _context;

        public DeletePersonalDataModel(
            UserManager<Perfil> userManager,
            SignInManager<Perfil> signInManager,
            ILogger<DeletePersonalDataModel> logger,
            IWebHostEnvironment webHostEnvironment,
            ApplicationDbContext applicationDbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
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
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public bool RequirePassword { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            if (RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(user, Input.Password))
                {
                    ModelState.AddModelError(string.Empty, "Password Incorreta");
                    return Page();
                }
            }

            // Remover foto do usuário
            if (!string.IsNullOrEmpty(user.FotoNome))
            {
                var fotoPath = Path.Combine(_webHostEnvironment.WebRootPath, "Images/Users", user.FotoNome);
                if (System.IO.File.Exists(fotoPath))
                {
                    System.IO.File.Delete(fotoPath);
                }
            }

            // Remover registros relacionados
            var emprestimos = _context.Emprestimo.Where(e => e.PerfilId == user.Id);
            _context.Emprestimo.RemoveRange(emprestimos);

            if (user.LivroFavoritos != null && user.LivroFavoritos.Any())
            {
                user.LivroFavoritos.Clear();
            }

            if (user.Reserva != null && user.Reserva.Any())
            {
                user.Reserva.Clear();
            }

            var livros = _context.Livro.Include(l => l.Reviews).ToList();

            foreach (Livro l in livros)
            {
                // Encontrar todas as reviews a serem removidas
                var reviewsParaRemover = l.Reviews.Where(r => r.UserId == user.Id).ToList();

                // Remover cada review encontrada
                foreach (var review in reviewsParaRemover)
                {
                    _context.Review.Remove(review);
                    l.Reviews.Remove(review);
                }
            }

            // Remove os chats do utilizador
            var messages = _context.Messages.Where(m => m.SenderId == user.Id || m.ReceiverId == user.Id);
            _context.Messages.RemoveRange(messages);


            // Salvar alterações no banco de dados
            await _context.SaveChangesAsync();


            var result = await _userManager.DeleteAsync(user);

            //remover das outras tabelas

            var userId = await _userManager.GetUserIdAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleting user.");
            }

            await _signInManager.SignOutAsync();

            _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

            return Redirect("~/");
        }
    }
}
