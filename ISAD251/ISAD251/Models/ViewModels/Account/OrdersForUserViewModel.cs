using System;
using System.Collections.Generic;

namespace KungFuTea.Models.ViewModels.Account
{
    public class OrdersForUserViewModel
    {
        public int OrderNumber { get; set; }
        public decimal Total { get; set; }
        public Dictionary<string, int> ItemsAndQty { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}