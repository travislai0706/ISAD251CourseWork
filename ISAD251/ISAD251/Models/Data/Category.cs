using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KungFuTea.Models.Data
{
    [Table("Categories")]
    public class Category
    {
        [Key]
        public int Id { get; set; }
        public int Sorting { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
    }
}