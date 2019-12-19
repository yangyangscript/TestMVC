using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using FineUIMvc;
using NLog;
using TestMVC.Controllers;

namespace TestMVC.Areas.TestArea.Controllers
{
    [Authorize]
    public class TestAreaController : BaseController
    {
        public ActionResult Test123()
        {
            return View();
        }

        private string GetIp()
        {
            var serverVariable = HttpContext.Request.ServerVariables;           
            string result = serverVariable["HTTP_CDN_SRC_IP"];          
            if (string.IsNullOrEmpty(result))
                result = serverVariable["REMOTE_ADDR"];
            if (string.IsNullOrEmpty(result))
                result = HttpContext.Request.UserHostAddress;
            if (string.IsNullOrEmpty(result))
                return "unknow";

            return result;
        }
        public static bool IsIP(string ip)
        {
            return Regex.IsMatch(ip, "^((2[0-4]\\d|25[0-5]|[01]?\\d\\d?)\\.){3}(2[0-4]\\d|25[0-5]|[01]?\\d\\d?)$");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TestControllerName()
        {
            OpreateLogger("", "阿三大苏打实打实的");
            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult OpreatTest()
        {
            
            //ViewContext.RouteData.Values["controller"].ToString().ToLower();
            var loginer = LogManager.GetLogger("opreater");
            LogEventInfo lei = new LogEventInfo();

            lei.Properties["cName"] = "TestAreaController";
            lei.Properties["userId"] = 123;
            lei.Properties["mesFir"] = "这是一条信息";
            lei.Properties["mesSec"] = "这是第二条信息";
            lei.Level = LogLevel.Info;
            loginer.Log(lei);
            return UIHelper.Result();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NlogTest()
        {
            var loginer = LogManager.GetLogger("loginer");
            LogEventInfo lei = new LogEventInfo();
            lei.Properties["userId"] = 123;
            lei.Properties["ipAddress"] = GetIp();
            lei.Level = LogLevel.Info;
            loginer.Log(lei);
            return UIHelper.Result();
        }
    }
}