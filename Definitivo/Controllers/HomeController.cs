using Definitivo.Data;
using Definitivo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mscc.GenerativeAI;
using NuGet.Packaging;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Xml.Linq;

namespace Definitivo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Perfil> _userManager; 
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<Perfil> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _context = context; // Armazena o DbContext para uso no controlador
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
	        var livros = _context.Livro
		        .Include(l => l.Autor)
		        .Include(l => l.Reviews)
		        .Include(l => l.Categoria)
		        .AsEnumerable() // Converte para uma cole��o em mem�ria antes do GroupBy
		        .GroupBy(l => l.ISBN)
		        .Select(g => g.First())
		        .ToList();// Seleciona o primeiro livro de cada grupo

	        livros = CalcularMetricasEOrdenarLivros(livros, false);

            var userName = User.Identity.IsAuthenticated ? User.Identity.Name : null;

            IndexViewModel viewModel = new IndexViewModel
            {
                Livros = livros
            };

            if (userName != null)
            {
                viewModel.perfil = _context.Users
                    .Include(u => u.LivroFavoritos)
                    .Include(u => u.Emprestimos)
                    .Include(u => u.Reserva)
                    .FirstOrDefault(u => u.UserName == userName);
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

			if (ordemCrescente)
				return livrosQuery.OrderBy(l => metricasLivros[l.ISBN]).ToList();

			return livrosQuery.OrderByDescending(l => metricasLivros[l.ISBN]).ToList();
		}

		[ResponseCache(Duration = 40, Location = ResponseCacheLocation.Any)]
        public IActionResult AboutUs()
        {
            var viewModel = _context.SobreNosModel
                .Include(s => s.MembrosEquipa)
                .Include(s => s.Biblioteca)
                .Include(s => s.HorasFucionamento)
                .Include(s => s.ObjetivoBiblioteca)
                .FirstOrDefault();

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult GuardarAlteracoesAboutUs(IFormCollection formData)
        {

	        // Processar arquivos enviados
	        foreach (var file in formData.Files)
	        {
		        if (file.Length > 0)
		        {
			        var fileKey = file.Name;
			        if (fileKey.StartsWith("Membros") && !fileKey.StartsWith("MembrosNew"))
			        {
				        // Lista de extensões permitidas
				        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
				        var validMimeTypes = new[] { "image/jpeg", "image/png", "image/gif" };
				        const long maxFileSize = 2 * 1024 * 1024; // 2 MB

				        var fileExtension = Path.GetExtension(file.FileName).ToLower();

						if (allowedExtensions.Contains(fileExtension) &&
						    validMimeTypes.Contains(file.ContentType.ToLower()) && file.Length <= maxFileSize)
						{
							
							var uploadsFolder = Path.Combine("wwwroot", "Images", "Funcionarios");
							if (!Directory.Exists(uploadsFolder))
							{
								Directory.CreateDirectory(uploadsFolder);
							}

							var fileName = $"{Guid.NewGuid()}_{file.FileName}";
							var filePath = Path.Combine(uploadsFolder, fileName);

							using (var stream = new FileStream(filePath, FileMode.Create))
							{
								file.CopyTo(stream);
							}

							// Atualizar caminho do arquivo para o membro correspondente, se aplic�vel
							var memberId = int.Parse(fileKey.Split('[')[1].Split(']')[0]);
							var membro = _context.MembroEquipa.FirstOrDefault(m => m.Id == memberId);
							if (membro != null)
							{
								// Caminho completo da imagem anterior
								var oldImagePath = Path.Combine(uploadsFolder, membro.FotoNome);

								// Remove a imagem anterior se ela existir e n�o for o caminho padr�o
								if (!string.IsNullOrEmpty(membro.FotoNome) && System.IO.File.Exists(oldImagePath))
								{
									System.IO.File.Delete(oldImagePath);
								}

								// Atualiza com o novo caminho da imagem
								membro.FotoNome = fileName;
							}
						}

			        }
		        }
	        }


			// Processar campos de texto
			var dados = formData.Keys.ToDictionary(key => key, key => formData[key]);

            // Atualizar os dados do objetivo
            if (dados.ContainsKey("paragrafoInicial"))
            {
                var bulletPoints = new List<string>();
                var objetivo = dados["paragrafoInicial"];
                foreach (var bulletPoint in dados.Where(b => b.Key.StartsWith("bulletPointObjetivo")))
                {
                    if (!String.IsNullOrEmpty(bulletPoint.Value))
                    {
                        bulletPoints.Add(bulletPoint.Value);
                    }
                }

                if (bulletPoints.Count == 0)
                    _context.Objetivo.FirstOrDefault().bulletPoints = "";
                else
                    _context.Objetivo.FirstOrDefault().bulletPoints = String.Join(";", bulletPoints);

                _context.Objetivo.FirstOrDefault().paragrafoInicial = objetivo;
            }

            // Atualizar os dados dos membros da equipa
            if (_context.MembroEquipa.Any() || dados.Where(d => d.Key.StartsWith("Membros")).Any())
            {
                // Filtra os dados relacionados aos membros da equipe
                var membrosEquipaDados = dados
                    .Where(d => d.Key.StartsWith("Membros"))
                    .GroupBy(d => d.Key.Split('[')[1].Split(']')[0]) // Agrupa pelo id do membro
                    .ToDictionary(g => g.Key, g => g.ToDictionary(d => d.Key, d => d.Value));

                foreach (var membroGrupo in membrosEquipaDados)
                {
                    //se for um membro novo
                    if(membroGrupo.Key.StartsWith("NEW_"))
                    {
                        var membroData = membroGrupo.Value; //vai buscar o valor associado a essa key
                        var fileName = ""; //armazena aqui o nome da imagem que o membro vai ter

                        // Obt�m os valores de nome, cargo e descri��o
                        membroData.TryGetValue($"MembrosNew[{membroGrupo.Key}].Nome", out var nomeMembro);
                        membroData.TryGetValue($"MembrosNew[{membroGrupo.Key}].Cargo", out var funcaoMembro);
                        membroData.TryGetValue($"MembrosNew[{membroGrupo.Key}].Descricao", out var descricaoMembro);
                        membroData.TryGetValue($"MembrosNew[{membroGrupo.Key}].Foto", out var fotoMembro);

                        //se o membro tiver algum atributo preenchido, entao vou cria-lo, primeiro verifico se tem foto para adicionar e depois adiciona a bd
                        if (!string.IsNullOrEmpty(nomeMembro) || !string.IsNullOrEmpty(funcaoMembro) || !string.IsNullOrEmpty(descricaoMembro))
                        {
                            var file = formData.Files.FirstOrDefault(f =>
                                f.Length > 0 &&
                                f.Name == $"MembrosNew[{membroGrupo.Key}].Foto");

							if (file != null)
                            {
	                            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
	                            var validMimeTypes = new[] { "image/jpeg", "image/png", "image/gif" };
	                            const long maxFileSize = 2 * 1024 * 1024; // 2 MB
	                            var fileExtension = Path.GetExtension(file.FileName).ToLower();

								if (allowedExtensions.Contains(fileExtension) &&
								    validMimeTypes.Contains(file.ContentType.ToLower()) && file.Length <= maxFileSize)
								{


									var uploadsFolder = Path.Combine("wwwroot", "Images", "Funcionarios");
		                            Directory.CreateDirectory(uploadsFolder);

		                            fileName = $"{Guid.NewGuid()}_{file.FileName}";
		                            var filePath = Path.Combine(uploadsFolder, fileName);

		                            using var stream = new FileStream(filePath, FileMode.Create);
		                            file.CopyTo(stream);
	                            }
                            }

                            if(_context.SobreNosModel.FirstOrDefault().MembrosEquipa == null)
                            {
                                _context.SobreNosModel.FirstOrDefault().MembrosEquipa = new List<MembroEquipa>();
                            }

                            _context.SobreNosModel.FirstOrDefault().MembrosEquipa.Add(new MembroEquipa
                            {
                                Nome = nomeMembro,
                                Cargo = funcaoMembro,
                                Descricao = descricaoMembro,
                                FotoNome = fileName == "" ? "userDefault.png" : fileName
                            });

                        }
                    }
                    else //se nao e um memebro novo
                    {
                        int membroId = Convert.ToInt32(membroGrupo.Key);
                        var membroData = membroGrupo.Value;

                        // Obt�m os valores de nome, cargo e descri��o
                        membroData.TryGetValue($"Membros[{membroId}].Nome", out var nomeMembro);
                        membroData.TryGetValue($"Membros[{membroId}].Cargo", out var funcaoMembro);
                        membroData.TryGetValue($"Membros[{membroId}].Descricao", out var descricaoMembro);
                        membroData.TryGetValue($"Membros[{membroId}].Foto", out var fotoMembro);

                        //verifica se ele tem algum atributo preenchido, se nao tiver elimina-o da bd pois nao tem nada preenchido, se nao atualiza as infos
                        if (!string.IsNullOrEmpty(nomeMembro) || !string.IsNullOrEmpty(funcaoMembro) || !string.IsNullOrEmpty(descricaoMembro))
                        {
                            // Localiza o membro existente no banco
                            var membroExistente = _context.MembroEquipa.FirstOrDefault(m => m.Id == membroId);

                            if (membroExistente != null)
                            {
                                // Atualiza os dados do membro
                                membroExistente.Nome = nomeMembro; ;
                                membroExistente.Cargo = funcaoMembro;
                                membroExistente.Descricao = descricaoMembro;
                            }
                        }
                        else
                        {
                            var membroExistente = _context.MembroEquipa.FirstOrDefault(m => m.Id == membroId);

                            if (membroExistente != null)
                            {
                                // remove os dados do membro
                                //remover a imagem
                                var uploadsFolder = Path.Combine("wwwroot", "Images", "Funcionarios");
                                if (Directory.Exists(uploadsFolder) && membroExistente.FotoNome != "userDefault.png")
                                {
                                    // Caminho completo da imagem anterior
                                    var oldImagePath = Path.Combine(uploadsFolder, membroExistente.FotoNome);

                                    // Remove a imagem anterior se ela existir e n�o for o caminho padr�o
                                    if (!string.IsNullOrEmpty(membroExistente.FotoNome) && System.IO.File.Exists(oldImagePath))
                                    {
                                        System.IO.File.Delete(oldImagePath);
                                    }
                                }

                                _context.MembroEquipa.Remove(membroExistente);
                            }
                        }
                    }
                    
                }

                // Salva as altera��es no banco
                _context.SaveChanges();
            }

            //Atualizar os dados da biblioteca
            if (_context.Biblioteca.Any())
            {
                // Filtra os dados relacionados a biblioteca
                var infoBiblioteca = dados
                    .Where(d => d.Key.StartsWith("Biblioteca"))
                    .ToDictionary(g => g.Key, d => d.Value);

                foreach (var info in infoBiblioteca)
                {
                    if (info.Key == "Biblioteca.CodigoPostal")
                    {
                        _context.Biblioteca.FirstOrDefault().CodigoPostal = info.Value;
                    }
                    else if (info.Key == "Biblioteca.Morada")
                    {
                        _context.Biblioteca.FirstOrDefault().Morada = info.Value;
                    }
                    else if (info.Key == "Biblioteca.Telefone")
                    {
                        _context.Biblioteca.FirstOrDefault().Telefone = info.Value;
                    }
                    else if (info.Key == "Biblioteca.Email")
                    {
                        _context.Biblioteca.FirstOrDefault().Email = info.Value;
                    }
                    else if (info.Key == "Biblioteca.Cidade")
                    {
                        _context.Biblioteca.FirstOrDefault().Cidade = info.Value;
                    }
                }
                _context.SaveChanges();
            }

            if (_context.HoraFuncionamento.Any() || dados.Where(d => d.Key.StartsWith("HorasFucionamento")).Any())
            {
                // Filtra os dados relacionados aos membros da equipe
                var horasDeFuncionamento = dados
                    .Where(d => d.Key.StartsWith("HorasFucionamento"))
                    .GroupBy(d => d.Key.Split('[')[1].Split(']')[0]) // Agrupa pelo id do membro
                    .ToDictionary(g => g.Key, g => g.ToDictionary(d => d.Key, d => d.Value));

                foreach (var hora in horasDeFuncionamento)
                {
                    if(hora.Key.StartsWith("new_"))
                    {
                        // Obt�m os valores de nome, cargo e descri��o
                        var horaData = hora.Value;

                        // Obt�m os valores de nome, cargo e descri��o
                        horaData.TryGetValue($"HorasFucionamentoNew[{hora.Key}].Dia", out var diaFuncionamento);
                        horaData.TryGetValue($"HorasFucionamentoNew[{hora.Key}].Hora", out var horaFuncionamento);

                        if (!string.IsNullOrEmpty(diaFuncionamento) || !string.IsNullOrEmpty(horaFuncionamento))
                        {
                            if(_context.SobreNosModel.FirstOrDefault().HorasFucionamento == null)
                            {
                                _context.SobreNosModel.FirstOrDefault().HorasFucionamento = new List<HoraFuncionamento>();
                            }
                            // Cria a hora na bd
                            _context.SobreNosModel.FirstOrDefault().HorasFucionamento.Add(new HoraFuncionamento
                            {
                                Dia = diaFuncionamento,
                                Hora = horaFuncionamento
                            });
                        }
                    }
                    else
                    {
                        int horaId = Convert.ToInt32(hora.Key);
                        var horaData = hora.Value;

                        // Obt�m os valores de nome, cargo e descri��o
                        horaData.TryGetValue($"HorasFucionamento[{horaId}].Dia", out var diaFuncionamento);
                        horaData.TryGetValue($"HorasFucionamento[{horaId}].Hora", out var horaFuncionamento);

                        if (!string.IsNullOrEmpty(diaFuncionamento) || !string.IsNullOrEmpty(horaFuncionamento))
                        {
                            // Localiza o membro existente no banco
                            var horaExistente = _context.HoraFuncionamento.FirstOrDefault(h => h.Id == horaId);

                            if (horaExistente != null)
                            {
                                // Atualiza os dados do membro
                                horaExistente.Dia = diaFuncionamento;
                                horaExistente.Hora = horaFuncionamento;
                            }
                        }
                        else
                        {
                            //Remove a hora de funcionamento se n�o tiver dados
                            var horaExistente = _context.HoraFuncionamento.FirstOrDefault(h => h.Id == horaId);
                            if (horaExistente != null)
                            {
                                _context.HoraFuncionamento.Remove(horaExistente);
                            }
                        }
                    }
                    

                }

                _context.SaveChanges();
            }


            if (dados.ContainsKey("historia"))
            {
                var historia = dados["historia"];

                _context.SobreNosModel.FirstOrDefault().Historia = historia;
            }

            _context.SaveChanges();
            return Ok(); // Retornar sucesso

        }

        [HttpPost]
        public IActionResult RemoverHorario(int id)
        {
            try
            {
                var horario = _context.HoraFuncionamento.Find(id);
                if (horario != null)
                {
                    _context.HoraFuncionamento.Remove(horario);
                    _context.SaveChanges();
                    return Ok();
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult RemoverMembro(int id)
        {
            try
            {
                var membro = _context.MembroEquipa.Find(id);
                if (membro != null)
                {
                    _context.MembroEquipa.Remove(membro);
                    _context.SaveChanges();
                    return Ok();
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }


        [ResponseCache(Duration = 40, Location = ResponseCacheLocation.Any)]
        public IActionResult FAQ()
        {
            var faq = _context.faqModel
                .Include(f => f.elementos)
                .FirstOrDefault();

            return View(faq);
        }
        [ResponseCache(Duration = 40, Location = ResponseCacheLocation.Any)]
        public IActionResult PoliticasPrivacidade()
        {
            var politica = _context.PoliticaPrivacidadeModel
                .Include(p => p.topicos)
                .FirstOrDefault();
            return View(politica);
        }

        [HttpPost]
        public IActionResult GuardarPoliticaPrivacidade([FromBody] Dictionary<string, string> dados)
        {
            if(_context.PoliticaPrivacidadeModel.Any())
            {
                // Filtra os dados relacionados aos membros da equipe
                var topicos = dados
                    .Where(d => d.Key.StartsWith("Topico"))
                    .GroupBy(d => d.Key.Split('[')[1].Split(']')[0]) // Agrupa pelo id do membro
                    .ToDictionary(g => g.Key, g => g.ToDictionary(d => d.Key, d => d.Value));

                List<TopicoPoliticaPrivacidade> listaTopicos = new List<TopicoPoliticaPrivacidade>();

                foreach (var topico in topicos)
                {

                    TopicoPoliticaPrivacidade novoTopico = new TopicoPoliticaPrivacidade();
                    StringBuilder conteudoTopico = new StringBuilder();

                    if (!topico.Key.StartsWith("new"))
                    {
                        //novoTopico.id = Convert.ToInt32(topico.Key);
                        novoTopico.titulo = topico.Value["Topico[" + topico.Key + "].Titulo"];
                        foreach (var conteudo in topico.Value)
                        {
                            if (conteudo.Key.StartsWith("Topico[" + topico.Key + "].Paragrafo") && !String.IsNullOrEmpty(conteudo.Value)) // Aceita o Paragrafo, <p> representa o paragrafo
                            {
                                conteudoTopico.Append("<p>" + conteudo.Value + "<p>");
                            }

                            if (conteudo.Key.StartsWith("Topico[" + topico.Key + "].Bullet") && !String.IsNullOrEmpty(conteudo.Value)) // Aceita o Bullet, <b> representa o bullet
                            {
                                conteudoTopico.Append("<b>" + conteudo.Value + "<b>");
                            }
                        }
                        novoTopico.conteudo = conteudoTopico.ToString();
                        if (!String.IsNullOrEmpty(novoTopico.titulo) || !String.IsNullOrEmpty(novoTopico.conteudo))
                            listaTopicos.Add(novoTopico);
                        else
                            _context.TopicoPoliticaPrivacidade.Remove(novoTopico);
                    }
                    else
                    {
                        //novoTopico.id = Convert.ToInt32(topico.Key);
                        novoTopico.titulo = topico.Value["TopicoNew[" + topico.Key + "].Titulo"];
                        foreach (var conteudo in topico.Value)
                        {
                            if (conteudo.Key.StartsWith("TopicoNew[" + topico.Key + "].Paragrafo") && !String.IsNullOrEmpty(conteudo.Value)) // Aceita o Paragrafo, <p> representa o paragrafo
                            {
                                conteudoTopico.Append("<p>" + conteudo.Value + "<p>");
                            }

                            if (conteudo.Key.StartsWith("TopicoNew[" + topico.Key + "].Bullet") && !String.IsNullOrEmpty(conteudo.Value)) // Aceita o Bullet, <b> representa o bullet
                            {
                                conteudoTopico.Append("<b>" + conteudo.Value + "<b>");
                            }
                        }
                        novoTopico.conteudo = conteudoTopico.ToString();
                        if (!String.IsNullOrEmpty(novoTopico.titulo) || !String.IsNullOrEmpty(novoTopico.conteudo))
                            listaTopicos.Add(novoTopico);
                    }
                }

                var model = _context.PoliticaPrivacidadeModel.Include(l => l.topicos).FirstOrDefault();
                if (model != null)
                {
                    // Create a list of elements to be removed
                    var elementsToRemove = model.topicos.ToList();

                    foreach (var f in elementsToRemove)
                    {
                        _context.TopicoPoliticaPrivacidade.Remove(f);
                    }

                    _context.SaveChanges();

                    foreach (var f in listaTopicos)
                    {
                        _context.TopicoPoliticaPrivacidade.Add(f);
                    }
                    _context.SaveChanges();

                    _context.PoliticaPrivacidadeModel.FirstOrDefault().topicos = listaTopicos;
                    _context.PoliticaPrivacidadeModel.FirstOrDefault().dataUltimaModificacao = DateTime.Now;
					_context.SaveChanges();

                }

            }

            
            return Ok();
        }

        [HttpPost]
        public IActionResult RemoverTopicoPoliticaPrivacidade(int id)
        {
            try
            {
                var topico = _context.TopicoPoliticaPrivacidade.Find(id);
                if (topico != null)
                {
                    _context.TopicoPoliticaPrivacidade.Remove(topico);
                    _context.SaveChanges();
                    return Ok(new { message = "T�pico removido com sucesso." });
                }
                return NotFound(new { message = "T�pico n�o encontrado." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao remover o topico.", error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult GuardarAlteracoesFaq([FromBody] Dictionary<string, string> dados)
        {
            if (_context.faqElemento.Any())
            {
                var todosElementos = dados
                    .Where(d => d.Key.StartsWith("FAQ") || d.Key.StartsWith("NEW"))
                    .GroupBy(d => d.Key.Split('[')[1].Split(']')[0]) // Agrupa pelo id do elemento faq
                    .ToDictionary(g => g.Key, g => g.ToDictionary(d => d.Key, d => d.Value));

                var faqElementos = dados
                    .Where(d => d.Key.StartsWith("FAQ"))
                    .GroupBy(d => d.Key.Split('[')[1].Split(']')[0]) // Agrupa pelo id do elemento faq
                    .ToDictionary(g => g.Key, g => g.ToDictionary(d => d.Key, d => d.Value));

                var newElementos = dados
                    .Where(d => d.Key.StartsWith("NEW"))
                    .GroupBy(d => d.Key.Split('[')[1].Split(']')[0]) // Agrupa pelo id do elemento faq
                    .ToDictionary(g => g.Key, g => g.ToDictionary(d => d.Key, d => d.Value));

                List<faqElemento> listaElementos = new List<faqElemento>((faqElementos.Count + newElementos.Count));

                foreach (var elemento in faqElementos)
                {
                    faqElemento el = new faqElemento();
                    StringBuilder conteudoTopico = new StringBuilder();

                    var index = todosElementos.Keys.ToList().IndexOf(elemento.Key);

                    //el.id = maxValueFaqElementos + 1;
                    //maxValueFaqElementos++;

                    if (elemento.Value.ContainsKey($"FAQ[{elemento.Key}].Pergunta"))
                    {
                        el.pergunta = elemento.Value[$"FAQ[{elemento.Key}].Pergunta"];
                    }
                    else
                    {
                        el.pergunta = "";
                    }

                    if (elemento.Value.ContainsKey($"FAQ[{elemento.Key}].Resposta"))
                    {
                        el.resposta = elemento.Value[$"FAQ[{elemento.Key}].Resposta"];
                    }
                    else
                    {
                        el.resposta = "";
                    }


                    if (!String.IsNullOrEmpty(el.pergunta) || !String.IsNullOrEmpty(el.resposta))
                        listaElementos.Insert(index, el);
                    else
                        _context.faqElemento.Remove(el);
                }
                _context.SaveChangesAsync();

                foreach (var novoElemento in newElementos)
                {
                    faqElemento el = new faqElemento();
                    StringBuilder conteudoTopico = new StringBuilder();
                    var index = todosElementos.Keys.ToList().IndexOf(novoElemento.Key);

                    //el.id = maxValueFaqElementos + 1;
                    //maxValueFaqElementos++;

                    if (novoElemento.Value.ContainsKey($"NEW[{novoElemento.Key}].Pergunta"))
                    {
                        el.pergunta = novoElemento.Value[$"NEW[{novoElemento.Key}].Pergunta"];
                    }
                    else
                    {
                        el.resposta = "";
                    }

                    if (novoElemento.Value.ContainsKey($"NEW[{novoElemento.Key}].Resposta"))
                    {
                        el.resposta = novoElemento.Value[$"NEW[{novoElemento.Key}].Resposta"];
                    }
                    else
                    {
                        el.resposta = "";
                    }


                    if (!String.IsNullOrEmpty(el.pergunta) || !String.IsNullOrEmpty(el.resposta))
                        listaElementos.Insert(index, el);

                }

                var model = _context.faqModel.Include(l => l.elementos).FirstOrDefault();
                if (model != null)
                {
                    // Create a list of elements to be removed
                    var elementsToRemove = model.elementos.ToList();

                    foreach (var f in elementsToRemove)
                    {
                        _context.faqElemento.Remove(f);
                    }

                    _context.SaveChanges();

                    foreach (var f in listaElementos)
                    {
                        _context.faqElemento.Add(f);
                    }
                    _context.SaveChanges();

                    _context.faqModel.FirstOrDefault().elementos = listaElementos;
                    _context.SaveChanges();

                }
            }

            return Ok();
        }

        [HttpPost]
        public IActionResult RemoverFAQs([FromBody] List<int> ids)
        {
            if (ids != null && ids.Any())
            {
                var faqsToRemove = _context.faqElemento.Where(f => ids.Contains(f.id)).ToList();

                if (faqsToRemove.Any())
                {
                    _context.faqElemento.RemoveRange(faqsToRemove);
                    _context.SaveChanges();
                }
            }

            return Ok();
        }

        [HttpPost]
        public IActionResult SalvarEdicao(IFormCollection formData)
        {
            foreach (var key in formData.Keys)
            {
                var value = formData[key];
                Console.WriteLine($"{key}: {value}");
            }

            foreach (var file in formData.Files)
            {
                if (file.Length > 0)
                {
                    var filePath = Path.Combine("wwwroot/Images", file.FileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                }
            }

            return Ok();
        }


        public IActionResult Shader()
        {
            return View();
        }

        [Authorize]
        public IActionResult Perfil(string sectionActivated = "emprestimos-tab", int page = 1, int itemsPerPage = 3, bool isAjax = false)
        {
            ViewData["MessageEmprestimosPerfil"] = TempData["MessageEmprestimosPerfil"];
            ViewData["MessageReservasPerfil"] = TempData["MessageReservasPerfil"];
            ViewData["MessagePerfil"] = TempData["MessagePerfil"];

			//System.Threading.Thread.Sleep(1500); // Simula um atraso de 1 segundo

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var perfil = _context.Users
                .Include(u => u.Emprestimos)
                    .ThenInclude(e => e.Livro)
                        .ThenInclude(a => a.Autor)
                .Include(u => u.LivroFavoritos)
                .Include(u => u.Reserva)
                    .ThenInclude(e => e.Autor)
                .FirstOrDefault(u => u.Id == userId);

            if (perfil == null)
            {
                return NotFound();
            }

            // Pagina��o aplicada � se��o ativa
            object paginatedData = null;
            int totalItems = 0;
            var perfilEmprestimos = perfil.Emprestimos.ToList();

            if (sectionActivated == "emprestimos-tab" && perfil.Emprestimos != null)
            {
                //por devolver
                var emprestimos = perfilEmprestimos.Where(e => e.DataDevolucao == null).ToList();
                totalItems = emprestimos.Count;
                perfil.Emprestimos = emprestimos
                    .Skip((page - 1) * itemsPerPage)
                    .Take(itemsPerPage)
                    .ToList();
                ViewBag.TotalPagesEmprestimos = (int)Math.Ceiling(totalItems / (double)itemsPerPage);

                //ja devolvidos para a pagina 1
                var emprestimos1 = perfilEmprestimos.Where(e => e.DataDevolucao != null).ToList();
                totalItems = emprestimos1.Count;
                perfil.Emprestimos.AddRange(emprestimos1
                    .Take(itemsPerPage)
                    .ToList());
                ViewBag.TotalPagesHistorico = (int)Math.Ceiling(totalItems / (double)itemsPerPage);

                //ja reservas para a pagina 1
                var reservas = perfil.Reserva.ToList();
                totalItems = reservas.Count;
                perfil.Reserva = reservas
                    .Take(itemsPerPage)
                    .ToList();
                ViewBag.TotalPagesReservas = (int)Math.Ceiling(totalItems / (double)itemsPerPage);


                ViewBag.CurrentPageEmprestimosAtivos = page;
                ViewBag.CurrentPageHistorico = 1;
                ViewBag.CurrentPageReservas = 1;

            }
            else if (sectionActivated == "historico-tab" && perfil.LivroFavoritos != null)
            {
                var emprestimos = perfilEmprestimos.Where(e => e.DataDevolucao != null).ToList();
                totalItems = emprestimos.Count;
                perfil.Emprestimos = emprestimos
                    .Skip((page - 1) * itemsPerPage)
                    .Take(itemsPerPage)
                    .ToList();
                ViewBag.TotalPagesHistorico = (int)Math.Ceiling(totalItems / (double)itemsPerPage);

                //por devolver
                var emprestimos1 = perfilEmprestimos.Where(e => e.DataDevolucao == null).ToList();
                totalItems = emprestimos1.Count;
                perfil.Emprestimos.AddRange(emprestimos1
                    .Take(itemsPerPage)
                    .ToList());
                ViewBag.TotalPagesEmprestimos = (int)Math.Ceiling(totalItems / (double)itemsPerPage);

                //ja reservas para a pagina 1
                var reservas = perfil.Reserva.ToList();
                totalItems = reservas.Count;
                perfil.Reserva = reservas
                    .Take(itemsPerPage)
                    .ToList();
                ViewBag.TotalPagesReservas = (int)Math.Ceiling(totalItems / (double)itemsPerPage);

                ViewBag.CurrentPageEmprestimosAtivos = 1;
                ViewBag.CurrentPageHistorico = page;
                ViewBag.CurrentPageReservas = 1;

            }
            else if (sectionActivated == "reservas-tab" && perfil.Reserva != null)
            {
                var reservas = perfil.Reserva.ToList();
                totalItems = reservas.Count;
                perfil.Reserva = reservas
                    .Skip((page - 1) * itemsPerPage)
                    .Take(itemsPerPage)
                    .ToList();
                ViewBag.TotalPagesReservas = (int)Math.Ceiling(totalItems / (double)itemsPerPage);

                var emprestimos = perfilEmprestimos.Where(e => e.DataDevolucao != null).ToList();
                totalItems = emprestimos.Count;
                perfil.Emprestimos = emprestimos
                    .Take(itemsPerPage)
                    .ToList();
                ViewBag.TotalPagesHistorico = (int)Math.Ceiling(totalItems / (double)itemsPerPage);

                var emprestimos1 = perfilEmprestimos.Where(e => e.DataDevolucao == null).ToList();
                totalItems = emprestimos1.Count;
                perfil.Emprestimos.AddRange(emprestimos1
                    .Take(itemsPerPage)
                    .ToList());
                ViewBag.TotalPagesEmprestimos = (int)Math.Ceiling(totalItems / (double)itemsPerPage);

                ViewBag.CurrentPageEmprestimosAtivos = 1;
                ViewBag.CurrentPageHistorico = 1;
                ViewBag.CurrentPageReservas = page;
            }


            // Adicionar dados ao ViewBag
            ViewBag.sectionActivated = sectionActivated;
            ViewBag.ItemsPerPage = itemsPerPage;

            // Retornar uma partial view para chamadas AJAX
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || isAjax)
            {
                return PartialView($"{sectionActivated}Listing", perfil); // Partial correspondente � se��o ativa
            }

            // Retornar a View com os dados paginados
            return View(perfil);
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadProfileImage(IFormFile profileImage)
        {
            if (profileImage != null && profileImage.Length > 0)
            {
                // Lista de extensões permitidas
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        
                // Verificar a extensão do ficheiro
                var fileExtension = Path.GetExtension(profileImage.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
	                TempData["MessagePerfil"] = "Error:Formato de ficheiro inválido. Apenas imagens (.jpg, .jpeg, .png, .gif, .webp) são permitidas.";
                    return RedirectToAction("Perfil", "Home", new { sectionActivated = "emprestimos-tab" });
                }

                // Verificar o tipo MIME (opcional para maior segurança)
                if (!profileImage.ContentType.StartsWith("image/"))
                {
	                TempData["MessagePerfil"] = "Error:O ficheiro carregado não é uma imagem válida.";
                    return RedirectToAction("Perfil", "Home", new { sectionActivated = "emprestimos-tab" });
                }

                const long maxFileSize = 10 * 1024 * 1024; // 2 MB
                if (profileImage.Length > maxFileSize)
                {
	                TempData["MessagePerfil"] = "Error: O ficheiro é demasiado grande. O tamanho máximo permitido é de 10 MB.";
	                return RedirectToAction("Perfil", "Home", new { sectionActivated = "emprestimos-tab" });
                }

				var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);

                // Remove a imagem anterior, se existir
                if (!string.IsNullOrEmpty(user.FotoNome))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images/Users", user.FotoNome);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

				// Generate a unique filename
				var fileName = $"{Guid.NewGuid()}_{profileImage.FileName}";
				var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images/Users", fileName);

				// Guardar o ficheiro
				try
				{
					using (var fileStream = new FileStream(filePath, FileMode.Create))
					{
						await profileImage.CopyToAsync(fileStream);
					}

					// Atualizar a base de dados
					user.FotoNome = fileName;
					await _userManager.UpdateAsync(user);
				}
				catch (Exception)
				{
					TempData["MessagePerfil"] = "Error: Ocorreu um problema ao guardar a imagem no servidor.";
					return RedirectToAction("Perfil", "Home", new { sectionActivated = "emprestimos-tab" });
				}

				TempData["MessagePerfil"] = "Success: A imagem de perfil foi atualizada com sucesso!";
				return RedirectToAction("Perfil", "Home", new { sectionActivated = "emprestimos-tab" });
			}

            // If no file was uploaded, redirect back to the profile page
            return RedirectToAction("Perfil", "Home", new { sectionActivated = "emprestimos-tab" });
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
