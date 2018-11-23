using FineUIMvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Data.Entity;
using TestMVC.Models;

namespace TestMVC.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            LoadData();

            return View();
        }

        // GET: Themes
        public ActionResult Themes()
        {
            return View();
        }

        #region LoadData

        private void LoadData()
        {
            // 用户可见的菜单列表
            List<TestMVC.Models.Menu> menus = ResolveUserMenuList();
            if (menus.Count == 0)
            {
                Response.Write("系统管理员尚未给你配置菜单！");
                Response.End();

                return;
            }

            ViewBag.MenuTreeNodes = GetTreeNodes(menus).ToArray();

            ViewBag.UserName = GetIdentityName();
            ViewBag.OnlineUserCount = GetOnlineCount().ToString();
            ViewBag.ProductVersion = GetProductVersion();
            ViewBag.ConfigTitle = String.Format("TestMVC v{0}", GetProductVersion());

            ViewBag.SystemHelpItems = GetSystemHelpItems().ToArray();
        }

        // 帮助菜单
        private List<MenuItem> GetSystemHelpItems()
        {
            List<MenuItem> items = new List<MenuItem>();

            JArray ja = JArray.Parse(ConfigHelper.HelpList);
            foreach (JObject jo in ja)
            {
                string text = jo.Value<string>("Text");
                Icon icon = IconHelper.String2Icon(jo.Value<string>("Icon"), true);
                string id = jo.Value<string>("ID");
                string url = jo.Value<string>("URL");

                if (!String.IsNullOrEmpty(text) && !String.IsNullOrEmpty(id) && !String.IsNullOrEmpty(url))
                {
                    MenuButton menuItem = new MenuButton();
                    menuItem.Text = text;
                    menuItem.Icon = icon;
                    menuItem.OnClientClick = String.Format("addExampleTab('{0}','{1}','{2}')", id, Url.Content(url), text);

                    items.Add(menuItem);
                }
            }

            return items;
        }

        #endregion

        #region GetTreeNodes

        /// <summary>
        /// 创建树菜单
        /// </summary>
        /// <param name="menus"></param>
        /// <returns></returns>
        private IList<TreeNode> GetTreeNodes(List<TestMVC.Models.Menu> menus)
        {
            IList<TreeNode> nodes = new List<TreeNode>();

            // 生成树
            ResolveMenuTree(menus, null, nodes);

            // 展开第一个树节点
            nodes[0].Expanded = true;

            return nodes;
        }

        /// <summary>
        /// 生成菜单树
        /// </summary>
        /// <param name="menus"></param>
        /// <param name="parentMenuId"></param>
        /// <param name="nodes"></param>
        private int ResolveMenuTree(List<TestMVC.Models.Menu> menus, TestMVC.Models.Menu parentMenu, IList<TreeNode> nodes)
        {
            int count = 0;
            foreach (var menu in menus.Where(m => m.Parent == parentMenu))
            {
                TreeNode node = new TreeNode();
                nodes.Add(node);
                count++;

                node.Text = menu.Name;
                node.IconUrl = menu.ImageUrl;
                if (!String.IsNullOrEmpty(menu.NavigateUrl))
                {
                    node.NavigateUrl = Url.Content(menu.NavigateUrl);
                }

                if (menu.Children.Count == 0)
                {
                    node.Leaf = true;

                    // 如果是叶子节点，但不是超链接，则是空目录，删除
                    if (String.IsNullOrEmpty(menu.NavigateUrl))
                    {
                        nodes.Remove(node);
                        count--;
                    }
                }
                else
                {
                    int childCount = ResolveMenuTree(menus, menu, node.Nodes);

                    // 如果是目录，但是计算的子节点数为0，可能目录里面的都是空目录，则要删除此父目录
                    if (childCount == 0 && String.IsNullOrEmpty(menu.NavigateUrl))
                    {
                        nodes.Remove(node);
                        count--;
                    }
                }

            }

            return count;
        }

        #endregion

        #region ResolveUserMenuList

        // 获取用户可用的菜单列表
        private List<TestMVC.Models.Menu> ResolveUserMenuList()
        {
            // 当前登陆用户的权限列表
            List<string> rolePowerNames = GetRolePowerNames();

            // 当前用户所属角色可用的菜单列表
            List<TestMVC.Models.Menu> menus = new List<TestMVC.Models.Menu>();

            foreach (var menu in db.Menus.Include(m => m.ViewPower).OrderBy(m => m.SortIndex))
            {
                // 如果此菜单不属于任何模块，或者此用户所属角色拥有对此模块的权限
                if (menu.ViewPower == null || rolePowerNames.Contains(menu.ViewPower.Name))
                {
                    menus.Add(menu);
                }
            }

            return menus;
        }

        #endregion

        #region onSignOut_Click

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult onSignOut_Click()
        {
            FormsAuthentication.SignOut();
            
            // 清空Session，因为可能会在Session中保存数据
            Session.Abandon();

            return RedirectToAction("Index", "Login");
        }


        #endregion


    }
}