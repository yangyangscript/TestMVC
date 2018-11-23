using TestMVC.Models;
using FineUIMvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace TestMVC.Controllers
{
    public class LoginController : BaseController
    {
        // GET: Login
        public ActionResult Index()
        {
            LoadData();

            return View();
        }

        private void LoadData()
        {
            ViewBag.Window1Title = String.Format("TestMVC v{0}", GetProductVersion());

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnSubmit_Click(string tbxUserName, string tbxPassword)
        {
            string userName = tbxUserName.Trim();
            string password = tbxPassword.Trim();

            User user = db.Users.Where(u => u.Name == userName).FirstOrDefault();

            if (user != null)
            {
                if (PasswordUtil.ComparePasswords(user.Password, password))
                {
                    if (!user.Enabled)
                    {
                        Alert.Show("用户未启用，请联系管理员！");
                    }
                    else
                    {
                        // 登录成功
                        LoginSuccess(user);
                    }
                }
                else
                {
                    Alert.Show("用户名或密码错误！");
                }
            }
            else
            {
                Alert.Show("用户名或密码错误！");
            }

            return UIHelper.Result();
        }

        private void LoginSuccess(User user)
        {
            RegisterOnlineUser(user);

            // 用户所属的角色字符串，以逗号分隔
            string roleIDs = String.Empty;
            if (user.Roles != null)
            {
                roleIDs = String.Join(",", user.Roles.Select(r => r.ID).ToArray());
            }

            bool isPersistent = false;
            DateTime expiration = DateTime.Now.AddMinutes(120);
            CreateFormsAuthenticationTicket(user.Name, roleIDs, isPersistent, expiration);

            // 重定向到登陆后首页
            Response.Redirect(FormsAuthentication.DefaultUrl);
        }

    }
}