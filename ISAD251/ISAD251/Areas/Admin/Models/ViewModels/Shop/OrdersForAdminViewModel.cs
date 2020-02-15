using System;
using System.Collections.Generic;

namespace KungFuTea.Areas.Admin.Models.ViewModels.Shop
{
    public class OrdersForAdminViewModel
    {
        public int OrderNumber { get; set; }
        public string UserName { get; set; }
        public decimal Total { get; set; }
        public Dictionary<string, int> ProductsAndQty { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}