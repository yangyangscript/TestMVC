using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TestMVC.Models
{
    public class OpreateRecord
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreateTime { get; set; }

        [StringLength(50)]
        public string ControllerName { get; set; }
        public int UserId { get; set; }

        [StringLength(150)]
        public string MesFir { get; set; }

        [StringLength(150)]
        public string MesSec { get; set; }
    }
}