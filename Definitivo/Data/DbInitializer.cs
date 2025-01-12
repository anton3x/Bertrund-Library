using Definitivo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using System;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Runtime.Intrinsics.X86;

namespace Definitivo.Data
{
    public class DbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Perfil> _userManager; // Perfil é a classe de utilizador personalizada
        private readonly RoleManager<IdentityRole> _roleManager; // RoleManager para gerenciar roles

        public DbInitializer(ApplicationDbContext context, UserManager<Perfil> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async void Run()
        {
            _context.Database.EnsureCreated();
            // Criação dos roles
            if (_context.Roles.Any())
            {
                return;
            }

            await _roleManager.CreateAsync(new IdentityRole { Name = "Admin", NormalizedName = "ADMIN" });
            await _roleManager.CreateAsync(new IdentityRole { Name = "Bibliotecario", NormalizedName = "BIBLIOTECARIO" });
            await _roleManager.CreateAsync(new IdentityRole { Name = "Leitor", NormalizedName = "LEITOR" });

            var biblioteca = new Biblioteca
            {
                Nome = "Bertrund",
                Morada = "Av. da Universidade",
                Cidade = "Vila Real",
                CodigoPostal = "5000-703",
                Telefone = "+351259249382",
                Email = "info@bertrund.pt"
            };

            _context.Biblioteca.Add(biblioteca);
            _context.SaveChanges();

            //Criacao dos membros da equipa
            var MembrosEquipa = new List<MembroEquipa>
            {
                new MembroEquipa
                {
                    Nome = "Margarida Silva",
                    Cargo = "Bibliotecária",
                    Descricao = "Com mais de 20 anos de experiência, Margarida lidera a nossa equipe com paixão e dedicação..",
                    FotoNome = "margarida.png"
                },
                new MembroEquipa
                {
                    Nome = "Adriano Santos",
                    Cargo = "Analista de Dados",
                    Descricao = "Adriano utiliza a análise de dados para melhorar os nossos serviços e compreender as necessidades dos utilizadores.",
                    FotoNome = "adriano.png"
                },
                new MembroEquipa
                {
                    Nome = "Manuela Ferreira",
                    Cargo = "Consultora de Informação",
                    Descricao = "Manuela é especialista em organizar e tornar acessível a nossa vasta coleção de recursos.",
                    FotoNome = "manuela.png"
                },
                new MembroEquipa
                {
                    Nome = "Paulo Rodrigues",
                    Cargo = "Desenvolvedor Web",
                    Descricao = "Paulo mantém a nossa presença online e desenvolve ferramentas digitais para melhorar a experiência dos utilizadores.",
                    FotoNome = "paulo.png"
                }
            };

            _context.MembroEquipa.AddRange(MembrosEquipa);
            _context.SaveChanges();


            //Criacao dos horarios de funcionamento
            var HorasFucionamento = new List<HoraFuncionamento>
            {
                new HoraFuncionamento { Dia = "Segunda a Sexta-feira", Hora = "9h00 - 20h00" },
                new HoraFuncionamento { Dia = "Sábado", Hora = "10h00 - 18h00" },
                new HoraFuncionamento { Dia = "Domingo", Hora = "14h00 - 18h00" }
            };

            _context.HoraFuncionamento.AddRange(HorasFucionamento);
            _context.SaveChanges();

            //Criacao do objetivo da biblioteca
            var ObjetivoBiblioteca = new Objetivo
            {
                paragrafoInicial = "O nosso objetivo é proporcionar acesso fácil ao conhecimento e incentivar a leitura, permitindo que todos requisitem livros e recursos sempre que precisarem.\nProcuramos promover a igualdade de oportunidades e apoiar o crescimento intelectual da nossa comunidade.",
                bulletPoints = "Promover a leitura e a aprendizagem ao longo da vida.;Fornecer acesso a informações e recursos de qualidade.;Criar um ambiente inclusivo e acolhedor para todos.;Apoiar a educação e o desenvolvimento pessoal."

            };

            _context.Objetivo.Add(ObjetivoBiblioteca);
            _context.SaveChanges();

            //Criacao do sobrenosmodel da biblioteca
            var Historia = "Fundada em 1999, a nossa biblioteca tem servido a comunidade de forma contínua, evoluindo para um espaço totalmente online. Adaptamo-nos às mudanças tecnológicas, oferecendo uma vasta gama de recursos digitais para atender às necessidades dos nossos utilizadores no mundo moderno.";

            var SobreNosModel = new SobreNosModel
            {
                MembrosEquipa = MembrosEquipa,
                Historia = Historia,
                Biblioteca = biblioteca,
                ObjetivoBiblioteca = ObjetivoBiblioteca,
                HorasFucionamento = HorasFucionamento
            };

            _context.SobreNosModel.Add(SobreNosModel);
            _context.SaveChanges();

            // Criação dos tópicos da política de privacidade
            var TopicosPoliticaPrivacidade = new List<TopicoPoliticaPrivacidade>
            {
                new TopicoPoliticaPrivacidade { titulo = "Introdução", conteudo = "<p>A Biblioteca Bertrund está comprometida em proteger a sua privacidade. Esta Política de Privacidade explica como recolhemos, utilizamos, divulgamos e protegemos as suas informações pessoais ao utilizar os nossos serviços.<p>" },
                new TopicoPoliticaPrivacidade { titulo = "Informações que Recolhemos", conteudo = "<p>Podemos recolher os seguintes tipos de informações:<p><b>Informações de identificação pessoal (nome, email, telefone)<b>Informações de conta da biblioteca (histórico de empréstimos)<b>Informações de uso do site (endereço IP, tipo de navegador, páginas visitadas)<b>Informações de pesquisa e preferências de leitura<b>" },
                new TopicoPoliticaPrivacidade { titulo = "Como Utilizamos as Suas Informações", conteudo = "<p>Utilizamos as suas informações para:<p><b>Fornecer e melhorar os nossos serviços de biblioteca<b>Processar empréstimos e reservas de livros<b>Comunicar sobre eventos, programas e serviços da biblioteca<b>Personalizar a sua experiência no nosso site<b>Realizar análises e pesquisas para melhorar os nossos serviços<b>" },
                new TopicoPoliticaPrivacidade { titulo = "Partilha de Informações", conteudo = "<p>Não vendemos, alugamos ou partilhamos as suas informações pessoais com terceiros, exceto nas seguintes circunstâncias:<p><b>Com o seu consentimento<b>Para cumprir obrigações legais<b>Com prestadores de serviços que nos ajudam a operar a biblioteca<b>" },
                new TopicoPoliticaPrivacidade { titulo = "Segurança das Informações", conteudo = "<p>Implementamos medidas de segurança técnicas e organizacionais para proteger as suas informações contra acesso não autorizado, alteração, divulgação ou destruição. No entanto, nenhum método de transmissão pela Internet ou método de armazenamento electrónico é 100% seguro.<p>" },
                new TopicoPoliticaPrivacidade { titulo = "Os Seus Direitos", conteudo = "<p>Tem o direito de:<p><b>Aceder às informações pessoais que temos sobre si<b>Corrigir informações imprecisas ou incompletas<b>Solicitar a eliminação das suas informações pessoais<b>" },
                new TopicoPoliticaPrivacidade { titulo = "Alterações nesta Política", conteudo = "<p>Podemos actualizar esta Política de Privacidade periodicamente. Recomendamos que reveja esta página regularmente para se manter informado sobre quaisquer alterações. A data da última actualização será sempre indicada no topo desta página.<p>" },
                new TopicoPoliticaPrivacidade { titulo = "Contacto", conteudo = "<p>Caso tenha dúvidas sobre esta Política de Privacidade ou sobre as suas informações pessoais, entre em contacto connosco:<p>Email: privacidade@bertrund.pt<p>Telefone: +351 259249382<p>Endereço: Av. da Universidade, 5000-703, Vila Real<p>" }
            };

            _context.TopicoPoliticaPrivacidade.AddRange(TopicosPoliticaPrivacidade);
            _context.SaveChanges();

            // Criacao do model da politica de privacidade
            var PoliticaPrivacidadeModel = new PoliticaPrivacidadeModel
            {
                topicos = TopicosPoliticaPrivacidade,
                dataUltimaModificacao = DateTime.Now
            };

            _context.PoliticaPrivacidadeModel.Add(PoliticaPrivacidadeModel);
            _context.SaveChanges();

            // Criação dos elementos das FAQ
            var faqElementos = new List<faqElemento> {
                new faqElemento { pergunta = "Como posso requisitar livros?", resposta = "Pode requisitar livros através do nosso site. Basta fazer login na sua conta, escolher os livros que deseja requisitar e confirmar o pedido. Depois, dirija-se à biblioteca para levantar os livros." },
                new faqElemento { pergunta = "Quantos livros posso requisitar de uma vez?", resposta = "Pode requisitar quantos livros desejar de uma vez, desde que todos sejam diferentes. Não há limite para o número de livros." },
                new faqElemento { pergunta = "Preciso de um cartão de biblioteca para requisitar livros?", resposta = "Não, não é necessário um cartão de biblioteca. Apenas precisa de criar uma conta no nosso site com o seu username ou email." },
                new faqElemento { pergunta = "Como funciona a devolução de livros?", resposta = "Após requisitar os livros, pode devolvê-los na biblioteca até à data indicada. Não há multas em caso de incumprimento das datas de devolução." },
                new faqElemento { pergunta = "Posso renovar os meus empréstimos?", resposta = "Não é possível renovar empréstimos. Caso necessite do livro novamente, deverá requisitá-lo novamente através do site, desde que esteja disponível." },
                new faqElemento { pergunta = "A biblioteca oferece acesso a e-books?", resposta = "Atualmente, não disponibilizamos e-books. O nosso foco é oferecer livros físicos que podem ser requisitados e levantados na biblioteca." },
                new faqElemento { pergunta = "Como posso criar uma conta no site da biblioteca?", resposta = "Para criar uma conta, aceda ao nosso site e clique em 'Registar'. Insira o seu username ou email e crie uma palavra-passe. Após o registo, poderá requisitar livros." },
                new faqElemento { pergunta = "Há limite para o período de empréstimo dos livros?", resposta = "Sim, o período de empréstimo é de 2 semanas. Após este período, os livros devem ser devolvidos à biblioteca." },
                new faqElemento { pergunta = "A biblioteca oferece programas ou eventos?", resposta = "Sim, oferecemos eventos como clubes de leitura, workshops e atividades comunitárias." },
                new faqElemento { pergunta = "Como posso sugerir um livro para a biblioteca adquirir?", resposta = "Valorizamos as sugestões dos nossos utilizadores. Pode enviar a sua sugestão para o email sugestoes@bertrund.pt. Embora nem todas as sugestões possam ser atendidas, consideramos todas com cuidado." }
            };

            _context.faqElemento.AddRange(faqElementos);
            _context.SaveChanges();

            // Criacao do model das FAQ
            var faqModel = new faqModel
            {
                elementos = faqElementos
            };

            _context.faqModel.Add(faqModel);
            _context.SaveChanges();

            var categorias = new List<Categoria>
            {
                new Categoria { Nome = "Poesia", Descricao = "Obras poéticas", Estado = true },
                new Categoria { Nome = "Romance", Descricao = "Obras de ficção em prosa", Estado = true },
                new Categoria { Nome = "Ciência", Descricao = "Obras científicas e académicas", Estado = true },
                new Categoria { Nome = "Ficção Científica", Descricao = "Obras de ficção científica", Estado = true },
                new Categoria { Nome = "Terror", Descricao = "Obras de terror e suspense", Estado = true },
                new Categoria { Nome = "História", Descricao = "Obras históricas", Estado = true },
                new Categoria { Nome = "Biografia", Descricao = "Histórias da vida de pessoas", Estado = true },
                new Categoria { Nome = "Ficção Policial", Descricao = "Obras de mistério e investigação policial", Estado = true },
                new Categoria { Nome = "Autoajuda", Descricao = "Obras para desenvolvimento pessoal e motivação", Estado = true },
                new Categoria { Nome = "Fantasia", Descricao = "Obras com elementos de magia e mundos imaginários", Estado = true },
                new Categoria { Nome = "Drama", Descricao = "Obras focadas em narrativa emocional e conflitos", Estado = true },
                new Categoria { Nome = "Aventura", Descricao = "Obras com foco em exploração e ação", Estado = true },
                new Categoria { Nome = "Humor", Descricao = "Obras com temática humorística", Estado = true },
                new Categoria { Nome = "Filosofia", Descricao = "Obras filosóficas que tratam de questões existenciais", Estado = true },
                new Categoria { Nome = "Psicologia", Descricao = "Obras sobre psicologia e comportamento humano", Estado = true },
                new Categoria { Nome = "Literatura Infantil", Descricao = "Obras destinadas ao público infantil", Estado = true },
                new Categoria { Nome = "Teatro", Descricao = "Obras dramáticas destinadas à representação teatral", Estado = true },
                new Categoria { Nome = "Contos", Descricao = "Coleções de narrativas curtas", Estado = true }
            };


            _context.Categoria.AddRange(categorias);
            _context.SaveChanges();

            // Criação dos autores
            var autores = new List<Autor>
            {
                new Autor { Nome = "Carlos Drummond de Andrade", Biografia = "Carlos Drummond de Andrade foi um poeta, jornalista e crítico literário brasileiro, amplamente considerado um dos maiores poetas da literatura brasileira. Nascido em Itabira, Minas Gerais, em 31 de outubro de 1902, ele se destacou por sua capacidade de capturar a vida urbana e as complexidades da condição humana em suas obras. Drummond foi um dos principais representantes do Modernismo no Brasil, e seu primeiro livro, Alguma Poesia (1930), estabeleceu sua reputação. Ao longo de sua carreira, ele produziu mais de quinze volumes de poesia, além de crônicas e ensaios. Seu estilo é caracterizado por uma linguagem coloquial e uma profunda reflexão sobre a vida cotidiana. Drummond faleceu em 17 de agosto de 1987, no Rio de Janeiro.", FotoNome = "CarlosAndrade.jpg", Nacionalidade = "Brasileira", DataNascimento = new DateOnly(1902, 10, 31), DataFalecimento = new DateOnly(1987, 8, 17) },
                new Autor { Nome = "Machado de Assis", Biografia = "Joaquim Maria Machado de Assis, nascido em 21 de junho de 1839 no Rio de Janeiro, é considerado o maior escritor da literatura brasileira. Ele foi um romancista, poeta e dramaturgo que co-fundou a Academia Brasileira de Letras e se destacou por suas obras que exploram a psicologia humana e as complexidades sociais do Brasil do século XIX. Entre suas obras mais notáveis estão Memórias Póstumas de Brás Cubas (1881) e Dom Casmurro (1899). Machado era um escritor inovador que desafiou as normas literárias da época com seu estilo único e suas narrativas profundas. Ele faleceu em 29 de setembro de 1908.", FotoNome = "MachadoAssis.jpg", Nacionalidade = "Brasileira", DataNascimento = new DateOnly(1839, 6, 21), DataFalecimento = new DateOnly(1908, 9, 29) },
                new Autor { Nome = "Jorge Amado", Biografia = "Jorge Amado nasceu em Itabuna, Bahia, em 10 de agosto de 1912. Ele foi um romancista brasileiro renomado e é conhecido por suas obras que retratam a cultura e os conflitos sociais do Brasil. Amado escreveu cerca de quarenta livros, incluindo clássicos como Gabriela, Cravo e Canela (1958) e Dona Flor e Seus Dois Maridos (1966). Suas histórias frequentemente abordam temas como a luta pela justiça social e a vida dos marginalizados. Amado também teve uma carreira política ativa como membro do Partido Comunista Brasileiro. Faleceu em 6 de agosto de 2001 em Salvador.", FotoNome = "JorgeAmado.jpg", Nacionalidade = "Brasileira", DataNascimento = new DateOnly(1912, 8, 10), DataFalecimento = new DateOnly(2001, 8, 6) },
                new Autor { Nome = "Freida McFadden", Biografia = "Freida McFadden é uma autora americana nascida em 1º de maio de 1967. Ela é conhecida por seus thrillers psicológicos que conquistaram uma vasta audiência. Antes de se tornar escritora em tempo integral, McFadden trabalhou como médica especializada em distúrbios cerebrais. Sua carreira literária começou com o livro Devil W Scrubs, que se tornou um sucesso inesperado. Desde então, ela publicou mais de vinte livros que venderam milhões de cópias e frequentemente aparecem nas listas dos mais vendidos.", FotoNome = "FreidaMcFadden.jpg", Nacionalidade = "Americana", DataNascimento = new DateOnly(1967, 5, 1)},
                new Autor { Nome = "Leslie Wolfe", Biografia = "Leslie Wolfe é uma autora americana conhecida por seus thrillers e mistérios. Nascida em 7 de outubro de 1967, ela começou sua carreira literária após uma trajetória como analista política e defensora dos direitos humanos das mulheres. Wolfe combina sua experiência profissional com narrativas intrigantes que exploram questões sociais complexas. Ela tem ganhado reconhecimento por suas histórias envolventes que misturam suspense com questões éticas.", FotoNome = "LeslieWolf.jpg", Nacionalidade = "Americana", DataNascimento = new DateOnly(1967, 10, 7)},
                new Autor { Nome = "T L Swan", Biografia = "T L Swan é uma autora australiana nascida em 7 de julho de 1970. Ela é conhecida por seus romances contemporâneos que frequentemente exploram temas emocionais profundos e relacionamentos complexos. Swan começou sua carreira na escrita após anos dedicados à educação dos filhos e rapidamente ganhou popularidade com suas histórias cativantes que ressoam com os leitores.", FotoNome = "TLSwanjpg.jpg", Nacionalidade = "Australiana", DataNascimento = new DateOnly(1970, 7, 7)},
                new Autor { Nome = "Catharina Maura", Biografia = "Catharina Maura é uma autora americana nascida em 17 de dezembro de 1985. Ela se destacou na literatura romântica contemporânea com suas histórias envolventes que muitas vezes incluem elementos fantásticos e emocionais. Maura começou a escrever desde jovem e ganhou notoriedade após publicar seu primeiro romance em 2020.", FotoNome = "CatharinaMaura.jpg", Nacionalidade = "Americana", DataNascimento = new DateOnly(1985, 12, 17)},
                new Autor { Nome = "Manuel Clemente", Biografia = "Manuel Clemente é um autor português nascido em Torres Vedras em 16 de julho de 1948. Ele é conhecido por suas obras sobre autoajuda e reflexões pessoais que abordam a espiritualidade e o crescimento pessoal. Clemente também tem uma carreira significativa na Igreja Católica como Patriarca Emérito de Lisboa.", FotoNome = "ManuelClemente.jpg", Nacionalidade = "Portuguesa", DataNascimento = new DateOnly(1948, 7, 16)},
                new Autor { Nome = "Cláudio Ramos", Biografia = "Cláudio Ramos é um autor e personalidade televisiva portuguesa nascido em Vila Nova da Barquinha no dia 10 de novembro de 1973. Ele ganhou notoriedade na televisão portuguesa como apresentador e comentarista cultural. Além disso, Ramos também escreve sobre temas variados incluindo cultura pop e estilo de vida.", FotoNome = "ClaudioRamos.jpg", Nacionalidade = "Portuguesa", DataNascimento = new DateOnly(1973, 11, 10)},
                new Autor { Nome = "Elsie Silver", Biografia = "Elsie Silver é autora de romances atrevidos e sensuais passados em meios pequenos. Perde-se por um bom namorado literário e as heroínas fortes que os fazem cair de joelhos. Tornou-se uma grande fã da quietude das cinco da manhã. É nesta altura do dia que gosta de beber uma chávena de café quente e idealizar mundos fictícios cheios de histórias românticas para partilhar com os seus leitores. Elsie vive nos arredores de Vancouver, com o marido, o filho e três cães, e é uma leitora voraz de romances desde provavelmente mais cedo do que devia. ", FotoNome = "ElsieSilver.webp", Nacionalidade = "Americana", DataNascimento = new DateOnly(1985, 1, 10)},
                new Autor { Nome = "Jorge Nuno Pinto da Costa ", Biografia = "Jorge Nuno Pinto da Costa nasceu no Porto, a 28 de dezembro de 1937, e é figura absolutamente indissociável do Futebol Clube do Porto. Sócio do clube desde 1953, seccionista desde 1958, diretor desde 1968 e presidente durante 42 anos, de 1982 a 2024. É, no contexto do futebol mundial, o dirigente desportivo com maior longevidade e mais títulos – assim se resume de forma breve um percurso longo e de incomparável êxito e prestígio, tanto a nível doméstico como internacional.", FotoNome = "JorgeNunoPintodaCosta.webp", Nacionalidade = "Portuguesa", DataNascimento = new DateOnly(1937, 12, 28)},
                new Autor { Nome = "Filipe Santos Costa", Biografia = "Filipe Santos Costa começou a sua carreira no Público, onde integrou a editoria de Política. Faz regularmente comentário televisivo e foi apresentador e autor de programas de debate político na SIC Notícias e na CNN.", FotoNome = "FilipeSantosCosta.jpg", Nacionalidade = "Portuguesa", DataNascimento = new DateOnly(1979, 5, 18)},
                new Autor { Nome = "Anne Jacobs", Biografia = "Anne Jacobs publicou sob pseudónimo vários romances históricos e sagas inesquecíveis que se tornaram fenómenos de vendas em vários países. Mas foi com a saga A Vila dos Tecidos, um fenómeno de vendas mundial, que se confirmou como uma autora best-seller.", FotoNome = "AnneJacobs.webp", Nacionalidade = "Alema", DataNascimento = new DateOnly(1950, 8, 9)},
                new Autor { Nome = "Quino", Biografia = "Quino, pseudônimo de Joaquín Salvador Lavado, foi um cartunista e ilustrador argentino, criador da célebre personagem Mafalda, uma menina crítica e questionadora sobre temas sociais e políticos. Iniciou sua carreira como ilustrador em Buenos Aires, produzindo tiras e caricaturas. Mafalda surgiu em 1964 e rapidamente se tornou popular por seu humor inteligente e crítico. Quino encerrou as tiras de Mafalda em 1973 para explorar novos temas no humor gráfico. Recebeu diversos prêmios, como o Príncipe das Astúrias em 2014. Quino é lembrado por seu humor afiado e sua habilidade de criticar a sociedade com simplicidade e profundidade.", FotoNome = "Quino.webp", Nacionalidade = "Argentina", DataNascimento = new DateOnly(1932, 7, 17), DataFalecimento= new DateOnly(2020, 9, 30)},
                new Autor { Nome = "Mia Couto", Biografia = "Mia Couto, nascido na Beira, Moçambique, em 1955, é biólogo e escritor, e já foi jornalista e professor. Sua obra está traduzida em várias línguas. Ele recebeu prêmios como o Vergílio Ferreira (1999), o Prémio União Latina de Literaturas Românicas (2007), o Prémio Camões (2013), e o Prémio Jan Michalski de Literatura (2020), entre outros. \"Terra Sonâmbula\" foi reconhecido como um dos melhores livros africanos do século XX. Mia Couto é conhecido por sua contribuição ao desenvolvimento da língua portuguesa e pelo uso inovador da linguagem na ficção.", FotoNome = "MiaCouto.webp", Nacionalidade = "Portuguesa", DataNascimento = new DateOnly(1955, 7, 5)},
                new Autor { Nome = "José Rodrigues dos Santos", Biografia = "José Rodrigues dos Santos nasceu em 1964 em Moçambique. É sobretudo conhecido pelo seu trabalho como jornalista, carreira que abraçou em 1981, na Rádio Macau. Trabalhou na BBC, em Londres, de 1987 a 1990, e seguiu para a RTP, onde começou a apresentar o 24 horas. Em 1991 passou para a apresentação do Telejornal e tornou-se colaborador permanente da CNN entre 1993 e 2002.\r\nDoutorado em Ciências da Comunicação, é professor da Universidade Nova de Lisboa e jornalista da RTP, tendo ocupado por duas vezes o cargo de Diretor de Informação da televisão pública. É um dos mais premiados jornalistas portugueses, galardoado com dois prémios do Clube Português de Imprensa e três da CNN, entre outros.", FotoNome = "JoseRodriguesdosSantos.webp", Nacionalidade = "Portuguesa", DataNascimento = new DateOnly(1964, 4,1)},
                new Autor { Nome = "Raul Minh'alma", Biografia = "Raul Minh'alma nasceu em 1992 e é um dos escritores mais vendidos da atualidade. Depois de ter publicado o livro mais vendido em 2019, o romance Foi Sem Querer Que Te Quis, que conquistou o coração dos portugueses, seguiram-se outros fenómenos de vendas nos anos seguintes. Com presença habitual nos lugares cimeiros dos tops nacionais, em 2020 e 2021, foi mesmo o autor português que mais livros vendeu. Os seus romances caracterizam-se por serem histórias de amor com uma forte componente de desenvolvimento pessoal. O compromisso é ajudar o leitor enquanto lhe conta uma emocionante história. O autor vendeu, até ao momento, mais de 600 mil exemplares, sendo que todos os seus livros são bestsellers.", FotoNome = "RaulMinhalma.jpg", Nacionalidade = "Portuguesa", DataNascimento = new DateOnly(1992, 5, 6)},
                new Autor { Nome = "Alexei Navalny", Biografia = "Alexei Navalny foi um líder da oposição russa, político, ativista anticorrupção e preso político que granjeou reconhecimento e respeito internacionais. As suas muitas distinções internacionais incluem o Prémio Sakharov, atribuído anualmente pelo Parlamento Europeu em prol dos Direitos Humanos, e o Prémio Dresden da Paz, entre outros. Morreu em 2024.", FotoNome = "AlexeiNavalny.webp", Nacionalidade = "Russa", DataNascimento = new DateOnly(1976, 6, 4)},
                new Autor { Nome = "Yuval Noah Harari", Biografia = "Yuval Noah Harari é historiador, investigador e professor de História do Mundo na Universidade Hebraica de Jerusalém, considerada uma das melhores instituições de ensino a nível internacional. Doutorado em História pela Universidade de Oxford, Harari tem-se dedicado ao estudo e ensino da História, encorajando os seus alunos a questionar os conhecimentos e ideias que têm por garantidos sobre a vida, o mundo e a humanidade. Harari foi duas vezes vencedor do Prémio Polonski para Criatividade e Originalidade nas Disciplinas de Humanidades, em 2009 e 2012. É autor de inúmeros artigos científicos e de vários livros, entre eles 'Sapiens: História Breve da Humanidade' (2013), 'Homo Deus: História Breve do Amanhã' (2017), e '21 Lições para o Século XXI' (2018), todos bestsellers internacionais traduzidos em múltiplas línguas e recomendados por figuras como Barack Obama e Bill Gates.", FotoNome = "YuvalNoahHarari.webp", Nacionalidade = "Israelita", DataNascimento = new DateOnly(1976, 2, 24) },
                new Autor { Nome = "James A. Robinson", Biografia = "James Alan Robinson é um economista e cientista político britânico-americano. Ele é o Professor Rev. Dr. Richard L. Pearson de Estudos de Conflito Global e Professor Universitário na Harris School of Public Policy da Universidade de Chicago, onde também dirige o Instituto Pearson para o Estudo e Resolução de Conflitos Globais. Anteriormente, lecionou na Universidade de Harvard de 2004 a 2015. Robinson é coautor de vários livros com Daron Acemoglu, incluindo 'Why Nations Fail' e 'The Narrow Corridor'. Em 2024, Robinson, Acemoglu e Simon Johnson foram premiados com o Prêmio Nobel Memorial em Ciências Econômicas por seus estudos comparativos sobre prosperidade entre nações.", FotoNome = "JamesARobinson.webp", Nacionalidade = "Britânico-Americano", DataNascimento = new DateOnly(1960, 1, 1) },
                new Autor { Nome = "Sarah J. Maas", Biografia = "Sarah Janet Maas, conhecida como Sarah J. Maas, é uma autora americana de fantasia, nascida em 5 de março de 1986. Ela é famosa por suas séries de fantasia *Throne of Glass*, *A Court of Thorns and Roses*, e *Crescent City*. Até 2024, ela vendeu quase 40 milhões de cópias de seus livros, que foram traduzidos para 38 idiomas. Criada no Upper West Side de Manhattan, Maas formou-se magna cum laude em escrita criativa pela Hamilton College em Nova York. Ela começou a escrever seu romance de estreia, *Throne of Glass*, aos dezesseis anos, inspirando-se na história da Cinderela. Além disso, sua série *A Court of Thorns and Roses* é uma releitura solta de 'A Bela e a Fera'.", FotoNome = "SarahJMaas.webp", Nacionalidade = "Americana", DataNascimento = new DateOnly(1986, 3, 5) },
                new Autor { Nome = "Bob Woodward", Biografia = "Robert Upshur Woodward, nascido em 26 de março de 1943, é um jornalista investigativo americano. Ele começou a trabalhar para o *The Washington Post* em 1971 e ganhou destaque ao lado de Carl Bernstein por sua cobertura do escândalo Watergate, que levou à renúncia do presidente Richard Nixon. Woodward escreveu 21 livros sobre política americana e assuntos atuais, 14 dos quais foram best-sellers. Ele é amplamente reconhecido como um dos maiores repórteres investigativos de sua geração.", FotoNome = "BobWoodward.jpg", Nacionalidade = "Americano", DataNascimento = new DateOnly(1943, 3, 26) },
                new Autor { Nome = "Fiódor Dostoiévski", Biografia = "Fiódor Mikhailovitch Dostoiévski, nascido em 11 de novembro de 1821 em Moscou, foi um renomado escritor, filósofo e jornalista russo. É amplamente considerado um dos maiores romancistas da literatura mundial, conhecido por explorar temas como a moralidade, a liberdade e a condição humana. Após ser preso por atividades políticas, passou quatro anos na Sibéria, experiência que influenciou profundamente sua obra. Entre seus romances mais famosos estão *Crime e Castigo*, *O Idiota* e *Os Irmãos Karamazov*. Dostoiévski faleceu em 9 de fevereiro de 1881, em São Petersburgo, vítima de uma hemorragia pulmonar.", FotoNome = "FiodorDostoievski.webp", Nacionalidade = "Russo", DataNascimento = new DateOnly(1821, 11, 11), DataFalecimento = new DateOnly(1881, 2, 9) },
                new Autor { Nome = "Laurie Gilmore", Biografia = "Laurie Gilmore é uma autora contemporânea conhecida por suas obras que exploram temas de autodescoberta e a luta pela realização pessoal. Com um estilo envolvente, Gilmore captura as complexidades da vida moderna, como evidenciado em seu livro *The Pumpkin Spice Café*, onde os personagens enfrentam questões sobre identidade e propósito. Até o momento, ela possui 7 obras publicadas e acumulou mais de 132 mil avaliações e quase 20 mil resenhas em plataformas literárias.", FotoNome = "LaurieGilmore.jpg", Nacionalidade = "Americana", DataNascimento = new DateOnly(1980, 11, 15) },
                new Autor { Nome = "Lynn Painter", Biografia = "Lynn Painter é uma autora americana conhecida por seus romances contemporâneos e comédias românticas. Ela é autora de vários best-sellers, incluindo *Mr. Wrong Number* e *The Do-Over*, que conquistaram leitores com suas histórias divertidas e personagens cativantes. Lynn é apaixonada por contar histórias que exploram o amor, a amizade e as reviravoltas da vida. Além de escrever, ela também é uma entusiasta de viagens e adora passar tempo com sua família.", FotoNome = "LynnPainter.webp", Nacionalidade = "Americana", DataNascimento = new DateOnly(1982, 8, 15) },
                new Autor { Nome = "Sally Rooney", Biografia = "Sally Rooney, nascida em 20 de fevereiro de 1991, em Castlebar, Condado de Mayo, Irlanda, é uma escritora e roteirista irlandesa. Ela é amplamente reconhecida por seus romances contemporâneos, incluindo *Conversas entre amigos* (2017) e *Pessoas Normais* (2018), o último dos quais foi adaptado para uma série de televisão de sucesso. Rooney é considerada uma das principais vozes da geração millennial e ganhou diversos prêmios, incluindo o Jovem Escritor do Ano pelo Sunday Times em 2017. Seu trabalho explora temas de relacionamentos e a complexidade das interações humanas.", FotoNome = "SallyRooney.webp", Nacionalidade = "Irlandesa", DataNascimento = new DateOnly(1991, 2, 20) },
                new Autor { Nome = "Mario Benedetti", Biografia = "Mario Benedetti, nascido em 14 de setembro de 1920 em Paso de Los Toros, Tacuarembó, Uruguai, foi um renomado poeta, escritor e ensaísta uruguaio. Membro da Geração de 45, Benedetti é considerado um dos principais escritores do Uruguai e da literatura latino-americana. Sua carreira literária começou em 1949, e ele ganhou destaque em 1956 com a publicação de *Poemas de Oficina*. Ao longo de sua vida, escreveu mais de 80 livros, incluindo romances, contos e ensaios. Benedetti faleceu em 17 de maio de 2009, em Montevidéu, aos 88 anos.", FotoNome = "MarioBenedetti.webp", Nacionalidade = "Uruguaio", DataNascimento = new DateOnly(1920, 9, 14), DataFalecimento = new DateOnly(2009, 5, 17) },
                new Autor { Nome = "Robert Kiyosaki", Biografia = "Robert Toru Kiyosaki, nascido em 8 de abril de 1947 em Hilo, Havai, é um empresário, investidor e autor americano, amplamente conhecido por seu livro *Pai Rico, Pai Pobre*, publicado em 1997. A obra se tornou um best-seller mundial, vendendo mais de 27 milhões de cópias e sendo traduzida para 51 idiomas. Kiyosaki é defensor da educação financeira e acredita que o conhecimento sobre investimentos deve ser ensinado desde cedo. Ele fundou a Rich Dad Company e Cashflow Technologies, onde desenvolveu jogos educativos sobre finanças pessoais. Apesar de seu sucesso, Kiyosaki enfrentou controvérsias e desafios financeiros ao longo de sua carreira, incluindo a falência de uma de suas empresas em 2012.", FotoNome = "RobertKiyosaki.webp", Nacionalidade = "Americano", DataNascimento = new DateOnly(1947, 4, 8) },
                new Autor { Nome = "Alice Kellen", Biografia = "Alice Kellen, nascida em 1989 em Valência, Espanha, é uma escritora espanhola especializada em literatura romântica juvenil e adulta. Ela iniciou sua carreira literária em 2013 com o romance *Llévame a cualquier lugar*, que rapidamente se tornou um sucesso de vendas. Sob o nome verdadeiro de Silvia Hervás, Kellen optou por um pseudônimo inspirado em *Alice no País das Maravilhas*. Desde então, publicou cerca de quinze livros e se destacou por sua habilidade em criar histórias envolventes que exploram temas de amor e autodescoberta. Suas obras foram traduzidas para várias línguas e ela tem sido reconhecida como uma voz importante na literatura contemporânea.", FotoNome = "AliceKellen.jpg", Nacionalidade = "Espanhola", DataNascimento = new DateOnly(1989, 1, 1) },
                new Autor { Nome = "Isabel Allende", Biografia = "Isabel Allende Llona, nascida em 2 de agosto de 1942 em Lima, Peru, é uma escritora chilena/norte-americana reconhecida por suas obras que exploram temas de amor, política e história. É autora de best-sellers como *A Casa dos Espíritos* (1982), que se tornou um marco da literatura latino-americana e foi adaptado para o cinema. Allende também escreveu *Paula* (1994), um relato autobiográfico sobre sua filha que estava em coma. Com uma carreira marcada por prêmios, incluindo o Prêmio Nacional de Literatura do Chile (2010) e a Medalha Presidencial da Liberdade (2014), Allende é uma das vozes mais influentes da literatura contemporânea. Atualmente, reside na Califórnia, EUA.", FotoNome = "IsabelAllende.webp", Nacionalidade = "Chilena; Norte-Americana", DataNascimento = new DateOnly(1942, 8, 2) },
                new Autor { Nome = "Arturo Pérez-Reverte", Biografia = "Arturo Pérez-Reverte, nascido em 24 de novembro de 1951 em Cartagena, Espanha, é um renomado novelista e jornalista espanhol. Formado em Jornalismo pela Universidade Complutense de Madrid, ele trabalhou como repórter de guerra por 21 anos, cobrindo conflitos em diversas regiões do mundo. Desde meados da década de 1990, Pérez-Reverte se dedica exclusivamente à escrita e é conhecido por suas obras como *A rainha do Sul* e a série *Capitão Alatriste*. Em 2003, tornou-se membro da Real Academia Espanhola. Suas histórias frequentemente exploram temas como aventura, moralidade e a complexidade da condição humana.", FotoNome = "ArturoPérezReverte.webp", Nacionalidade = "Espanhol", DataNascimento = new DateOnly(1951, 11, 24) },
                new Autor { Nome = "Almudena Grandes", Biografia = "Almudena Grandes Hernández, nascida em 7 de maio de 1960 em Madrid, Espanha, foi uma escritora, roteirista e jornalista espanhola. Estudou Geografia e História na Universidade Complutense de Madrid. Em 1989, ganhou o Prêmio La Sonrisa Vertical pela novela erótica *Las edades de Lulú*, que foi adaptada para o cinema. Suas obras frequentemente exploram a sociedade espanhola após a ditadura franquista, com um forte enfoque em realismo e introspecção psicológica. Grandes faleceu em 27 de novembro de 2021, em Madrid, devido a um câncer colorretal.", FotoNome = "AlmudenaGrandes.webp", Nacionalidade = "Espanhola", DataNascimento = new DateOnly(1960, 5, 7), DataFalecimento = new DateOnly(2021, 11, 27) },
                new Autor { Nome = "Jordi Sierra i Fabra", Biografia = "Jordi Sierra i Fabra, nascido em 26 de julho de 1947 em Barcelona, Espanha, é um escritor e jornalista espanhol, amplamente reconhecido por suas contribuições à literatura infantil e juvenil. Desde 1975, ele publicou mais de 527 livros que foram traduzidos para várias línguas e adaptados para o cinema e a televisão. Sua obra mais notável, *Campos de fresas*, é um best-seller juvenil. Além de escritor, Sierra i Fabra é um estudioso da música pop e co-fundador da revista *Super Pop*. Em 2012, ele lançou suas memórias literárias, *Mis (primeros) 400 libros*. Ele também fundou a Fundação Jordi Sierra i Fabra, que apoia jovens escritores e promove a leitura.", FotoNome = "JordiSierraiFabra.webp", Nacionalidade = "Espanhol", DataNascimento = new DateOnly(1947, 7, 26) },
                new Autor { Nome = "Mario Vargas Llosa", Biografia = "Jorge Mario Pedro Vargas Llosa, nascido em 28 de março de 1936 em Arequipa, Peru, é um renomado escritor, jornalista e político peruano. Considerado um dos mais importantes romancistas da América Latina, Vargas Llosa ganhou notoriedade internacional com obras como *A Cidade e os Cachorros* (1963), *A Casa Verde* (1966) e *Conversação na Catedral* (1969). Em 2010, foi agraciado com o Prêmio Nobel de Literatura por sua 'cartografia de estruturas de poder' e suas 'imagens vigorosas sobre a resistência, revolta e derrota individual'. Além de sua carreira literária, Vargas Llosa também se envolveu na política, candidatando-se à presidência do Peru em 1990. Ele é membro da Real Academia Espanhola desde 1994 e detém o título de Marquês de Vargas Llosa desde 2011.", FotoNome = "MarioVargasLlosa.webp", Nacionalidade = "Peruano", DataNascimento = new DateOnly(1936, 3, 28) },
                new Autor { Nome = "María Dueñas", Biografia = "María Dueñas Fernández, nascida em 4 de dezembro de 2002 em Granada, Espanha, é uma violinista e compositora espanhola de destaque. Em 2021, ela conquistou o primeiro prêmio na Competição Yehudi Menuhin, na categoria sênior, e é considerada uma das violinistas mais promissoras de sua geração. Desde jovem, Dueñas recebeu apoio familiar para sua formação musical, iniciando seus estudos no Conservatório Ángel Barrios aos sete anos. Ela também estudou em Dresden e Viena. Além de sua carreira como solista com orquestras renomadas, Dueñas é compositora e fundadora do Hamamelis Quartett. Em 2023, recebeu o Prêmio Luitpold do festival Kissinger Sommer.", FotoNome = "MariaDuenas.webp", Nacionalidade = "Espanhola", DataNascimento = new DateOnly(2002, 12, 4) },
                new Autor { Nome = "Annie Ernaux", Biografia = "Annie Ernaux, nascida Annie Duchesne em 1 de setembro de 1940 em Lillebonne, França, é uma escritora e professora francesa. Conhecida por sua obra literária autobiográfica, Ernaux explora questões de classe, gênero e memória em um estilo claro e incisivo. Ela se destacou na literatura com obras como *Les Armoires Vides* (1974), *La Place* (1983) e *Les Années* (2008), que receberam diversos prêmios literários. Em 2022, foi laureada com o Prêmio Nobel de Literatura, tornando-se a primeira mulher francesa a receber essa honraria. Sua escrita é frequentemente descrita como uma radiografia da intimidade feminina e das mudanças sociais na França do pós-guerra.", FotoNome = "AnnieErnaux.webp", Nacionalidade = "Francesa", DataNascimento = new DateOnly(1940, 9, 1) },
                new Autor { Nome = "Jean-Baptiste Andrea", Biografia = "Jean-Baptiste Andrea, nascido em 4 de abril de 1971 em Saint-Germain-en-Laye, França, é um escritor, roteirista e diretor francês. Ele tem origens italianas, gregas e pied-noir da Argélia. Formado pela Institut d'études politiques de Paris e pela ESCP, Andrea começou sua carreira no cinema antes de se dedicar à literatura. Seu primeiro romance, *Ma reine*, publicado em 2017, recebeu diversos prêmios, incluindo o Prix Femina des lycéens. Seus trabalhos subsequentes, como *Cent millions d'années et un jour* e *Des diables et des saints*, também foram aclamados. Em 2023, ele ganhou o prestigioso Prix Goncourt por seu quarto romance, *Veiller sur elle*, que aborda temas como o fascismo e a luta pela liberdade na Itália entre guerras.", FotoNome = "JeanBaptisteAndrea.webp", Nacionalidade = "Francês", DataNascimento = new DateOnly(1971, 4, 4) },
                new Autor { Nome = "Valérie Perrin", Biografia = "Valérie Perrin, nascida em 19 de janeiro de 1967 em Remiremont, França, é uma escritora, roteirista e fotógrafa francesa. Ela é conhecida por seus romances, incluindo *Les Oubliés du dimanche* (2015), traduzido como *Os Esquecidos de Domingo*, e *Changer l'eau des fleurs* (2018), conhecido em português como *A Breve Vida das Flores*. Além de sua carreira literária, Perrin também trabalhou como roteirista em filmes como *Salaud, on t'aime* (2014) e *Un plus une* (2015). Seus livros foram traduzidos para mais de 30 idiomas e receberam diversos prêmios literários, incluindo o National Lion's Prize for Literature em 2016.", FotoNome = "ValeriePerrin.webp", Nacionalidade = "Francesa", DataNascimento = new DateOnly(1967, 1, 19) },
                new Autor { Nome = "Sophia de Mello Breyner Andresen", Biografia = "Sophia de Mello Breyner Andresen, nascida em 6 de novembro de 1919 no Porto, Portugal, e falecida em 2 de julho de 2004 em Lisboa, foi uma das mais importantes poetisas portuguesas do século XX. Ela foi a primeira mulher a receber o Prémio Camões em 1999, o mais prestigioso prêmio literário da língua portuguesa. Sua obra inclui poesia, contos e literatura infantil, destacando-se pela sensibilidade e pela forte ligação ao mar e à natureza. Entre suas obras mais conhecidas estão *Poesia* (1944), *Mar Novo* (1958) e *A Menina do Mar* (1958). Sophia também foi tradutora e membro da Academia das Ciências de Lisboa. Seu corpo foi trasladado para o Panteão Nacional em 2014, em reconhecimento à sua contribuição à literatura portuguesa.", FotoNome = "SophiaDeMelloBreynerAndresen.webp", Nacionalidade = "Portuguesa", DataNascimento = new DateOnly(1919, 11, 6), DataFalecimento = new DateOnly(2004, 7, 2)},
                new Autor { Nome = "José Saramago", Biografia = "José de Sousa Saramago nasceu a 16 de novembro de 1922, na Azinhaga, Golegã, Portugal, e faleceu a 18 de junho de 2010, em Tías, Lanzarote, Espanha. Foi um escritor, jornalista e poeta português, reconhecido internacionalmente pela sua obra literária. Em 1998, recebeu o Prémio Nobel de Literatura, tornando-se o primeiro autor de língua portuguesa a ser laureado com este prémio. Entre as suas obras mais destacadas estão 'Memorial do Convento' (1982), 'O Evangelho Segundo Jesus Cristo' (1991) e 'Ensaio sobre a Cegueira' (1995). Saramago foi também membro do Partido Comunista Português e viveu os últimos anos da sua vida em Lanzarote, nas Ilhas Canárias.", FotoNome = "JoseSaramago.webp", Nacionalidade = "Portuguesa", DataNascimento = new DateOnly(1922, 11, 16), DataFalecimento = new DateOnly(2010, 6, 18) },
                new Autor { Nome = "William Shakespeare", Biografia = "William Shakespeare, nascido em 26 de abril de 1564 em Stratford-upon-Avon, Inglaterra, e falecido em 23 de abril de 1616, foi um poeta, dramaturgo e ator inglês, amplamente considerado como o maior escritor da língua inglesa e um dos mais influentes dramaturgos do mundo. Entre suas obras mais conhecidas estão 'Hamlet', 'Romeu e Julieta', 'Macbeth' e 'Otelo'. Shakespeare também escreveu 154 sonetos e diversos poemas narrativos. Suas peças foram traduzidas para todas as principais línguas e são encenadas mais frequentemente do que as de qualquer outro dramaturgo.", FotoNome = "WilliamShakespeare.webp", Nacionalidade = "Inglesa", DataNascimento = new DateOnly(1564, 4, 26), DataFalecimento = new DateOnly(1616, 4, 23) }
            };


            _context.Autor.AddRange(autores);
            _context.SaveChanges();

            // Criação dos utilizadores
            var utilizadores = new List<Perfil>
            {
                new Perfil
                {
                    UserName = "carlos@example.com",
                    Email = "carlos@example.com",
                    PhoneNumber = "+351912345679",
                    Morada = "Rua da Poesia, 100",
                    EstadoAtivacao = "Ativo",
                    FotoNome = "userDefault.png"
                },
                new Perfil
                {
                    UserName = "machado@example.com",
                    Email = "machado@example.com",
                    PhoneNumber = "+351912345678",
                    Morada = "Rua da Poesia, 100",
                    EstadoAtivacao = "PorAtivar",
                    FotoNome = "userDefault.png"
                }
            };

            var userExist = await _userManager.FindByEmailAsync(utilizadores[0].Email);
            if (userExist == null)
            {
                var result = await _userManager.CreateAsync(utilizadores[0], "Password123!"); // Define a senha padrão
                if (result.Succeeded)
                {
                    // Atribui o papel (role) Leitor ao utilizador
                    await _userManager.AddToRoleAsync(utilizadores[0], "Leitor");
                }
            }

            userExist = await _userManager.FindByEmailAsync(utilizadores[1].Email);
            if (userExist == null)
            {
                var result = await _userManager.CreateAsync(utilizadores[1], "Password123!"); // Define a senha padrão
                if (result.Succeeded)
                {
                    // Atribui o papel (role) Leitor ao utilizador
                    await _userManager.AddToRoleAsync(utilizadores[1], "Bibliotecario");
                }
            }
            //inicializacao da tabela de livros
            var livros = new List<Livro>
            {
                 // Dom Casmurro - Machado de Assis
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id,
                    AutorId = autores[1].Id,
                    ISBN = "9788535914849",
                    Dimensoes = "21 x 14 x 1 mm",
                    NumeroPaginas = 208,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "Dom Casmurro",
                    FotoNome = "DomCasmurro.webp",
                    DataInsercao = new DateTime(2024, 10, 18),
                    Cliques = 0,
                    Sinopse = "Dom Casmurro conta a história de Bento Santiago (Bentinho), apelidado de Dom Casmurro por ser calado e introvertido. Na adolescência, apaixona-se por Capítu, abandonando o seminário e, com ele, os desígnios traçados por sua mãe, Dona Glória, para que se tornasse padre. Casam-se e tudo corre bem, até o amor se tornar ciúme e desconfiança. É esta a grande questão que magistralmente Dom Casmurro expõe ao longo de 148 capítulos: a dúvida que paira ao longo de toda a obra sobre a possibilidade de traição de Capítu, agravada pela extraordinária semelhança do filho de ambos, Ezequiel, com o grande amigo de Bentinho, Escobar."
                },
                // Antologia Poética - Carlos Drummond de Andrade
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[0].Id,
                    AutorId = autores[0].Id,
                    ISBN = "9788535921182",
                    Dimensoes = "23 x 16 x 2 mm",
                    NumeroPaginas = 256,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 3,
                    NumeroExemplaresTotal = 3,
                    Titulo = "Antologia Poética",
                    FotoNome = "AntologiaPoetica.webp",
                    DataInsercao = new DateTime(2024, 10, 19),
                    Cliques = 0,
                    Sinopse = "Esta Antologia poética reúne cinquenta anos de poesia de um homem cujo espírito apaixonado e irreverente, como poeta, dramaturgo e letrista, determinou um «antes» e um «depois» de Vinicius. A acompanhar os poemas, um belo caderno de imagens do poeta, assim como reproduções de manuscritos e documentos raros."
                },
                // Capitães da Areia - Jorge Amado
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id,
                    AutorId = autores[2].Id,
                    ISBN = "9788574066944",
                    Dimensoes = "24 x 17 x 2 mm",
                    NumeroPaginas = 300,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 4,
                    NumeroExemplaresTotal = 4,
                    Titulo = "Capitães da Areia",
                    FotoNome = "CapitaesAreia.webp",
                    DataInsercao = new DateTime(2024, 10, 21),
                    Cliques = 0,
                    Sinopse = "Pedro Bala, Professor, Gato, Sem Pernas e Boa Vida são adolescentes abandonados por suas famílias, que crescem nas ruas de Salvador e vivem em comunidade no Trapiche. Eles praticam uma série de assaltos e são constantemente perseguidos pela polícia. Um dia, Professor conhece Dora e seu irmão Zé Fuinha e os leva até o Trapiche, o que desencadeia a excitação dos demais garotos, que não estão acostumados à presença de uma mulher no local. Aos poucos, nasce o afeto entre o líder do grupo e a jovem."
                },
                // A Professora - Freida McFadden
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[4].Id,
                    AutorId = autores[3].Id,
                    ISBN = "9789895702978",
                    Dimensoes = "23 x 15 x 2 cm",
                    NumeroPaginas = 352,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 4,
                    NumeroExemplaresTotal = 4,
                    Titulo = "A Professora",
                    FotoNome = "AProfessora.webp",
                    DataInsercao = new DateTime(2024, 10, 21),
                    Cliques = 0,
                    Sinopse = "Eve tem uma vida boa. Levanta-se todos os dias de manhãzinha, recebe um beijo do seu marido, Nate, e vai dar aulas de matemática na escola secundária da cidade em que vivem. Tudo é perfeito, tal como deve ser. Exceto que…\r\n\r\nNo ano anterior, um escândalo abalou a Escola Secundária de Caseham. No centro de toda a polémica, estava Addie, uma aluna, sobre a qual corriam rumores de que tinha um caso com um dos professores. Eve tem a certeza de que, por trás do escândalo, se esconde uma verdade bem mais sombria. \r\n\r\nAddie não é de confiança. A rapariga mente. Magoa as pessoas. Destrói vidas. Pelo menos, é o que toda a gente diz.\r\n\r\nPorém, ninguém conhece a verdadeira Addie. Os segredos que guarda poderão vir a arruiná-la, por isso fará de tudo para que as coisas se mantenham exatamente como estão."
                },
                // A Rapariga que Mataste - Leslie Wolfe
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[4].Id,
                    AutorId = autores[4].Id,
                    ISBN = "9789895702794",
                    Dimensoes = "23 x 15 x 2 cm",
                    NumeroPaginas = 360,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 4,
                    NumeroExemplaresTotal = 4,
                    Titulo = "A Rapariga que Mataste",
                    FotoNome = "ARaparigaQueMataste.webp",
                    DataInsercao = new DateTime(2024, 10, 21),
                    Cliques = 0,
                    Sinopse = "Andrea e Craig tinham a vida quase perfeita. Acabadinhos de se mudar para um dos subúrbios mais desejados de Houston, planeavam desfrutar da nova casa e do bairro onde viviam.\r\n\r\nPorém, alguns meses mais tarde, Craig vê-se preso numa teia intrincada e acusado do assassínio da sua jovem esposa. Assassino ou vítima? As opiniões dividem-se, e o julgamento polariza a cidade. O caso, altamente mediatizado, alimenta a imprensa e as redes sociais.\r\n\r\nO seu casamento está na boca do mundo e é exposto e esmiuçado ao detalhe. Mas a névoa não se dissipa. A vida de Andrea continua um mistério que nem a polícia consegue resolver. Seria ela a esposa feliz e dedicada que todos julgavam ser? Ou era tudo uma grande mentira?"
                },
                // Segredo Milionário - T L Swan
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id,
                    AutorId = autores[5].Id,
                    ISBN = "9789895702817",
                    Dimensoes = "23 x 15 x 2 cm",
                    NumeroPaginas = 448,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 4,
                    NumeroExemplaresTotal = 4,
                    Titulo = "Segredo Milionário",
                    FotoNome = "SegredoMilionario.webp",
                    DataInsercao = new DateTime(2024, 10, 21),
                    Cliques = 0,
                    Sinopse = "Christopher Miles tem tudo aquilo que um homem pode desejar: dinheiro, poder, influência, carros, mulheres, luxos, mordomias, mas… porque é que se sentia tão vazio por dentro? \r\n\r\nEm busca de se encontrar a si mesmo e de viver uma vida com mais propósito, Christopher decide tirar férias da sua vida privilegiada e passar um ano inteiro a viajar de mochila às costas pela Europa.\r\n\r\nUma nova identidade, sem contactos e sem dinheiro. «Não é um mau plano», pensa ele. Até lá chegar. Ao instalar-se num hostel sobrelotado – cheio de odores corporais e muita cerveja –, começa a pôr em causa se terá sido uma boa decisão.\r\n\r\nMesmo assim, no meio de todo aquele caos, conhece Hayden Whitmore, que dorme na cama em frente à sua. Ela é bonita, inteligente e inocente… bem diferente de outras mulheres com quem ele se envolveu. "
                },
                // A Noiva Errada - Catharina Maura
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id,
                    AutorId = autores[6].Id,
                    ISBN = "9789895702688",
                    Dimensoes = "23 x 15 x 2 cm",
                    NumeroPaginas = 320,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 4,
                    NumeroExemplaresTotal = 4,
                    Titulo = "A Noiva Errada",
                    FotoNome = "ANoivaErrada.webp",
                    DataInsercao = new DateTime(2024, 10, 21),
                    Cliques = 0,
                    Sinopse = "Ares Windsor é rico e poderoso. O seu casamento com a filha mais velha dos DuPont, Hannah, foi combinado há já muito tempo. Contudo, Hannah tem sempre procurado adiar o enlace. \r\n\r\nAmbas as famílias são influentes e poderosas. A mãe de Hannah é extremamente narcisista. A avó de Ares, implacável. Como não pretendem esperar mais tempo pelo matrimónio, decidem encontrar outra noiva para o jovem Windsor. A escolhida é Raven, a irmã mais nova de Hannah, amiga de Ares e secretamente apaixonada por ele há anos. \r\n\r\nRaven não tem como escapar aos planos da família. Vê-se obrigada a casar com o noivo da irmã, porém, é assaltada por imensas dúvidas: Poderá ela assumir o lugar de Hannah? Conseguirá que Ares esqueça a irmã e se apaixone por ela? Que tipo de casamento será o deles? Será possível ao verdadeiro amor florescer no seio de um casamento arranjado? "
                },
                // A Criada Está a Ver - Freida McFadden
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[4].Id,
                    AutorId = autores[3].Id,
                    ISBN = "9789895702756",
                    Dimensoes = "23 x 15 x 2 cm",
                    NumeroPaginas = 360,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 4,
                    NumeroExemplaresTotal = 4,
                    Titulo = "A Criada Está a Ver",
                    FotoNome = "ACriadaEstaaVer.webp",
                    DataInsercao = new DateTime(2024, 10, 21),
                    Cliques = 0,
                    Sinopse = "A Sra. Lowell transborda simpatia ao acenar-me através da cerca que separa as nossas casas. “Devem ser os nossos novos vizinhos!” Agarro na mão da minha filha e sorrio de volta. No entanto, assim que vê o meu marido, uma expressão estranha atravessa-lhe o rosto. \r\n\r\nMillie, a memorável protagonista dos bestsellers A Criada e O Segredo da Criada, está de volta! \r\n\r\nEu costumava limpar a casa de outras pessoas. Nem posso acreditar que esta casa é realmente minha... Apesar de ter ficado de pé atrás com a Suzette Lowell, quando ela nos convida para jantar em sua casa, decido que é uma boa oportunidade para fazer amizade. Mas o olhar frio da empregada, quando nos abre a porta… dá-me arrepios. \r\n\r\nE a empregada dos Lowell não é a coisa mais estranha desta rua. Sinto uma figura sombria a observar-nos frequentemente, e o meu marido começa a sair de casa tarde, a meio da noite. Além disso, a mulher que vive do outro lado da rua cruza-se comigo, e as suas palavras arrepiam-me até aos ossos: “Tenham cuidado com os vossos vizinhos.” \r\n\r\nEu e o meu marido poupámos durante anos para dar aos nossos filhos a vida que merecem. Pensei que o passado tinha ficado para trás… será que cometemos um erro ao mudarmo-nos para aqui? "
                },
                // Sem Ti Não Vais a Lado Nenhum - Manuel Clemente
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[8].Id,
                    AutorId = autores[7].Id,
                    ISBN = "9789895702664",
                    Dimensoes = "23 x 15 x 2 cm",
                    NumeroPaginas = 232,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 4,
                    NumeroExemplaresTotal = 4,
                    Titulo = "Sem Ti Não Vais a Lado Nenhum",
                    FotoNome = "SemTiNaoVaisaLadoNenhum.webp",
                    DataInsercao = new DateTime(2024, 10, 21),
                    Cliques = 0,
                    Sinopse = "Vivemos em sentido contrário. Não estamos nos lugares que nos fazem sentir bem. Desperdiçamos horas intermináveis com atividades que não nos preenchem, rodeados de pessoas que nada nos acrescentam. Reprimimos o que sentimos, diariamente, para conseguir encaixar numa realidade que não coincide connosco. \r\n\r\nDurante quanto tempo é possível vivermos desfasados de nós próprios, encolhidos e sem entusiasmo? Quanta frustração temos de suportar até decidir mudar? Quantos sustos"
                },
                // O Rapaz - Cláudio Ramos
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id,
                    AutorId = autores[8].Id,
                    ISBN = "9789895702398",
                    Dimensoes = "23 x 15 x 2 cm",
                    NumeroPaginas = 320,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 4,
                    NumeroExemplaresTotal = 4,
                    Titulo = "O Rapaz",
                    FotoNome = "ORapaz.webp",
                    DataInsercao = new DateTime(2024, 10, 21),
                    Cliques = 0,
                    Sinopse = "Sou dos que acreditam que se pode amar uma pessoa a vida toda e toda a vida pode até ser pequena para tanto amor. Não temos de amar todos na mesma medida, nem tomar o pulso ao que os outros sentem. Vamos aprendendo com a idade que o amor é só isso e nada mais. Não o devemos exibir, complicar, humilhar, amachucar de forma a chamar a atenção para outra coisa qualquer. O amor é só amor e qualquer história de amor conta isso mesmo.\r\n\r\nA história deste livro é a de uma paixão vivida fora de tempo, talvez fora de horas, por duas pessoas que se encontram no lugar certo, mas no tempo errado."
                },
                // Proposta Irrecusável - T L Swan
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id,
                    AutorId = autores[5].Id,
                    ISBN = "9789895702818",
                    Dimensoes = "23 x 15 x 2 cm",
                    NumeroPaginas = 450,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 4,
                    NumeroExemplaresTotal = 4,
                    Titulo = "Proposta Irrecusável",
                    FotoNome = "PropostaIrrecusavel.webp",
                    DataInsercao = new DateTime(2024, 10, 21),
                    Cliques = 0,
                    Sinopse = "Conheci o Tristan Miles numa reunião em que me fez uma proposta para comprar a empresa do meu falecido marido. Ele é poderoso, arrogante e irritantemente lindo. Representa tudo o que odeio no mundo dos negócios. E detestei-o com todas as células do meu corpo. Recusei firmemente a sua proposta. \r\n\r\nPara grande surpresa minha, ligou novamente dias depois e convidou-me para jantar. Preferia morrer a aceitar sair com um homem como o Tristan – embora tenha de admitir que o convite foi ótimo para o meu ego. Recusá-lo acabou por ser o único ponto alto de um ano que estava a ser terrível. "
                },
                // O Escritório - Freida McFadden
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[4].Id,
                    AutorId = autores[3].Id,
                    ISBN = "9789895702801",
                    Dimensoes = "23 x 15 x 2 cm",
                    NumeroPaginas = 370,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 4,
                    NumeroExemplaresTotal = 4,
                    Titulo = "O Escritório",
                    FotoNome = "OEscritorio.webp",
                    DataInsercao = new DateTime(2024, 10, 21),
                    Cliques = 0,
                    Sinopse = "Dawn Schiff é uma mulher estranha. Pelo menos, é o que toda a gente pensa na Vixed, a empresa de suplementos nutricionais onde trabalha como contabilista. Dawn nunca diz a coisa certa. Não tem amigos. E senta-se todos os dias à secretária, para trabalhar, precisamente às 8h45 da manhã.\r\n\r\nTalvez seja por isso que, certa manhã, quando Dawn não aparece para trabalhar, a sua colega Natalie Farrell – bonita, popular e a melhor vendedora da empresa há cinco anos consecutivos – se surpreenda. E mais ainda quando o telefone de Dawn toca e alguém do outro lado da linha diz apenas «Socorro».\r\n\r\nAquele telefonema alterou tudo… afinal, nada liga tanto duas pessoas como partilhar um segredo. E agora Natalie está irrevogavelmente ligada a Dawn e vê-se envolvida num jogo do gato e do rato. Parece que Dawn não era simplesmente uma pessoa estranha, antissocial e desajeitada, mas estava a ser perseguida por alguém próximo. \r\n\r\nÀ medida que o mistério se adensa, Natalie não consegue deixar de se questionar: afinal, quem é a verdadeira vítima? Mas uma coisa é clara: alguém odiava Dawn Schiff. O suficiente para a matar. "
                },
                // Reckless - Elsie Silver
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id,
                    AutorId = autores[9].Id, 
                    ISBN = "9781738844739",
                    Dimensoes = "20.32 x 12.70 x 2.92 cm",
                    NumeroPaginas = 464,
                    Idioma = "Inglês", 
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5, 
                    NumeroExemplaresTotal = 5,
                    Titulo = "Reckless",
                    FotoNome = "Reckless.webp", 
                    DataInsercao = new DateTime(2024, 10, 29), 
                    Cliques = 0,
                    Sinopse = "Theo Silva é um vaqueiro indomável e notório conquistador. Ele é irresistível e persistente, derretendo as defesas de Winter Hamilton, que tenta se afastar após um casamento tóxico. A história explora a tentação e a complexidade das relações, com Winter encontrando redenção e crescimento pessoal."
                },

                // Adição do livro Azul até ao Fim - Jorge Nuno Pinto da Costa
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[5].Id, 
                    AutorId = autores[10].Id, 
                    ISBN = "9789896664831", 
                    Dimensoes = "149 x 236 x 15 mm", 
                    NumeroPaginas = 216, 
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5, 
                    NumeroExemplaresTotal = 5, 
                    Titulo = "Azul até ao Fim",
                    FotoNome = "AzulAteAoFim.webp", 
                    DataInsercao = new DateTime(2024, 10, 29), 
                    Cliques = 0,
                    Sinopse = "«O primeiro dos meus últimos dias». Foi este o meu pensamento, quando, em resposta a uma pergunta direta que fiz ao Dr. Lindoro, ele me disse: «Sim, o tumor que tem é maligno.»\n\nNo dia em que soube sofrer de cancro, Jorge Nuno Pinto da Costa, o dirigente com mais títulos do futebol mundial, começou assim o texto que abre este livro, no qual partilha os pensamentos, as inquietações, as emoções, mas também as certezas dos seus três anos derradeiros de vida.\n\nTerminou-o às três da manhã, deixando registadas três decisões: nunca mais perder tempo com os inimigos, aproveitar os dias que lhe restavam para conviver com aqueles de quem gostava e lutar até ao último dia pelo FC Porto. A partir de então, e até ao momento em que lhe deram três meses de vida, foi-se preparando para o final, escondendo a doença dos mais próximos e registando o correr dos dias.\n\nEntre outros temas, escreveu sobre o desaparecimento de alguns dos mais próximos, o atual casamento, os natais em família, os 42 anos de presidência, a última época desportiva, o dia-a-dia do clube, as 1000 vitórias, o último título, a decisão de não se recandidatar, a reversão dessa decisão e as eleições em que saiu derrotado.\n\nSão todas essas páginas que Pinto da Costa torna agora públicas, num livro que emociona e espanta, e que termina com a partilha daquilo que pretende para o seu funeral, incluindo quem quer e quem não quer que nele esteja presente. Nesse dia, os adeptos do clube que tornou referência internacional chorarão a perda do maior dos seus."
                },
                // Só Neste País - Grandes histórias da pequena política

                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[2].Id,
                    AutorId = autores[11].Id,
                    ISBN = "9789897247644", 
                    Dimensoes = "157 x 233 x 23 mm",
                    NumeroPaginas = 356, 
                    Idioma = "Português", 
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5, 
                    NumeroExemplaresTotal = 5,
                    Titulo = "Só Neste País - Grandes histórias da pequena política",
                    FotoNome = "SoNestePais.webp", 
                    DataInsercao = new DateTime(2024, 10, 29), 
                    Cliques = 0,
                    Sinopse = "O lado mais caricato dos 50 anos de democracia.\n\nEste é um livro de histórias. A nossa vida, individual e coletiva, é feita de histórias e não há nada de menor nestas aqui reunidas, embora boa parte não tenha feito parar o país. Com elevada probabilidade, algumas já caíram no esquecimento coletivo ou para lá caminham.\n\nMas são histórias que destapam aquilo que fomos e somos enquanto democracia, que nos expõem a pequenez, o improviso, as vistas curtas, a pompa risível e a circunstância balofa, as contradições, o ridículo e a falta de noção. Pode não ser grandioso, mas é divertido. Ou constrangedor.\n\nFilipe Santos Costa e Liliana Valente, jornalistas experientes e profundos conhecedores dos meandros políticos nacionais, passaram horas a folhear jornais, recolhendo histórias improváveis, inverosímeis, insólitas, mas nunca insonsas.\n\nPor muito bizarros, extravagantes ou inacreditáveis que pareçam os episódios que aqui contamos, estão documentados e registados. São património nacional, como os políticos que os protagonizaram. São nossos. Isto só neste país."
                },
                //Tempestade sobre a Vila dos Tecidos - Anne Jacobs

                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id, 
                    AutorId = autores[12].Id, 
                    ISBN = "9789897779305", 
                    Dimensoes = "155 x 238 x 37 mm", 
                    NumeroPaginas = 560, 
                    Idioma = "Português", 
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5, 
                    NumeroExemplaresTotal = 5, 
                    Titulo = "Tempestade sobre a Vila dos Tecidos",
                    FotoNome = "TempestadeSobreAVilaDosTecidos.webp", 
                    DataInsercao = new DateTime(2024, 10, 29), 
                    Cliques = 0,
                    Sinopse = "Augsburgo, 1935. A tempestade que se abate sobre a Alemanha também ameaça a paz e a tranquilidade da família Melzer e da Vila dos Tecidos. O famoso ateliê de costura de Marie está à beira da ruína quando se espalha a notícia de que ela é descendente de judeus.\n\nO seu marido, Paul, tem de lidar com a terrível situação financeira da fábrica dos tecidos Melzer e com a crescente pressão do governo. Quando Paul é aconselhado a divorciar-se da mulher, com urgência, Marie terá de tomar uma decisão importante que mudará a vida de todos para sempre."
                },

                // Toda a Mafalda - Quino
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[6].Id, 
                    AutorId = autores[13].Id, 
                    ISBN = "9789895832415",
                    Dimensoes = "207 x 269 x 43 mm", 
                    NumeroPaginas = 608, 
                    Idioma = "Português", 
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5, 
                    Titulo = "Toda a Mafalda",
                    FotoNome = "TodaAMafalda.webp", 
                    DataInsercao = new DateTime(2024, 10, 29),
                    Cliques = 0,
                    Sinopse = "Inconformada, única e inesquecível, a Mafalda é uma irreverente criança de seis anos dotada de uma personalidade forte e de uma mente inquisidora, com um olhar crítico sobre as complexidades e contradições do mundo em que vive e uma obstinação em não fechar os olhos — e a boca! Defensora dos direitos humanos, a Mafalda opõe-se sagazmente às guerras, injustiças e desigualdades. E, acima de tudo, luta por que se acabe com a sopa no mundo, e com a prepotência de quem a quer impor.\n\nToda a Mafalda é a edição completa que reúne todos os desenhos e tiras que Quino, o seu brilhante criador, fez da sua personagem mais querida. Com prefácio de Umberto Eco, que considera a Mafalda «a heroína do nosso tempo», esta edição inclui uma contextualização histórica das tiras, homenagens de personalidades internacionais e portuguesas do mundo das artes e do espetáculo, entre outros materiais inéditos, que nos mostram que, após sessenta anos, as observações ousadas e perspicazes da Mafalda continuam tão pertinentes como nunca."
                },

                // Mia Couto - A Cegueira do Rio
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id, 
                    AutorId = autores[14].Id, 
                    ISBN = "9789722133111",
                    Dimensoes = "137 x 213 x 21 mm",
                    NumeroPaginas = 328,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5, 
                    NumeroExemplaresTotal = 5, 
                    Titulo = "A Cegueira do Rio",
                    FotoNome = "ACegueiraDoRio.webp", 
                    DataInsercao = new DateTime(2024, 10, 29), 
                    Cliques = 0,
                    Sinopse = "O primeiro incidente militar numa aldeia do Norte de Moçambique marca, em agosto de 1914, o início da Primeira Guerra Mundial no continente africano. Esse inesperado episódio despoleta uma série de misteriosos eventos que culminam com o desaparecimento da escrita no mundo. Livros, relatórios, documentos, fotografias, mapas surgem deslavados e ninguém mais parece ser capaz de dominar a arte da escrita. Os habitantes dessa aldeia são chamados a restabelecer a ordem no mundo, ensinando aos europeus o ofício da escrita e as artes da navegação."
                },

                // O Protocolo Caos - José Rodrigues dos Santos
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id, 
                    AutorId = autores[15].Id, 
                    ISBN = "9789897779336",
                    Dimensoes = "156 x 235 x 39 mm",
                    NumeroPaginas = 624,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "O Protocolo Caos",
                    FotoNome = "OProtocoloCaos.webp", 
                    DataInsercao = new DateTime(2024, 10, 29), 
                    Cliques = 0,
                    Sinopse = "Um homem encapuzado sai do carro e abre fogo contra a multidão. Morrem dezenas de pessoas, incluindo bebés. Depois do massacre, tira a máscara e revela a sua identidade: Tomás Noronha. \n\nUm polícia na Rússia é alertado para atividades suspeitas na cave de um prédio. O que descobre irá mudar a história do país. \n\nUma família americana desfaz-se sem que perceba porquê. A tragédia arrasta-a para uma conspiração que vai dilacerar os Estados Unidos. \n\nUma médica brasileira é perseguida por tentar salvar vidas. Para a ajudar, Maria Flor tem de enfrentar a turba. \n\nUm birmanês tem na sua posse um documento comprometedor. Se quiser chegar a ele, Tomás Noronha precisa de mergulhar no inferno. A ligar todos estes episódios está uma mensagem enigmática.\n\nInspirado em factos reais, O Protocolo Caos transporta-nos ao coração da atualidade mais escaldante e mostra-nos como a Rússia e os seus cavalos de Troia no Ocidente usam as redes sociais para destruir o nosso mundo."
                },

                // Fomos Mais do que um Erro - Marta Coelho
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id,  
                    AutorId = autores[16].Id,  
                    ISBN = "9789899181663",
                    Dimensoes = "156 x 235 x 20 mm",
                    NumeroPaginas = 320,
                    Idioma = "Português", 
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5, 
                    NumeroExemplaresTotal = 5,
                    Titulo = "Fomos Mais do que um Erro",
                    FotoNome = "FomosMaisDoQueUmErro.webp", 
                    DataInsercao = new DateTime(2024, 10, 29),
                    Cliques = 0,
                    Sinopse = "Quando Benedita chega à Herdade do Aragão, acompanhada pelo namorado que se prepara para fechar um grande negócio, está longe de imaginar que aquele lugar vai mudar a sua vida para sempre. A viver um relacionamento tóxico, mas incapaz de sair dele, vê-se mergulhada num dilema quando conhece Pedro, o tratador de cavalos da herdade. Será Benedita capaz de perceber qual é qual?"
                },

                // Patriota - Alexei Navalny
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[2].Id,  
                    AutorId = autores[17].Id,  
                    ISBN = "9789897403828",
                    Dimensoes = "152 x 235 x 34 mm",
                    NumeroPaginas = 512,
                    Idioma = "Português",  
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5, 
                    NumeroExemplaresTotal = 5,  
                    Titulo = "Patriota",
                    FotoNome = "Patriota.webp",  
                    DataInsercao = new DateTime(2024, 10, 29),  
                    Cliques = 0,
                    Sinopse = "Alexei Navalny começou a escrever Patriota pouco depois do seu envenenamento quase fatal em 2020. Esta é a história completa da sua vida: a juventude, a vocação para o ativismo, o casamento e a família, o empenho em desafiar uma superpotência mundial determinada em silenciá-lo e a sua total convicção de que não se pode resistir à mudança – e de que esta um dia chegará."
                },
                
                // Nexus - Yuval Noah Harari
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[2].Id, 
                    AutorId = autores[18].Id,  
                    ISBN = "9781911717096",
                    Dimensoes = "157 x 233 x 23 mm",
                    NumeroPaginas = 356,
                    Idioma = "Inglês",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "Nexus",
                    FotoNome = "Nexus.webp",
                    DataInsercao = new DateTime(2024, 10, 29), 
                    Cliques = 0,
                    Sinopse = "Nexus é uma exploração das redes de informação, desde a Idade da Pedra até a IA, escrita por Yuval Noah Harari. Ele apresenta uma visão abrangente sobre o impacto da comunicação e do conhecimento ao longo da história."
                },
                // Why Nations Fail - Daron Acemoglu e James A. Robinson
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[2].Id,  
                    AutorId = autores[19].Id,  
                    ISBN = "9781846684302",
                    Dimensoes = "158 x 235 x 34 mm",
                    NumeroPaginas = 544,
                    Idioma = "Inglês",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "Why Nations Fail",
                    FotoNome = "WhyNationsFail.webp",
                    DataInsercao = new DateTime(2024, 10, 29),
                    Cliques = 0,
                    Sinopse = "Why are some nations more prosperous than others? This book sets out to answer this question, with a compelling and elegantly argued new theory: that it is not down to climate, geography or culture, but because of institutions. It explains why the world is divided into nations with wildly differing levels of prosperity." // Sinopse completa do livro
                },

                // A Court of Thorns and Roses - Sarah J. Maas
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[9].Id,  
                    AutorId = autores[20].Id,  
                    ISBN = "9781526605399",
                    Dimensoes = "198 x 130 x 28 mm",
                    NumeroPaginas = 448,
                    Idioma = "Inglês",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 4,
                    NumeroExemplaresTotal = 4,
                    Titulo = "A Court of Thorns and Roses",
                    FotoNome = "ACourtOfThornsAndRoses.webp",
                    DataInsercao = new DateTime(2024, 10, 29),
                    Cliques = 0,
                    Sinopse = "THE FIRST BOOK IN THE BESTSELLING SERIES AND A TIKTOK SENSATION.\n\n'With bits of Buffy, Game Of Thrones and Outlander, this is a glorious series of total joy' STYLIST.\n\nFeyre is a huntress. And when she sees a deer in the forest being pursued by a wolf, she kills the predator and takes its prey to feed herself and her family. But the wolf was not what it seemed, and Feyre cannot predict the high price she will have to pay for its death...\n\nDragged away from her family for the murder of a faerie, Feyre discovers that her captor, his face obscured by a jewelled mask, is hiding even more than his piercing green eyes suggest. As Feyre's feelings for Tamlin turn from hostility to passion, she learns that the faerie lands are a far more dangerous place than she realized. And Feyre must fight to break an ancient curse, or she will lose him forever.\n\nSarah J. Maas's books have sold millions of copies worldwide and have been translated into 37 languages. Discover the tantalising, sweeping romantic fantasy, soon to be a major TV series, for yourself." // Sinopse completa do livro
                },

                // Bob Woodward - War
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[5].Id,  
                    AutorId = autores[21].Id,  
                    ISBN = "9781398541443",
                    Dimensoes = "160 x 235 x 30 mm",
                    NumeroPaginas = 448,
                    Idioma = "Inglês",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 3,
                    NumeroExemplaresTotal = 3,
                    Titulo = "War",
                    FotoNome = "War.webp",
                    DataInsercao = new DateTime(2024, 10, 29),
                    Cliques = 0,
                    Sinopse = "Two-time Pulitzer Prize winner Bob Woodward tells the revelatory, behind-the-scenes story of three wars—Ukraine, the Middle East and the struggle for the American Presidency.\n\nWar is an intimate and sweeping account of one of the most tumultuous periods in presidential politics and American history.\n\nWe see President Joe Biden and his top advisers in tense conversations with Russian president Vladimir Putin, Israeli prime minister Benjamin Netanyahu and Ukrainian president Volodymyr Zelensky. We also see Donald Trump, conducting a shadow presidency and seeking to regain political power.\n\nWith unrivaled, inside-the-room reporting, Woodward shows President Biden’s approach to managing the war in Ukraine, the most significant land war in Europe since World War II, and his tortured path to contain the bloody Middle East conflict between Israel and the terrorist group Hamas.\n\nWoodward reveals the extraordinary complexity and consequence of wartime back-channel diplomacy and decision-making to deter the use of nuclear weapons and a rapid slide into World War III.\n\nThe raw cage-fight of politics accelerates as Americans prepare to vote in 2024, starting between President Biden and Trump, and ending with the unexpected elevation of Vice President Kamala Harris as the Democratic nominee for president.\n\nWar provides an unvarnished examination of the vice president as she tries to embrace the Biden legacy and policies while beginning to chart a path of her own as a presidential candidate.\n\nWoodward’s reporting once again sets the standard for journalism at its most authoritative and illuminating." // Sinopse completa do livro
                },

                // Fiódor Dostoiévski - White 
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id,  
                    AutorId = autores[22].Id,  
                    ISBN = "9780241252086",
                    Dimensoes = "125 x 200 x 15 mm",
                    NumeroPaginas = 128,
                    Idioma = "Inglês",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "White Nights",
                    FotoNome = "WhiteNights.webp",
                    DataInsercao = new DateTime(2024, 10, 29),
                    Cliques = 0,
                    Sinopse = "'My God! A whole minute of bliss! Is that really so little for the whole of a man's life?'\n\nA poignant tale of love and loneliness from Russia's foremost writer. \n\nOne of 46 new books in the bestselling Little Black Classics series, to celebrate the first ever Penguin Classic in 1946. Each book gives readers a taste of the Classics' huge range and diversity, with works from around the world and across the centuries - including fables, decadence, heartbreak, tall tales, satire, ghosts, battles and elephants." // Sinopse completa do livro
                },

                // The Christmas Tree Farm - Laurie Gilmore
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[11].Id, 
                    AutorId = autores[23].Id, 
                    ISBN = "9780008610746",
                    Dimensoes = "140 x 210 x 20 mm",
                    NumeroPaginas = 368,
                    Idioma = "Inglês",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 6,
                    NumeroExemplaresTotal = 6,
                    Titulo = "The Christmas Tree Farm",
                    FotoNome = "TheChristmasTreeFarm.webp",
                    DataInsercao = new DateTime(2024, 10, 29),
                    Cliques = 0,
                    Sinopse = "From the author of the viral TikTok sensation, The Pumpkin Spice Cafe and The Cinnamon Bun Book Store, comes the only spicy grumpy x sunshine Christmas romcom you need this year!\n\n'A charming break from reality' Publishers Weekly." // Sinopse completa do livro
                },

                // The Pumpkin Spice Cafe - Laurie Gilmore
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id, 
                    AutorId = autores[23].Id, 
                    ISBN = "9780008610678",
                    Dimensoes = "15.7 x 23.3 x 2.3 cm",
                    NumeroPaginas = 384,
                    Idioma = "Inglês",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "The Pumpkin Spice Cafe",
                    FotoNome = "ThePumpkinSpiceCafe.webp",
                    DataInsercao = new DateTime(2024, 10, 29),
                    Cliques = 0,
                    Sinopse = "perfect autumnal romance read, Gilmore's heartwarming tale finds upbeat city girl Jeannie relocate to a small town to run her aunt's beloved cafe and cause one irascible local's heart to flutter. When Jeanie's aunt gifts her the beloved Pumpkin Spice Cafe in the small town of Dream Harbor, Jeanie jumps at the chance for a fresh start away from her very dull desk job. Logan is a local farmer who avoids Dream Harbor's gossip at all costs. But Jeanie's arrival disrupts Logan's routine and he wants nothing to do with the irritatingly upbeat new girl, except that he finds himself inexplicably drawn to her."
                },

                // Nothing Like The Movies - Lynn Painter
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id, 
                    AutorId = autores[24].Id, 
                    ISBN = "9781398536425",
                    Dimensoes = "13.97 x 21.59 x 1.91 cm",
                    NumeroPaginas = 448,
                    Idioma = "Inglês",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "Nothing Like The Movies",
                    FotoNome = "NothingLikeTheMovies.webp",
                    DataInsercao = new DateTime(2024, 10, 29),
                    Cliques = 0,
                    Sinopse = "Wes had his dream girl but then he lost her - and the only way to get her back is to scheme like a rom-com hero. For a few beautiful months, Wes and girl-next-door Liz were together. But right as the two were about to set off to UCLA together, tragedy struck and their relationship ended. Flash forward and Wes and Liz find themselves in college, together. In a healthier place now, Wes knows he broke Liz’s heart, but is determined to make her fall back in love with him. But after his best efforts get him nowhere, he’s left wondering if their relationship is really over for good."
                },

                // Intermezzo - Sally Rooney
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id, 
                    AutorId = autores[25].Id, 
                    ISBN = "9780571365470",
                    Dimensoes = "15.24 x 22.86 x 2.54 cm",
                    NumeroPaginas = 432,
                    Idioma = "Inglês",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "Intermezzo",
                    FotoNome = "Intermezzo.webp",
                    DataInsercao = new DateTime(2024, 10, 29),
                    Cliques = 0,
                    Sinopse = "Aside from the fact that they are brothers, Peter and Ivan Koubek seem to have little in common. Peter is a Dublin lawyer in his thirties - successful, competent and apparently unassailable. Ivan is a twenty-two-year-old competitive chess player. Now, in the early weeks of his bereavement, Ivan meets Margaret, an older woman emerging from her own turbulent past, and their lives become rapidly and intensely intertwined. For two grieving brothers and the people they love, this is a new interlude - a period of desire, despair and possibility."
                },

                //El Amor, Las Mujeres Y La Vida - Mario 
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[0].Id,  
                    AutorId = autores[26].Id, 
                    ISBN = "9788490626771",
                    Dimensoes = "156 x 235 x 20 mm",
                    NumeroPaginas = 216,
                    Idioma = "Espanhol",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "El Amor, Las Mujeres Y La Vida",
                    FotoNome = "ElAmorLasMujeresYLaVida.webp",
                    DataInsercao = new DateTime(2024, 10, 29),
                    Cliques = 0,
                    Sinopse = "El amor, las mujeres y la vida recoge una selección de poemas aclamados por varias generaciones, aquellos en los que Benedetti vuelca su concepción de la vida: el amor como compensación de la muerte se levanta en sus versos lleno de fe, como fuerza principal que mueve al ser humano, como una proclama de la existencia, que va de la erótica del amante hasta la esperanza del revolucionário o la gratitud del amigo. «El amor es uno de los elementos emblemáticos de la vida. Breve o extendido, espontáneo o minuciosamente construido, es de cualquier manera un apogeo en las relaciones humanas.»" // Sinopse completa do livro
                },

                // Padre Rico, Padre Pobre - Robert T. Kiyosaki
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[8].Id,  
                    AutorId = autores[27].Id, 
                    ISBN = "9788466362788",
                    Dimensoes = "156 x 235 x 20 mm",
                    NumeroPaginas = 264,
                    Idioma = "Espanhol",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "Padre Rico, Padre Pobre",
                    FotoNome = "PadreRicoPadrePobre.webp",
                    DataInsercao = new DateTime(2024, 10, 29),
                    Cliques = 0,
                    Sinopse = "Padre Rico, Padre Pobre es el libro de finanzas personales nº 1 en todo el mundo, el manual de Robert T. Kiyosaki que enseña a las personas a hacerse millonarias. Este livro ajuda a derribar el mito de que necesitas tener ingresos elevados para hacerte rico, desafiar la creencia de que tu casa es una inversión, y enseñar a tus hijos sobre el dinero para que tengan éxito financiero en el futuro. Robert, también conocido como el 'maestro' millonario, ha transformado la forma en que millones de personas perciben el concepto del dinero." // Sinopse completa do livro
                },

                // Nosotros En La Luna - Alice Kellen
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id, 
                    AutorId = autores[28].Id, 
                    ISBN = "9788408237389",
                    Dimensoes = "156 x 235 x 20 mm",
                    NumeroPaginas = 480,
                    Idioma = "Espanhol",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "Nosotros En La Luna",
                    FotoNome = "NosotrosEnLaLuna.webp",
                    DataInsercao = new DateTime(2024, 10, 29),
                    Cliques = 0,
                    Sinopse = "No hay nada más eterno que un encuentro fugaz. Una noche en París. Dos caminos entrelazándose. Cuando Rhys y Ginger se conocen en las calles de la ciudad de la luz, no imaginan que sus vidas se unirán para siempre, a pesar de la distancia y de que no puedan ser más diferentes. Ella vive en Londres y a veces se siente tan perdida que se ha olvidado hasta de sus propios sueños. Él es incapaz de quedarse quieto en ningún lugar y cree saber quién es. Cada noche su amistad crece entre e-mails llenos de confidencias, dudas e inquietudes." // Sinopse completa do livro
                },

                // Violeta - Isabel Allende
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[9].Id,  
                    AutorId = autores[29].Id, 
                    ISBN = "9780063021768",
                    Dimensoes = "156 x 235 x 20 mm",
                    NumeroPaginas = 320,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "Violeta",
                    FotoNome = "Violeta.webp",
                    DataInsercao = new DateTime(2024, 10, 29),
                    Cliques = 0,
                    Sinopse = "Violeta, de Isabel Allende, narra a história de uma mulher desde o seu nascimento em 1920 até o final da sua vida em um século marcado por revoluções, tragédias e mudanças sociais. A história é contada através de uma longa carta, na qual Violeta compartilha memórias íntimas e emocionantes, incluindo suas paixões, tristezas, e sua capacidade de se reinventar diante de todos os desafios que a vida lhe impôs." // Sinopse completa do livro
                },

                // Los Perros Duros No Bailan - Arturo Pérez-Reverte
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[7].Id,  
                    AutorId = autores[30].Id, 
                    ISBN = "9788490629094",
                    Dimensoes = "156 x 235 x 20 mm",
                    NumeroPaginas = 280,
                    Idioma = "Espanhol",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "Los Perros Duros No Bailan",
                    FotoNome = "LosPerrosDurosNoBailan.webp",
                    DataInsercao = new DateTime(2024, 10, 29),
                    Cliques = 0,
                    Sinopse = "Uma história protagonizada por cães em um cenário onde não há espaço para fracos. O protagonista, um cão chamado Negro, tem de lidar com as dificuldades da vida e proteger os seus amigos, enquanto enfrenta dilemas que se assemelham aos dos seres humanos. O livro é uma reflexão sobre a lealdade e a sobrevivência em um mundo hostil." // Sinopse completa do livro
                },
                // El Corazón Helado - Almudena Grandes
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[11].Id, 
                    AutorId = autores[31].Id, 
                    ISBN = "9788490622747",
                    Dimensoes = "15,2 x 23,0 x 5,5 cm",
                    NumeroPaginas = 752,
                    Idioma = "Espanhol", 
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5, 
                    NumeroExemplaresTotal = 5, 
                    Titulo = "El Corazón Helado",
                    FotoNome = "ElCorazonHelado.webp", 
                    DataInsercao = new DateTime(2024, 10, 29), 
                    Cliques = 0,
                    Sinopse = "El corazón helado é uma novela da escritora espanhola Almudena Grandes. O enredo do livro gira em torno das consequências da Guerra Civil Espanhola na vida das famílias e como os conflitos de gerações diferentes estão interligados. A narrativa abrange uma grande gama de personagens, interligando dramas pessoais com os eventos históricos de um passado complexo e traumático."
                },

                // Campos de Fresas - Jordi Sierra i Fabra
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[11].Id, 
                    AutorId = autores[32].Id, 
                    ISBN = "9788408067623",
                    Dimensoes = "13,5 x 21,0 x 1,5 cm",
                    NumeroPaginas = 144,
                    Idioma = "Espanhol", 
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5, 
                    NumeroExemplaresTotal = 5, 
                    Titulo = "Campos de Fresas",
                    FotoNome = "CamposDeFresas.webp", 
                    DataInsercao = new DateTime(2024, 10, 29), 
                    Cliques = 0,
                    Sinopse = "Campos de fresas narra a história de Luciana, uma jovem que entra em coma depois de consumir drogas em uma festa. A história apresenta uma crítica às drogas e os efeitos devastadores que elas podem ter sobre a juventude, além de explorar os sentimentos de seus amigos e familiares enquanto enfrentam a difícil situação."
                },

                // Travesuras de la Niña Mala - Mario Vargas Llosa
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id, 
                    AutorId = autores[33].Id, 
                    ISBN = "9788466339511",
                    Dimensoes = "15,0 x 23,0 x 3,0 cm",
                    NumeroPaginas = 376,
                    Idioma = "Espanhol",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5, 
                    NumeroExemplaresTotal = 5, 
                    Titulo = "Travesuras de la Niña Mala",
                    FotoNome = "TravesurasDeLaNiñaMala.webp", 
                    DataInsercao = new DateTime(2024, 10, 29),
                    Cliques = 0,
                    Sinopse = "Travesuras de la niña mala é um romance do escritor peruano Mario Vargas Llosa que conta a história de um homem, Ricardo, que se apaixona por uma mulher inconstante, que aparece e desaparece de sua vida em diferentes momentos. A narrativa oscila entre a paixão intensa e o amor impossível, passando por diversos cenários, desde Lima até Paris e Tóquio."
                },

                // Las Hijas del Capitán - María Dueñas
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id, 
                    AutorId = autores[34].Id, 
                    ISBN = "9788408197405",
                    Dimensoes = "16,0 x 24,0 x 4,0 cm",
                    NumeroPaginas = 624,
                    Idioma = "Espanhol", 
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5, 
                    NumeroExemplaresTotal = 5, 
                    Titulo = "Las Hijas del Capitán",
                    FotoNome = "LasHijasDelCapitan.webp", 
                    DataInsercao = new DateTime(2024, 10, 29), 
                    Cliques = 0,
                    Sinopse = "Las hijas del capitán conta a história de três irmãs espanholas que emigram para Nova York nos anos 30. A narrativa descreve as dificuldades que enfrentam enquanto tentam se adaptar a uma nova vida, ao mesmo tempo que lutam para continuar o legado do pai e abrir seu próprio caminho em um país novo e diferente."
                },

                //Línea de Fuego - Arturo Pérez-Reverte
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[6].Id, 
                    AutorId = autores[30].Id, 
                    ISBN = "9788432236200",
                    Dimensoes = "15,0 x 23,0 x 4,0 cm",
                    NumeroPaginas = 688,
                    Idioma = "Espanhol", 
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5, 
                    NumeroExemplaresTotal = 5, 
                    Titulo = "Línea de Fuego",
                    FotoNome = "LineaDeFuego.webp", 
                    DataInsercao = new DateTime(2024, 10, 29), 
                    Cliques = 0,
                    Sinopse = "Línea de fuego é uma novela de Arturo Pérez-Reverte que relata um episódio fictício durante a Guerra Civil Espanhola. A narrativa detalha a luta de ambos os lados do conflito e mostra a complexidade do cenário político e social da Espanha, entrelaçando histórias de soldados e civis que lutam por sobrevivência e ideais."
                },

                //Passion Simple - Annie Ernaux
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id, 
                    AutorId = autores[35].Id, 
                    ISBN = "9782070377375",
                    Dimensoes = "19 x 12 x 1 cm",
                    NumeroPaginas = 112,
                    Idioma = "Francês",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 3,
                    NumeroExemplaresTotal = 3,
                    Titulo = "Passion Simple",
                    FotoNome = "PassionSimple.webp",
                    DataInsercao = new DateTime(2024, 10, 29),
                    Cliques = 0,
                    Sinopse = "Passion Simple é um romance autobiográfico de Annie Ernaux que descreve o relacionamento intenso entre a narradora e um homem casado, explorando as emoções cruas e desejos que tomam conta dela."
                },

                //Veiller Sur Elle - Jean-Baptiste Andrea
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id,  
                    AutorId = autores[36].Id,  
                    ISBN = "9782370732880",
                    Dimensoes = "21 x 14 x 2 cm",
                    NumeroPaginas = 320,
                    Idioma = "Francês",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "Veiller Sur Elle",
                    FotoNome = "VeillerSurElle.webp",
                    DataInsercao = new DateTime(2024, 10, 29),
                    Cliques = 0,
                    Sinopse = "Veiller Sur Elle é uma história sobre um escultor que, após a perda do amor de sua vida, se compromete a construir uma estátua para manter viva a memória da mulher que tanto amava. É um conto de beleza e devoção, cheio de emoções profundas."
                },

                //Les Années - Annie Ernaux
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[14].Id, 
                    AutorId = autores[35].Id,  
                    ISBN = "9782070453857",
                    Dimensoes = "20 x 13 x 2 cm",
                    NumeroPaginas = 256,
                    Idioma = "Francês",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 4,
                    NumeroExemplaresTotal = 4,
                    Titulo = "Les Années",
                    FotoNome = "LesAnnees.webp",
                    DataInsercao = new DateTime(2024, 10, 29),
                    Cliques = 0,
                    Sinopse = "Les Années é um romance memorialístico de Annie Ernaux que narra a experiência pessoal e coletiva de uma mulher, desde sua infância no pós-guerra até os dias modernos. É um retrato do século XX e início do XXI através dos olhos da autora."
                },

                //Changer L'Eau Des Fleurs - Valérie Perrin
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id, 
                    AutorId = autores[37].Id,  
                    ISBN = "9782714473582",
                    Dimensoes = "22 x 14 x 3 cm",
                    NumeroPaginas = 480,
                    Idioma = "Francês",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 4,
                    NumeroExemplaresTotal = 4,
                    Titulo = "Changer L'Eau Des Fleurs",
                    FotoNome = "ChangerLeauDesFleurs.webp",
                    DataInsercao = new DateTime(2024, 10, 29),
                    Cliques = 0,
                    Sinopse = "Changer L'Eau Des Fleurs é uma história encantadora de Valérie Perrin sobre Violette Toussaint, uma zeladora de cemitério que cultiva a sua solidão e uma vida tranquila até que um encontro inesperado a leva a redescobrir a beleza e as surpresas da vida."
                },

                //Les Années - Annie Ernaux (edição diferente)
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id,
                    AutorId = autores[35].Id, 
                    ISBN = "9782070453858",
                    Dimensoes = "19 x 13 x 2 cm",
                    NumeroPaginas = 254,
                    Idioma = "Francês",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 3,
                    NumeroExemplaresTotal = 3,
                    Titulo = "Les Années",
                    FotoNome = "LesAnnees_Ed2.webp",
                    DataInsercao = new DateTime(2024, 10, 29),
                    Cliques = 0,
                    Sinopse = "Les Années é um relato semi-autobiográfico que explora as memórias pessoais e os eventos que marcaram a vida de Annie Ernaux. Com uma abordagem única, Ernaux captura a essência da memória coletiva e individual de sua geração."
                },


                // A Fada Oriana - Sophia de Mello Breyner Andresen
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[15].Id,  
                    AutorId = autores[38].Id,  
                    ISBN = "9789720706321",
                    Dimensoes = "24 x 17 x 0.8 cm",
                    NumeroPaginas = 96,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "A Fada Oriana",
                    FotoNome = "AFadaOriana.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "A Fada Oriana é uma história encantadora que ensina às crianças a importância de cuidar da natureza, da floresta, dos animais selvagens e das pessoas. Ao mesmo tempo, aborda as consequências da vaidade e de quebrar promessas. As ilustrações complementam perfeitamente a história e cativam a atenção dos leitores do início ao fim."
                },

                // O Cavaleiro da Dinamarca - Sophia de Mello Breyner Andresen
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[15].Id, 
                    AutorId = autores[38].Id,  
                    ISBN = "9789720706328",
                    Dimensoes = "24 x 17 x 0.8 cm",
                    NumeroPaginas = 56,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "O Cavaleiro da Dinamarca",
                    FotoNome = "OCavaleiroDaDinamarca.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "No regresso de uma longa peregrinação à Palestina, o Cavaleiro tem apenas um desejo: voltar a casa a tempo de celebrar o Natal com a sua família. Nessa viagem, maravilha-se com as cidades de Veneza e Florença, e ouve histórias espantosas sobre pintores, poetas e navegadores."
                },

                // Saga - Sophia de Mello Breyner Andresen
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id,  
                    AutorId = autores[38].Id, 
                    ISBN = "9789720706315",
                    Dimensoes = "24 x 17 x 0.8 cm",
                    NumeroPaginas = 48,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "Saga",
                    FotoNome = "Saga.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "Saga é uma narrativa que explora a jornada de Hans, um jovem que, após a morte do pai, parte em busca de fortuna. O acaso leva-o para uma cidade do sul, onde, com a ajuda de Hoyle, prospera e enriquece. Aos poucos, aprende a amar esta cidade desconhecida. Contudo, a sua alma permanece no mar, onde procura a canção secreta da sua saga."
                },

                // A Menina do Mar - Sophia de Mello Breyner Andresen
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[15].Id,  
                    AutorId = autores[38].Id, 
                    ISBN = "9789720706329",
                    Dimensoes = "24 x 17 x 0.8 cm",
                    NumeroPaginas = 48,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "A Menina do Mar",
                    FotoNome = "AMeninaDoMar.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "A Menina do Mar é o primeiro conto de Sophia para a infância e foi editado, pela primeira vez, em 1958. Tendo a praia como cenário, este conto revela-nos uma história de amizade entre um rapaz e a Menina do Mar. Cada um vive no seu mundo, o rapaz na terra e a menina no mar, mas a curiosidade de ambos leva-os a querer partilhar essas diferenças: a menina fica a saber o que é o amor, a saudade e a alegria; o rapaz aceita viver com ela no fundo do mar."
                },

                // O Rapaz de Bronze - Sophia de Mello Breyner Andresen
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[15].Id, 
                    AutorId = autores[38].Id, 
                    ISBN = "9789720706316",
                    Dimensoes = "24 x 17 x 0.8 cm",
                    NumeroPaginas = 64,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "O Rapaz de Bronze",
                    FotoNome = "ORapazDeBronze.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "A história de uma estátua que, durante a noite, ganha vida e comanda todas as plantas do jardim maravilhoso onde mora. Sophia de Mello Breyner brinda-nos com mais uma obra magnífica e cheia de magia, ensinando-nos que o verdadeiro valor das coisas não está no seu tamanho."
                },

                // Contos Exemplares - Sophia de Mello Breyner Andresen
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[17].Id,  
                    AutorId = autores[38].Id,  
                    ISBN = "9789720706317",
                    Dimensoes = "24 x 17 x 0.8 cm",
                    NumeroPaginas = 96,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "Contos Exemplares",
                    FotoNome = "ContosExemplares.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "Esta coletânea de contos foi publicada pela primeira vez em 1962 e o título faz uma referência explícita às 'Novelas Exemplares' de Cervantes. Inclui os contos 'O Jantar do Bispo', 'A Viagem', 'Retrato de Mónica', 'Praia', 'Homero', 'O Homem' e 'Os Três Reis do Oriente'."
                },

                // Os Ciganos - Sophia de Mello Breyner Andresen e Pedro Sousa Tavares
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[15].Id,  
                    AutorId = autores[38].Id,  
                    ISBN = "9789720706318",
                    Dimensoes = "24 x 17 x 0.8 cm",
                    NumeroPaginas = 64,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "Os Ciganos",
                    FotoNome = "OsCiganos.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "Ruy vive numa casa com demasiadas regras e muitas rotinas. Um dia, é surpreendido pelo rataplã de um tambor que o desafia a saltar o muro do jardim e a percorrer os campos ao encontro de um acampamento de ciganos. Esta história teve início num fragmento de um conto de Sophia de Mello Breyner Andresen, localizado no seu espólio na primavera de 2009. Este conto encontrava-se inacabado e Pedro Sousa Tavares, jornalista e neto da escritora, assumiu a responsabilidade de continuar a história."
                },

                // A Floresta - Sophia de Mello Breyner Andresen
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[15].Id,  
                    AutorId = autores[38].Id,  
                    ISBN = "9789720706319",
                    Dimensoes = "24 x 17 x 0.8 cm",
                    NumeroPaginas = 64,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "A Floresta",
                    FotoNome = "AFloresta.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "A Floresta é uma história que nos transporta para um mundo mágico onde a natureza e a fantasia se entrelaçam. A narrativa acompanha a jornada de uma jovem que, ao adentrar uma floresta encantada, descobre seres e segredos que transformam a sua perceção do mundo. Esta obra de Sophia de Mello Breyner Andresen destaca-se pela sua linguagem poética e pela profundidade das suas metáforas, convidando o leitor a refletir sobre a relação entre o ser humano e a natureza."
                },


                // O Colar - Sophia de Mello Breyner Andresen
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[16].Id,  
                    AutorId = autores[38].Id, 
                    ISBN = "9789723717164",
                    Dimensoes = "14,8 x 20,5 x 1,1 cm",
                    NumeroPaginas = 136,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 3,
                    NumeroExemplaresTotal = 3,
                    Titulo = "O Colar",
                    FotoNome = "OColar.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "O Colar é uma peça de teatro ambientada na cidade de Veneza, que narra a história de Vanina, uma jovem que se apaixona por Pietro, um fidalgo arruinado que ganha a vida a cantar pelos canais da cidade. A obra explora temas como a inocência, a determinação e as ilusões da juventude."
                },

                // Histórias da Terra e do Mar - Sophia de Mello Breyner Andresen
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[17].Id,  
                    AutorId = autores[38].Id,  
                    ISBN = "9789720706320",
                    Dimensoes = "24 x 17 x 0.8 cm",
                    NumeroPaginas = 96,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "Histórias da Terra e do Mar",
                    FotoNome = "HistoriasDaTerraEDoMar.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "Nestas histórias, em terra e no mar, as personagens procuram a sua verdadeira vida, que se revela nos espaços, na noite, no silêncio, no som do mar."
                },

                // O Bojador - Sophia de Mello Breyner Andresen
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[15].Id,  
                    AutorId = autores[38].Id, 
                    ISBN = "9789720706335",
                    Dimensoes = "24 x 17 x 0.8 cm",
                    NumeroPaginas = 64,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "O Bojador",
                    FotoNome = "OBojador.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "O Bojador é uma obra que narra a história de um jovem que, movido pela curiosidade e pelo desejo de aventura, decide enfrentar o temido Cabo Bojador. Esta narrativa simboliza a coragem e a determinação necessárias para superar os medos e os desafios desconhecidos. Através de uma linguagem poética e envolvente, Sophia de Mello Breyner Andresen convida o leitor a refletir sobre a importância de ultrapassar os limites impostos pelo desconhecido."
                },

                // Obra Poética - Sophia de Mello Breyner Andresen
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[0].Id,  
                    AutorId = autores[38].Id,  
                    ISBN = "9789720706322",
                    Dimensoes = "24 x 17 x 0.8 cm",
                    NumeroPaginas = 64,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "Obra Poética",
                    FotoNome = "ObraPoetica.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "A Obra Poética de Sophia de Mello Breyner Andresen reúne a totalidade da sua produção poética, desde os primeiros poemas até aos últimos escritos. Esta coletânea evidencia a profundidade e a sensibilidade da autora, abordando temas como o mar, a natureza, a liberdade e a condição humana. Através de uma linguagem lírica e evocativa, Sophia convida o leitor a mergulhar nas suas reflexões e emoções, consolidando-se como uma das vozes mais marcantes da poesia portuguesa do século XX."
                },


                // Os Três Reis do Oriente - Sophia de Mello Breyner Andresen
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[15].Id,  
                    AutorId = autores[38].Id,  
                    ISBN = "9789720726398",
                    Dimensoes = "20 x 20 x 0.5 cm",
                    NumeroPaginas = 40,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "Os Três Reis do Oriente",
                    FotoNome = "OsTresReisDoOriente.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "Neste conto, Gaspar, Melchior e Baltasar deixam para trás o ouro, a segurança da ciência, o apoio dos poderosos e as mentiras dos mais fortes, para seguir uma estrela que se ergue a Oriente. No silêncio da noite, esta luz revela a alegria de uma Boa Nova."
                },

                // A Noite de Natal - Sophia de Mello Breyner Andresen
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[15].Id,  
                    AutorId = autores[38].Id,  
                    ISBN = "9789720706324",
                    Dimensoes = "24 x 17 x 0.8 cm",
                    NumeroPaginas = 64,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "A Noite de Natal",
                    FotoNome = "ANoiteDeNatal.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "A Noite de Natal é uma história que celebra a importância da família, dos reencontros e dos afetos. A narrativa evoca memórias, saudades e os cheiros exclusivos da noite de consoada, como a resina do pinheiro que decora a sala de estar. Este conto é um verdadeiro hino ao espírito natalício, ideal para ser lido em família durante a época festiva."
                },

                // Memorial do Convento - José Saramago
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id,  
                    AutorId = autores[39].Id,  
                    ISBN = "9789720046717",
                    Dimensoes = "14 x 21 x 2,5 cm",
                    NumeroPaginas = 400,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "Memorial do Convento",
                    FotoNome = "MemorialDoConvento.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "Memorial do Convento é um romance histórico de José Saramago que retrata a construção do Convento de Mafra no século XVIII, durante o reinado de D. João V. A narrativa entrelaça a história de amor entre Baltasar e Blimunda com a ambição real e a opressão da Inquisição, oferecendo uma crítica social e política da época."
                },

                // Ensaio sobre a Cegueira - José Saramago
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id,  
                    AutorId = autores[39].Id,  
                    ISBN = "9789720706323",
                    Dimensoes = "24 x 17 x 0.8 cm",
                    NumeroPaginas = 312,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "Ensaio sobre a Cegueira",
                    FotoNome = "EnsaioSobreACegueira.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "Ensaio sobre a Cegueira é uma obra perturbadora que nos confronta com os aspectos mais sombrios da natureza humana. Saramago utiliza a cegueira como metáfora para refletir sobre a natureza humana, a falta de empatia e a decadência da sociedade."
                },


                // As Intermitências da Morte - José Saramago
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id,  
                    AutorId = autores[39].Id,  
                    ISBN = "9789720706399",
                    Dimensoes = "24 x 17 x 0.8 cm",
                    NumeroPaginas = 256,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "As Intermitências da Morte",
                    FotoNome = "AsIntermitenciasDaMorte.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "Publicado em 2005, 'As Intermitências da Morte' explora um cenário onde a morte deixa de ocorrer, levando a sociedade a enfrentar as consequências da imortalidade. A obra reflete sobre a vida, a morte e o sentido da existência humana."
                },

                // O Ano da Morte de Ricardo Reis - José Saramago
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id, 
                    AutorId = autores[39].Id,  
                    ISBN = "9789720706336",
                    Dimensoes = "24 x 17 x 0.8 cm",
                    NumeroPaginas = 496,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "O Ano da Morte de Ricardo Reis",
                    FotoNome = "OAnoDaMorteDeRicardoReis.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "Publicado em 1984, 'O Ano da Morte de Ricardo Reis' é um romance que segue a personagem Ricardo Reis, um dos heterónimos de Fernando Pessoa, após a morte do poeta. A narrativa decorre entre dezembro de 1935 e setembro de 1936, período marcado pela ascensão do fascismo em Portugal e na Europa. A obra explora temas como a identidade, a política e a relação entre a vida e a morte, apresentando encontros fictícios entre Ricardo Reis e o fantasma de Fernando Pessoa."
                },

                // A Maior Flor do Mundo - José Saramago
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[15].Id,  
                    AutorId = autores[39].Id, 
                    ISBN = "9789722111437",
                    Dimensoes = "21 x 28 x 0.5 cm",
                    NumeroPaginas = 32,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "A Maior Flor do Mundo",
                    FotoNome = "AMaiorFlorDoMundo.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "Publicado em 2001, 'A Maior Flor do Mundo' é um conto infantil de José Saramago que narra a história de um menino que, ao aventurar-se além da sua aldeia, encontra uma flor murcha e decide salvá-la, regando-a até que se torne a maior flor do mundo. A narrativa aborda temas como a solidariedade, a persistência e a relação harmoniosa entre o ser humano e a natureza."
                },

                // Claraboia - José Saramago
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id, 
                    AutorId = autores[39].Id,  
                    ISBN = "9789722120446",
                    Dimensoes = "15 x 23 x 2.5 cm",
                    NumeroPaginas = 400,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "Claraboia",
                    FotoNome = "Claraboia.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "Concluído em 1953 e publicado postumamente em 2011, 'Claraboia' retrata a vida dos moradores de um prédio lisboeta nos anos 50. Através de uma claraboia, o narrador observa as interações e os dramas pessoais de seis famílias, oferecendo uma visão íntima da sociedade portuguesa da época."
                },

                // Ensaio sobre a Lucidez - José Saramago
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id,   
                    AutorId = autores[39].Id, 
                    ISBN = "9789720046526",
                    Dimensoes = "14 x 21 cm",
                    NumeroPaginas = 360,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "Ensaio sobre a Lucidez",
                    FotoNome = "EnsaioSobreALucidez.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "Neste romance, José Saramago apresenta uma alegoria sobre a fragilidade do sistema político e das instituições que nos governam. Numa manhã de votação na capital de um país imaginário, os eleitores optam massivamente pelo voto em branco, desencadeando uma crise política que questiona os fundamentos da democracia. A obra explora temas como o poder, a liberdade e a responsabilidade cívica, convidando o leitor a refletir sobre a natureza das instituições democráticas e a participação cidadã."
                },

                // A Noite - José Saramago
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[16].Id,  
                    AutorId = autores[39].Id,  
                    ISBN = "9789720046482",
                    Dimensoes = "14,8 x 20,5 x 1,1 cm",
                    NumeroPaginas = 136,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 3,
                    NumeroExemplaresTotal = 3,
                    Titulo = "A Noite",
                    FotoNome = "ANoite.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "A Noite é a primeira obra dramática de José Saramago, lançada em 1979. A história passa-se na redação de um jornal em Lisboa, na noite de 24 para 25 de abril de 1974, abordando as tensões e dilemas vividos pelos jornalistas durante a Revolução dos Cravos."
                },

                // A Jangada de Pedra - José Saramago
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id,  
                    AutorId = autores[39].Id,  
                    ISBN = "9789720706325",
                    Dimensoes = "24 x 17 x 0.8 cm",
                    NumeroPaginas = 320,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "A Jangada de Pedra",
                    FotoNome = "AJangadaDePedra.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "Publicado em 1986, 'A Jangada de Pedra' é um romance alegórico de José Saramago que narra a separação física da Península Ibérica do continente europeu, transformando-a numa ilha à deriva no Atlântico. A obra explora temas como identidade nacional, isolamento e solidariedade, através das jornadas de cinco personagens principais que testemunham e vivenciam este fenómeno inexplicável."
                },

                // O Evangelho Segundo Jesus Cristo - José Saramago
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id,  
                    AutorId = autores[39].Id,  
                    ISBN = "9789722105634",
                    Dimensoes = "23 x 15 x 2.5 cm",
                    NumeroPaginas = 384,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "O Evangelho Segundo Jesus Cristo",
                    FotoNome = "OEvangelhoSegundoJesusCristo.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "Publicado em 1991, 'O Evangelho Segundo Jesus Cristo' é uma obra polémica de José Saramago que reinterpreta a vida de Jesus de uma perspetiva humanizada. O romance explora temas como a culpa, a responsabilidade e a relação entre o divino e o humano, oferecendo uma visão crítica e contemporânea da figura de Cristo."
                },

                // A Viagem do Elefante - José Saramago
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id,  
                    AutorId = autores[39].Id,  
                    ISBN = "9789722120361",
                    Dimensoes = "23 x 15 x 1.5 cm",
                    NumeroPaginas = 264,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "A Viagem do Elefante",
                    FotoNome = "AViagemDoElefante.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "Em meados do século XVI, o rei D. João III decide oferecer ao arquiduque Maximiliano da Áustria um elefante indiano chamado Salomão. Esta obra narra a jornada épica do elefante e do seu cornaca, Subhro, desde Lisboa até Viena, atravessando a Europa e enfrentando diversos desafios. Com a sua habitual ironia e profundidade, Saramago explora temas como o poder, a fé e a condição humana."
                },

                // O Conto da Ilha Desconhecida - José Saramago
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id,  
                    AutorId = autores[39].Id,  
                    ISBN = "9789720726336",
                    Dimensoes = "24 x 17 x 0.8 cm",
                    NumeroPaginas = 64,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "O Conto da Ilha Desconhecida",
                    FotoNome = "OContoDaIlhaDesconhecida.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "Situada num tempo e num espaço indeterminados, a história do homem que queria um barco para ir à procura da ilha desconhecida promete ser a história de todos os homens que lutam contra as convenções em busca dos seus sonhos e de si próprios."
                },

                // Levantado do Chão - José Saramago
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id,  
                    AutorId = autores[39].Id,  
                    ISBN = "9789720706326",
                    Dimensoes = "24 x 17 x 0.8 cm",
                    NumeroPaginas = 400,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "Levantado do Chão",
                    FotoNome = "LevantadoDoChao.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "Publicado em 1980, 'Levantado do Chão' é um romance que retrata a luta de gerações de camponeses alentejanos contra a opressão e as desigualdades sociais. Através da história da família Mau-Tempo, José Saramago explora temas como a resistência, a injustiça e a busca por dignidade, oferecendo uma visão profunda da realidade rural portuguesa do século XX."
                },

                // Caim - José Saramago
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id,  
                    AutorId = autores[39].Id,  
                    ISBN = "9789720041538",
                    Dimensoes = "23,5 x 15,5 x 1,5 cm",
                    NumeroPaginas = 176,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "Caim",
                    FotoNome = "Caim.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "Em 'Caim', José Saramago revisita episódios do Antigo Testamento, oferecendo uma perspetiva crítica e irónica sobre a relação entre o homem e a divindade. Através da figura de Caim, o autor questiona a justiça divina e explora temas como o livre-arbítrio e a moralidade. Esta obra provocadora convida o leitor a refletir sobre as narrativas bíblicas e o papel de Deus na história humana."
                },

                // Hamlet - William Shakespeare
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[16].Id,  
                    AutorId = autores[40].Id,  
                    ISBN = "9789723704331",
                    Dimensoes = "23,5 x 15,5 x 1,5 cm",
                    NumeroPaginas = 256,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "Hamlet",
                    FotoNome = "Hamlet.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "Considerada uma das obras-primas de William Shakespeare, 'Hamlet' é uma tragédia que explora temas como a vingança, a loucura e a corrupção. A peça narra a história do Príncipe Hamlet da Dinamarca, que busca vingar a morte do pai, o rei, assassinado pelo tio Cláudio. A obra é conhecida pelo seu profundo exame da condição humana e pelos seus solilóquios memoráveis, incluindo o famoso 'Ser ou não ser, eis a questão'."
                },

                // Romeu e Julieta - William Shakespeare
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[16].Id,  
                    AutorId = autores[40].Id,  
                    ISBN = "9789720706327",
                    Dimensoes = "24 x 17 x 0.8 cm",
                    NumeroPaginas = 160,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "Romeu e Julieta",
                    FotoNome = "RomeuEJulieta.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "Romeu e Julieta é uma das tragédias mais conhecidas de William Shakespeare, narrando a história de dois jovens amantes cujas famílias estão em conflito. Ambientada em Verona, a peça explora temas como o amor proibido, o ódio entre famílias e o destino trágico dos protagonistas. Esta obra-prima continua a ser uma referência na literatura mundial, destacando-se pela profundidade emocional e pela crítica às rivalidades familiares."
                },

                // Otelo - William Shakespeare
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id,  
                    AutorId = autores[40].Id,  
                    ISBN = "9789897840670",
                    Dimensoes = "19,8 x 12,8 x 1,5 cm",
                    NumeroPaginas = 256,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "Otelo",
                    FotoNome = "Otelo.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "Otelo é uma das tragédias mais conhecidas de William Shakespeare, explorando temas como o ciúme, a traição e a manipulação. A peça narra a história de Otelo, um general mouro a serviço de Veneza, cuja vida é devastada pela inveja e intrigas de Iago, seu alferes. Esta obra-prima do teatro elisabetano continua a ser estudada e encenada em todo o mundo, refletindo a profundidade e complexidade das emoções humanas."
                },

                // Macbeth - William Shakespeare
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[16].Id,  
                    AutorId = autores[40].Id,  
                    ISBN = "9789896233185",
                    Dimensoes = "19,8 x 12,8 x 1,5 cm",
                    NumeroPaginas = 160,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "Macbeth",
                    FotoNome = "Macbeth.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "'Macbeth' é uma das tragédias mais conhecidas de William Shakespeare, explorando temas como ambição desmedida, poder e culpa. A peça narra a ascensão e queda de Macbeth, um nobre escocês que, influenciado por profecias e pela sua esposa, comete atos atrozes para se tornar rei. Esta obra-prima continua a ser estudada e encenada em todo o mundo, refletindo sobre a natureza humana e as consequências das ações impulsionadas pela ambição."
                },

                // Muito Barulho por Nada - William Shakespeare
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[10].Id,  
                    AutorId = autores[40].Id,  
                    ISBN = "9789723718215",
                    Dimensoes = "23 x 15 x 1.5 cm",
                    NumeroPaginas = 160,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "Muito Barulho por Nada",
                    FotoNome = "MuitoBarulhoPorNada.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "Considerada uma das melhores comédias de Shakespeare, 'Muito Barulho por Nada' apresenta um enredo simples e hilariante, com personagens muitíssimo interessantes. A peça aborda temas como a vida pública, o papel da mulher na época vitoriana e o poder dos rumores, proporcionando momentos de puro brilhantismo.",
    
                },

                // O Mercador de Veneza - William Shakespeare
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id, 
                    AutorId = autores[40].Id,  
                    ISBN = "9789898872975",
                    Dimensoes = "24 x 17 x 1.5 cm",
                    NumeroPaginas = 160,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "O Mercador de Veneza",
                    FotoNome = "OMercadorDeVeneza.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "Na grande república mercantil de Veneza, António, o mercador anti-semita, cede aos estranhos contornos de um empréstimo junto do judeu Shylock para ajudar Bassânio, o seu jovem amigo, a conquistar Pórcia, uma herdeira bastante rica. Incapaz de cumprir as condições acordadas, António vê-se obrigado a pagar entregando a Shylock meio quilo escrupulosamente pesado da sua própria carne. Porém, Pórcia trasveste-se de advogado e defende António em tribunal. O tom ambíguo do texto de Shakespeare permite que o leitor (ou o actor) decida se interpreta o judeu Shylock como vilão ou como vítima; a alternância entre os pontos de vista anti-semitas e os discursos morais, bem como o talento de Pórcia para a defesa legal (travestida de homem), são tudo elementos de uma modernidade extraordinária que faz desta comédia de Shakespeare um dos seus textos mais marcantes, o que justifica o facto de ser uma das peças do bardo mais vezes levada à cena, ao grande ecrã ou às ondas radiofónicas."
                },

                // António e Cleópatra - William Shakespeare
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[16].Id,  
                    AutorId = autores[40].Id, 
                    ISBN = "9789896418970",
                    Dimensoes = "23 x 15 x 1.5 cm",
                    NumeroPaginas = 320,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "António e Cleópatra",
                    FotoNome = "AntonioECleopatra.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "Levada a cena pela primeira vez em 1607, e baseada em acontecimentos históricos verídicos narrados por Plutarco no século I, 'António e Cleópatra' é um drama histórico em cinco atos que explora a relação tumultuosa entre o general romano Marco António e a rainha egípcia Cleópatra. A peça aborda temas de poder, amor e tragédia, destacando a queda de um general sem habilidade política e uma rainha com ambição."
                },

                // O Rei Lear - William Shakespeare
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[1].Id, 
                    AutorId = autores[40].Id,  
                    ISBN = "9789720041597",
                    Dimensoes = "23,5 x 15,5 x 1,5 cm",
                    NumeroPaginas = 176,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "O Rei Lear",
                    FotoNome = "OReiLear.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "Em 'O Rei Lear', William Shakespeare apresenta uma das suas tragédias mais intensas, explorando temas como a ambição, a traição e a loucura. A peça narra a história do rei Lear, que decide dividir o seu reino entre as três filhas, resultando em consequências trágicas. Esta obra-prima do teatro elisabetano continua a ser uma reflexão profunda sobre a natureza humana e o poder."
                },

                // Noite de Reis ou Como Lhe Queiram Chamar - William Shakespeare
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[16].Id,  
                    AutorId = autores[40].Id,  
                    ISBN = "9789727954695",
                    Dimensoes = "21 x 14 x 1 cm",
                    NumeroPaginas = 160,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "Noite de Reis ou Como Lhe Queiram Chamar",
                    FotoNome = "NoiteDeReisOuComoLheQueiramChamar.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "Peça contemporânea de Hamlet, 'Noite de Reis' faz da sua imponderabilidade mozartiana um contraponto à natureza sanguínea das grandes tragédias do autor. A tradução, originalmente feita para a encenação de Ricardo Pais, é de António M. Feijó, que a reviu para esta edição."
                },


                // A Tempestade - William Shakespeare
                new Livro
                {
                    BibliotecaId = biblioteca.Id,
                    CategoriaId = categorias[16].Id,  
                    AutorId = autores[40].Id,  
                    ISBN = "9789896419890",
                    Dimensoes = "23 x 15 x 1.5 cm",
                    NumeroPaginas = 160,
                    Idioma = "Português",
                    IdBibliotecarioInseriu = utilizadores[1].Id,
                    Estado = "Disponível",
                    NumeroExemplaresDisponiveis = 5,
                    NumeroExemplaresTotal = 5,
                    Titulo = "A Tempestade",
                    FotoNome = "ATempestade.webp",
                    DataInsercao = new DateTime(2024, 11, 2),
                    Cliques = 0,
                    Sinopse = "Considerada uma das últimas peças escritas por William Shakespeare, 'A Tempestade' é uma obra que mistura elementos de drama, comédia e fantasia. A história centra-se em Próspero, o legítimo Duque de Milão, que, após ser traído pelo irmão, é exilado numa ilha mágica com a sua filha Miranda. Utilizando os seus poderes mágicos, Próspero provoca uma tempestade que faz naufragar os seus inimigos na ilha, iniciando uma trama de vingança, perdão e reconciliação. Esta peça explora temas como o poder, a traição, a redenção e a natureza humana, sendo considerada uma das obras-primas de Shakespeare."
                },


    };

            _context.Livro.AddRange(livros);
            _context.SaveChanges();
            //inicializacao da tabela de emprestimos

            // Verifica se existe um livro no banco de dados
            var livro = _context.Livro.FirstOrDefault();

            if (livro != null)
            {
                // Garante que a coleção Reviews está inicializada
                if (livro.Reviews == null)
                {
                    livro.Reviews = new List<Review>();
                }

                // Cria uma nova review com BookId associado ao livro encontrado
                var exampleReview = new Review
                {
                    Text = "Uma leitura envolvente com personagens bem desenvolvidos e uma trama surpreendente. Recomendo muito!",
                    Rating = 5,
                    TituloLivro = livro.Titulo,
                    UserId = utilizadores[0].Id,
                    CreatedAt = DateTime.Now,
                    Lido = false
                };

                // Adiciona a review à coleção de reviews do livro
                livro.Reviews.Add(exampleReview);

                // Salva as mudanças no banco de dados
                _context.SaveChanges();
            }
            else
            {
                Console.WriteLine("Nenhum livro encontrado.");
            }


            var emprestimos = new List<Emprestimo>
            {
                new Emprestimo
                {
                    DataEmprestimo = new DateTime(2024, 10, 4),
                    PerfilId = utilizadores[1].Id,
                    LivroId = livros[0].ID,
                    DataPrevista = new DateTime(2024, 10, 19),
                    DataDevolucao = new DateTime(2024, 10, 18),
                    Id_bibliotecario_entregou = utilizadores[0].Id,
                    Id_bibliotecario_recebeu = utilizadores[0].Id
                },
                new Emprestimo
                {
                    DataEmprestimo = new DateTime(2024, 10, 6),
                    DataPrevista = new DateTime(2024, 10, 21),
                    PerfilId = utilizadores[1].Id,
                    LivroId = livros[1].ID,
                    Id_bibliotecario_entregou = utilizadores[0].Id
                }
            };

            _context.Emprestimo.AddRange(emprestimos);
            _context.SaveChanges();

        }
    }

}