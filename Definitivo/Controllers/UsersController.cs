using Definitivo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Definitivo.Data;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Newtonsoft.Json;
using Definitivo.Models.Services;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

public class UsersController : Controller
{
    private readonly UserManager<Perfil> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<Perfil> _signInManager;
    private readonly ILogger<UsersController> _logger;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ApplicationDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly ChatService _chatService;
    private readonly ProcessarReservasService _reservasService;

    public UsersController( ChatService chatService,
        UserManager<Perfil> userManager,
        RoleManager<IdentityRole> roleManager,
        SignInManager<Perfil> signInManager,
        ILogger<UsersController> logger,
        IWebHostEnvironment webHostEnvironment,
        ApplicationDbContext applicationDbContext,
        IEmailSender emailSender,
        ProcessarReservasService reservasService
        )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _logger = logger;
        _webHostEnvironment = webHostEnvironment;
        _context = applicationDbContext;
        _emailSender = emailSender;
        _chatService = chatService;
        _reservasService = reservasService;

    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GerirUtilizadores(string query = null, string sortOrder = null, int paginaAtual = 1, int itensPorPagina = 6, bool isAjax = false)
    {
        ViewData["MessageUsers"] = TempData["MessageUsers"];

        // Filtrar usuários
        var usersQuery = string.IsNullOrEmpty(query)
            ? _userManager.Users
            : _userManager.Users.Where(u => u.UserName.Contains(query) || u.Email.Contains(query));

        //Viewbag dos users enviadas para a view
        List<string> usersList = usersQuery.Select(u => u.NormalizedUserName.ToLower()).Distinct().ToList();
        usersList.AddRange(usersQuery.Select(u => u.NormalizedEmail.ToLower()).Distinct());
        ViewBag.TagsUsers = new HtmlString(JsonConvert.SerializeObject(usersList));


        // Ordenar usuários
        switch (sortOrder)
        {
            case "name_desc":
                usersQuery = usersQuery.OrderByDescending(l => l.UserName);
                break;
            case "name":
                usersQuery = usersQuery.OrderBy(l => l.UserName);
                break;
            case "cargo_desc":
                usersQuery = usersQuery.OrderByDescending(u =>
                    (from userRole in _context.UserRoles
                     join role in _context.Roles on userRole.RoleId equals role.Id
                     where userRole.UserId == u.Id
                     select role.Name).FirstOrDefault());
                break;
            case "cargo_asc":
                usersQuery = usersQuery.OrderBy(u =>
                    (from userRole in _context.UserRoles
                     join role in _context.Roles on userRole.RoleId equals role.Id
                     where userRole.UserId == u.Id
                     select role.Name).FirstOrDefault());
                break;
            case "estado_desc":
                usersQuery = usersQuery.OrderByDescending(l => l.EstadoAtivacao);
                break;
            case "estado":
                usersQuery = usersQuery.OrderBy(l => l.EstadoAtivacao);
                break;
            default:
                usersQuery = usersQuery.OrderBy(l => l.UserName);
                break;
        }

        // Contar total de usuários
        int totalUsuarios = await usersQuery.CountAsync();

        // Aplicar paginação
        var usersPaginados = await usersQuery
            .Skip((paginaAtual - 1) * itensPorPagina)
            .Take(itensPorPagina)
            .ToListAsync();

        var userViewModels = new List<UserRolesViewModel>();
        var usersComMuitosEmprestimosCancelados = new List<string>();

        foreach (var user in usersPaginados)
        {
            var roles = await _userManager.GetRolesAsync(user); // Obtém as roles do usuário

            // Verifica se o usuário tem mais de 5 empréstimos cancelados
            if (user.NumeroEmprestimosCanceladosPorEntregar > 5)
            {
                usersComMuitosEmprestimosCancelados.Add($"{user.UserName} tem {user.NumeroEmprestimosCanceladosPorEntregar} empréstimos cancelados");
            }

            var userViewModel = new UserRolesViewModel
            {
                User = user,
                Role = roles.Count > 0 ? roles[0] : "Sem Role", // Define uma role padrão se o usuário não tiver roles
                AdministradorQueOCriou = roles.Contains("Admin")
                    ? await _context.Users.Where(a => a.Id == user.IdAdministradorQueCriou).FirstOrDefaultAsync()
                    : null
            };

            userViewModels.Add(userViewModel);
        }

        // Passar dados para ViewBag
        ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalUsuarios / itensPorPagina);
        ViewBag.PaginaAtual = paginaAtual;
        ViewBag.ItensPorPagina = itensPorPagina;
        ViewBag.SortOrder = sortOrder;
        ViewBag.Query = query;
        ViewBag.UsersComMuitosEmprestimosCancelados = usersComMuitosEmprestimosCancelados;

