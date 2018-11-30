using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FineUIMvc;

namespace TestMVC.Controllers
{
    public partial class MasterPageController
    {

        public ActionResult ManagerPage()
        {
            var items = GetDatas(100);
            return View(items);
        }

        public ActionResult BindGrid()
        {
            
            return UIHelper.Result();
        }
    }
}