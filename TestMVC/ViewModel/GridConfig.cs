using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestMVC.ViewModel
{
    public class GridConfig
    {
        public string[] Fields { get; set; }

        public bool IsPaging { get; set; }

        public bool IsSorting { get; set; }

        public int PageSize { get; set; }

        public int PageIndex { get; set; }

        public string SortField { get; set; }

        public string SortDirection { get; set; }
    }
}