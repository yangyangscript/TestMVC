using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Data.Entity;
using FineUIMvc;

namespace TestMVC.Models
{
    /// <summary>
    /// TestMVC自定义权限验证过滤器
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CheckPowerAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 权限名称
        /// </summary>
        public string Name { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpContextBase context = filterContext.HttpContext;
            // 权限验证不通过
            if (!String.IsNullOrEmpty(Name) && !TestMVC.Controllers.BaseController.CheckPower(context, Name))
            {
                if (context.Request.HttpMethod == "GET")
                {
                    TestMVC.Controllers.BaseController.CheckPowerFailWithPage(context);

                    //http://stackoverflow.com/questions/9837180/how-to-skip-action-execution-from-an-actionfilter
                    // -修正越权访问页面时会报错[服务器无法在发送 HTTP 标头之后追加标头]（龙涛软件-9374）。
                    filterContext.Result = new EmptyResult();
                }
                else if (context.Request.HttpMethod == "POST")
                {
                    TestMVC.Controllers.BaseController.CheckPowerFailWithAlert();
                    filterContext.Result = UIHelper.Result();
                }
            }

        }




    }
}