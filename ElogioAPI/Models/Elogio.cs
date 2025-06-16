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
        public string Texto { get; set; }
    }
}