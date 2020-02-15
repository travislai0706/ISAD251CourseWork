using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KungFuTea.Models.Data
{
    [Table("OrderDetails")]
    public class OrderDetails
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order Orders { get; set; }
        [ForeignKey("UserId")]
        public virtual User Users { get; set; }
        [ForeignKey("ItemId")]
        public virtual Item Items { get; set; }
    }
}