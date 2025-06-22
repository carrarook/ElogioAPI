using Postgrest.Models;
using Postgrest.Attributes;

namespace ElogioAPI.Models
{
    [Table("fotos")]
    public class Foto : BaseModel
    {
        [PrimaryKey("id")]
        public long Id { get; set; }

        [Column("nome_arquivo")]
        public string? NomeArquivo { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // --- NOVOS CAMPOS ---
        [Column("data_upload")]
        public DateTime DataUpload { get; set; }

        [Column("data_foto")]
        public DateTime DataFoto { get; set; }

        // O '?' torna o long anulável, correspondendo ao banco de dados
        [Column("elogio_linkado_id")]
        public long? ElogioLinkadoId { get; set; }
    }
}