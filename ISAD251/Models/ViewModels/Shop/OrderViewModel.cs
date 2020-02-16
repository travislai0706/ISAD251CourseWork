using KungFuTea.Models.Data;
using System;

namespace KungFuTea.Models.ViewModels.Shop
{
    public class OrderViewModel
    {
        public OrderViewModel()
        {
        }

        public OrderViewModel(Order row)
        {
            OrderId = row.OrderId;
            UserId = row.UserId;
            CreatedDate = row.CreatedDate;
        }

        public int OrderId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}