using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Definitivo.Data;
using Definitivo.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.AspNetCore.Html;
using Newtonsoft.Json;

namespace Definitivo.Controllers
{
    public class AutoresController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AutoresController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Autores
        [Authorize(Roles = "Bibliotecario")]
        public async Task<IActionResult> Index()
        {
            return View("GerirAutores");
        }

        public async Task<IActionResult> Autor(int id, int paginaAtual = 1, int itensPorPagina = 8)
        {
            var autor = await _context.Autor.FindAsync(id);
            if (autor == null)
            {
                return NotFound();
            }

            var livros = await _context.Livro
                .Where(l => l.AutorId == id)
                .Where(l => l.Estado != "Desativado")
                .Include(l => l.Categoria)
                .ToListAsync();

            var generos = livros.Distinct().Select(l => l.Categoria).Distinct().ToList();
            var obrasNotaveis = livros.Distinct().OrderByDescending(l => l.Cliques).Take(2).ToList();

            var totalLivros = livros.Count();
            var totalPaginas = (int)Math.Ceiling(totalLivros / (double)itensPorPagina);

            livros = livros
                .Skip((paginaAtual - 1) * itensPorPagina)
                .Take(itensPorPagina)
                .ToList();

            var viewModel = new AutorViewModel
            {
                Autor = autor,
                Generos = generos,
                Livros = livros,
                ObrasNotaveis = obrasNotaveis
            };

            // Configurar os dados de paginação
            ViewBag.PaginaAtual = paginaAtual;
            ViewBag.TotalPaginas = totalPaginas;

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ObrasAutores(int autorId, int paginaAtual = 1, int itensPorPagina = 8)
        {
            var autor = await _context.Autor.FindAsync(autorId);
            if (autor == null)
            {
                return NotFound();
            }

            var livros = await _context.Livro
                .Where(l => l.AutorId == autorId)
                .Where(l => l.Estado != "Desativado")
                .Include(l => l.Categoria)
                .ToListAsync();

            var generos = livros.Distinct().Select(l => l.Categoria).Distinct().ToList();
            var obrasNotaveis = livros.Distinct().OrderByDescending(l => l.Cliques).Take(2).ToList();
            var totalLivros = livros.Count();
            var totalPaginas = (int)Math.Ceiling(totalLivros / (double)itensPorPagina);

            livros = livros
                .Skip((paginaAtual - 1) * itensPorPagina)
                .Take(itensPorPagina)
                .ToList();

            var viewModel = new AutorViewModel
            {
                Autor = autor,
                Generos = generos,
                Livros = livros,
                ObrasNotaveis = obrasNotaveis
            };

            // Configurar os dados de paginação
            ViewBag.PaginaAtual = paginaAtual;
            ViewBag.TotalPaginas = totalPaginas;

            // Retornar a view parcial
            return PartialView("_AutorLivrosListing", viewModel);
        }

        // GET: GerirAutores
        [Authorize(Roles = "Bibliotecario")]
        public async Task<IActionResult> GerirAutores(string orderSelect = "", string searchBox = "", int paginaAtual = 1, int itensPorPagina = 6, bool isAjax = false)
        {
            ViewData["MessageAutores"] = TempData["MessageAutores"];

            //System.Threading.Thread.Sleep(500); // Simular atraso de 1 segundo
            // Query inicial da base de dados
            var autores = _context.Autor.AsQueryable();

            //Viewbag das tags enviadas para a view
            List<string> listAutores = autores.Select(c => c.Nome).Distinct().ToList();
            ViewBag.TagsAutores = new HtmlString(JsonConvert.SerializeObject(listAutores));

            // Aplicar filtro baseado na query
            if (!string.IsNullOrEmpty(searchBox))
            {
                searchBox = searchBox.ToLower();
                autores = autores.Where(a => a.Nome.ToLower().Contains(searchBox) ||
                                              a.Nacionalidade.ToLower().Contains(searchBox));
            }

            // Aplicar ordenação
            switch (orderSelect)
            {
                case "nome":
                    autores = autores.OrderBy(a => a.Nome);
                    break;
                case "nome_desc":
                    autores = autores.OrderByDescending(a => a.Nome);
                    break;
                case "data":
                    autores = autores.OrderBy(a => a.DataNascimento);
                    break;
                case "data_desc":
                    autores = autores.OrderByDescending(a => a.DataNascimento);
                    break;
                case "nacionalidade":
                    autores = autores.OrderBy(a => a.Nacionalidade);
                    break;
                case "nacionalidade_desc":
                    autores = autores.OrderByDescending(a => a.Nacionalidade);
                    break;
                default:
                    autores = autores.OrderBy(a => a.Nome); // Ordenação padrão
                    break;
            }

            // Contar total de autores após filtros
            int totalAutores = await autores.CountAsync();

            // Aplicar paginação
            var autoresPaginados = await autores
                .Skip((paginaAtual - 1) * itensPorPagina)
                .Take(itensPorPagina)
                .ToListAsync();

            // Configurar ViewBag para paginação e informações auxiliares
            ViewBag.QuerySearched = searchBox;
            ViewBag.orderSelect = orderSelect ?? "Nenhum";
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalAutores / itensPorPagina);
            ViewBag.PaginaAtual = paginaAtual;
            ViewBag.ItensPorPagina = itensPorPagina;


            // Retornar uma partial view para chamadas AJAX
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView($"GerirAutoresListing", autoresPaginados);
            }

