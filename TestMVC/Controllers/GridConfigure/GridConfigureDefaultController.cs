using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FineUIMvc;
using Newtonsoft.Json.Linq;
using TestMVC.CommonHelp;
using TestMVC.ViewModel;

namespace AppBoxMvc.Controllers.GridConfigure
{
    public class GridConfigureDefaultController : Controller
    {
        // GET: GridConfigureDefault
        public ActionResult DefaultPage()
        {
            var items = TestMVC.CommonHelp.GirdConfigHelp.GirdFistConfig.Select(s =>(ExportConfig) s.Clone()).ToList();
            return View(items);
        }

        

        

        public ActionResult ShowOnClick(string hfData)
        {
            var items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ExportConfig>>(hfData);
            Alert.Show(items[0].Title);
            return UIHelper.Result();
        }


        public ActionResult ChangeItems(string hfData,string name,int status,int rowIndex)
        {
            var items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ExportConfig>>(hfData);                       
            switch (status)
            {
                case 1://前移
                    if (rowIndex == 0) return UIHelper.Result();
                    var parSort = items[rowIndex].SortIndex;
                    var tarSort = items[rowIndex - 1].SortIndex;
                    items[rowIndex].SortIndex = tarSort;
                    items[rowIndex - 1].SortIndex = parSort; 
                    break;
                case 2://后移
                    if (rowIndex == items.Count) return UIHelper.Result();
                    var parSort2 = items[rowIndex].SortIndex;
                    var tarSort2 = items[rowIndex + 1].SortIndex;
                    items[rowIndex].SortIndex = tarSort2;
                    items[rowIndex + 1].SortIndex = parSort2;
                    break;
                case 3://变更
                    items[rowIndex].IsExport = !items[rowIndex].IsExport;
                    break;
                default: return UIHelper.Result();
            }

            items = items.OrderByDescending(s => s.IsExport).ThenBy(s => s.SortIndex).ToList();


            UIHelper.HiddenField("hfData").Text(Newtonsoft.Json.JsonConvert.SerializeObject(items));
            var grid1 = UIHelper.Grid("Grid1");
            grid1.DataSource(items, new string[]{ "Name", "IsExport", "Title" });
            return UIHelper.Result();
        }


    }
}