using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TestMVC.ViewModel;

namespace TestMVC.CommonHelp
{
    public class GirdConfigHelp
    {
        private static List<ExportConfig> GetGridConfigs<T>()
        {
            var attrType = typeof(ExportConfig_Atterbute);
            var propertyItems = typeof(T).GetProperties()
                .Select(s => new
                    {Name = s.Name, Attr = (ExportConfig_Atterbute) s.GetCustomAttributes(attrType, false).FirstOrDefault()})
                .Where(s => s.Attr != null).ToList();
            var ret= new List<ExportConfig>();
            for (int i = 0; i < propertyItems.Count; i++)
            {
                ret.Add(new ExportConfig()
                {
                    Name = propertyItems[i].Name,
                    Title = propertyItems[i].Attr.Title,
                    IsExport = true,
                    SortIndex = i+1,
                });
            }
            return ret;
        }

        private static List<ExportConfig> _girdFistConfig;

        public static List<ExportConfig> GirdFistConfig {
            get
            {
                if (_girdFistConfig == null)
                {
                    _girdFistConfig = GetGridConfigs<ExportFirst>();
                }
                return _girdFistConfig;
            }
        }
    }
}