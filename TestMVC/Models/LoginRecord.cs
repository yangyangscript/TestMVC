using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TestMVC.Models
{
    public class LoginRecord
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreateTime { get; set; }

        public int UserId { get; set; }

        public string IpAddress { get; set; }        
    }
}