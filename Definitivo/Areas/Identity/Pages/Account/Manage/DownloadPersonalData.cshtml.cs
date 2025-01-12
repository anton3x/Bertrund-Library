// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Definitivo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Definitivo.Areas.Identity.Pages.Account.Manage
{
    public class DownloadPersonalDataModel : PageModel
    {
        private readonly UserManager<Perfil> _userManager;
        private readonly ILogger<DownloadPersonalDataModel> _logger;

        public DownloadPersonalDataModel(
            UserManager<Perfil> userManager,
            ILogger<DownloadPersonalDataModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.Users
                .Include(u => u.Emprestimos)          
                .Include(u => u.LivroFavoritos)       
                .Include(u => u.Bloqueio)
                .Include(u => u.Reserva)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            _logger.LogInformation("User with ID '{UserId}' asked for their personal data.", _userManager.GetUserId(User));

            // Only include personal data for download
            var personalData = new Dictionary<string, string>();
            var personalDataProps = typeof(Perfil).GetProperties().Where(
                            prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
            foreach (var p in personalDataProps)
            {
                personalData.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
            }

            // Adicionando o número de empréstimos
            var numEmprestimos = user.Emprestimos?.Count() ?? 0; // Conta os empréstimos, se houver
            personalData.Add("Numero de Emprestimos: ", numEmprestimos.ToString());
            foreach(Emprestimo em in user.Emprestimos)
            {
                // Verifica se DataEmprestimo é nula antes de chamar ToString
                string dataEmprestimo = em.DataEmprestimo.ToString() ?? "null";

                // Verifica se DataPrevista é nula antes de chamar ToString e ajusta a data se não for nula
                string dataPrevista = em.DataPrevista.HasValue
                    ? em.DataPrevista.Value.AddDays(-15).ToString()
                    : "null";

                // Verifica se DataDevolucao é nula antes de chamar ToString
                string dataDevolucao = em.DataDevolucao?.ToString() ?? "null";

                personalData.Add($"Emprestimo {em.Id}",
                    $"Data de Emprestimo: {dataEmprestimo}, " +
                    $"Data de Entrega: {dataPrevista}, " +
                    $"Data de Devolucao: {dataDevolucao}");
            }

            // Adicionando o número de livros favoritos
            var numLivrosFavoritos = user.LivroFavoritos?.Count() ?? 0; // Conta os livros favoritos, se houver
            personalData.Add("Numero de Livros Favoritos: ", numLivrosFavoritos.ToString());
            foreach (Livro l in user.LivroFavoritos)
            {
                personalData.Add($"Livro Favorito {l.ID}", $"Titulo: {l.Titulo}, Autor: {l.Autor}, ISBN: {l.ISBN}");
            }

            var numReservas = user.Reserva?.Count() ?? 0; // Conta as reservas, se houver
            personalData.Add("Numero de Reservas: ", numReservas.ToString());
            foreach(Livro l in user.Reserva)
            {
                personalData.Add($"Reserva {l.ID}", $"Titulo: {l.Titulo}, Autor: {l.Autor}, ISBN: {l.ISBN}");
            }

            // Verificando se o usuário foi bloqueado
            var isBlocked = user.Bloqueio != null; // Se a propriedade Bloqueio não for null, significa que foi bloqueado
            personalData.Add("Foi Bloqueado: ", isBlocked ? "Sim" : "Nao");

            var logins = await _userManager.GetLoginsAsync(user);
            foreach (var l in logins)
            {
                personalData.Add($"{l.LoginProvider} external login provider key", l.ProviderKey);
            }

            personalData.Add($"Authenticator Key", await _userManager.GetAuthenticatorKeyAsync(user));

            Response.Headers.TryAdd("Content-Disposition", "attachment; filename=PersonalData.json");
            return new FileContentResult(JsonSerializer.SerializeToUtf8Bytes(personalData), "application/json");
        }
    }
}
