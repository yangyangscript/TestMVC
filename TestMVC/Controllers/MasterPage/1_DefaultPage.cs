﻿using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}