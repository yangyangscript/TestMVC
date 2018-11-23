﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestMVC.Models
{
    public class Role : IKeyID
    {
        [Key]
        public int ID { get; set; }

        [Display(Name="名称")]
        [StringLength(50)]
        [Required]
        public string Name { get; set; }

        [Display(Name = "备注")]
        [StringLength(500)]
        public string Remark { get; set; }


        public virtual ICollection<User> Users { get; set; }


        public virtual ICollection<Power> Powers { get; set; }
        
    }
}