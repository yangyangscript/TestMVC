using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TestMVC.ViewModel
{
    public class GridConfig:ICloneable
    {
        [Display(Name = "名字")]
        public string Name { get; set; }

        [Display(Name = "列名")]
        public string Title { get; set; }

        [Display(Name = "是否导出")]
        public bool IsExport { get; set; }
        public int SortIndex { get; set; }

        /// <summary>创建作为当前实例副本的新对象。</summary>
        /// <returns>作为此实例副本的新对象。</returns>
        public object Clone()
        {
            return new GridConfig()
            {
                Name = this.Name,
                Title = this.Title,
                IsExport = this.IsExport,
                SortIndex = this.SortIndex,
            };
        }
    }
}