﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KungFuTea.Models.Data
{
    [Table("tblRoles")]
    public class RoleDTO
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}