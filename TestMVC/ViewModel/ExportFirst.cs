using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestMVC.ViewModel
{
    public class ExportFirst
    {
        [ExportConfig_Atterbute(Title = "第一列")]
        public string Name1 { get; set; }

        [ExportConfig_Atterbute(Title = "第二列")]
        public string Name2 { get; set; }

        [ExportConfig_Atterbute(Title = "第三列")]
        public string Name3 { get; set; }

        [ExportConfig_Atterbute(Title = "第四列")]
        public string Name4 { get; set; }

        [ExportConfig_Atterbute(Title = "第五列")]
        public string Name5 { get; set; }

        [ExportConfig_Atterbute(Title = "第六列")]
        public string Name6 { get; set; }
    }
}