        // Retornar uma partial view para chamadas AJAX
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || isAjax)
        {
            return PartialView($"UsersListing", userViewModels);
        }

        return View(userViewModels);
    }





    // GET: Users/Create
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }


    // GET: Users/Edit/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(string id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var perfil = await _userManager.FindByIdAsync(id);
        if (perfil == null)
        {
            return NotFound();
        }

        var userRoles = await _userManager.GetRolesAsync(perfil);
        ViewBag.UserRoles = userRoles;
        ViewBag.AvailableRoles = await GetAvailableRolesAsync();

        return View(perfil);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(string id, [Bind("Id,UserName,Email,PhoneNumber,Morada,FotoNome,EstadoAtivacao,EmailConfirmed,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnabled,LockoutEnd")] Perfil perfil, string role, IFormFile? FotoUpload)
    {
        if (id != perfil.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                if (FotoUpload != null && FotoUpload.Length > 0)
                {
                    // Lista de extensões permitidas
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

                    // Verificar a extensão do ficheiro
                    var fileExtension = Path.GetExtension(FotoUpload.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("FotoNome", "Formato de ficheiro inválido. Apenas imagens (.jpg, .jpeg, .png, .gif, .webp) são permitidas.");
                        ViewBag.UserRoles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(id));
                        ViewBag.AvailableRoles = await GetAvailableRolesAsync();
                        return View(perfil);
                    }

                    // Verificar o tipo MIME (opcional para maior segurança)
                    if (!FotoUpload.ContentType.StartsWith("image/"))
                    {
                        ModelState.AddModelError("FotoNome", "O ficheiro carregado não é uma imagem válida.");
                        ViewBag.UserRoles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(id));
                        ViewBag.AvailableRoles = await GetAvailableRolesAsync();
                        return View(perfil);
                    }

                    const long maxFileSize = 10 * 1024 * 1024; // 10 MB
                    if (FotoUpload.Length > maxFileSize)
                    {

                        ModelState.AddModelError("FotoNome", "O ficheiro é demasiado grande. O tamanho máximo permitido é de 10 MB.");
                        ViewBag.UserRoles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(id));
                        ViewBag.AvailableRoles = await GetAvailableRolesAsync();
                        return View(perfil);
                    }
                }


                // Verifica se o username já está em uso por outro usuário
                if (user.UserName != perfil.UserName)
                {
                    var existingUserName = await _userManager.FindByNameAsync(perfil.UserName);
                    if (existingUserName != null)
                    {
                        ModelState.AddModelError("UserName", "Este nome de utilizador já está em uso."); 
                        ViewBag.UserRoles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(id));
                        ViewBag.AvailableRoles = await GetAvailableRolesAsync();
                        return View(perfil);
                    }
                }

                // Verifica se o email já está em uso por outro usuário
                if (user.Email != perfil.Email)
                {
                    var existingEmail = await _userManager.FindByEmailAsync(perfil.Email);
                    if (existingEmail != null)
                    {
                        ModelState.AddModelError("Email", "Este email já está em uso.");
                        ViewBag.UserRoles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(id));
                        ViewBag.AvailableRoles = await GetAvailableRolesAsync();
                        return View(perfil);
                    }
                }
                if (perfil.PhoneNumber != user.PhoneNumber)
                {
                    // Verifica se já existe um user com o número de telemovel fornecido
                    var userWithThatNumber = await _userManager.Users
                        .FirstOrDefaultAsync(u => u.PhoneNumber == perfil.PhoneNumber);

                    if (userWithThatNumber != null)
                    {
                        ModelState.AddModelError("PhoneNumber", "Este número de telemóvel já está em uso.");
                        // Recarregar roles
                        ViewBag.UserRoles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(id));
                        ViewBag.AvailableRoles = await GetAvailableRolesAsync();
                        return View(perfil);
                    }
                }


                // Atualiza as propriedades do usuário
                user.UserName = perfil.UserName;
                user.Email = perfil.Email;
                user.PhoneNumber = perfil.PhoneNumber;
                user.Morada = perfil.Morada;
                user.FotoNome = perfil.FotoNome;
                user.EmailConfirmed = perfil.EmailConfirmed;
                user.PhoneNumberConfirmed = perfil.PhoneNumberConfirmed;
                user.TwoFactorEnabled = perfil.TwoFactorEnabled;
                user.LockoutEnabled = perfil.LockoutEnabled;
                user.LockoutEnd = perfil.LockoutEnd;
                user.EstadoAtivacao = perfil.EstadoAtivacao;

                // Processa o upload da foto
                if (FotoUpload != null && FotoUpload.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images/Users");

                    // Remove a foto antiga
                    if (!string.IsNullOrEmpty(user.FotoNome))
                    {
                        var oldImagePath = Path.Combine(uploadsFolder, user.FotoNome);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            try
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                            catch (IOException ex)
                            {
                                _logger.LogError($"Erro ao eliminar a imagem antiga: {ex.Message}");
                            }
                        }
                    }

                    // Salva a nova foto
                    var uniqueFileName = $"{Guid.NewGuid()}_{FotoUpload.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await FotoUpload.CopyToAsync(fileStream);
                    }

                    user.FotoNome = uniqueFileName;
                }

                // Atualiza o usuário
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Atualiza as roles
                    var userRoles = await _userManager.GetRolesAsync(user);
                    var roleToRemove = userRoles[0];
                    var roleToAdd = role;

                    if (roleToRemove != roleToAdd)
                    {
                        await _userManager.RemoveFromRoleAsync(user, roleToRemove);
                        await _userManager.AddToRoleAsync(user, roleToAdd);
                        await _userManager.UpdateSecurityStampAsync(user);
                        _logger.LogInformation($"User {user.UserName} roles changed. Security stamp updated.");
                    }
                    TempData["MessageUsers"] = "Success:O Utilizador '" + user.NormalizedUserName.ToLowerInvariant() + "' foi alterado com sucesso.";
                    return RedirectToAction(nameof(GerirUtilizadores));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await PerfilExists(perfil.Id))
                {
                    return NotFound();
                }
                throw;
            }

        }
        // Se chegamos aqui, algo deu errado, reexiba o formulário
        var currentRoles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(id));
        ViewBag.UserRoles = currentRoles;
        ViewBag.AvailableRoles = await GetAvailableRolesAsync();
        return View(perfil);
    }



    private async Task<bool> PerfilExists(string id)
    {
        return await _userManager.FindByIdAsync(id) != null;
    }

    private async Task<IEnumerable<SelectListItem>> GetAvailableRolesAsync()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        return roles.Select(r => new SelectListItem { Value = r.Name, Text = r.Name });
    }



    // POST: Users/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([Bind("UserName,Email,PhoneNumber,Morada,FotoNome")] Perfil model, IFormFile? FotoUpload)
    {
        if (ModelState.IsValid)
        {
            //obter o administrador da sessao atual
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            if (adminId == null )
            {
                return View(model);
            }

            if (FotoUpload != null && FotoUpload.Length > 0)
            {
                // Lista de extensões permitidas
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

                // Verificar a extensão do ficheiro
                var fileExtension = Path.GetExtension(FotoUpload.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("FotoNome", "Formato de ficheiro inválido. Apenas imagens (.jpg, .jpeg, .png, .gif, .webp) são permitidas.");
                    return View(model);
                }

                // Verificar o tipo MIME (opcional para maior segurança)
                if (!FotoUpload.ContentType.StartsWith("image/"))
                {
                    ModelState.AddModelError("FotoNome", "O ficheiro carregado não é uma imagem válida.");
                    return View(model);
                }

                const long maxFileSize = 10 * 1024 * 1024; // 2 MB
                if (FotoUpload.Length > maxFileSize)
                {

                    ModelState.AddModelError("FotoNome", "O ficheiro é demasiado grande. O tamanho máximo permitido é de 10 MB.");
                    return View(model);
                }
            }

            // Verificar se o email já existe
            var existingEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingEmail != null)
            {
                ModelState.AddModelError("Email", "Esse email já está em uso.");
                return View(model);
            }

            // Verificar se o email já existe
            var existingUser = await _userManager.FindByNameAsync(model.UserName);
            if (existingUser != null)
            {
                ModelState.AddModelError("UserName", "Esse username já está em uso.");
                return View(model);
            }
            if (model.PhoneNumber != null)
            {
                // Verifica se já existe um user com o número de telemovel fornecido
                var userWithThatNumber = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);

                if (userWithThatNumber != null)
                {
                    ModelState.AddModelError("PhoneNumber", "Este número de telemóvel já está em uso.");
                    return View(model);
                }
            }

            // Gerar senha aleatória
            string password = GenerateRandomPassword();

            var user = new Perfil
            {
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Morada = model.Morada,
                FotoNome = model.FotoNome,
                EstadoAtivacao = "Ativo",
                IdAdministradorQueCriou = adminId
            };

            if (FotoUpload != null && FotoUpload.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images/Users");
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + FotoUpload.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await FotoUpload.CopyToAsync(fileStream);
                }

                user.FotoNome = uniqueFileName;
            }

            var result = await _userManager.CreateAsync(user,password);

            if (result.Succeeded)
            {
                // Verifica se a role de Admin existe, se não, cria
                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                // Adiciona o usuário à role de Admin
                await _userManager.AddToRoleAsync(user, "Admin");


                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code },
                    protocol: Request.Scheme);

				// Email template
				string subject = "Confirmação de Conta e Credenciais de Acesso";
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
						            <h2>Bem-vindo(a) ao Sistema</h2>
						            <p>A sua conta foi criada com sucesso!</p>
						            <p><strong>Email:</strong> {user.Email}</p>
						            <p><strong>Password temporária:</strong> {password}</p>
						        
						            <p>Obrigado por se registar na nossa plataforma. Para começar a utilizar a sua conta, precisamos de confirmar o seu endereço de email.</p>
						        
						            <div style='text-align: center;'>
						                <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' class='button'>Confirmar Conta</a>
						            </div>
						        
						            <p>Se o botão acima não funcionar, pode copiar e colar o seguinte link no seu navegador:</p>
						            <p style='font-size: 12px; word-break: break-all;'>
						                {HtmlEncoder.Default.Encode(callbackUrl)}
						            </p>
						        
						            <p><strong>Nota:</strong> Este link é válido por 24 horas.</p>
						        
						            <p>Após confirmar o seu email, poderá fazer login e alterar a sua senha.</p>
						            <p>Se não solicitou este registo, pode ignorar este email.</p>
						        </div>
						        <div class='footer'>
						            <p>Este é um email automático. Por favor, não responda.</p>
						            <p>&copy; {DateTime.Now.Year} Bertrund. Todos os direitos reservados.</p>
						        </div>
						    </body>
                        </html>";

				// Send email
				await _emailSender.SendEmailAsync(user.Email, subject, emailTemplate);

                TempData["MessageUsers"] = "Success:O Utilizador '" + user.NormalizedUserName.ToLowerInvariant() + "' foi criado com sucesso. Um email foi enviado com as credenciais.";
                return RedirectToAction(nameof(GerirUtilizadores));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // Se chegamos aqui, algo deu errado, reexiba o formulário
        return View(model);
    }

    private string GenerateRandomPassword()
    {
        // Gera uma senha que atende aos requisitos do Identity
        var chars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@#$%^&*?_-";
        var random = new Random();
        var password = new string(
            Enumerable.Repeat(chars, 12)
                     .Select(s => s[random.Next(s.Length)])
                     .ToArray());

        // Garantir que tem pelo menos um de cada:
        password = "A" + // maiúscula
                  "a" + // minúscula
                  "9" + // número
                  "@" + // caractere especial
                  password; // resto aleatório

        return password;
    }

    [AcceptVerbs("GET", "POST")]
    public async Task<IActionResult> VerificarEmailUnico(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return Json(true);
        }
        else
        {
            return Json($"O email {email} já está em uso.");
        }
    }


    // GET: Users/Delete/5
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var userRoles = await _userManager.GetRolesAsync(user);
        var viewModel = new UserRolesViewModel
        {
            User = user,
            Role = userRoles.Count > 0 ? userRoles[0] : "Sem Role"  // Define uma role padrão se o usuário não tiver roles
        };

        return View(viewModel);
    }

    // POST: Users/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(string id,
        string searchTermUsersCopy = "",
        string orderSelectUsersCopy = "")
    {
        var user = await _context.Users
            .Include(u => u.LivroFavoritos)
            .Include(f => f.Reserva)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        try
        {
            var rule = await _userManager.GetRolesAsync(user);

            if (rule.Contains("Bibliotecario"))
            {
                TempData["MessageUsers"] = "Error: Não é possível excluir um bibliotecário!!";
                return RedirectToAction(nameof(GerirUtilizadores));

            }

            // Remover foto do utilizador
            if (!string.IsNullOrEmpty(user.FotoNome))
            {
                var fotoPath = Path.Combine(_webHostEnvironment.WebRootPath, "Images/Users", user.FotoNome);
                if (System.IO.File.Exists(fotoPath))
                {
                    System.IO.File.Delete(fotoPath);
                }
            }


            if(rule.Contains("Leitor"))
            {
                // Remover registros relacionados
                // Se ele for leitor
                var emprestimos = _context.Emprestimo
                    .Include(e => e.Livro)
                    .Where(e => e.PerfilId == id);

                var livroEmprestadosToUser = emprestimos.Select(e => e.LivroId).Distinct().ToList();

                foreach (Emprestimo e in emprestimos)
                {
                    if (e.Livro.NumeroExemplaresDisponiveis == 0 && e.Livro.Estado != "Desativado")
                    {
                        e.Livro.Estado = "Disponível";
                    }
                    e.Livro.NumeroExemplaresDisponiveis++;
                }

                _context.Emprestimo.RemoveRange(emprestimos); // Remove os empréstimos

                //verifica se os livros tem reservas e processa as reservas
                foreach (int livroId in livroEmprestadosToUser)
                {
                    await _reservasService.HandleReservasLivro(livroId);
                }

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
                
                // Salvar as alterações no banco de dados
                _context.SaveChanges();


                //remove o bloqueio do bibliotecario da tabela
                var bloqueio = _context.BloqueiaUser.Where(b => b.ID_PerfilUser == id);
                _context.BloqueiaUser.RemoveRange(bloqueio);
            }
            
            // Remove os chats do utilizador
            var messages = _context.Messages.Where(m => m.SenderId == id || m.ReceiverId == id);
            foreach(var message in messages)
            {
                if (message.FileName != null)
                {
                    //remover o ficheiro antes de excluir a mensagem
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Files/uploads/", message.FileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                _context.Messages.Remove(message);
            }

            // Salvar alterações no banco de dados
            await _context.SaveChangesAsync();

            //Elimina as chamadas que ele possa ter ativas
            _chatService.DeleteActiveCalls(user.Id); 

            // Remover o usuário
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                // Retornar uma partial view para chamadas AJAX
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    TempData["MessageUsers"] = "Success:O Utilizador '" + user.NormalizedUserName.ToLowerInvariant() + "' foi excluído com sucesso.";
                    return await GerirUtilizadores(isAjax: true, query: searchTermUsersCopy, sortOrder: orderSelectUsersCopy);
                }

                TempData["MessageUsers"] = "Success:O Utilizador '" + user.NormalizedUserName.ToLowerInvariant() + "' foi excluído com sucesso.";
                return RedirectToAction(nameof(GerirUtilizadores));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Ocorreu um erro ao tentar excluir o utilizador: " + ex.Message);
        }

        // Se chegamos aqui, algo deu errado
        var userRoles = await _userManager.GetRolesAsync(user);
        var viewModel = new UserRolesViewModel
        {
            User = user,
            Role = userRoles.Count > 0 ? userRoles[0] : "Sem Role"
        };

        TempData["MessageUsers"] = "Error:Erro ao excluir o utilizador " + user.NormalizedUserName.ToLowerInvariant() + "'.";
        // Retornar uma partial view para chamadas AJAX
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return await GerirUtilizadores(isAjax: true, query: searchTermUsersCopy, sortOrder: orderSelectUsersCopy);
        }

        return View(viewModel);
    }

    
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> PerfilPublico(string userId)
    {
        var user = await _context.Users
            .Include(u => u.LivroFavoritos)
            .Include(f => f.Reserva)
            .Include(f => f.Emprestimos)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound();
        }

        var userViewModel = new UserProfileViewModel
        {
            User = user,
            TotalReviews = _context.Review.Where(r => r.UserId == userId).Count(),
            BooksRead = _context.Emprestimo.Where(e => e.PerfilId == userId && e.DataDevolucao != null).Count()
        };

        userViewModel.BooksRead = _context.Emprestimo
            .Where(e => e.PerfilId == userId && e.DataDevolucao != null)
            .Count();

        userViewModel.FavoriteGenres = _context.Emprestimo
            .Where(r => r.PerfilId == userId && r.Livro.Categoria != null) // Filtra os empréstimos válidos com categoria não nula
            .GroupBy(r => r.Livro.Categoria.Nome)                               // Agrupa por categoria
            .OrderByDescending(g => g.Count())                             // Ordena pelas categorias com mais empréstimos// Seleciona as 5 categorias mais populares
            .Select(g => g.Key)                                            // Seleciona apenas o nome da categoria (chave do agrupamento)
            .ToList();                                                     // Converte para uma lista


        userViewModel.CurrentlyReading = _context.Emprestimo.
            Where(e => e.PerfilId == userId && e.DataDevolucao == null)
            .OrderByDescending(e => e.DataEmprestimo)
            .Select(e => e.Livro.Titulo)
            .ToList();

        userViewModel.Reviews = await _context.Review
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        userViewModel.Achievements = new List<string>();
        if (userViewModel.BooksRead >= 5)
        {
            userViewModel.Achievements.Add("Leitor Ávido");
        }
        if (userViewModel.BooksRead >= 10)
        {
            userViewModel.Achievements.Add("Leitor Voraz");
        }
        if (userViewModel.BooksRead >= 15)
        {
            userViewModel.Achievements.Add("Leitor Insaciável");
        }


        return View(userViewModel);
    }


    // GET: Users/Aprovar/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Aprovar(string id,
        string searchTermUsersCopy = "",
        string orderSelectUsersCopy = "")
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // Obtém o ID do administrador atual
        var adminId = _userManager.GetUserId(User);

        // Verifica se já existe uma aprovação para este bibliotecário
        var aprovacaoExistente = await _context.AprovaBibliotecario
            .FirstOrDefaultAsync(a => a.ID_PerfilBibliotecario == id);

        if (aprovacaoExistente == null)
        {
            // Cria nova instância de aprovação
            var aprovacao = new AprovaBibliotecario
            {
                ID_PerfilBibliotecario = id,
                ID_PerfilAdmin = adminId
            };

            // Adiciona a aprovação ao contexto
            await _context.AprovaBibliotecario.AddAsync(aprovacao);
        }

        // Atualiza o estado de ativação do usuário
        user.EstadoAtivacao = "Ativo";

        // Salva as alterações no usuário
        var result = await _userManager.UpdateAsync(user);

        // Atualiza o SecurityStamp do usuário
        await _userManager.UpdateSecurityStampAsync(user);

        if (result.Succeeded)
        {
            // Salva as alterações no contexto (incluindo a aprovação)
            await _context.SaveChangesAsync();
            TempData["MessageUsers"] = "Success:O Utilizador '" + user.NormalizedUserName.ToLowerInvariant() + "' foi aprovado com sucesso.";
            // Retornar uma partial view para chamadas AJAX
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return await GerirUtilizadores(isAjax: true, query: searchTermUsersCopy, sortOrder: orderSelectUsersCopy);
            }

            return RedirectToAction(nameof(GerirUtilizadores));

        }

        TempData["MessageUsers"] = "Error:O Utilizador '" + user.NormalizedUserName.ToLowerInvariant() + "' não foi aprovado, ocorreu um erro.";
        // Se houver erros na atualização do utilizador
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        // Retornar uma partial view para chamadas AJAX
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return await GerirUtilizadores(isAjax: true, query: searchTermUsersCopy, sortOrder: orderSelectUsersCopy);
        }

        return RedirectToAction(nameof(GerirUtilizadores));
    }


    // GET: Users/Bloquear/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Bloquear(string userId, string motivo,
        string searchTermUsersCopy = "",
        string orderSelectUsersCopy = "")
    {
        if (userId == null)
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        // Verifica se o usuário já está bloqueado
        var bloqueioExistente = await _context.BloqueiaUser
            .FirstOrDefaultAsync(b => b.ID_PerfilUser == userId);

        if (bloqueioExistente != null)
        {
            // Usuário já está bloqueado
            return RedirectToAction(nameof(GerirUtilizadores));
        }

        // Obtém o ID do administrador atual
        var adminId = _userManager.GetUserId(User);

        if(adminId == null)
        {
            return RedirectToAction(nameof(GerirUtilizadores));
        }

        // Cria novo registro de bloqueio
        var novoBloqueio = new BloqueiaUser
        {
            ID_PerfilUser = userId,
            ID_PerfilAdmin = adminId,
            Motivo = motivo.IsNullOrEmpty() ? "Sem motivo" : motivo,
            dataBloqueio = DateTime.Now
        };

        // Atualiza o estado do usuário e adiciona o bloqueio
        user.EstadoAtivacao = "Bloqueado";

        //Elimina as chamadas que ele possa ter ativas
        _chatService.DeleteActiveCalls(user.Id);

        await _context.BloqueiaUser.AddAsync(novoBloqueio);

        // Salva as alterações
        await _context.SaveChangesAsync();
        await _userManager.UpdateAsync(user);
        
        // Atualiza o SecurityStamp do usuário
        await _userManager.UpdateSecurityStampAsync(user);

        TempData["MessageUsers"] = "Success:O Utilizador '" + user.NormalizedUserName.ToLowerInvariant() + "' foi bloqueado com sucesso";
        // Retornar uma partial view para chamadas AJAX
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return await GerirUtilizadores(isAjax: true, query: searchTermUsersCopy, sortOrder: orderSelectUsersCopy);
        }

        return RedirectToAction(nameof(GerirUtilizadores));
    }

    // GET: Users/Desbloquear/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Desbloquear(
        string id,
        string searchTermUsersCopy = "",
        string orderSelectUsersCopy = "")
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // Verifica se o usuário está bloqueado
        var bloqueioExistente = await _context.BloqueiaUser
            .FirstOrDefaultAsync(b => b.ID_PerfilUser == id);

        if (bloqueioExistente == null)
        {
            // Usuário não está bloqueado

            TempData["MessageUsers"] = "Error:O Utilizador '" + user.NormalizedUserName.ToLowerInvariant() + "' não pode ser desbloqueado pois não está bloqueado.";
            return RedirectToAction(nameof(GerirUtilizadores));
        }

        // Remove o bloqueio
        _context.BloqueiaUser.Remove(bloqueioExistente);
        user.EstadoAtivacao = "Ativo";

        // Salva as alterações
        await _context.SaveChangesAsync();
        await _userManager.UpdateAsync(user);
        // Atualiza o SecurityStamp do usuário
        await _userManager.UpdateSecurityStampAsync(user);

        TempData["MessageUsers"] = "Success:O Utilizador '" + user.NormalizedUserName.ToLowerInvariant() + "' foi desbloqueado com sucesso";
        // Retornar uma partial view para chamadas AJAX
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return await GerirUtilizadores(isAjax: true, query: searchTermUsersCopy, sortOrder: orderSelectUsersCopy);
        }


        return RedirectToAction(nameof(GerirUtilizadores));
    }





}
