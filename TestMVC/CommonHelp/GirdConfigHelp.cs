using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TestMVC.ViewModel;

namespace TestMVC.CommonHelp
{
    public class GirdConfigHelp
    {
        private static List<GridConfig> GetGridConfigs<T>()
        {
            var attrType = typeof(Grid_Atterbute);
            var propertyItems = typeof(T).GetProperties()
                .Select(s => new
                    {Name = s.Name, Attr = (Grid_Atterbute) s.GetCustomAttributes(attrType, false).FirstOrDefault()})
                .Where(s => s.Attr != null).ToList();
            var ret= new List<GridConfig>();
            for (int i = 0; i < propertyItems.Count; i++)
            {
                ret.Add(new GridConfig()
                {
                    Name = propertyItems[i].Name,
                    Title = propertyItems[i].Attr.Title,
                    IsExport = true,
                    SortIndex = i+1,
                });
            }
            return ret;
        }

        private static List<GridConfig> _girdFistConfig;

        public static List<GridConfig> GirdFistConfig {
            get
            {
                if (_girdFistConfig == null)
                {
                    _girdFistConfig = GetGridConfigs<GridFirst>();
                }
                return _girdFistConfig;
            }
        }
    }
}