using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Definitivo.Data;
using Definitivo.Models;
using Definitivo.Models.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Newtonsoft.Json;

namespace Definitivo.Controllers
{
    [Authorize(Roles = "Bibliotecario")]
    public class EmprestimosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Perfil> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ProcessarReservasService _reservasService;

        public EmprestimosController(ApplicationDbContext context,UserManager<Perfil> userManager, RoleManager<IdentityRole> roleManager
        , ProcessarReservasService reservasService 
        )
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _reservasService = reservasService;

        }

        // GET: Emprestimos
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Emprestimo.Include(e => e.BibliotecarioEntregou).Include(e => e.BibliotecarioRecebeu).Include(e => e.Livro).Include(e => e.Perfil);
            return View(await applicationDbContext.ToListAsync());
        }

        [Authorize(Roles = "Bibliotecario")]
        public async Task<JsonResult>  ObterValoresGerirEmprestimo()
        {
            var emprestimos = _context.Emprestimo
                .Include(e => e.BibliotecarioEntregou)
                .Include(e => e.BibliotecarioRecebeu)
                .Include(e => e.Livro)
                    .ThenInclude(e => e.Autor)
                .Include(e => e.Perfil)
                .AsQueryable();

            var valores = new List<int>
            {
                await emprestimos.CountAsync(),
                await emprestimos.CountAsync(e => e.DataDevolucao == null && e.DataPrevista != null),
                await emprestimos.CountAsync(e => e.DataPrevista == null),
                await emprestimos.CountAsync(e => e.DataDevolucao == null && e.DataPrevista < DateTime.Now)
            };

            return Json(valores);

        }

        // GET: Emprestimos
        [Authorize(Roles = "Bibliotecario")]
        public async Task<IActionResult> GerirEmprestimos(
            string filterStatus = null,
            string searchTerm = null,
            string orderSelect = "",
            int paginaAtual = 1,
            int itensPorPagina = 6, //aumentar para 6
            bool isAjax = false)
        {
            ViewData["MessageEmprestimos"] = TempData["MessageEmprestimos"];
            //System.Threading.Thread.Sleep(400); // Simular atraso de 1 segundo

            var emprestimos = _context.Emprestimo
                .Include(e => e.BibliotecarioEntregou)
                .Include(e => e.BibliotecarioRecebeu)
                .Include(e => e.Livro)
                    .ThenInclude(e => e.Autor)
                .Include(e => e.Perfil)
                .AsQueryable();

            //ViewBag de emprestimos enviadas para a view
            List<string> listEmprestimos = emprestimos.Select(p => p.Perfil.UserName).Distinct().ToList();
            listEmprestimos.AddRange(emprestimos.Select(p => p.Livro.Titulo).Distinct());
            ViewBag.TagsEmprestimos = new HtmlString(JsonConvert.SerializeObject(listEmprestimos));

            // Contar total de empréstimos após filtros
            int totalEmprestimos = await emprestimos.CountAsync();

            // Dados auxiliares para contagem de empréstimos
            ViewBag.TotalEmprestimos = totalEmprestimos;
            ViewBag.TotalEmprestimosAtivos = await emprestimos.CountAsync(e => e.DataDevolucao == null && e.DataPrevista != null);
            ViewBag.TotalEmprestimosPendentes = await emprestimos.CountAsync(e => e.DataPrevista == null);
            ViewBag.TotalEmprestimosAtrasados = await emprestimos.CountAsync(e => e.DataDevolucao == null && e.DataPrevista < DateTime.Now);

            // Aplicar filtro de pesquisa, se fornecido
            if (!string.IsNullOrEmpty(searchTerm))
            {
                string searchTermCopy = searchTerm;
                searchTerm = searchTerm.Trim().ToUpper();
                emprestimos = emprestimos.Where(e =>
                    (e.Perfil != null && e.Perfil.NormalizedUserName != null && e.Perfil.NormalizedUserName.Contains(searchTerm)) ||
                    (e.Livro != null && e.Livro.Titulo != null && e.Livro.Titulo.ToUpper().Trim().Contains(searchTerm))
                );
                ViewBag.SearchTerm = searchTermCopy;
            }
            else
            {
                ViewBag.SearchTerm = "";
            }

            // Aplicar filtro de status, se fornecido
            if (!string.IsNullOrEmpty(filterStatus))
            {
                ViewBag.StatusFilter = filterStatus;
                switch (filterStatus)
                {
                    case "Ativo":
                        emprestimos = emprestimos.Where(e => e.DataDevolucao == null && e.DataPrevista != null);
                        break;
                    case "Atrasado":
                        emprestimos = emprestimos.Where(e => e.DataDevolucao == null && e.DataPrevista < DateTime.Now);
                        break;
                    case "Devolvido":
                        emprestimos = emprestimos.Where(e => e.DataDevolucao != null);
                        break;
                    case "Pendente":
                        emprestimos = emprestimos.Where(e => e.DataPrevista == null);
                        break;
                }
            }
            else
            {
                ViewBag.StatusFilter = "Todos";
            }

            // Aplicar ordenação
            switch (orderSelect)
            {
                case "autor":
                    emprestimos = emprestimos.OrderBy(e => e.Livro.Autor.Nome);
                    break;
                case "autor_desc":
                    emprestimos = emprestimos.OrderByDescending(e => e.Livro.Autor.Nome);
                    break;
                case "cliente":
                    emprestimos = emprestimos.OrderBy(e => e.Perfil.NormalizedUserName);
                    break;
                case "cliente_desc":
                    emprestimos = emprestimos.OrderByDescending(e => e.Perfil.NormalizedUserName);
                    break;
                case "titulo":
                    emprestimos = emprestimos.OrderBy(e => e.Livro.Titulo);
                    break;
                case "titulo_desc":
                    emprestimos = emprestimos.OrderByDescending(e => e.Livro.Titulo);
                    break;
                case "dataPrevista":
                    emprestimos = emprestimos.OrderBy(e => e.DataPrevista);
                    break;
                case "dataPrevista_desc":
                    emprestimos = emprestimos.OrderByDescending(e => e.DataPrevista);
                    break;
                default:
                    emprestimos = emprestimos.OrderBy(e => e.Livro.Titulo); // Ordenação padrão
                    break;
            }

            ViewBag.TotalPaginas = (int)Math.Ceiling((double)emprestimos.Count() / itensPorPagina);
            
            // Aplicar paginação
            var emprestimosPaginados = await emprestimos
                .Skip((paginaAtual - 1) * itensPorPagina)
                .Take(itensPorPagina)
                .ToListAsync();

            // Configurar ViewBag para paginação e informações auxiliares
            //ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalEmprestimos / itensPorPagina);
            ViewBag.PaginaAtual = paginaAtual;
            ViewBag.ItensPorPagina = itensPorPagina;
            ViewBag.OrderSelect = orderSelect ?? "Nenhum";


            // Retornar uma partial view para chamadas AJAX
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || isAjax)
            {
                return PartialView($"GerirEmprestimosListing", emprestimosPaginados);
            }

            // Retornar a view com os empréstimos filtrados, ordenados e paginados
            return View(emprestimosPaginados);
        }




        // GET: Emprestimos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var emprestimo = await _context.Emprestimo
                .Include(e => e.BibliotecarioEntregou)
                .Include(e => e.BibliotecarioRecebeu)
                .Include(e => e.Livro)
                .Include(e => e.Perfil)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (emprestimo == null)
            {
                return NotFound();
            }

            return View(emprestimo);
        }

        // GET: Emprestimos/Create
        public async Task<IActionResult> Create()
        {
            // Get all users with the "Bibliotecario" role
            var bibliotecarioRole = await _roleManager.FindByNameAsync("Bibliotecario");
            var leitorRole = await _roleManager.FindByNameAsync("Leitor");

            var bibliotecariosUsers = await _userManager.GetUsersInRoleAsync(bibliotecarioRole.Name);
            var leitoresUsers = await _userManager.GetUsersInRoleAsync(leitorRole.Name);

            var bibliotecarios = bibliotecariosUsers.Select(u => new { u.Id, u.UserName }).ToList();
            var leitores = leitoresUsers.Select(u => new { u.Id, u.UserName }).ToList();

            ViewData["Id_bibliotecario_entregou"] = new SelectList(bibliotecarios, "Id", "UserName");
            ViewData["Id_bibliotecario_recebeu"] = new SelectList(bibliotecarios, "Id", "UserName");
            ViewData["LivroId"] = new SelectList(_context.Livro, "ID", "Titulo");
            ViewData["PerfilId"] = new SelectList(leitores, "Id", "UserName");
            return View();
        }

        // POST: Emprestimos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DataEmprestimo,DataDevolucao,PerfilId,LivroId,Id_bibliotecario_entregou,Id_bibliotecario_recebeu")] Emprestimo emprestimo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(emprestimo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(GerirEmprestimos));
            }

            // Get all users with the "Bibliotecario" role
            var bibliotecarioRole = await _roleManager.FindByNameAsync("Bibliotecario");
            var leitorRole = await _roleManager.FindByNameAsync("Leitor");

            var bibliotecariosUsers = await _userManager.GetUsersInRoleAsync(bibliotecarioRole.Name);
            var leitoresUsers = await _userManager.GetUsersInRoleAsync(leitorRole.Name);

            var bibliotecarios = bibliotecariosUsers.Select(u => new { u.Id, u.UserName }).ToList();
            var leitores = leitoresUsers.Select(u => new { u.Id, u.UserName }).ToList();

            ViewData["Id_bibliotecario_entregou"] = new SelectList(bibliotecarios, "Id", "UserName", emprestimo.Id_bibliotecario_entregou);
            ViewData["Id_bibliotecario_recebeu"] = new SelectList(bibliotecarios, "Id", "UserName", emprestimo.Id_bibliotecario_recebeu);
            ViewData["LivroId"] = new SelectList(_context.Livro, "ID", "Titulo", emprestimo.LivroId);
            ViewData["PerfilId"] = new SelectList(leitores, "Id", "UserName", emprestimo.PerfilId);

            TempData["MessageEmprestimos"] = "Success:Emprestimo criado com sucesso!";
            return View(emprestimo);
        }

        // GET: Emprestimos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var emprestimo = await _context.Emprestimo.FindAsync(id);
            if (emprestimo == null)
            {
                return NotFound();
            }

            // Get all users with the "Bibliotecario" role
            var bibliotecarioRole = await _roleManager.FindByNameAsync("Bibliotecario");
            var leitorRole = await _roleManager.FindByNameAsync("Leitor");

            var bibliotecariosUsers = await _userManager.GetUsersInRoleAsync(bibliotecarioRole.Name);
            var leitoresUsers = await _userManager.GetUsersInRoleAsync(leitorRole.Name);

            var bibliotecarios = bibliotecariosUsers.Select(u => new { u.Id, u.UserName }).ToList();
            var leitores = leitoresUsers.Select(u => new { u.Id, u.UserName }).ToList();

            ViewData["Id_bibliotecario_entregou"] = new SelectList(bibliotecarios, "Id", "UserName", emprestimo.Id_bibliotecario_entregou);
            ViewData["Id_bibliotecario_recebeu"] = new SelectList(bibliotecarios, "Id", "UserName", emprestimo.Id_bibliotecario_recebeu);
            ViewData["LivroId"] = new SelectList(_context.Livro, "ID", "Titulo", emprestimo.LivroId);
            ViewData["PerfilId"] = new SelectList(leitores, "Id", "UserName", emprestimo.PerfilId);
            return View(emprestimo);
        }

        // POST: Emprestimos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DataEmprestimo,DataPrevista,DataDevolucao,PerfilId,LivroId,Id_bibliotecario_entregou,Id_bibliotecario_recebeu")] Emprestimo emprestimo)
        {
            if (id != emprestimo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(emprestimo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmprestimoExists(emprestimo.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["MessageEmprestimos"] = "Success:Emprestimo alterado com sucesso!";
                return RedirectToAction(nameof(GerirEmprestimos));
            }

            // Get all users with the "Bibliotecario" role
            var bibliotecarioRole = await _roleManager.FindByNameAsync("Bibliotecario");
            var leitorRole = await _roleManager.FindByNameAsync("Leitor");

            var bibliotecariosUsers = await _userManager.GetUsersInRoleAsync(bibliotecarioRole.Name);
            var leitoresUsers = await _userManager.GetUsersInRoleAsync(leitorRole.Name);

            var bibliotecarios = bibliotecariosUsers.Select(u => new { u.Id, u.UserName }).ToList();
            var leitores = leitoresUsers.Select(u => new { u.Id, u.UserName }).ToList();

            ViewData["Id_bibliotecario_entregou"] = new SelectList(bibliotecarios, "Id", "UserName", emprestimo.Id_bibliotecario_entregou);
            ViewData["Id_bibliotecario_recebeu"] = new SelectList(bibliotecarios, "Id", "UserName", emprestimo.Id_bibliotecario_recebeu);
            ViewData["LivroId"] = new SelectList(_context.Livro, "ID", "Titulo", emprestimo.LivroId);
            ViewData["PerfilId"] = new SelectList(leitores, "Id", "UserName", emprestimo.PerfilId);

            return View(emprestimo);
        }

        // GET: Emprestimos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var emprestimo = await _context.Emprestimo
                .Include(e => e.BibliotecarioEntregou)
                .Include(e => e.BibliotecarioRecebeu)
                .Include(e => e.Livro)
                .Include(e => e.Perfil)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (emprestimo == null)
            {
                return NotFound();
            }

            return View(emprestimo);
        }

        // POST: Emprestimos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id,
            string filterStatusEmprestimosCopy = null,
            string searchTermEmprestimosCopy = null,
            string orderSelectEmprestimosCopy = ""
            )
        {
            var emprestimo = await _context.Emprestimo
                .Include(e => e.Livro) // Assuming Livro is the navigation property
                .FirstOrDefaultAsync(e => e.Id == id);

            if (emprestimo.DataPrevista != null && emprestimo.DataDevolucao == null) //Esta no estado ativo
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    TempData["MessageEmprestimos"] = "Error:Emprestimo não foi excluido, pois encontra-se ativo!";
                    // Retornar partial view para AJAX
                    return RedirectToAction("GerirEmprestimos", "Emprestimos", new
                    {
                        isAjax = true,
                        filterStatus = filterStatusEmprestimosCopy,
                        searchTerm = searchTermEmprestimosCopy,
                        orderSelect = orderSelectEmprestimosCopy,
                    });
                }

                TempData["MessageEmprestimos"] = "Error:Emprestimo não foi excluido, pois encontra-se ativo!";
                return RedirectToAction(nameof(GerirEmprestimos));
            }

            if (emprestimo != null)
            {
                if(emprestimo.DataDevolucao == null) //se nao tiver devolvido tem que fazer isto
                {
                    if (emprestimo.Livro.NumeroExemplaresDisponiveis == 0)
                    {
                        emprestimo.Livro.Estado = "Disponível";
                    }
                    emprestimo.Livro.NumeroExemplaresDisponiveis++; //incrementa o numero de exemplares disponiveis
                }

                //vai verificar se tem reservas e se as tiver ira tratrar delas para o livro do emprestimo
                await _reservasService.HandleReservasLivro(emprestimo.LivroId);

                _context.Emprestimo.Remove(emprestimo);
            }

            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                TempData["MessageEmprestimos"] = "Success:Emprestimo excluido com sucesso!";
                // Retornar partial view para AJAX
                return RedirectToAction("GerirEmprestimos", "Emprestimos", new { 
                    isAjax = true,
                    filterStatus = filterStatusEmprestimosCopy,
                    searchTerm = searchTermEmprestimosCopy,
                    orderSelect = orderSelectEmprestimosCopy,
                });
            }

            TempData["MessageEmprestimos"] = "Success:Emprestimo excluido com sucesso!";
            return RedirectToAction(nameof(GerirEmprestimos));
        }

        private bool EmprestimoExists(int id)
        {
            return _context.Emprestimo.Any(e => e.Id == id);
        }
    }
}
