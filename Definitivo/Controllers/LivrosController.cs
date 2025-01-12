using Definitivo.Data;
using Definitivo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using System.Security.Claims;
using Definitivo.Models.Services;

namespace Definitivo.Controllers
{
    public class LivrosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Perfil> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ProcessarReservasService _reservasService;

        public LivrosController(ApplicationDbContext context, UserManager<Perfil> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment webHostEnvironment, ProcessarReservasService processarReservasService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
            _reservasService = processarReservasService;
        }

        // GET: Livros/Catalogo
        public async Task<IActionResult> Catalogo(int page = 1, string sortOrder = "", List<int> categorias = null, List<string> estados = null)
        {
            ViewData["MessageEmprestimosPerfil"] = TempData["MessageEmprestimosPerfil"];

            //System.Threading.Thread.Sleep(1500);
            var viewModel = new LivroCatalogoViewModel
            {
                PaginaAtual = page,
                TamanhoPagina = 8,
                SortOrder = sortOrder,
                CategoriasSelecionadas = categorias ?? new List<int>(),
                EstadosSelecionados = estados ?? new List<string>()
            };

            var livrosQuery = _context.Livro
                .Include(l => l.Biblioteca)
                .Include(l => l.Categoria)
                .Include(l => l.Autor)
                .Include(l => l.Reviews)
				.ToList();

            livrosQuery = livrosQuery.Where(l => l.Estado != "Desativado").ToList();

            double metrica = 0;
            double mediaAvaliacoes = 0;
            var emprestimosDaCategoriaDoLivro = 1;
            Dictionary<string, double> metricasLivros = new Dictionary<string, double>();

			// Aplicar ordenação
			switch (sortOrder)
            {
                case "title_desc":
                    livrosQuery = livrosQuery.OrderByDescending(l => l.Titulo).ToList();
                    break;
                case "author":
                    livrosQuery = livrosQuery.OrderBy(l => l.Autor.Nome).ToList();
                    break;
                case "author_desc":
                    livrosQuery = livrosQuery.OrderByDescending(l => l.Autor.Nome).ToList();
                    break;
                case "publicationYear":
                    livrosQuery = livrosQuery.OrderBy(l => l.AnoPublicacao).ToList();
                    break;
                case "publicationYear_desc":
                    livrosQuery = livrosQuery.OrderByDescending(l => l.AnoPublicacao).ToList();
                    break;
                case "insertionYear":
                    livrosQuery = livrosQuery.OrderBy(l => l.DataInsercao).ToList();
                    break;
                case "insertionYear_desc":
                    livrosQuery = livrosQuery.OrderByDescending(l => l.DataInsercao).ToList();
                    break;
                case "popularity":
	                livrosQuery = CalcularMetricasEOrdenarLivros(livrosQuery, ordemCrescente: false);
	                break;
				case "popularity_asc":
					livrosQuery = CalcularMetricasEOrdenarLivros(livrosQuery, ordemCrescente: true);
					break;
                default:
                    livrosQuery = livrosQuery.OrderBy(l => l.Titulo).ToList();
                    break;
            }

            livrosQuery = livrosQuery.ToList();

			// Apply filters
			if (categorias != null && categorias.Any())
            {
                livrosQuery = livrosQuery.Where(l => categorias.Contains(l.CategoriaId)).ToList();
            }

            if (estados != null && estados.Any())
            {
                livrosQuery = livrosQuery.Where(l => estados.Contains(l.Estado)).ToList();
            }

            // Apply sorting (your existing sorting code here)

            viewModel.TotalLivros = livrosQuery.Count();
            viewModel.TotalPaginas = (int)Math.Ceiling(viewModel.TotalLivros / (double)viewModel.TamanhoPagina);

            viewModel.Livros = livrosQuery
                .Skip((viewModel.PaginaAtual - 1) * viewModel.TamanhoPagina)
                .Take(viewModel.TamanhoPagina)
                .ToList();

            viewModel.Categorias = _context.Categoria.ToList();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                // Requisição AJAX: retorna apenas a view parcial
                return PartialView("BookGrid", viewModel);
            }
            return View(viewModel);
        }

        public List<Livro> CalcularMetricasEOrdenarLivros(List<Livro> livrosQuery, bool ordemCrescente = true)
        {
	        double metrica = 0;
	        double mediaAvaliacoes = 0;
	        int emprestimosDaCategoriaDoLivro = 1;

	        Dictionary<string, double> metricasLivros = new Dictionary<string, double>();

	        if (User.Identity.IsAuthenticated && User.IsInRole("Leitor"))
	        {
				var emprestimos =
					_context.Perfil.Include(p => p.Emprestimos).FirstOrDefault(p => p.UserName == User.Identity.Name).Emprestimos;

				foreach (var book in livrosQuery)
		        {
			        emprestimosDaCategoriaDoLivro = emprestimos
				        .Where(e => e.Livro.CategoriaId == book.CategoriaId)
				        .Count();

			        mediaAvaliacoes = book.Reviews.Any() ? book.Reviews.Sum(r => r.Rating) / book.Reviews.Count : 0;
			        metrica = 0.3333 * (book.Cliques ?? 0)
			                  + 0.3333 * mediaAvaliacoes / 5
			                  + 0.3333 * (emprestimosDaCategoriaDoLivro / (double)emprestimos.Count);

			        metricasLivros.Add(book.ISBN, metrica);
		        }
	        }
	        else
	        {
		        foreach (var book in livrosQuery)
		        {
			        mediaAvaliacoes = book.Reviews.Any() ? book.Reviews.Sum(r => r.Rating) / book.Reviews.Count : 0;
			        metrica = 0.5 * (book.Cliques ?? 0)
			                  + 0.5 * mediaAvaliacoes / 5;

			        metricasLivros.Add(book.ISBN, metrica);
		        }
	        }

	        if(ordemCrescente)
				return livrosQuery.OrderBy(l => metricasLivros[l.ISBN]).ToList();
	        
	        return livrosQuery.OrderByDescending(l => metricasLivros[l.ISBN]).ToList();
		}


		[HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Leitor")]
        public async Task<IActionResult> DeleteReview(int reviewId, int livroId, string sortOrder = "date-desc")
        {
            var review = _context.Review.FirstOrDefault(r => r.Id == reviewId);

            if (review == null)
            {
                return NotFound();
            }
            _context.Review.Remove(review);
            _context.SaveChanges();

            return await Review(bookId: livroId, sortOrder: sortOrder);
        }


        [HttpPost]
        [Authorize(Roles = "Leitor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReview(int bookId, string text, int rating, string sortOrder = "date-desc")
        {
            // Verifica se o livro existe no banco de dados
            var livro = _context.Livro
                .Include(l => l.Reviews)
                .FirstOrDefault(l => l.ID == bookId);
            var leitor = await _userManager.GetUserAsync(User);

            if (livro == null)
            {
                return NotFound("Livro não encontrado.");
            }

            // Cria uma nova instância de Review
            var review = new Review
            {
                Text = text,
                Rating = rating,
                UserId = leitor.Id, 
                CreatedAt = DateTime.Now,
                TituloLivro = livro.Titulo
            };

            // Carrega os empréstimos do leitor com os livros relacionados
            var emprestimos = await _context.Emprestimo
                .Include(e => e.Livro)
                .Where(e => e.PerfilId == leitor.Id)
                .ToListAsync();

            bool jaLeu = emprestimos.Any(e => e.Livro.ID == bookId && e.DataDevolucao != null);

            if (jaLeu)
            {
                review.Lido = true;
            }

            // Inicializa a coleção Reviews se for nula
            if (livro.Reviews == null)
            {
                livro.Reviews = new List<Review>();
            }

            // Adiciona a nova review à coleção
            livro.Reviews.Add(review);

            // Salva as mudanças no banco de dados
            _context.SaveChanges();

            // Redireciona de volta para a página de detalhes do livro com uma mensagem de sucesso
            return await Review(bookId: bookId, sortOrder: sortOrder);
        }

        [HttpGet]
        public async Task<IActionResult> Review(int bookId, int page = 1, string sortOrder = "date-desc")
        {
            //Thread.Sleep(3000);

            const int ReviewsPerPage = 3; // Número de reviews por página

            // Buscar o livro específico
            var book = await _context.Livro
                .Include(l => l.Reviews)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(l => l.ID == bookId);

            if (book == null)
            {
                return NotFound();
            }

            // Paginação das reviews
            var reviewsQuery = book.Reviews.AsQueryable();

            switch (sortOrder)
            {
                case "date-asc":
                    reviewsQuery = reviewsQuery.OrderBy(r => r.CreatedAt);
                    break;
                case "date-desc":
                    reviewsQuery = reviewsQuery.OrderByDescending(r => r.CreatedAt);
                    break;
                case "stars-asc":
                    reviewsQuery = reviewsQuery.OrderBy(r => r.Rating);
                    break;
                case "stars-desc":
                    reviewsQuery = reviewsQuery.OrderByDescending(r => r.Rating);
                    break;
                default:
                    reviewsQuery = reviewsQuery.OrderByDescending(r => r.CreatedAt);
                    break;
            }

            // Paginação das reviews
            int totalReviews = reviewsQuery.Count();
            var reviews = reviewsQuery
                .Skip((page - 1) * ReviewsPerPage)
                .Take(ReviewsPerPage)
                .ToList();

            book.Reviews = reviews;

            var viewModel1 = new BookDetailsViewModel
            {
                Book = book,
                IsInFavorites = false,
                EstaEmprestadoAEle = false, //se o user ja o requisitou e ja foi busca-lo a biblioteca mas ainda nao o devolveu
                PrecisaIrBuscarLivro = false, //se o user ja foi buscar o livro a biblioteca depois de o pedir emprestado
                EstaReservadoAEle = false, //se o user ja foi buscar o livro a biblioteca depois de o pedir emprestado
                PaginaAtual = page,
                TotalPaginas = (int)Math.Ceiling(totalReviews / (double)ReviewsPerPage),
                SortOrder = sortOrder
            };

            return PartialView("ReviewsGrid", viewModel1);
        }

        [HttpPost]
        public IActionResult UpdateReview(int livroId, int reviewId, string Text, int ratingUpdated, string sortOrder = "date-desc")
        {
            // Código para atualizar a review no banco de dados
            // Exemplo:
            var review = _context.Review.FirstOrDefault(r => r.Id == reviewId);
            if (review != null)
            {
                review.Text = Text;
                review.Rating = ratingUpdated;
                _context.SaveChanges();
            }

            // Retorna a partial ou a mesma view com as reviews atualizadas
            var book = _context.Livro
                        .Include(b => b.Reviews)
                        .ThenInclude(r => r.User)
                        .FirstOrDefault(b => b.ID == livroId);

            if (book == null)
            {
                return NotFound();
            }


            // Paginação das reviews
            var reviewsQuery = book.Reviews.AsQueryable();

            switch (sortOrder)
            {
                case "date-asc":
                    reviewsQuery = reviewsQuery.OrderBy(r => r.CreatedAt);
                    break;
                case "date-desc":
                    reviewsQuery = reviewsQuery.OrderByDescending(r => r.CreatedAt);
                    break;
                case "stars-asc":
                    reviewsQuery = reviewsQuery.OrderBy(r => r.Rating);
                    break;
                case "stars-desc":
                    reviewsQuery = reviewsQuery.OrderByDescending(r => r.Rating);
                    break;
                default:
                    reviewsQuery = reviewsQuery.OrderByDescending(r => r.CreatedAt);
                    break;
            }

            // Paginação das reviews
            int totalReviews = reviewsQuery.Count();
            var reviews = reviewsQuery
                .Take(3)
                .ToList();

            book.Reviews = reviews;

            var viewModel1 = new BookDetailsViewModel
            {
                Book = book,
                IsInFavorites = false,
                EstaEmprestadoAEle = false, //se o user ja o requisitou e ja foi busca-lo a biblioteca mas ainda nao o devolveu
                PrecisaIrBuscarLivro = false, //se o user ja foi buscar o livro a biblioteca depois de o pedir emprestado
                EstaReservadoAEle = false, //se o user ja foi buscar o livro a biblioteca depois de o pedir emprestado
                PaginaAtual = 1,
                TotalPaginas = (int)Math.Ceiling(totalReviews / (double)3),
                SortOrder = "date-desc"
            };

            return PartialView("ReviewsGrid", viewModel1);
        }


        [HttpGet]
        public async Task<IActionResult> Book(int? id, bool? veioDoCatalogo = false, int page = 1, string sortOrder = "date-asc")
        {
            const int ReviewsPerPage = 3; // Número de reviews por página

            ViewData["MessageLivroPage"] = TempData["MessageLivroPage"];

            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var book = await _context.Livro
                .Include(l => l.Autor)
                .Include(l => l.Biblioteca)
                .Include(l => l.BibliotecarioInseriu)
                .Include(l => l.Categoria)
                .Include(b => b.Reviews)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (book == null)
            {
                return NotFound();
            }

            if (veioDoCatalogo == true)
            {
                // Incrementar o atributo Cliques
                book.Cliques++;
                await _context.SaveChangesAsync();
            }

            if (user != null)
            {
                var userWithFavorites = await _context.Users
                .Include(u => u.LivroFavoritos)
                    .ThenInclude(l => l.Autor)
                .Include(u => u.LivroFavoritos)
                    .ThenInclude(l => l.Biblioteca)
                .Include(u => u.LivroFavoritos)
                    .ThenInclude(l => l.Categoria)
                .Include(u => u.LivroFavoritos)
                    .ThenInclude(l => l.BibliotecarioInseriu)
                .Include(u => u.LivroFavoritos)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

                var isInFavorites = userWithFavorites.LivroFavoritos.Any(l => l.ID == id);

                // o user ja o requisitou e ja foi busca-lo a biblioteca mas ainda nao o devolveu
                bool isEmprestado = await _context.Emprestimo
                .AnyAsync(e => e.LivroId == id &&
                       e.PerfilId == user.Id &&
                       e.DataDevolucao == null && e.DataPrevista != null);

                //se o user ja foi buscar o livro a biblioteca depois de o pedir emprestado
                bool isEntregue = await _context.Emprestimo
                .AnyAsync(e => e.LivroId == id &&
                       e.PerfilId == user.Id &&
                       e.DataDevolucao == null && e.Id_bibliotecario_entregou == null);

                bool isReservado = await _context.Perfil.AnyAsync(p => p.Reserva.Any(l => l.ID == id && p.Id == user.Id));

                // Buscar e projetar as reviews associadas ao livro
                var reviewsQuery = book.Reviews
                    .AsQueryable();

                // Aplicar ordenação com base no sortOrder
                switch (sortOrder)
                {
                    case "date-asc":
                        reviewsQuery = reviewsQuery.OrderBy(r => r.CreatedAt);
                        break;
                    case "date-desc":
                        reviewsQuery = reviewsQuery.OrderByDescending(r => r.CreatedAt);
                        break;
                    case "stars-asc":
                        reviewsQuery = reviewsQuery.OrderBy(r => r.Rating);
                        break;
                    case "stars-desc":
                        reviewsQuery = reviewsQuery.OrderByDescending(r => r.Rating);
                        break;
                    default:
                        reviewsQuery = reviewsQuery.OrderBy(r => r.CreatedAt);
                        break;
                }

                // Paginação das reviews
                int totalReviews = reviewsQuery.Count();
                var reviews = reviewsQuery
                    .Skip((page - 1) * ReviewsPerPage)
                    .Take(ReviewsPerPage)
                    .ToList();

                book.Reviews = reviews;

                var viewModel1 = new BookDetailsViewModel
                {
                    Book = book,
                    IsInFavorites = isInFavorites,
                    EstaEmprestadoAEle = isEmprestado, //se o user ja o requisitou e ja foi busca-lo a biblioteca mas ainda nao o devolveu
                    PrecisaIrBuscarLivro = isEntregue, //se o user ja foi buscar o livro a biblioteca depois de o pedir emprestado
                    EstaReservadoAEle = isReservado, //se o user ja foi buscar o livro a biblioteca depois de o pedir emprestado
                    PaginaAtual = page,
                    TotalPaginas = (int)Math.Ceiling(totalReviews / (double)ReviewsPerPage),
                    SortOrder = sortOrder
                };

                return View(viewModel1);

            }

            var viewModel2 = new BookDetailsViewModel
            {
                Book = book,
                IsInFavorites = false,
                EstaEmprestadoAEle = false,
                EstaReservadoAEle = false
            };
            
            return View(viewModel2);
        }



        [HttpGet]
        [Authorize(Roles = "Leitor")]
        public async Task<IActionResult> PedirEmprestado(int livroId)
        {
            //System.Threading.Thread.Sleep(500);
            //verificar se nao excede o limite de livros por user

            var user = await _userManager.GetUserAsync(User);
            var livro = await _context.Livro.FindAsync(livroId);

            if (user == null || livro == null)
            {
                return NotFound();
            }

            if (livro.NumeroExemplaresDisponiveis <= 0)
            {
                TempData["MessageLivroPage"] = "Error:Não há exemplares disponíveis para empréstimo.";
                return RedirectToAction("Book","Livros", new { id = livroId });
            }

            var emprestimo = new Emprestimo
            {
                LivroId = livroId,
                PerfilId = user.Id,
                DataEmprestimo = DateTime.Now
            };

            livro.NumeroExemplaresDisponiveis--;
            if (livro.NumeroExemplaresDisponiveis == 0)
            {
                livro.Estado = "Indisponivel";
            }

            _context.Emprestimo.Add(emprestimo);
            await _context.SaveChangesAsync();

            ViewData["MessageLivroPage"] = "Success:Pedido de empréstimo efetuado com sucesso!";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var botaoTexto = await GetBotaoTexto(livroId);
                var actionLivro = await GetActionLivro(livroId);
                var modeloFormulario = new FormularioLivroViewModel
                {
                    LivroId = livroId,
                    BotaoTexto = botaoTexto,
                    ActionLivro = actionLivro
                };

                return PartialView("BotaoEmprestimoLivro", modeloFormulario);
            }

            TempData["MessageLivroPage"] = "Success:Pedido de empréstimo efetuado com sucesso!";
            return RedirectToAction("Book","Livros", new { id = livroId });
        }

        [HttpGet]
        [Authorize(Roles = "Leitor")]
        public async Task<IActionResult> ReservarLivro(int livroId)
        {
            var user = await _userManager.GetUserAsync(User);
            var perfil = await _context.Perfil.Include(p => p.Reserva).FirstOrDefaultAsync(p => p.Id == user.Id);
            var livro = await _context.Livro.Include(l => l.ReservadoPor).FirstOrDefaultAsync(l => l.ID == livroId);

            if (perfil == null || livro == null || user == null)
            {
                return NotFound();
            }

            // Check if the book is already reserved by this profile
            if (!perfil.Reserva.Contains(livro))
            {
                perfil.Reserva.Add(livro);
                livro.ReservadoPor.Add(perfil);

                await _context.SaveChangesAsync();
            }

            ViewData["MessageLivroPage"] = "Success:Reserva efetuada com sucesso!";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var botaoTexto = await GetBotaoTexto(livroId);
                var actionLivro = await GetActionLivro(livroId);
                var modeloFormulario = new FormularioLivroViewModel
                {
                    LivroId = livroId,
                    BotaoTexto = botaoTexto,
                    ActionLivro = actionLivro
                };

                return PartialView("BotaoEmprestimoLivro", modeloFormulario);
            }

            TempData["MessageLivroPage"] = "Success:Reserva efetuada com sucesso!";
            return RedirectToAction("Book", "Livros", new { id = livroId });
        }

        [HttpGet]
        [Authorize(Roles = "Leitor")]
        public async Task<IActionResult> CancelarReserva(int livroId, bool cancelaNaViewLivro = true)
        {
            var user = await _userManager.GetUserAsync(User);
            var perfil = await _context.Perfil.Include(p => p.Reserva).FirstOrDefaultAsync(p => p.Id == user.Id);
            var livro = await _context.Livro.Include(l => l.ReservadoPor).FirstOrDefaultAsync(l => l.ID == livroId);

            if (perfil == null || livro == null || user == null)
            {
                return NotFound();
            }

            // Check if the book is already reserved by this profile
            if (perfil.Reserva.Contains(livro))
            {
                perfil.Reserva.Remove(livro);
                livro.ReservadoPor.Remove(perfil);

                await _context.SaveChangesAsync();
            }
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" && !cancelaNaViewLivro)
            {
                TempData["MessageReservasPerfil"] = "Success:Reserva cancelada com sucesso!";
                return RedirectToAction("Perfil", "Home", new { sectionActivated = "reservas-tab", page = 1, isAjax = true });
            }

            ViewData["MessageLivroPage"] = "Success:Reserva cancelada com sucesso!";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" && cancelaNaViewLivro)
            {

                var botaoTexto = await GetBotaoTexto(livroId);
                var actionLivro = await GetActionLivro(livroId);
                var modeloFormulario = new FormularioLivroViewModel
                {
                    LivroId = livroId,
                    BotaoTexto = botaoTexto,
                    ActionLivro = actionLivro
                };

                return PartialView("BotaoEmprestimoLivro", modeloFormulario);
            }
            TempData["MessageLivroPage"] = "Success:Reserva cancelada com sucesso!";
            return RedirectToAction("Book", "Livros", new { id = livroId });
        }


        [HttpPost]
        [Authorize(Roles = "Bibliotecario")] //so o bibliotecario pode fazer isto
        public async Task<IActionResult> DevolverLivro(
            int livroId, 
            string userId, 
            bool inBookPage = true,
            string filterStatusEmprestimosCopy = null,
            string searchTermEmprestimosCopy = null,
            string orderSelectEmprestimosCopy = "")
        {
            var bibliotecario = await _userManager.GetUserAsync(User);
            var emprestimo = await _context.Emprestimo
                .FirstOrDefaultAsync(e => e.LivroId == livroId && e.PerfilId == userId && e.DataDevolucao == null);
            var livro = await _context.Livro.FindAsync(livroId);

            if (bibliotecario == null || emprestimo == null || livro == null)
            {
                return NotFound();
            }

            emprestimo.DataDevolucao = DateTime.Now; //define a data de devolucao
            if (livro.NumeroExemplaresDisponiveis == 0 && livro.Estado != "Desativado")
            {
                livro.Estado = "Disponível";
            }
            livro.NumeroExemplaresDisponiveis++; //incrementa o numero de exemplares disponiveis

            if(livro.NumeroExemplaresDisponiveis > livro.NumeroExemplaresTotal)
            {
                livro.NumeroExemplaresTotal = livro.NumeroExemplaresDisponiveis;
            }

            emprestimo.Id_bibliotecario_recebeu = bibliotecario.Id; //atualiza que bibliotecario recebeu o livro

            await _context.SaveChangesAsync();

            await _reservasService.HandleReservasLivro(livroId);//verifica se existem reservas do livro, e se existirem atribui logo

            TempData["MessageEmprestimos"] = "Success:Livro devolvido com sucesso!";

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" && !inBookPage)
            {
                // Retornar partial view para AJAX
                return RedirectToAction("GerirEmprestimos", "Emprestimos", new
                {
                    isAjax = true,
                    filterStatus = filterStatusEmprestimosCopy,
                    searchTerm = searchTermEmprestimosCopy,
                    orderSelect = orderSelectEmprestimosCopy,
                });
            }

            return RedirectToAction("GerirEmprestimos", "Emprestimos");
        }

        
        [Authorize(Roles = "Leitor")]
        [HttpGet]
        public async Task<JsonResult> ObterValoresDisponibilidadeLivro(int livroId)
        {
            var book = await _context.Livro
                .FirstOrDefaultAsync(l => l.ID == livroId);

            var valores = new List<string>
            {
                book.Estado,
                book.NumeroExemplaresDisponiveis + " de " + book.NumeroExemplaresTotal,
                (book.NumeroExemplaresDisponiveis * 100 / book.NumeroExemplaresTotal).ToString()
            };

            return Json(valores);

        }



        [HttpGet]
        [Authorize(Roles = "Leitor")] // Apenas o leitor pode fazer isto
        public async Task<IActionResult> CancelarEmprestimo(int livroId, bool inBookPage = true)
        {
            var leitor = await _userManager.GetUserAsync(User);
            var emprestimo = await _context.Emprestimo
                .FirstOrDefaultAsync(e => e.LivroId == livroId && e.PerfilId == leitor.Id && e.DataPrevista == null);
            var livro = await _context.Livro.FindAsync(livroId);

            if (leitor == null || emprestimo == null || livro == null)
            {
                return NotFound();
            }

            _context.Emprestimo.Remove(emprestimo); // Remove o empréstimo
            if (livro.NumeroExemplaresDisponiveis == 0 && livro.Estado != "Desativado")
            {
                livro.Estado = "Disponível";
            }
            livro.NumeroExemplaresDisponiveis++; // Incrementa o número de exemplares disponíveis

            await _reservasService.HandleReservasLivro(livroId); //verifica se existem reservas do livro, e se existirem atribui logo

            await _context.SaveChangesAsync();


            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" && !inBookPage)
            {
                TempData["MessageEmprestimosPerfil"] = "Success:Empréstimo cancelado com sucesso!";
                return RedirectToAction("Perfil", "Home", new { sectionActivated = "emprestimos-tab", page = 1, isAjax = true});
            }


            ViewData["MessageLivroPage"] = "Success:Empréstimo cancelado com sucesso!";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" && inBookPage)
            {
                var botaoTexto = await GetBotaoTexto(livroId); 
                var actionLivro = await GetActionLivro(livroId); 
                var modeloFormulario = new FormularioLivroViewModel
                {
                    LivroId = livroId,
                    BotaoTexto = botaoTexto,
                    ActionLivro = actionLivro
                };

                return PartialView("BotaoEmprestimoLivro", modeloFormulario);
            }

            TempData["Success"] = "Empréstimo cancelado com sucesso!";
            return RedirectToAction("Book", "Livros", new { id = livroId });

        }

        private async Task<string> GetBotaoTexto(int livroId)
        {
            var leitor = await _userManager.GetUserAsync(User);
            var livro = await _context.Livro.FindAsync(livroId);

            if (leitor == null || livro == null)
            {
                return "Error";
            }

            var userWithFavorites = await _context.Users
                .Include(u => u.LivroFavoritos)
                    .ThenInclude(l => l.Autor)
                .Include(u => u.LivroFavoritos)
                    .ThenInclude(l => l.Biblioteca)
                .Include(u => u.LivroFavoritos)
                    .ThenInclude(l => l.Categoria)
                .Include(u => u.LivroFavoritos)
                    .ThenInclude(l => l.BibliotecarioInseriu)
                .FirstOrDefaultAsync(u => u.Id == leitor.Id);

            var isInFavorites = userWithFavorites.LivroFavoritos.Any(l => l.ID == livroId);

            // o user ja o requisitou e ja foi busca-lo a biblioteca mas ainda nao o devolveu
            bool isEmprestado = await _context.Emprestimo
                .AnyAsync(e => e.LivroId == livroId &&
                   e.PerfilId == leitor.Id &&
                   e.DataDevolucao == null && e.DataPrevista != null);

            //se o user ja foi buscar o livro a biblioteca depois de o pedir emprestado
            bool isEntregue = await _context.Emprestimo
                .AnyAsync(e => e.LivroId == livroId &&
                   e.PerfilId == leitor.Id &&
                   e.DataDevolucao == null && e.Id_bibliotecario_entregou == null);

            bool isReservado = await _context.Perfil.AnyAsync(perfil => perfil.Reserva.Any(l => l.ID == livroId && perfil.Id == leitor.Id));

            bool EstaEmprestadoAEle = isEmprestado; //se o user ja o requisitou e ja foi busca-lo a biblioteca mas ainda nao o devolveu
            bool PrecisaIrBuscarLivro = isEntregue; //se o user ja foi buscar o livro a biblioteca depois de o pedir emprestado
            bool EstaReservadoAEle = isReservado;

            string botaoTexto = "";
            if (livro.Estado == "Desativado")
            {
                botaoTexto = "Desativado";
            }
            else if (EstaReservadoAEle)
            {
                botaoTexto = "Cancelar Reserva";
            }
            else if (livro.Estado == "Indisponivel" && (!PrecisaIrBuscarLivro && !EstaEmprestadoAEle))
            {
                botaoTexto = "Reservar Livro";
            }
            else if (PrecisaIrBuscarLivro)
            {
                botaoTexto = "Cancelar Emprestimo";
            }
            else if (!EstaEmprestadoAEle)
            {
                botaoTexto = "Pedir Emprestimo";
            }
            else
            {
                botaoTexto = "Ja se encontra Emprestado";
            }

            return botaoTexto;
        }

        [HttpGet] //metodo para obter as tags dos livros/autores
        public JsonResult GetTagsLivrosAutores(string term)
        {
            var suggestions = _context.Livro
                .Where(l => l.Titulo.Contains(term))
                .Select(l => l.Titulo)
                .Distinct()
                .ToList();

            suggestions.AddRange(_context.Autor.Where(a => a.Nome.Contains(term))
                .Select(a => a.Nome)
                .Distinct());

            return Json(suggestions);
        }


        private async Task<string> GetActionLivro(int livroId)
        {
            var leitor = await _userManager.GetUserAsync(User);
            var livro = await _context.Livro.FindAsync(livroId);

            if (leitor == null || livro == null)
            {
                return "Error";
            }

            var userWithFavorites = await _context.Users
                .Include(u => u.LivroFavoritos)
                    .ThenInclude(l => l.Autor)
                .Include(u => u.LivroFavoritos)
                    .ThenInclude(l => l.Biblioteca)
                .Include(u => u.LivroFavoritos)
                    .ThenInclude(l => l.Categoria)
                .Include(u => u.LivroFavoritos)
                    .ThenInclude(l => l.BibliotecarioInseriu)
                .FirstOrDefaultAsync(u => u.Id == leitor.Id);

            var isInFavorites = userWithFavorites.LivroFavoritos.Any(l => l.ID == livroId);

            // o user ja o requisitou e ja foi busca-lo a biblioteca mas ainda nao o devolveu
            bool isEmprestado = await _context.Emprestimo
                .AnyAsync(e => e.LivroId == livroId &&
                   e.PerfilId == leitor.Id &&
                   e.DataDevolucao == null && e.DataPrevista != null);

            //se o user ja foi buscar o livro a biblioteca depois de o pedir emprestado
            bool isEntregue = await _context.Emprestimo
                .AnyAsync(e => e.LivroId == livroId &&
                   e.PerfilId == leitor.Id &&
                   e.DataDevolucao == null && e.Id_bibliotecario_entregou == null);

            bool isReservado = await _context.Perfil.AnyAsync(perfil => perfil.Reserva.Any(l => l.ID == livroId && perfil.Id == leitor.Id));

            bool EstaEmprestadoAEle = isEmprestado; //se o user ja o requisitou e ja foi busca-lo a biblioteca mas ainda nao o devolveu
            bool PrecisaIrBuscarLivro = isEntregue; //se o user ja foi buscar o livro a biblioteca depois de o pedir emprestado
            bool EstaReservadoAEle = isReservado;

            string actionLivro = "";
            if (livro?.Estado == "Desativado")
            {
                actionLivro = "";
            }
            else if (EstaReservadoAEle)
            {
                actionLivro = "CancelarReserva";
            }
            else if (livro?.Estado == "Indisponivel" && (!PrecisaIrBuscarLivro && !EstaEmprestadoAEle))
            {
                actionLivro = "ReservarLivro";
            }
            else if (PrecisaIrBuscarLivro)
            {
                actionLivro = "CancelarEmprestimo";
            }
            else if (!EstaEmprestadoAEle)
            {
                actionLivro = "PedirEmprestado";
            }
            else
            {
                actionLivro = "";
            }

            return actionLivro;
        }


        [HttpPost]
        [Authorize(Roles = "Bibliotecario")] //so o bibliotecario pode fazer isto
        public async Task<IActionResult> AprovarEmprestimo(
            int livroId,
            string userId, 
            bool inBookPage = true,
            string filterStatusEmprestimosCopy = null,
            string searchTermEmprestimosCopy = null,
            string orderSelectEmprestimosCopy = "")
        {
            //Obter o bibliotecario que esta a fazer a acao
            var bibliotecario = await _userManager.GetUserAsync(User);
            //obter o emprestimo em questao
            var emprestimo = await _context.Emprestimo
                .FirstOrDefaultAsync(e => e.LivroId == livroId && e.PerfilId == userId && e.DataDevolucao == null && e.DataPrevista == null);
            var livro = await _context.Livro.FindAsync(livroId);

            if (bibliotecario == null || emprestimo == null || livro == null)
            {
                TempData["MessageEmprestimos"] = "Error:Emprestimo não encontrado!";
                return NotFound();
            }

            emprestimo.DataPrevista = DateTime.Now.AddDays(15); //define a data de devolucao prevista
            emprestimo.Id_bibliotecario_entregou = bibliotecario.Id; //define quem entregou o livro ao leitor

            await _context.SaveChangesAsync(); //guarda as alteracoes

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" && !inBookPage)
            {
                TempData["MessageEmprestimos"] = "Success:Emprestimo aprovado com sucesso!";
                // Retornar partial view para AJAX
                return RedirectToAction("GerirEmprestimos", "Emprestimos", new
                {
                    isAjax = true,
                    filterStatus = filterStatusEmprestimosCopy,
                    searchTerm = searchTermEmprestimosCopy,
                    orderSelect = orderSelectEmprestimosCopy,
                });
            }

            TempData["MessageEmprestimos"] = "Success:Emprestimo aprovado com sucesso!";
            return RedirectToAction("GerirEmprestimos", "Emprestimos");
        }

        [HttpGet]
        public async Task<IActionResult> Pesquisar(string termo, int pagina = 1)
        {
            const int itensPorPagina = 8;

            //viewmodel para retornar para o catalogo
            var viewModel1 = new LivroCatalogoViewModel
            {
                PaginaAtual = 1,
                TamanhoPagina = 16,
                SortOrder = "",
                CategoriasSelecionadas = new List<int>(),
                EstadosSelecionados = new List<string>()
            };

            if (string.IsNullOrWhiteSpace(termo))
            {
                var livrosQuery = _context.Livro
                    .Include(l => l.Biblioteca)
                    .Include(l => l.Categoria)
                    .Include(l => l.Autor)
                    .AsQueryable();

                livrosQuery = livrosQuery.Where(l => l.Estado != "Desativado");

                // Apply sorting (your existing sorting code here)

                viewModel1.TotalLivros = await livrosQuery.CountAsync();
                viewModel1.TotalPaginas = (int)Math.Ceiling(viewModel1.TotalLivros / (double)viewModel1.TamanhoPagina);

                viewModel1.Livros = await livrosQuery
                    .Skip((viewModel1.PaginaAtual - 1) * viewModel1.TamanhoPagina)
                    .Take(viewModel1.TamanhoPagina)
                    .ToListAsync();

                viewModel1.Categorias = await _context.Categoria.ToListAsync();

                return View("Catalogo", viewModel1);
            }

            var query = _context.Livro
                .Include(l => l.Autor)
                .Include(l => l.Categoria)
                .Where(l => l.Titulo.Contains(termo) ||
                            l.Autor.Nome.Contains(termo) ||
                            l.ISBN.Contains(termo));

            query = query.Where(l => l.Estado != "Desativado");


            var totalItens = await query.CountAsync();
            var totalPaginas = (int)Math.Ceiling(totalItens / (double)itensPorPagina);

            var livros = await query
                .Skip((pagina - 1) * itensPorPagina)
                .Take(itensPorPagina)
                .ToListAsync();

            var viewModel = new PesquisaViewModel
            {
                Livros = livros,
                PaginaAtual = pagina,
                TotalPaginas = totalPaginas,
                TermoPesquisa = termo
            };

            return View("Pesquisa", viewModel);
        }



        [HttpGet]
        [Authorize(Roles = "Leitor")]
        public async Task<IActionResult> Favoritos(int page = 1)
        {
            ViewData["MessageFavoritosPage"] = TempData["MessageFavoritosPage"];

            const int itemsPerPage = 4; // Número fixo de itens por página
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }


            // Carregue o usuário com LivroFavoritos e inclua as informações relacionadas
            var userWithFavorites = await _context.Users
                .Include(u => u.LivroFavoritos)
                    .ThenInclude(l => l.Autor)
                .Include(u => u.LivroFavoritos)
                    .ThenInclude(l => l.Biblioteca)
                .Include(u => u.LivroFavoritos)
                    .ThenInclude(l => l.Categoria)
                .Include(u => u.LivroFavoritos)
                    .ThenInclude(l => l.BibliotecarioInseriu)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            if (userWithFavorites == null)
            {
                return NotFound();
            }

            var favoritos = userWithFavorites.LivroFavoritos?.Where(l => l.Estado != "Desativado").ToList() ?? new List<Livro>();

            // Calcular total de páginas
            var totalItems = favoritos.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)itemsPerPage);

            // Garantir que a página não esteja fora dos limites
            if (page < 1) page = 1;
            if (page > totalPages) 
                page = totalPages;

            // Aplicar paginação
            var paginatedFavorites = favoritos
                .Skip((page - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToList();

            // Passar informações de paginação para a View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.ItemsPerPage = itemsPerPage;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                // Retornar partial view para AJAX
                return PartialView("LivrosFavoritosListing", paginatedFavorites);
            }

            return View(paginatedFavorites);
        }



        [HttpGet]
        [Authorize]
        [Authorize(Roles = "Leitor")]
        public async Task<IActionResult> RemoverFavorito(int livroId, bool veioDaPaginaDoLivro = false)
        {
            //System.Threading.Thread.Sleep(500);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users
                .Include(u => u.LivroFavoritos)
                .FirstOrDefault(u => u.Id == userId);
            var livro = _context.Livro.Find(livroId);

            if (user != null && livro != null)
            {
                user.LivroFavoritos?.Remove(livro);
                _context.SaveChanges();
            }

            // Se a requisição for AJAX, retorne JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" && !veioDaPaginaDoLivro)
            {
                TempData["MessageFavoritosPage"] = "Success:Livro removido dos favoritos!";
                // Retornar partial view para AJAX
                return await Favoritos(page: 1);
            }

            ViewData["MessageFavoritosPage"] = "Success:Livro removido dos favoritos!";
            // Se a requisição for AJAX, retorne JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" && veioDaPaginaDoLivro)
            {
                var actionFavorito = "AdicionarFavorito";
                var favoritoTexto = "Adicionar aos Favoritos";
                var favoritoClasse = "btn-outline-danger";
                var favoritoIcon =  "";

                var ModelBotaoFavoritos = new FormularioLivroViewModel
                {
                    ActionLivro = actionFavorito,
                    BotaoTexto = favoritoTexto,
                    LivroId = livroId,
                    FavoritosClasse = favoritoClasse,
                    FavoritosIcon = favoritoIcon
                };

                // Retornar partial view para AJAX
                return PartialView("BotaoFavoritosLivro", ModelBotaoFavoritos);
            }

            if (veioDaPaginaDoLivro)
            {
                TempData["MessageLivroPage"] = "Success:Livro removido dos favoritos!";
                return RedirectToAction("Book", "Livros", new { id = livroId });
            }

            TempData["MessageFavoritosPage"] = "Success:Livro removido dos favoritos!";
            return RedirectToAction("Favoritos", "Livros");

        }

        [HttpGet]
        [Authorize(Roles = "Leitor")]
        public IActionResult AdicionarFavorito(int livroId, bool veioDaPaginaDoLivro = false)
        {
            //System.Threading.Thread.Sleep(500);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users
                .Include(u => u.LivroFavoritos)
                .FirstOrDefault(u => u.Id == userId);
            var livro = _context.Livro.Find(livroId);

            if (user != null && livro != null)
            {
                // Check if the book is not already in favorites
                if (!user.LivroFavoritos.Any(l => l.ID == livroId))
                {
                    user.LivroFavoritos.Add(livro);
                    _context.SaveChanges();
                }
            }

            ViewData["MessageFavoritosPage"] = "Success:Livro adicionado aos favoritos!";
            // Se a requisição for AJAX, retorne JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" && veioDaPaginaDoLivro)
            {
                var actionFavorito = "RemoverFavorito";
                var favoritoTexto = "Remover dos Favoritos";
                var favoritoClasse = "btn-danger";
                var favoritoIcon = "-fill";

                var ModelBotaoFavoritos = new FormularioLivroViewModel
                {
                    ActionLivro = actionFavorito,
                    BotaoTexto = favoritoTexto,
                    LivroId = livroId,
                    FavoritosClasse = favoritoClasse,
                    FavoritosIcon = favoritoIcon
                };

                // Retornar partial view para AJAX
                return PartialView("BotaoFavoritosLivro", ModelBotaoFavoritos);
            }

            TempData["MessageFavoritosPage"] = "Success:Livro adicionado aos favoritos!";
            return RedirectToAction("Book", "Livros", new { id = livroId });
        }


        // GET: ManageBooks
        [Authorize(Roles = "Bibliotecario")]
        public async Task<IActionResult> GerirLivros(string query = null, string category = null, string state = null, int paginaAtual = 1, int itensPorPagina = 8, bool isAjax = false)
        {
            ViewData["Message"] = TempData["Message"];

            //System.Threading.Thread.Sleep(500);
            // Inicializa a consulta base
            var livrosQuery = _context.Livro
                .Include(l => l.Biblioteca)
                .Include(l => l.Categoria)
                .Include(l => l.Autor)
                .Include(l => l.BibliotecarioInseriu)
                .Include(l => l.Emprestimos)
                .Include(l => l.PerfisAFavoritar)
                .AsQueryable();

            List<string> listBooks = livrosQuery.Select(l => l.Titulo).ToList();
            ViewBag.TagsLivros = new HtmlString(JsonConvert.SerializeObject(listBooks));

            // Filtrar por query
            if (!string.IsNullOrEmpty(query))
            {
                livrosQuery = livrosQuery.Where(l => l.Titulo.ToUpper().Contains(query.ToUpper()) ||
                                                     l.Autor.Nome.ToUpper().Contains(query.ToUpper()));
            }

            // Filtrar por categoria
            if (!string.IsNullOrEmpty(category) && category != "Todos")
            {
                livrosQuery = livrosQuery.Where(l => l.Categoria.Id.ToString() == category);
            }

            // Filtrar por estado
            if (!string.IsNullOrEmpty(state) && state != "Todos")
            {
                livrosQuery = livrosQuery.Where(l => l.Estado == state);
            }

            // Conta o total de livros após os filtros
            int totalLivros = await livrosQuery.CountAsync();

            // Aplica paginação
            var livrosPaginados = await livrosQuery
                .Skip((paginaAtual - 1) * itensPorPagina)
                .Take(itensPorPagina)
                .ToListAsync();

            // Dados auxiliares para dropdowns
            var bibliotecarios = await _userManager.GetUsersInRoleAsync("Bibliotecario");
            var bibliotecarioList = bibliotecarios.Select(u => new { u.Id, u.UserName }).ToList();

            ViewData["IdBibliotecarioInseriu"] = new SelectList(bibliotecarioList, "Id", "UserName");
            ViewData["AutorId"] = new SelectList(_context.Autor, "Id", "Nome");
            ViewData["BibliotecaId"] = new SelectList(_context.Biblioteca, "Id", "Nome");
            ViewData["CategoriaId"] = new SelectList(_context.Categoria, "Id", "Nome");
            /*if (messageToView != "")
            {
                ViewData["Message"] = messageToView;
            }*/

            // Passa informações para ViewBag
            ViewBag.QuerySearched = query;
            ViewBag.CategorySelected = category ?? "Todos";
            ViewBag.StateSelected = state ?? "Todos";
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalLivros / itensPorPagina);
            ViewBag.PaginaAtual = paginaAtual;

            // Retornar uma partial view para chamadas AJAX
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || isAjax)
            {
                
                return PartialView($"GerirLivrosListing", livrosPaginados); // Partial correspondente à seção ativa
            }

            return View(livrosPaginados);
        }





        // GET: Livros/Create
        [Authorize(Roles = "Bibliotecario")]
        public async Task<IActionResult> Create()
        {
            // Get all users with the "Bibliotecario" role
            var bibliotecarioRole = await _roleManager.FindByNameAsync("Bibliotecario");
            var usersInRole = await _userManager.GetUsersInRoleAsync(bibliotecarioRole.Name);

            var bibliotecarios = usersInRole.Select(u => new { u.Id, u.UserName }).ToList();

            ViewData["IdBibliotecarioInseriu"] = new SelectList(bibliotecarios, "Id", "UserName");
            ViewData["AutorId"] = new SelectList(_context.Autor, "Id", "Nome");
            ViewData["BibliotecaId"] = new SelectList(_context.Biblioteca, "Id", "Nome");
            ViewData["CategoriaId"] = new SelectList(_context.Categoria, "Id", "Nome");

            return View();
        }

        // POST: Livros/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Bibliotecario")]
        public async Task<IActionResult> Create([Bind("ID,BibliotecaId,AnoPublicacao, Sinopse,IdBibliotecarioInseriu, CategoriaId,AutorId,ISBN,Dimensoes,NumeroPaginas,Idioma,Estado,NumeroExemplaresDisponiveis,NumeroExemplaresTotal,Titulo,FotoNome")] Livro livro, IFormFile? FotoUpload)
        {
            if (ModelState.IsValid)
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
                        ViewData["AutorId"] = new SelectList(_context.Autor, "Id", "Nome", livro.AutorId);
                        ViewData["BibliotecaId"] = new SelectList(_context.Biblioteca, "Id", "Nome", livro.BibliotecaId);
                        ViewData["IdBibliotecarioInseriu"] = new SelectList(_context.Perfil, "Id", "UserName", livro.IdBibliotecarioInseriu);
                        ViewData["CategoriaId"] = new SelectList(_context.Categoria, "Id", "Nome", livro.CategoriaId);

                        return View(livro);
                    }

                    // Verificar o tipo MIME (opcional para maior segurança)
                    if (!FotoUpload.ContentType.StartsWith("image/"))
                    {
                        ModelState.AddModelError("FotoNome", "O ficheiro carregado não é uma imagem válida.");
                        ViewData["AutorId"] = new SelectList(_context.Autor, "Id", "Nome", livro.AutorId);
                        ViewData["BibliotecaId"] = new SelectList(_context.Biblioteca, "Id", "Nome", livro.BibliotecaId);
                        ViewData["IdBibliotecarioInseriu"] = new SelectList(_context.Perfil, "Id", "UserName", livro.IdBibliotecarioInseriu);
                        ViewData["CategoriaId"] = new SelectList(_context.Categoria, "Id", "Nome", livro.CategoriaId);

                        return View(livro);
                    }

                    const long maxFileSize = 10 * 1024 * 1024; // 2 MB
                    if (FotoUpload.Length > maxFileSize)
                    {

                        ModelState.AddModelError("FotoNome", "O ficheiro é demasiado grande. O tamanho máximo permitido é de 10 MB.");
                        ViewData["AutorId"] = new SelectList(_context.Autor, "Id", "Nome", livro.AutorId);
                        ViewData["BibliotecaId"] = new SelectList(_context.Biblioteca, "Id", "Nome", livro.BibliotecaId);
                        ViewData["IdBibliotecarioInseriu"] = new SelectList(_context.Perfil, "Id", "UserName", livro.IdBibliotecarioInseriu);
                        ViewData["CategoriaId"] = new SelectList(_context.Categoria, "Id", "Nome", livro.CategoriaId);

                        return View(livro);
                    }
                }

                // Verifica se o ISBN já existe
                if (await _context.Livro.AnyAsync(l => l.ISBN == livro.ISBN))
                {
                    ModelState.AddModelError("ISBN", "Este ISBN já está registrado no sistema.");

                    ViewData["AutorId"] = new SelectList(_context.Autor, "Id", "Nome", livro.AutorId);
                    ViewData["BibliotecaId"] = new SelectList(_context.Biblioteca, "Id", "Nome", livro.BibliotecaId);
                    ViewData["IdBibliotecarioInseriu"] = new SelectList(_context.Perfil, "Id", "UserName", livro.IdBibliotecarioInseriu);
                    ViewData["CategoriaId"] = new SelectList(_context.Categoria, "Id", "Nome", livro.CategoriaId);

                    return View(livro);
                }

                // Verifica se número de exemplares disponíveis é maior que o total
                if (livro.NumeroExemplaresDisponiveis > livro.NumeroExemplaresTotal)
                {
                    ModelState.AddModelError("NumeroExemplaresDisponiveis", "O número de exemplares disponíveis tem que ser igual ao número de exemplares total.");

                    ViewData["AutorId"] = new SelectList(_context.Autor, "Id", "Nome", livro.AutorId);
                    ViewData["BibliotecaId"] = new SelectList(_context.Biblioteca, "Id", "Nome", livro.BibliotecaId);
                    ViewData["IdBibliotecarioInseriu"] = new SelectList(_context.Perfil, "Id", "UserName", livro.IdBibliotecarioInseriu);
                    ViewData["CategoriaId"] = new SelectList(_context.Categoria, "Id", "Nome", livro.CategoriaId);

                    return View(livro);
                }

                // Verifica se número de exemplares disponíveis é maior que o total
                if (livro.NumeroExemplaresDisponiveis != livro.NumeroExemplaresTotal)
                {
                    ModelState.AddModelError("NumeroExemplaresDisponiveis", "O número de exemplares disponíveis tem que ser igual ao número de exemplares total.");

                    ViewData["AutorId"] = new SelectList(_context.Autor, "Id", "Nome", livro.AutorId);
                    ViewData["BibliotecaId"] = new SelectList(_context.Biblioteca, "Id", "Nome", livro.BibliotecaId);
                    ViewData["IdBibliotecarioInseriu"] = new SelectList(_context.Perfil, "Id", "UserName", livro.IdBibliotecarioInseriu);
                    ViewData["CategoriaId"] = new SelectList(_context.Categoria, "Id", "Nome", livro.CategoriaId);

                    return View(livro);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Check if userId is null (user is not authenticated)
                if (string.IsNullOrEmpty(userId))
                {
                    // Add a model error if the user is not authenticated
                    ModelState.AddModelError("All", "Utilizador não autenticado.");

                    // Populate ViewData for dropdowns
                    ViewData["AutorId"] = new SelectList(_context.Autor, "Id", "Nome", livro.AutorId);
                    ViewData["BibliotecaId"] = new SelectList(_context.Biblioteca, "Id", "Nome", livro.BibliotecaId);
                    ViewData["IdBibliotecarioInseriu"] = new SelectList(_context.Perfil, "Id", "UserName", livro.IdBibliotecarioInseriu);
                    ViewData["CategoriaId"] = new SelectList(_context.Categoria, "Id", "Nome", livro.CategoriaId);

                    return View(livro);
                }

                // If user is authenticated, assign the user ID to the livro model
                livro.IdBibliotecarioInseriu = userId;

                if (livro.Estado != "Desativado")
                {
                    if (livro.NumeroExemplaresDisponiveis == 0 && livro.Estado == "Disponível")
                    {
                        livro.Estado = "Indisponivel";
                    }

                    if (livro.NumeroExemplaresDisponiveis > 0 && livro.Estado == "Indisponivel")
                    {
                        livro.Estado = "Disponível";
                    }
                }

                if (FotoUpload != null && FotoUpload.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images/Livros");
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + FotoUpload.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await FotoUpload.CopyToAsync(fileStream);
                    }

                    livro.FotoNome = uniqueFileName;
                }

                _context.Add(livro);

                await _context.SaveChangesAsync();
                TempData["Message"] = "Success: O livro " + livro.Titulo + " foi criado com sucesso.";
                return RedirectToAction(nameof(GerirLivros));
            }


            ViewData["AutorId"] = new SelectList(_context.Autor, "Id", "Nome", livro.AutorId);
            ViewData["BibliotecaId"] = new SelectList(_context.Biblioteca, "Id", "Nome", livro.BibliotecaId);
            ViewData["IdBibliotecarioInseriu"] = new SelectList(_context.Perfil, "Id", "UserName", livro.IdBibliotecarioInseriu);
            ViewData["CategoriaId"] = new SelectList(_context.Categoria, "Id", "Nome", livro.CategoriaId);
            return View(livro);
        }

        // GET: Livros/Edit/5
        [Authorize(Roles = "Bibliotecario")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var livro = await _context.Livro.FindAsync(id);
            if (livro == null)
            {
                return NotFound();
            }



            // Get all users with the "Bibliotecario" role
            var bibliotecarioRole = await _roleManager.FindByNameAsync("Bibliotecario");
            var usersInRole = await _userManager.GetUsersInRoleAsync(bibliotecarioRole.Name);

            var bibliotecarios = usersInRole.Select(u => new { u.Id, u.UserName }).ToList();

            ViewData["IdBibliotecarioInseriu"] = new SelectList(bibliotecarios, "Id", "UserName", livro.IdBibliotecarioInseriu);
            ViewData["AutorId"] = new SelectList(_context.Autor, "Id", "Nome", livro.AutorId);
            ViewData["BibliotecaId"] = new SelectList(_context.Biblioteca, "Id", "Nome", livro.BibliotecaId);
            ViewData["CategoriaId"] = new SelectList(_context.Categoria, "Id", "Nome", livro.CategoriaId);

            return View(livro);
        }

       
        // POST: Livros/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Bibliotecario")]
        public async Task<IActionResult> Edit(int id, [Bind("ID,BibliotecaId,AnoPublicacao, Sinopse,CategoriaId,AutorId,ISBN,Dimensoes,NumeroPaginas,Idioma,IdBibliotecarioInseriu,DataInsercao,Estado,NumeroExemplaresDisponiveis,NumeroExemplaresTotal,Titulo, FotoNome")] Livro livro, IFormFile? FotoUpload)
        {
            if (id != livro.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var numeroEmprestimosAtivosLivro = await _context.Emprestimo
                    .Where(e => e.LivroId == livro.ID && e.DataDevolucao == null) //ja foi entregue/esta para ser entregue mas ainda nao foi devolvido
                    .CountAsync();

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
                            ViewData["AutorId"] = new SelectList(_context.Autor, "Id", "Nome", livro.AutorId);
                            ViewData["BibliotecaId"] = new SelectList(_context.Biblioteca, "Id", "Nome", livro.BibliotecaId);
                            ViewData["IdBibliotecarioInseriu"] = new SelectList(_context.Perfil, "Id", "UserName", livro.IdBibliotecarioInseriu);
                            ViewData["CategoriaId"] = new SelectList(_context.Categoria, "Id", "Nome", livro.CategoriaId);

                            return View(livro);
                        }

                        // Verificar o tipo MIME (opcional para maior segurança)
                        if (!FotoUpload.ContentType.StartsWith("image/"))
                        {
                            ModelState.AddModelError("FotoNome", "O ficheiro carregado não é uma imagem válida.");
                            ViewData["AutorId"] = new SelectList(_context.Autor, "Id", "Nome", livro.AutorId);
                            ViewData["BibliotecaId"] = new SelectList(_context.Biblioteca, "Id", "Nome", livro.BibliotecaId);
                            ViewData["IdBibliotecarioInseriu"] = new SelectList(_context.Perfil, "Id", "UserName", livro.IdBibliotecarioInseriu);
                            ViewData["CategoriaId"] = new SelectList(_context.Categoria, "Id", "Nome", livro.CategoriaId);

                            return View(livro);
                        }

                        const long maxFileSize = 10 * 1024 * 1024; // 2 MB
                        if (FotoUpload.Length > maxFileSize)
                        {

                            ModelState.AddModelError("FotoNome", "O ficheiro é demasiado grande. O tamanho máximo permitido é de 10 MB.");
                            ViewData["AutorId"] = new SelectList(_context.Autor, "Id", "Nome", livro.AutorId);
                            ViewData["BibliotecaId"] = new SelectList(_context.Biblioteca, "Id", "Nome", livro.BibliotecaId);
                            ViewData["IdBibliotecarioInseriu"] = new SelectList(_context.Perfil, "Id", "UserName", livro.IdBibliotecarioInseriu);
                            ViewData["CategoriaId"] = new SelectList(_context.Categoria, "Id", "Nome", livro.CategoriaId);

                            return View(livro);
                        }
                    }

                    // Verifica se o número de exemplares disponíveis é válido
                    if (livro.NumeroExemplaresDisponiveis > livro.NumeroExemplaresTotal)
                    {
                        ModelState.AddModelError("NumeroExemplaresDisponiveis", "O número de exemplares disponíveis não pode ser maior que o total de exemplares.");
                        ViewData["AutorId"] = new SelectList(_context.Autor, "Id", "Nome", livro.AutorId);
                        ViewData["BibliotecaId"] = new SelectList(_context.Biblioteca, "Id", "Nome", livro.BibliotecaId);
                        ViewData["IdBibliotecarioInseriu"] = new SelectList(_context.Perfil, "Id", "UserName", livro.IdBibliotecarioInseriu);
                        ViewData["CategoriaId"] = new SelectList(_context.Categoria, "Id", "Nome", livro.CategoriaId);
                        return View(livro);
                    }

                    // Verifica se o número de exemplares disponíveis é válido
                    if (livro.NumeroExemplaresTotal < livro.NumeroExemplaresDisponiveis + numeroEmprestimosAtivosLivro)
                    {
                        ModelState.AddModelError("NumeroExemplaresTotal", $"O número de exemplares total não pode ser inferior que a soma dos exemplares emprestados e os exemplares disponíveis ({livro.NumeroExemplaresDisponiveis + numeroEmprestimosAtivosLivro}).");
                        ViewData["AutorId"] = new SelectList(_context.Autor, "Id", "Nome", livro.AutorId);
                        ViewData["BibliotecaId"] = new SelectList(_context.Biblioteca, "Id", "Nome", livro.BibliotecaId);
                        ViewData["IdBibliotecarioInseriu"] = new SelectList(_context.Perfil, "Id", "UserName", livro.IdBibliotecarioInseriu);
                        ViewData["CategoriaId"] = new SelectList(_context.Categoria, "Id", "Nome", livro.CategoriaId);
                        return View(livro);
                    }

                    // Verifica se o ISBN já existe
                    if (await _context.Livro.AnyAsync(l => l.ISBN == livro.ISBN && l.ID != livro.ID))
                    {
                        ModelState.AddModelError("ISBN", "Este ISBN já está registrado no sistema.");

                        ViewData["AutorId"] = new SelectList(_context.Autor, "Id", "Nome", livro.AutorId);
                        ViewData["BibliotecaId"] = new SelectList(_context.Biblioteca, "Id", "Nome", livro.BibliotecaId);
                        ViewData["IdBibliotecarioInseriu"] = new SelectList(_context.Perfil, "Id", "UserName", livro.IdBibliotecarioInseriu);
                        ViewData["CategoriaId"] = new SelectList(_context.Categoria, "Id", "Nome", livro.CategoriaId);

                        return View(livro);
                    }


                    // Verifica se número de exemplares disponíveis é maior que o total
                    if (livro.NumeroExemplaresTotal < numeroEmprestimosAtivosLivro)
                    {
                        ModelState.AddModelError("NumeroExemplaresTotal", $"O número de exemplares total não pode ser inferior ao número de exemplares emprestados ({numeroEmprestimosAtivosLivro})");

                        ViewData["AutorId"] = new SelectList(_context.Autor, "Id", "Nome", livro.AutorId);
                        ViewData["BibliotecaId"] = new SelectList(_context.Biblioteca, "Id", "Nome", livro.BibliotecaId);
                        ViewData["IdBibliotecarioInseriu"] = new SelectList(_context.Perfil, "Id", "UserName", livro.IdBibliotecarioInseriu);
                        ViewData["CategoriaId"] = new SelectList(_context.Categoria, "Id", "Nome", livro.CategoriaId);

                        return View(livro);
                    }

                    if (FotoUpload != null && FotoUpload.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images/Livros");

                        // Remover a imagem antiga, se existir
                        if (!string.IsNullOrEmpty(livro.FotoNome))
                        {
                            var oldImagePath = Path.Combine(uploadsFolder, livro.FotoNome);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + FotoUpload.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await FotoUpload.CopyToAsync(fileStream);
                        }

                        livro.FotoNome = uniqueFileName;
                    }

                    if (livro.Estado != "Desativado")
                    {
                        if (livro.NumeroExemplaresDisponiveis == 0 && livro.Estado == "Disponível")
                        {
                            livro.Estado = "Indisponivel";
                        }

                        if (livro.NumeroExemplaresDisponiveis > 0 && livro.Estado == "Indisponivel")
                        {
                            livro.Estado = "Disponível";
                        }
                    }

                    _context.Update(livro);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LivroExists(livro.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Message"] = "Success: O livro " + livro.Titulo + " foi editado com sucesso.";
                return RedirectToAction(nameof(GerirLivros));
            }
            ViewData["AutorId"] = new SelectList(_context.Autor, "Id", "Biografia", livro.AutorId);
            ViewData["BibliotecaId"] = new SelectList(_context.Biblioteca, "Id", "Nome", livro.BibliotecaId);
            ViewData["IdBibliotecarioInseriu"] = new SelectList(_context.Perfil, "Id", "UserName", livro.IdBibliotecarioInseriu);
            ViewData["CategoriaId"] = new SelectList(_context.Categoria, "Id", "Descricao", livro.CategoriaId);
            return View(livro);
        }

        // GET: Livros/Delete/5
        [Authorize(Roles = "Bibliotecario")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var livro = await _context.Livro
                .Include(l => l.Autor)
                .Include(l => l.Biblioteca)
                .Include(l => l.BibliotecarioInseriu)
                .Include(l => l.Categoria)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (livro == null)
            {
                return NotFound();
            }

            return View(livro);
        }

        // POST: Livros/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Bibliotecario")]
        public async Task<IActionResult> Delete(int id, string searchBoxLivrosCopy = "", string stateLivrosCopy = "", string categoryLivroCopy = "")
        {
            //System.Threading.Thread.Sleep(500);

            var livro = await _context.Livro
                .Include(l => l.Emprestimos)
                .Include(l => l.PerfisAFavoritar)
                .Include(l => l.ReservadoPor)
                .Include(l => l.Reviews)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (livro == null)
            {
                return NotFound();
            }

            var numeroEmprestimosAtivos = await _context.Emprestimo
                .Where(e => e.LivroId == livro.ID && e.DataDevolucao == null)
                .CountAsync();

            if (numeroEmprestimosAtivos > 0)
            {
                //TempData["Error"] = "Não é possível remover um livro que tenha exemplares emprestados ou reservados.";
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    TempData["Message"] = "Error:Não é possível remover um livro que tenha exemplares emprestados.";
                    return await GerirLivros(isAjax: true, query: searchBoxLivrosCopy, state: stateLivrosCopy, category: categoryLivroCopy);
                }
                return RedirectToAction(nameof(GerirLivros));
            }

            try
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

                // Remove a imagem física se existir
                if (!string.IsNullOrEmpty(livro.FotoNome))
                {
                    var imagemPath = Path.Combine(_webHostEnvironment.WebRootPath, "Images/Livros", livro.FotoNome);
                    if (System.IO.File.Exists(imagemPath))
                    {
                        System.IO.File.Delete(imagemPath);
                    }
                }

                // Remove o livro
                _context.Livro.Remove(livro);
                await _context.SaveChangesAsync();

                //TempData["Success"] = "Livro eliminado com sucesso.";
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    TempData["Message"] = "Success:Livro eliminado com sucesso.";
                    return await GerirLivros(isAjax: true, query: searchBoxLivrosCopy, state: stateLivrosCopy, category: categoryLivroCopy);
                }

                return RedirectToAction(nameof(GerirLivros));
            }
            catch (Exception ex)
            {
                //TempData["Error"] = "Ocorreu um erro ao eliminar o livro.";
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    TempData["Message"] = "Error:Ocorreu um erro ao eliminar o livro.";
                    return await GerirLivros(isAjax: true, query: searchBoxLivrosCopy, state: stateLivrosCopy, category: categoryLivroCopy);
                }
                return RedirectToAction(nameof(GerirLivros));
            }
        }

        [HttpPost, ActionName("DesativarLivro")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Bibliotecario")]
        public async Task<IActionResult> DesativarLivro(int id, string searchBoxLivrosCopy = "", string stateLivrosCopy = "", string categoryLivroCopy = "")
        {
            var livro = await _context.Livro
                .Include(l => l.Emprestimos)
                .Include(l => l.PerfisAFavoritar)
                .Include(l => l.ReservadoPor)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (livro == null)
            {
                return NotFound();
            }

            var numeroEmprestimosAtivos = await _context.Emprestimo
                .Where(e => e.LivroId == livro.ID && e.DataDevolucao == null)
                .CountAsync();

            if (numeroEmprestimosAtivos > 0)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    TempData["Message"] = "Error:Não é possível desativar um livro que tenha exemplares emprestados.";
                    return await GerirLivros(isAjax: true, query: searchBoxLivrosCopy, state: stateLivrosCopy, category: categoryLivroCopy);
                }
                return RedirectToAction(nameof(GerirLivros));
            }

            try
            {
                //desativa o livro
                livro.Estado = "Desativado";
                await _context.SaveChangesAsync();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    TempData["Message"] = "Success:Livro desativado com sucesso.";
                    return await GerirLivros(isAjax: true, query: searchBoxLivrosCopy, state: stateLivrosCopy, category: categoryLivroCopy);
                }

                return RedirectToAction(nameof(GerirLivros));
            }
            catch (Exception ex)   
            { 
                // Log do erro 
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") 
                {
                    TempData["Message"] = "Error:Ocorreu um erro ao desativar o livro."; 
                    return await GerirLivros(isAjax: true, query: searchBoxLivrosCopy, state: stateLivrosCopy, category: categoryLivroCopy);
                }
                return RedirectToAction(nameof(GerirLivros));
            }
        }

        [HttpPost, ActionName("AtivarLivro")] 
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Bibliotecario")] 
        public async Task<IActionResult> AtivarLivro(int id, string searchBoxLivrosCopy = "", string stateLivrosCopy = "", string categoryLivroCopy = "")
        {
            var livro = await _context.Livro
                .Include(l => l.Emprestimos)
                .Include(l => l.PerfisAFavoritar)
                .Include(l => l.ReservadoPor)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (livro == null)
            {
                return NotFound();
            }


            try
            {
                //ativa o livro, que fica indisponivel se nao tiver exemplares ou disponivel se os tiver

                if (livro.NumeroExemplaresDisponiveis == 0)
                {
                    livro.Estado = "Indisponivel";
                }
                else if (livro.NumeroExemplaresDisponiveis > 0)
                {
                    livro.Estado = "Disponível";
                }


                await _context.SaveChangesAsync();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    TempData["Message"] = "Success:Livro ativado com sucesso.";
                    return await GerirLivros(isAjax: true, query: searchBoxLivrosCopy, state: stateLivrosCopy, category: categoryLivroCopy);
                }

                return RedirectToAction(nameof(GerirLivros));
            }
            catch (Exception ex)
            {
                // Log do erro
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    TempData["Message"] = "Error:Ocorreu um erro ao ativar o livro.";
                    return await GerirLivros(isAjax: true, query: searchBoxLivrosCopy, state: stateLivrosCopy, category: categoryLivroCopy);
                }
                return RedirectToAction(nameof(GerirLivros));
            }
        }

        private bool LivroExists(int id)
        {
            return _context.Livro.Any(e => e.ID == id);
        }
    }
}
