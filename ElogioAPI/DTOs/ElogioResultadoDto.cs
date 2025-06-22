// DTOs/ElogioResultadoDto.cs
namespace ElogioAPI.DTOs
{
    public class ElogioResultadoDto
    {
        public long Id { get; set; }
        public string? Texto { get; set; }
        public DateTime DataElogio { get; set; }
    }
}