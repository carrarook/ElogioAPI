// DTOs/AdicionarFotoDto.cs
using Microsoft.AspNetCore.Mvc;

namespace ElogioAPI.DTOs
{
    public class AdicionarFotoDto
    {
        // O arquivo da imagem
        public IFormFile? Arquivo { get; set; }

        // A data temática da foto
        public DateTime DataFoto { get; set; }

        // O ID opcional do elogio linkado
        public long? ElogioLinkadoId { get; set; }
    }
}