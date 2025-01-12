using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Definitivo.Data;
using Definitivo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Newtonsoft.Json;

namespace Definitivo.Controllers
{
    [Authorize(Roles = "Bibliotecario")]
    public class CategoriasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Categorias
        public async Task<IActionResult> GerirCategorias(string orderSelect = "", string searchBox = "", int paginaAtual = 1, int itensPorPagina = 9, bool isAjax = false)
        {
            ViewData["MessageCategorias"] = TempData["MessageCategorias"];
            //System.Threading.Thread.Sleep(500);
            // Query inicial para categorias
            var categorias = _context.Categoria.AsQueryable();

            //ViewBag de categorias enviadas para a view
            List<string> listCategories = categorias.Select(c => c.Nome).Distinct().ToList();
            ViewBag.TagsCategorias = new HtmlString(JsonConvert.SerializeObject(listCategories));

            // Aplicar filtro de busca
            if (!string.IsNullOrEmpty(searchBox))
            {
                searchBox = searchBox.ToLower();
                categorias = categorias.Where(c => c.Nome.ToLower().Contains(searchBox) ||
                                                   (c.Descricao != null && c.Descricao.ToLower().Contains(searchBox)));
            }

            // Aplicar ordenação
            switch (orderSelect)
            {
                case "nome":
                    categorias = categorias.OrderBy(c => c.Nome);
                    break;
                case "nome_desc":
                    categorias = categorias.OrderByDescending(c => c.Nome);
                    break;
                case "descricao":
                    categorias = categorias.OrderBy(c => c.Descricao);
                    break;
                case "descricao_desc":
                    categorias = categorias.OrderByDescending(c => c.Descricao);
                    break;
                case "estado":
                    categorias = categorias.OrderBy(c => c.Estado);
                    break;
                case "estado_desc":
                    categorias = categorias.OrderByDescending(c => c.Estado);
                    break;
                default:
                    categorias = categorias.OrderBy(c => c.Nome); // Ordenação padrão
                    break;
            }

            // Contar total de categorias após filtros
            int totalCategorias = await categorias.CountAsync();

            // Aplicar paginação
            var categoriasPaginadas = await categorias
                .Skip((paginaAtual - 1) * itensPorPagina)
                .Take(itensPorPagina)
                .ToListAsync();


            // Configurar ViewBag para paginação e informações auxiliares
            ViewBag.QuerySearched = searchBox;
            ViewBag.OrderSelect = orderSelect ?? "Nenhum";
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalCategorias / itensPorPagina);
            ViewBag.PaginaAtual = paginaAtual;
            ViewBag.ItensPorPagina = itensPorPagina;

