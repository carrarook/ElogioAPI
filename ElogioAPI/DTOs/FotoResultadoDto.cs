// DTOs/FotoResultadoDto.cs
namespace ElogioAPI.DTOs
{
    public class FotoResultadoDto
    {
        public long Id { get; set; }
        public string? NomeArquivo { get; set; }
        public DateTime DataFoto { get; set; }
        public long? ElogioLinkadoId { get; set; }
    }
}