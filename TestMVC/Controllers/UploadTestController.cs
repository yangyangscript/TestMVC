using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FineUIMvc;

namespace TestMVC.Controllers
{
    public class UploadTestController : BaseController
    {
        public ActionResult UploadTestMaster()
        {
            return View();
        }
    
        public ActionResult BtnTest2OnClick(string  tbxName)
        {
            Alert.Show(tbxName);
            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BtnSaveOnClosed(string tbxName)
        {
            Alert.Show(tbxName);
            return UIHelper.Result();
        }




        public ActionResult TestLayout()
        {
            ViewBag.pict1Code = CommonHelp.StringHelp.defaultPicture;
            //UIHelper.HiddenField("hfPict1").Text(CommonHelp.StringHelp.defaultPicture);
            return View();
        }
    }
}