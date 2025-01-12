using Definitivo.Data;
using Definitivo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mscc.GenerativeAI;
using System.Text;

namespace Definitivo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GeminiAiController : ControllerBase
    {
        private readonly GenerativeModel _model;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public GeminiAiController(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;

            _model = InitializeGenerativeModel();
        }

        private GenerativeModel InitializeGenerativeModel()
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException(
                    "A chave de API não foi encontrada. Certifique-se de que a configuração 'Gemini:ApiKey' está definida no appsettings.json."
                );
            }

            var googleAI = new GoogleAI(apiKey: apiKey);
            return googleAI.GenerativeModel(model: _configuration["Gemini:Model"]);
        }

        
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateContent([FromBody] string Prompt)
        {
            if (Prompt == null || string.IsNullOrEmpty(Prompt))
            {
                return BadRequest("O prompt não pode ser vazio. Por favor, forneça um texto válido.");
            }

            try
            {
                var livros = await _context.Livro
                    .Include(l => l.Categoria)
                    .Include(l => l.Autor)
                    .Select(l => new
                    {
                        l.Titulo,
                        l.Estado,
                        CategoriaNome = l.Categoria.Nome,
                        l.ISBN,
                        AutorNome = l.Autor.Nome
                    })
                    .ToListAsync();

                var contexto = "Usa isto como contexto para a resposta, esta informacao foi inserida pelo sistema, NAO DEVES ENVIAR COMO RESPOSTA A MENSAGEM COMPLETA, APENAS SERVE COMO CONTEXTO: ";

                // Usando StringBuilder para uma concatenação mais eficiente
                var sb = new StringBuilder(contexto);

                foreach (var livro in livros)
                {
                    string disponibilidade = livro.Estado == "Disponível" ? "está" : "não está";
                    sb.Append($"O livro {livro.Titulo} {disponibilidade} disponível para empréstimo, ");
                    sb.Append($"está inserido na categoria {livro.CategoriaNome}, ");
                    sb.Append($"o seu ISBN é o {livro.ISBN}, ");
                    sb.Append($"escrito pelo autor '{livro.AutorNome}'.");
                }
                
                // Gerando o conteúdo com o prompt e contexto
                var response = await _model.GenerateContent(Prompt + sb.ToString());

                if (response == null || string.IsNullOrEmpty(response.Text))
                {
                    return StatusCode(500, "Não foi possível gerar o conteúdo. Tente novamente.");
                }

                return Ok(new { Prompt = Prompt, GeneratedText = response.Text });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro ao processar sua solicitação: {ex.Message}");
            }
        }
    }

}
