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
            return View();
        }

        public ActionResult BindGrid()
        {
            
            return UIHelper.Result();
        }
    }
}