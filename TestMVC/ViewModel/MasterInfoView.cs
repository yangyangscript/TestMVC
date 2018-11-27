using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TestMVC.ViewModel
{
    public class MasterInfoView
    {
        public int Id { get; set; }

        [Display(Name = "名字一")]
        public string Name1 { get; set; }

        [Display(Name = "名字二")]
        public string Name2 { get; set; }

        [Display(Name = "名字三")]
        public string Name3 { get; set; }

        [Display(Name = "名字四")]
        public string Name4 { get; set; }

        [Display(Name = "时间一")]
        public DateTime Time1 { get; set; }

        [Display(Name = "时间二")]
        public DateTime Time2 { get; set; }

        [Display(Name = "时间三")]
        public DateTime Time3 { get; set; }

        [Display(Name = "时间四")]
        public DateTime Time4 { get; set; }

        [Display(Name = "数字一")]
        public int Number1 { get; set; }

        [Display(Name = "数字二")]
        public int Number2 { get; set; }

        [Display(Name = "数字三")]
        public int Number3 { get; set; }

        [Display(Name = "数字四")]
        public int Number4 { get; set; }
    }
}