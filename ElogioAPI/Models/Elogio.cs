using Postgrest.Models;
using Postgrest.Attributes;

namespace ElogioAPI.Models
{
    [Table("elogios")]
    public class Elogio : BaseModel
    {
        [PrimaryKey("id")]
        public long Id { get; set; }

        [Column("texto")]
        public string? Texto { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // --- NOVOS CAMPOS ---
        [Column("data_upload")]
        public DateTime DataUpload { get; set; }

        [Column("data_elogio")]
        public DateTime DataElogio { get; set; }
    }
}