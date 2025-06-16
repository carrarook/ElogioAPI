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
        public string NomeArquivo { get; set; }
    }
}