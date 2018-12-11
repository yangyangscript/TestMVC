using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using FineUIMvc;
using Newtonsoft.Json.Linq;

namespace TestMVC.Controllers
{
    public partial class MasterPageController
    {

        public ActionResult ManagerPage()
        {
            var items = GetDatas(100);
            return View(items);
        }

        public ActionResult BindGrid(JObject girdItems)
        {
            var items = GetDatas(100);
            BindGird(items, "Gird1", girdItems);

            return UIHelper.Result();
        }

        [HttpPost]
        public ActionResult ExportToExcel(JObject girdItems)
        {  
            var bitys =new byte[8];
            return File(bitys, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "审车数据导出.xlsx");
        }

    }
}