            // Retornar a view com a lista de autores filtrados, ordenados e paginados
            return View(autoresPaginados);
        }



        // GET: Autores/Create
        [Authorize(Roles = "Bibliotecario")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Autores/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Bibliotecario")]
        public async Task<IActionResult> Create(
    [Bind("Nome,Biografia,FotoNome,DataNascimento,DataFalecimento,Nacionalidade")] Autor autor,
    IFormFile? FotoUpload)
        {
            if (!ModelState.IsValid)
            {
                return View(autor);
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
                    return View(autor);
                }

                // Verificar o tipo MIME (opcional para maior segurança)
                if (!FotoUpload.ContentType.StartsWith("image/"))
                {
                    ModelState.AddModelError("FotoNome", "O ficheiro carregado não é uma imagem válida.");
                    return View(autor);
                }

                const long maxFileSize = 10 * 1024 * 1024; // 2 MB
                if (FotoUpload.Length > maxFileSize)
                {

                    ModelState.AddModelError("FotoNome", "O ficheiro é demasiado grande. O tamanho máximo permitido é de 10 MB.");
                    return View(autor);
                }
            }


            var dataAtual = DateOnly.FromDateTime(DateTime.Now);

            // Validação das datas
            if (autor.DataNascimento > dataAtual)
            {
                ModelState.AddModelError("DataNascimento", "A data de nascimento não pode ser superior à data atual");
                return View(autor);
            }

            if (autor.DataFalecimento.HasValue)
            {
                if (autor.DataFalecimento > dataAtual)
                {
                    ModelState.AddModelError("DataFalecimento", "A data de falecimento não pode ser superior à data atual");
                    return View(autor);
                }

                if (autor.DataFalecimento < autor.DataNascimento)
                {
                    ModelState.AddModelError("DataFalecimento", "A data de falecimento não pode ser inferior à data de nascimento");
                    return View(autor);
                }
            }

            // Salvar autor e foto
            _context.Add(autor);

            if (FotoUpload?.Length > 0)
            {
                var uniqueFileName = $"{Guid.NewGuid()}_{FotoUpload.FileName}";
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images/Autores", uniqueFileName);

                using var fileStream = new FileStream(filePath, FileMode.Create);
                await FotoUpload.CopyToAsync(fileStream);
                autor.FotoNome = uniqueFileName;
            }

            await _context.SaveChangesAsync();
            TempData["MessageAutores"] = "Success:Autor criado com sucesso.";
            return RedirectToAction(nameof(GerirAutores));
        }


        // GET: Autores/Edit/5
        [Authorize(Roles = "Bibliotecario")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var autor = await _context.Autor.FindAsync(id);
            if (autor == null)
            {
                return NotFound();
            }
            return View(autor);
        }

        // POST: Autores/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Bibliotecario")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Biografia,FotoNome,DataNascimento, DataFalecimento, Nacionalidade")] Autor autor, IFormFile? FotoUpload)
        {
            if (id != autor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (FotoUpload != null && FotoUpload.Length > 0)
                    {
                        // Lista de extensões permitidas
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

                        // Verificar a extensão do ficheiro
                        var fileExtension = Path.GetExtension(FotoUpload.FileName).ToLower();
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("FotoNome", "Formato de ficheiro inválido. Apenas imagens (.jpg, .jpeg, .png, .gif, .webp) são permitidas.");
                            return View(autor);
                        }

                        // Verificar o tipo MIME (opcional para maior segurança)
                        if (!FotoUpload.ContentType.StartsWith("image/"))
                        {
                            ModelState.AddModelError("FotoNome", "O ficheiro carregado não é uma imagem válida.");
                            return View(autor);
                        }

                        const long maxFileSize = 10 * 1024 * 1024; // 2 MB
                        if (FotoUpload.Length > maxFileSize)
                        {

                            ModelState.AddModelError("FotoNome", "O ficheiro é demasiado grande. O tamanho máximo permitido é de 10 MB.");
                            return View(autor);
                        }
                    }

                    var dataAtual = DateOnly.FromDateTime(DateTime.Now);

                    // Validação das datas
                    if (autor.DataNascimento > dataAtual)
                    {
                        ModelState.AddModelError("DataNascimento", "A data de nascimento não pode ser superior à data atual");
                        return View(autor);
                    }

                    if (autor.DataFalecimento.HasValue)
                    {
                        if (autor.DataFalecimento > dataAtual)
                        {
                            ModelState.AddModelError("DataFalecimento", "A data de falecimento não pode ser superior à data atual");
                            return View(autor);
                        }

                        if (autor.DataFalecimento < autor.DataNascimento)
                        {
                            ModelState.AddModelError("DataFalecimento", "A data de falecimento não pode ser inferior à data de nascimento");
                            return View(autor);
                        }
                    }

                    if (FotoUpload != null && FotoUpload.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images/Autores");

                        // Remover a imagem antiga, se existir
                        if (!string.IsNullOrEmpty(autor.FotoNome))
                        {
                            var oldImagePath = Path.Combine(uploadsFolder, autor.FotoNome);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                try
                                {
                                    System.IO.File.Delete(oldImagePath);
                                }
                                catch (IOException ex)
                                {
                                    Console.WriteLine($"Erro ao eliminar imagem antiga no edit de autores: {ex.Message}");
                                }
                            }
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + FotoUpload.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await FotoUpload.CopyToAsync(fileStream);
                        }

                        autor.FotoNome = uniqueFileName;
                    }


                    _context.Update(autor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AutorExists(autor.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                TempData["MessageAutores"] = "Success:Autor alterado com sucesso.";
                return RedirectToAction(nameof(GerirAutores));

            }
            return View(autor);
        }

        // GET: Autores/Delete/5
        [Authorize(Roles = "Bibliotecario")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var autor = await _context.Autor
                .FirstOrDefaultAsync(m => m.Id == id);
            if (autor == null)
            {
                return NotFound();
            }

            // Buscar todos os livros do autor
            var livros = await _context.Livro
                .Where(l => l.AutorId == id)
                .Include(l => l.ReservadoPor)
                .Include(l => l.PerfisAFavoritar)
                .Include(l => l.Emprestimos)
                .ToListAsync();

            foreach (var livro in livros)
            {
                if (livro.NumeroExemplaresDisponiveis != livro.NumeroExemplaresTotal)
                {
                    TempData["Error"] = "Não é possível eliminar um autor que tenha livros emprestados.";
                    return RedirectToAction(nameof(GerirAutores));
                }
            }

            return View(autor);
        }

        // POST: Autores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Bibliotecario")]
        public async Task<IActionResult> DeleteConfirmed(int id, string orderSelectCopy = "", string searchBoxCopy = "")
        {
            var autor = await _context.Autor.FindAsync(id);
            if (autor != null)
            {
                // Buscar todos os livros do autor
                var livros = await _context.Livro
                    .Where(l => l.AutorId == id)
                    .Include(l => l.ReservadoPor)
                    .Include(l => l.PerfisAFavoritar)
                    .Include(l => l.Emprestimos)
                    .Include(l => l.Reviews)
                    .ToListAsync();

                foreach (var livro in livros)
                {
                    if(livro.NumeroExemplaresDisponiveis != livro.NumeroExemplaresTotal)
                    {
                        TempData["MessageAutores"] = "Error:Não é possível eliminar um autor que tenha livros emprestados.";
                        return RedirectToAction(nameof(GerirAutores));
                    }
                }

                foreach (var livro in livros)
                {
                    // Eliminar as reservas desses livros
                    if (livro.ReservadoPor != null)
                    {
                        livro.ReservadoPor.Clear();
                    }

                    // Eliminar os favoritos desses livros
                    if (livro.PerfisAFavoritar != null)
                    {
                        livro.PerfisAFavoritar.Clear();
                    }

                    // Eliminar os empréstimos desses livros
                    if (livro.Emprestimos != null)
                    {
                        _context.Emprestimo.RemoveRange(livro.Emprestimos);
                    }

                    foreach (var review in livro.Reviews)
                    {
                        _context.Review.Remove(review);
                    }

                    // Remove todas as relações de reservas
                    if (livro.Reviews != null)
                    {
                        livro.Reviews.Clear();
                    }

                    // Eliminar o livro
                    _context.Livro.Remove(livro);
                }

                // Eliminar o autor
                _context.Autor.Remove(autor);

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["MessageAutores"] = "Success:O Autor foi excluido com sucesso";

                    // Retornar uma partial view para chamadas AJAX
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return await GerirAutores(isAjax: true, searchBox: searchBoxCopy, orderSelect: orderSelectCopy);
                    }
                }
                catch (Exception ex)
                {
                    TempData["MessageAutores"] = "Error:Ocorreu um erro ao eliminar o autor e os seus livros.";
                }
            }
            else
            {
                TempData["MessageAutores"] = "Error:Autor não encontrado.";
            }

            return RedirectToAction(nameof(GerirAutores));
        }


        private bool AutorExists(int id)
        {
            return _context.Autor.Any(e => e.Id == id);
        }
    }
}