            // Retornar uma partial view para chamadas AJAX
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || isAjax)
            {
                return PartialView($"GerirCategoriasListing", categoriasPaginadas); // Partial correspondente à seção ativa
            }

            // Retornar a view com a lista de categorias filtradas, ordenadas e paginadas
            return View(categoriasPaginadas);
        }


        // GET: Categorias/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.Categoria
                .FirstOrDefaultAsync(m => m.Id == id);
            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }

        // GET: Categorias/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categorias/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,Descricao,Estado")] Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                // Verificar se já existe uma categoria com o mesmo nome
                var categoriaExistente = await _context.Categoria
                    .AnyAsync(c => c.Nome.ToLower() == categoria.Nome.ToLower());

                if (categoriaExistente)
                {
                    ModelState.AddModelError("Nome", "Já existe uma categoria com este nome.");
                    return View(categoria);
                }

                _context.Add(categoria);
                await _context.SaveChangesAsync();
                TempData["MessageCategorias"] = "Success:Categoria criada com sucesso";
                return RedirectToAction(nameof(GerirCategorias));
            }

            return View(categoria);
        }


        // GET: Categorias/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.Categoria.FindAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }
            return View(categoria);
        }

        // POST: Categorias/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Descricao,Estado")] Categoria categoria)
        {
            if (id != categoria.Id)
            {
                return NotFound();
            }

            // Buscar a categoria diretamente da base de dados
            var categoriaExistente = await _context.Categoria.FindAsync(id);

            if (categoriaExistente == null)
            {
                return NotFound();
            }

            // Verificar se o nome foi alterado
            if (!categoriaExistente.Nome.Equals(categoria.Nome, StringComparison.OrdinalIgnoreCase))
            {
                // Verificar se o novo nome já existe em outra categoria
                var nomeDuplicado = await _context.Categoria
                    .AnyAsync(c => c.Nome.ToLower() == categoria.Nome.ToLower() && c.Id != categoria.Id);

                if (nomeDuplicado)
                {
                    ModelState.AddModelError("Nome", "Já existe uma categoria com este nome.");
                    return View(categoria);
                }
            }

            // Atualizar a categoria com os novos dados
            categoriaExistente.Nome = categoria.Nome;
            categoriaExistente.Descricao = categoria.Descricao;
            categoriaExistente.Estado = categoria.Estado;

            try
            {
                _context.Update(categoriaExistente);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoriaExists(categoria.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            TempData["MessageCategorias"] = "Success:Categoria alterada com sucesso";
            return RedirectToAction(nameof(GerirCategorias));
        }



        // GET: Categorias/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.Categoria
                .FirstOrDefaultAsync(m => m.Id == id);
            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }

        // POST: Categorias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string orderSelectCopy = "", string searchBoxCopy = "")
        {
            var categoria = await _context.Categoria.FindAsync(id);
            if (categoria != null)
            {
                //livros com a categoria a ser eliminada
                var livros = await _context.Livro
                    .Where(l => l.CategoriaId == id)
                    .Include("Emprestimos")
                    .Include("PerfisAFavoritar")
                    .Include("ReservadoPor")
                    .Include(l => l.Reviews)
                    .ToListAsync();

                //verificar se os livros todos tem a quantidade disponivel igual a quantidade total
                foreach (var livro in livros)
                {
                    if (livro.NumeroExemplaresDisponiveis != livro.NumeroExemplaresTotal)
                    {
                        TempData["MessageCategorias"] = "Error:Não é possível eliminar a categoria porque existem livros com exemplares emprestados.";
                        return RedirectToAction(nameof(GerirCategorias));
                    }
                }

                foreach (var livro in livros)
                {
                    // Remove todos os empréstimos relacionados
                    if (livro.Emprestimos != null)
                    {
                        _context.Emprestimo.RemoveRange(livro.Emprestimos);
                    }

                    // Remove todas as relações de favoritos
                    if (livro.PerfisAFavoritar != null)
                    {
                        livro.PerfisAFavoritar.Clear();
                    }

                    // Remove todas as relações de reservas
                    if (livro.ReservadoPor != null)
                    {
                        livro.ReservadoPor.Clear();
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

                    // Remove o livro
                    _context.Livro.Remove(livro);
                }

                _context.Categoria.Remove(categoria);
            }

            await _context.SaveChangesAsync();

            // Retornar uma partial view para chamadas AJAX
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                TempData["MessageCategorias"] = "Success:Categoria eliminada com sucesso.";
                return await GerirCategorias(isAjax: true, searchBox: searchBoxCopy, orderSelect: orderSelectCopy);
            }

            return RedirectToAction(nameof(GerirCategorias));
        }


        //POST: Categorias/DesativarCategoria/5
        [HttpPost, ActionName("DesativarCategoria")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesativarCategoria(int id, string orderSelectCopy = "", string searchBoxCopy = "")
        {
            var categoria = await _context.Categoria.FindAsync(id);
            if (categoria != null)
            {
                categoria.Estado = false;
            }

            var livros = await _context.Livro.Where(l => l.CategoriaId == id).ToListAsync();

            //verificar se os livros todos tem a quantidade disponivel igual a quantidade total
            foreach (var livro in livros)
            {
                if (livro.NumeroExemplaresDisponiveis != livro.NumeroExemplaresTotal)
                {
                    TempData["MessageCategorias"] = "Error:Não é possível desativar a categoria porque existem livros com exemplares emprestados.";
                    return RedirectToAction(nameof(GerirCategorias));
                }
            }
            foreach (var livro in livros)
            {
                livro.Estado = "Desativado";
            }

            await _context.SaveChangesAsync();

            // Retornar uma partial view para chamadas AJAX
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                TempData["MessageCategorias"] = "Success:Categoria desativada com sucesso.";
                return await GerirCategorias(isAjax: true, searchBox: searchBoxCopy, orderSelect: orderSelectCopy);
            }

            return RedirectToAction(nameof(GerirCategorias));
        }

        //POST: Categorias/AtivarCategoria/5
        [HttpPost, ActionName("AtivarCategoria")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtivarCategoria(int id, string orderSelectCopy = "", string searchBoxCopy = "")
        {
            var categoria = await _context.Categoria.FindAsync(id);
            if (categoria != null)
            {
                categoria.Estado = true;
            }

            var livros = await _context.Livro.Where(l => l.CategoriaId == id).ToListAsync();
            foreach (var livro in livros)
            {
                if (livro.NumeroExemplaresDisponiveis == livro.NumeroExemplaresTotal)
                {
                    if(livro.NumeroExemplaresDisponiveis == 0)
                    {
                        livro.Estado = "Indisponivel";
                    }
                    else
                    {
                        livro.Estado = "Disponível";
                    }
                }
            }

            await _context.SaveChangesAsync();

            // Retornar uma partial view para chamadas AJAX
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                TempData["MessageCategorias"] = "Success:Categoria ativada com sucesso.";
                return await GerirCategorias(isAjax: true, searchBox: searchBoxCopy, orderSelect: orderSelectCopy);
            }

            return RedirectToAction(nameof(GerirCategorias));
        }

        private bool CategoriaExists(int id)
        {
            return _context.Categoria.Any(e => e.Id == id);
        }
    }
}
