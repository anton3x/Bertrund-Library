using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

namespace Definitivo.Models
{
    public class SobreNosModel
    {
        public int Id { get; set; }
        public List<MembroEquipa> MembrosEquipa { get; set; }
        public Biblioteca Biblioteca { get; set; }
        public List<HoraFuncionamento> HorasFucionamento { get; set; }
        public Objetivo ObjetivoBiblioteca { get; set; }
        public string? Historia { get; set; }

    }

    public class Objetivo
    {
        public int Id { get; set; }
        public string? paragrafoInicial { get; set; }
        public string? bulletPoints { get; set; }
    }
    public class MembroEquipa
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Cargo { get; set; }
        public string Descricao { get; set; }
        public string FotoNome { get; set; }
    }

    public class Contacto
    {
        public int Id { get; set; }
        public string Morada { get; set; }
        public string CodigoPostal { get; set; }
        public string Cidade { get; set; }
        public string Telefone { get; set; }
        public string Email { get; set; }
    }

    public class HoraFuncionamento
    {
        public int Id { get; set; }
        public string Dia { get; set; }
        public string Hora { get; set; }
    }

}
