using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestMVC.ViewModel
{
    public sealed class ExportConfig_Atterbute : System.Attribute
    {
        public string Title { get; set; }
    }
}