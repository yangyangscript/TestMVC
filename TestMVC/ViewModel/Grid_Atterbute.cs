using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestMVC.ViewModel
{
    public sealed class Grid_Atterbute : System.Attribute
    {
        public string Title { get; set; }
    }
}