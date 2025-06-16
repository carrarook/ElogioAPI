using Postgrest.Models;
using Postgrest.Attributes;

namespace ElogioAPI.Models
{
    [Table("logs_uso")]
    public class LogUso : BaseModel
    {
        // shouldInsert: true é importante para chaves primárias autoincrementáveis
        [PrimaryKey("id", shouldInsert: true)]
        public long Id { get; set; }

        [Column("elogio_id")]
        public long ElogioId { get; set; }

        [Column("foto_id")]
        public long FotoId { get; set; }

        [Column("data_uso")]
        public DateTime DataUso { get; set; }
    }
}