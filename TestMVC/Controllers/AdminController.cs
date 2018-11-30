using TestMVC.Models;
using FineUIMvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace TestMVC.Controllers
{
    [Authorize]
    public class AdminController : BaseController
    {
        #region Index

        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        #endregion

        #region Help

        // GET: Admin/Help
        public ActionResult Help()
        {
            return View();
        }

        // GET: Admin/HelpWanNianLi
        public ActionResult HelpWanNianLi()
        {
            return View();
        }

        // GET: Admin/HelpJiSuanQi
        public ActionResult HelpJiSuanQi()
        {
            return View();
        }


        #endregion

        #region ChangePassword

        // GET: Admin/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword_btnSave_OnClick(string tbxOldPassword, string tbxNewPassword, string tbxConfirmNewPassword)
        {
            // 检查当前密码是否正确
            string oldPass = tbxOldPassword.Trim();
            string newPass = tbxNewPassword.Trim();
            string confirmNewPass = tbxConfirmNewPassword.Trim();

            if (newPass != confirmNewPass)
            {
                UIHelper.TextBox("tbxConfirmNewPassword").MarkInvalid("确认密码和新密码不一致！");
            }
            else
            {
                User user = db.Users.Where(u => u.Name == User.Identity.Name).FirstOrDefault();

                if (user != null)
                {
                    if (!PasswordUtil.ComparePasswords(user.Password, oldPass))
                    {
                        UIHelper.TextBox("tbxOldPassword").MarkInvalid("当前密码不正确！");
                    }
                    else
                    {
                        user.Password = PasswordUtil.CreateDbPassword(newPass);
                        db.SaveChanges();

                        Alert.ShowInTop("修改密码成功！");
                    }
                }
            }

            return UIHelper.Result();
        }

        #endregion

        #region Config

        // GET: Admin/Config
        [CheckPower(Name = "CoreConfigView")]
        public ActionResult Config()
        {
            Config_LoadData();

            return View();
        }

        private void Config_LoadData()
        {
            ViewBag.CoreConfigEdit = CheckPower("CoreConfigEdit");

            JSBeautifyLib.JSBeautify jsb = new JSBeautifyLib.JSBeautify(ConfigHelper.HelpList, new JSBeautifyLib.JSBeautifyOptions());
            ViewBag.HelpListText = jsb.GetResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPower(Name = "CoreConfigEdit")]
        public ActionResult Config_btnSave_OnClick(int ddlPageSize, string tbxHelpList)
        {
            try
            {
                JArray.Parse(tbxHelpList.Trim());
            }
            catch (Exception)
            {
                UIHelper.TextArea("tbxHelpList").MarkInvalid("格式不正确，必须是JSON字符串！");
                return UIHelper.Result();
            }

            //string title = tbxTitle.Trim();
            //if (title.Length > 100)
            //{
            //    title = title.Substring(0, 100);
            //}

            //ConfigHelper.Title = title;
            ConfigHelper.PageSize = ddlPageSize;
            ConfigHelper.HelpList = tbxHelpList.Trim();
            ConfigHelper.SaveAll();

            PageContext.RegisterStartupScript("top.window.location.reload(false);");

            return UIHelper.Result();
        }

        #endregion

        #region Online

        // GET: Admin/Online
        [CheckPower(Name = "CoreOnlineView")]
        public ActionResult Online()
        {
            return View(Online_LoadData());
        }

        private List<Online> Online_LoadData()
        {
            var pagingInfo = new PagingInfoViewModel
            {
                SortField = "UpdateTime",
                SortDirection = "DESC",
                PageIndex = 0,
                PageSize = ConfigHelper.PageSize
            };
            ViewBag.PagingInfo = pagingInfo;

            return Online_GetData(pagingInfo, String.Empty);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Online_DoPostBack(string[] Grid1_fields, int Grid1_pageIndex, string Grid1_sortField, string Grid1_sortDirection,
            string ttbSearchMessage, int ddlGridPageSize, string actionType)
        {
            var ttbSearchMessageUI = UIHelper.TwinTriggerBox("ttbSearchMessage");
            if (actionType == "trigger1")
            {
                ttbSearchMessageUI.Text(String.Empty);
                ttbSearchMessageUI.ShowTrigger1(false);

                // 清空传入的搜索值
                ttbSearchMessage = String.Empty;
            }
            else if (actionType == "trigger2")
            {
                ttbSearchMessageUI.ShowTrigger1(true);
            }


            var grid1UI = UIHelper.Grid("Grid1");
            var pagingInfo = new PagingInfoViewModel
            {
                SortField = Grid1_sortField,
                SortDirection = Grid1_sortDirection,
                PageIndex = Grid1_pageIndex,
                PageSize = ddlGridPageSize
            };
            grid1UI.DataSource(Online_GetData(pagingInfo, ttbSearchMessage), Grid1_fields);
            grid1UI.RecordCount(pagingInfo.RecordCount);

            return UIHelper.Result();
        }


        private List<Online> Online_GetData(PagingInfoViewModel pagingInfo, string ttbSearchMessage)
        {
            IQueryable<Online> q = db.Onlines.Include(o => o.User);

            // 表单搜索
            string searchText = ttbSearchMessage.Trim();
            if (!String.IsNullOrEmpty(searchText))
            {
                q = q.Where(o => o.User.Name.Contains(searchText));
            }

            // 2个小时内活跃的用户
            DateTime lastD = DateTime.Now.AddHours(-2);
            q = q.Where(o => o.UpdateTime > lastD);

            // 在添加条件之后，排序和分页之前获取总记录数
            pagingInfo.RecordCount = q.Count();

            // 排列和数据库分页
            q = SortAndPage<Online>(q, pagingInfo);

            return q.ToList();
        }


        #endregion

        #region Menu

        // GET: Admin/Menu
        [CheckPower(Name = "CoreMenuView")]
        public ActionResult Menu()
        {
            Menu_LoadData();

            return View(MenuHelper.Menus);
        }

        private void Menu_LoadData()
        {
            ViewBag.CoreMenuNew = CheckPower("CoreMenuNew");
            ViewBag.CoreMenuEdit = CheckPower("CoreMenuEdit");
            ViewBag.CoreMenuDelete = CheckPower("CoreMenuDelete");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPower(Name = "CoreMenuDelete")]
        public ActionResult Menu_DeleteRow(string[] Grid1_fields, int deletedRowID)
        {
            int childCount = db.Menus.Where(m => m.Parent.ID == deletedRowID).Count();
            if (childCount > 0)
            {
                Alert.ShowInTop("删除失败！请先删除子菜单！");
                return UIHelper.Result();
            }


            var menu = db.Menus.Where(m => m.ID == deletedRowID).FirstOrDefault();
            db.Menus.Remove(menu);
            db.SaveChanges();

            MenuHelper.Reload();
            UIHelper.Grid("Grid1").DataSource(MenuHelper.Menus, Grid1_fields);

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Menu_Window1_Close(string[] Grid1_fields)
        {
            MenuHelper.Reload();
            UIHelper.Grid("Grid1").DataSource(MenuHelper.Menus, Grid1_fields);

            return UIHelper.Result();
        }


        #endregion

        #region MenuEdit

        // GET: Admin/MenuEdit
        [CheckPower(Name = "CoreMenuEdit")]
        public ActionResult MenuEdit(int id)
        {
            TestMVC.Models.Menu current = db.Menus
                .Include(m => m.Parent).Include(m => m.ViewPower)
                .Where(m => m.ID == id).FirstOrDefault();
            if (current == null)
            {
                return Content("无效参数！");
            }

            MenuEdit_LoadData(id);

            return View(current);
        }

        private void MenuEdit_LoadData(int id)
        {
            ViewBag.IconItems = MenuEdit_GetIconItems().ToArray();

            ViewBag.MenuDataSource = ResolveDDL<TestMVC.Models.Menu>(MenuHelper.Menus, id).ToArray();

        }

        public List<RadioItem> MenuEdit_GetIconItems()
        {
            List<RadioItem> items = new List<RadioItem>();

            string[] icons = new string[] { "tag_yellow", "tag_red", "tag_purple", "tag_pink", "tag_orange", "tag_green", "tag_blue" };
            foreach (string icon in icons)
            {
                string value = String.Format("~/res/icon/{0}.png", icon);
                string text = String.Format("<img style=\"vertical-align:bottom;\" src=\"{0}\" />&nbsp;{1}", Url.Content(value), icon);

                items.Add(new RadioItem(text, value));
            }

            return items;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPower(Name = "CoreMenuEdit")]
        public ActionResult MenuEdit_btnSaveClose_Click([Bind(Include = "ID,Name,ImageUrl,NavigateUrl,Remark,SortIndex,ParentID")]TestMVC.Models.Menu menu, string ViewPowerName)
        {
            if (ModelState.IsValid)
            {
                // 下拉列表的顶级节点值为-1
                if (menu.ParentID == -1)
                {
                    menu.ParentID = null;
                }

                // 从 String -> ViewPower
                if (!String.IsNullOrEmpty(ViewPowerName))
                {
                    menu.ViewPower = db.Powers.Where(p => p.Name == ViewPowerName).FirstOrDefault();
                    menu.ViewPowerID = menu.ViewPower.ID;
                }
                else
                {
                    menu.ViewPower = null;
                    menu.ViewPowerID = null;
                }

                db.Entry(menu).State = EntityState.Modified;
                db.SaveChanges();

                // 关闭本窗体（触发窗体的关闭事件）
                ActiveWindow.HidePostBack();
            }

            return UIHelper.Result();
        }

        #endregion

        #region MenuNew

        // GET: Admin/MenuEdit
        [CheckPower(Name = "CoreMenuNew")]
        public ActionResult MenuNew()
        {
            MenuNew_LoadData();

            return View();
        }

        private void MenuNew_LoadData()
        {
            ViewBag.IconItems = MenuEdit_GetIconItems().ToArray();

            ViewBag.MenuDataSource = ResolveDDL<TestMVC.Models.Menu>(MenuHelper.Menus).ToArray();

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPower(Name = "CoreMenuNew")]
        public ActionResult MenuNew_btnSaveClose_Click([Bind(Include = "Name,ImageUrl,NavigateUrl,Remark,SortIndex,ParentID")]TestMVC.Models.Menu menu, string ViewPowerName)
        {
            if (ModelState.IsValid)
            {
                // 下拉列表的顶级节点值为-1
                if (menu.ParentID == -1)
                {
                    menu.ParentID = null;
                }

                // 从 String -> ViewPower
                if (!String.IsNullOrEmpty(ViewPowerName))
                {
                    menu.ViewPower = db.Powers.Where(p => p.Name == ViewPowerName).FirstOrDefault();
                }

                db.Menus.Add(menu);
                db.SaveChanges();

                // 关闭本窗体（触发窗体的关闭事件）
                ActiveWindow.HidePostBack();
            }

            return UIHelper.Result();
        }

        #endregion

        #region Power

        // GET: Admin/Power
        [CheckPower(Name = "CorePowerView")]
        public ActionResult Power()
        {
            return View(Power_LoadData());
        }

        private List<Power> Power_LoadData()
        {
            ViewBag.CorePowerNew = CheckPower("CorePowerNew");
            ViewBag.CorePowerEdit = CheckPower("CorePowerEdit");
            ViewBag.CorePowerDelete = CheckPower("CorePowerDelete");

            var pagingInfo = new PagingInfoViewModel
            {
                SortField = "GroupName",
                SortDirection = "DESC",
                PageIndex = 0,
                PageSize = ConfigHelper.PageSize
            };
            ViewBag.PagingInfo = pagingInfo;

            return Power_GetData(pagingInfo, String.Empty);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Power_DoPostBack(string[] Grid1_fields, int Grid1_pageIndex, string Grid1_sortField, string Grid1_sortDirection,
            string ttbSearchMessage, int ddlGridPageSize, string actionType, int? deletedRowID)
        {
            var ttbSearchMessageUI = UIHelper.TwinTriggerBox("ttbSearchMessage");
            if (actionType == "trigger1")
            {
                ttbSearchMessageUI.Text(String.Empty);
                ttbSearchMessageUI.ShowTrigger1(false);

                // 清空传入的搜索值
                ttbSearchMessage = String.Empty;
            }
            else if (actionType == "trigger2")
            {
                ttbSearchMessageUI.ShowTrigger1(true);
            }
            else if (actionType == "delete")
            {
                // 在操作之前进行权限检查
                if (!CheckPower("CorePowerDelete"))
                {
                    CheckPowerFailWithAlert();
                    return UIHelper.Result();
                }

                int roleCount = db.Roles.Where(r => r.Powers.Any(p => p.ID == deletedRowID.Value)).Count();
                if (roleCount > 0)
                {
                    Alert.ShowInTop("删除失败！需要先清空使用此权限的角色！");
                    return UIHelper.Result();
                }

                // 执行数据库操作
                var power = db.Powers.Where(m => m.ID == deletedRowID.Value).FirstOrDefault();
                db.Powers.Remove(power);
                db.SaveChanges();
            }


            var grid1UI = UIHelper.Grid("Grid1");
            var pagingInfo = new PagingInfoViewModel
            {
                SortField = Grid1_sortField,
                SortDirection = Grid1_sortDirection,
                PageIndex = Grid1_pageIndex,
                PageSize = ddlGridPageSize
            };

            var powers = Power_GetData(pagingInfo, ttbSearchMessage);
            // 1. 设置总项数
            grid1UI.RecordCount(pagingInfo.RecordCount);
            // 2. 设置每页显示项数
            if (actionType == "changeGridPageSize")
            {
                grid1UI.PageSize(ddlGridPageSize);
            }
            // 3.设置分页数据
            grid1UI.DataSource(powers, Grid1_fields);

            return UIHelper.Result();
        }


        private List<Power> Power_GetData(PagingInfoViewModel pagingInfo, string ttbSearchMessage)
        {
            IQueryable<Power> q = db.Powers;

            // 表单搜索
            string searchText = ttbSearchMessage.Trim();
            if (!String.IsNullOrEmpty(searchText))
            {
                q = q.Where(p => p.Name.Contains(searchText) || p.Title.Contains(searchText));
            }

            // 在查询之后，排序和分页之前获取总记录数
            pagingInfo.RecordCount = q.Count();

            // 排列和数据库分页
            q = SortAndPage<Power>(q, pagingInfo);

            return q.ToList();
        }

        #endregion

        #region PowerEdit

        // GET: Admin/PowerEdit
        [CheckPower(Name = "CorePowerEdit")]
        public ActionResult PowerEdit(int id)
        {
            Power current = db.Powers
                .Where(m => m.ID == id).FirstOrDefault();
            if (current == null)
            {
                return Content("无效参数！");
            }

            return View(current);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPower(Name = "CorePowerEdit")]
        public ActionResult PowerEdit_btnSaveClose_Click([Bind(Include = "ID,Name,GroupName,Title,Remark")]Power power)
        {
            if (ModelState.IsValid)
            {
                db.Entry(power).State = EntityState.Modified;
                db.SaveChanges();

                // 关闭本窗体（触发窗体的关闭事件）
                ActiveWindow.HidePostBack();
            }

            return UIHelper.Result();
        }

        #endregion

        #region PowerNew

        // GET: Admin/PowerEdit
        [CheckPower(Name = "CorePowerNew")]
        public ActionResult PowerNew()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPower(Name = "CorePowerNew")]
        public ActionResult PowerNew_btnSaveClose_Click([Bind(Include = "Name,GroupName,Title,Remark")]Power power)
        {
            if (ModelState.IsValid)
            {
                db.Powers.Add(power);
                db.SaveChanges();

                // 关闭本窗体（触发窗体的关闭事件）
                ActiveWindow.HidePostBack();
            }

            return UIHelper.Result();
        }

        #endregion

        #region Role

        // GET: Admin/Role
        [CheckPower(Name = "CoreRoleView")]
        public ActionResult Role()
        {
            return View(Role_LoadData());
        }

        private List<Role> Role_LoadData()
        {
            ViewBag.CoreRoleNew = CheckPower("CoreRoleNew");
            ViewBag.CoreRoleEdit = CheckPower("CoreRoleEdit");
            ViewBag.CoreRoleDelete = CheckPower("CoreRoleDelete");

            var pagingInfo = new PagingInfoViewModel
            {
                SortField = "Name",
                SortDirection = "DESC",
                PageIndex = 0,
                PageSize = ConfigHelper.PageSize
            };
            ViewBag.PagingInfo = pagingInfo;

            return Role_GetData(pagingInfo, String.Empty);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Role_DoPostBack(string[] Grid1_fields, int Grid1_pageIndex, string Grid1_sortField, string Grid1_sortDirection,
            string ttbSearchMessage, int ddlGridPageSize, string actionType, int? deletedRowID)
        {
            var ttbSearchMessageUI = UIHelper.TwinTriggerBox("ttbSearchMessage");
            if (actionType == "trigger1")
            {
                ttbSearchMessageUI.Text(String.Empty);
                ttbSearchMessageUI.ShowTrigger1(false);

                // 清空传入的搜索值
                ttbSearchMessage = String.Empty;
            }
            else if (actionType == "trigger2")
            {
                ttbSearchMessageUI.ShowTrigger1(true);
            }
            else if (actionType == "delete")
            {
                // 在操作之前进行权限检查
                if (!CheckPower("CoreRoleDelete"))
                {
                    CheckPowerFailWithAlert();
                    return UIHelper.Result();
                }

                int userCount = db.Users.Where(u => u.Roles.Any(r => r.ID == deletedRowID)).Count();
                if (userCount > 0)
                {
                    Alert.ShowInTop("删除失败！需要先清空属于此角色的用户！");
                    return UIHelper.Result();
                }

                // 执行数据库操作
                var Role = db.Roles.Where(m => m.ID == deletedRowID.Value).FirstOrDefault();
                db.Roles.Remove(Role);
                db.SaveChanges();
            }


            var grid1UI = UIHelper.Grid("Grid1");
            var pagingInfo = new PagingInfoViewModel
            {
                SortField = Grid1_sortField,
                SortDirection = Grid1_sortDirection,
                PageIndex = Grid1_pageIndex,
                PageSize = ddlGridPageSize
            };
            var roles = Role_GetData(pagingInfo, ttbSearchMessage);
            // 1. 设置总项数
            grid1UI.RecordCount(pagingInfo.RecordCount);
            // 2. 设置每页显示项数
            if (actionType == "changeGridPageSize")
            {
                grid1UI.PageSize(ddlGridPageSize);
            }
            // 3.设置分页数据
            grid1UI.DataSource(roles, Grid1_fields);

            
            return UIHelper.Result();
        }


        private List<Role> Role_GetData(PagingInfoViewModel pagingInfo, string ttbSearchMessage)
        {
            IQueryable<Role> q = db.Roles;

            // 表单搜索
            string searchText = ttbSearchMessage.Trim();
            if (!String.IsNullOrEmpty(searchText))
            {
                q = q.Where(p => p.Name.Contains(searchText));
            }

            // 在查询之后，排序和分页之前获取总记录数
            pagingInfo.RecordCount = q.Count();

            // 排列和数据库分页
            q = SortAndPage<Role>(q, pagingInfo);

            return q.ToList();
        }

        #endregion

        #region RoleEdit

        // GET: Admin/RoleEdit
        [CheckPower(Name = "CoreRoleEdit")]
        public ActionResult RoleEdit(int id)
        {
            Role current = db.Roles
                .Where(m => m.ID == id).FirstOrDefault();
            if (current == null)
            {
                return Content("无效参数！");
            }

            return View(current);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPower(Name = "CoreRoleEdit")]
        public ActionResult RoleEdit_btnSaveClose_Click([Bind(Include = "ID,Name,Remark")]Role Role)
        {
            if (ModelState.IsValid)
            {
                db.Entry(Role).State = EntityState.Modified;
                db.SaveChanges();

                // 关闭本窗体（触发窗体的关闭事件）
                ActiveWindow.HidePostBack();
            }

            return UIHelper.Result();
        }

        #endregion

        #region RoleNew

        // GET: Admin/RoleEdit
        [CheckPower(Name = "CoreRoleNew")]
        public ActionResult RoleNew()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPower(Name = "CoreRoleNew")]
        public ActionResult RoleNew_btnSaveClose_Click([Bind(Include = "Name,Remark")]Role Role)
        {
            if (ModelState.IsValid)
            {
                db.Roles.Add(Role);
                db.SaveChanges();

                // 关闭本窗体（触发窗体的关闭事件）
                ActiveWindow.HidePostBack();
            }

            return UIHelper.Result();
        }

        #endregion

        #region RoleUser

        // GET: Admin/RoleUser
        [CheckPower(Name = "CoreRoleUserView")]
        public ActionResult RoleUser()
        {
            ViewBag.CoreRoleUserNew = CheckPower("CoreRoleUserNew");
            ViewBag.CoreRoleUserDelete = CheckPower("CoreRoleUserDelete");

            // 表格1
            var grid1PagingInfo = new PagingInfoViewModel
            {
                SortField = "Name",
                SortDirection = "DESC"
            };
            var grid1DataSource = Sort<Role>(db.Roles, grid1PagingInfo).ToList();
            if (grid1DataSource.Count == 0)
            {
                // 没有角色数据
                return Content("请先添加角色！");
            }
            var grid1SelectedRowID = grid1DataSource[0].ID;

            ViewBag.Grid1SelectedRowID = grid1SelectedRowID.ToString();
            ViewBag.Grid1PagingInfo = grid1PagingInfo;
            ViewBag.Grid1DataSource = grid1DataSource;


            return View(RoleUser_LoadData(grid1SelectedRowID));
        }

        private List<User> RoleUser_LoadData(int grid1SelectedRowID)
        {
            // 表格2
            var grid2PagingInfo = new PagingInfoViewModel
            {
                SortField = "Name",
                SortDirection = "DESC",
                PageIndex = 0,
                PageSize = ConfigHelper.PageSize
            };
            ViewBag.Grid2PagingInfo = grid2PagingInfo;
            return RoleUser_GetData(grid2PagingInfo, grid1SelectedRowID, String.Empty);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RoleUser_Grid2_DoPostBack(string[] Grid2_fields, int Grid2_pageIndex, string Grid2_sortField, string Grid2_sortDirection,
            string ttbSearchMessage, int ddlGridPageSize, string actionType, int selectedRoleID, int[] deletedUserIDs)
        {
            var ttbSearchMessageUI = UIHelper.TwinTriggerBox("ttbSearchMessage");
            if (actionType == "trigger1")
            {
                ttbSearchMessageUI.Text(String.Empty);
                ttbSearchMessageUI.ShowTrigger1(false);

                // 清空传入的搜索值
                ttbSearchMessage = String.Empty;
            }
            else if (actionType == "trigger2")
            {
                ttbSearchMessageUI.ShowTrigger1(true);
            }
            else if (actionType == "delete")
            {
                // 在操作之前进行权限检查
                if (!CheckPower("CoreRoleUserDelete"))
                {
                    CheckPowerFailWithAlert();
                    return UIHelper.Result();
                }

                Role role = db.Roles.Include(r => r.Users)
                    .Where(r => r.ID == selectedRoleID)
                    .FirstOrDefault();

                //role.Users.Where(u => userIDs.Contains(u.ID)).ToList().ForEach(u => role.Users.Remove(u));
                foreach (int userID in deletedUserIDs)
                {
                    User user = role.Users.Where(u => u.ID == userID).FirstOrDefault();
                    if (user != null)
                    {
                        role.Users.Remove(user);
                    }
                }

                db.SaveChanges();
            }

            var grid2UI = UIHelper.Grid("Grid2");
            var pagingInfo = new PagingInfoViewModel
            {
                SortField = Grid2_sortField,
                SortDirection = Grid2_sortDirection,
                PageIndex = Grid2_pageIndex,
                PageSize = ddlGridPageSize
            };
            var roleUers = RoleUser_GetData(pagingInfo, selectedRoleID, ttbSearchMessage);
            // 1. 设置总项数
            grid2UI.RecordCount(pagingInfo.RecordCount);
            // 2. 设置每页显示项数
            if (actionType == "changeGridPageSize")
            {
                grid2UI.PageSize(ddlGridPageSize);
            }
            // 3.设置分页数据
            grid2UI.DataSource(roleUers, Grid2_fields);


            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RoleUser_Grid1_Sort(string[] Grid1_fields, string Grid1_sortField, string Grid1_sortDirection)
        {
            var grid1UI = UIHelper.Grid("Grid1");
            var pagingInfo = new PagingInfoViewModel
            {
                SortField = Grid1_sortField,
                SortDirection = Grid1_sortDirection
            };

            IQueryable<Role> q = db.Roles;
            grid1UI.DataSource(Sort<Role>(q, pagingInfo).ToList(), Grid1_fields, clearSelection: false);

            return UIHelper.Result();
        }

        private List<User> RoleUser_GetData(PagingInfoViewModel pagingInfo, int roleID, string ttbSearchMessage)
        {
            IQueryable<User> q = db.Users;

            // 在用户名称中搜索
            string searchText = ttbSearchMessage.Trim();
            if (!String.IsNullOrEmpty(searchText))
            {
                q = q.Where(u => u.Name.Contains(searchText) || u.ChineseName.Contains(searchText) || u.EnglishName.Contains(searchText));
            }

            q = q.Where(u => u.Name != "admin");

            // 过滤选中角色下的所有用户
            q = q.Where(u => u.Roles.Any(r => r.ID == roleID));

            // 在查询添加之后，排序和分页之前获取总记录数
            pagingInfo.RecordCount = q.Count();

            // 排列和分页
            q = SortAndPage<User>(q, pagingInfo);

            return q.ToList();
        }

        #endregion

        #region RoleUserNew

        // GET: Admin/RoleEdit
        [CheckPower(Name = "CoreRoleUserNew")]
        public ActionResult RoleUserNew(int roleID)
        {
            Role current = db.Roles.Find(roleID);
            if (current == null)
            {
                return Content("无效参数！");
            }

            return View(RoleUserNew_LoadData(roleID));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPower(Name = "CoreRoleUserNew")]
        public ActionResult RoleUserNew_btnSaveClose_Click(int roleID, int[] selectedRowIDs)
        {
            Role role = db.Roles.Include(r => r.Users)
                .Where(r => r.ID == roleID)
                .FirstOrDefault();

            foreach (int userID in selectedRowIDs)
            {
                User user = Attach<User>(userID);
                role.Users.Add(user);
            }
            db.SaveChanges();

            // 关闭本窗体（触发窗体的关闭事件）
            ActiveWindow.HidePostBack();

            return UIHelper.Result();
        }

        private List<User> RoleUserNew_LoadData(int roleID)
        {
            var pagingInfo = new PagingInfoViewModel
            {
                SortField = "Name",
                SortDirection = "DESC",
                PageIndex = 0,
                PageSize = ConfigHelper.PageSize
            };
            ViewBag.PagingInfo = pagingInfo;

            return RoleUserNew_GetData(pagingInfo, roleID, String.Empty);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RoleUserNew_DoPostBack(string[] Grid1_fields, int Grid1_pageIndex, string Grid1_sortField, string Grid1_sortDirection,
            string ttbSearchMessage, int ddlGridPageSize, string actionType, int roleID)
        {
            var ttbSearchMessageUI = UIHelper.TwinTriggerBox("ttbSearchMessage");
            if (actionType == "trigger1")
            {
                ttbSearchMessageUI.Text(String.Empty);
                ttbSearchMessageUI.ShowTrigger1(false);

                // 清空传入的搜索值
                ttbSearchMessage = String.Empty;
            }
            else if (actionType == "trigger2")
            {
                ttbSearchMessageUI.ShowTrigger1(true);
            }


            var grid1UI = UIHelper.Grid("Grid1");
            var pagingInfo = new PagingInfoViewModel
            {
                SortField = Grid1_sortField,
                SortDirection = Grid1_sortDirection,
                PageIndex = Grid1_pageIndex,
                PageSize = ddlGridPageSize
            };
            grid1UI.DataSource(RoleUserNew_GetData(pagingInfo, roleID, ttbSearchMessage), Grid1_fields, clearSelection: false);
            grid1UI.RecordCount(pagingInfo.RecordCount);

            return UIHelper.Result();
        }


        private List<User> RoleUserNew_GetData(PagingInfoViewModel pagingInfo, int roleID, string ttbSearchMessage)
        {
            IQueryable<User> q = db.Users;

            // 在名称中搜索
            string searchText = ttbSearchMessage.Trim();
            if (!String.IsNullOrEmpty(searchText))
            {
                q = q.Where(u => u.Name.Contains(searchText) || u.ChineseName.Contains(searchText) || u.EnglishName.Contains(searchText));
            }

            q = q.Where(u => u.Name != "admin");

            // 排除已经属于本角色的用户
            q = q.Where(u => u.Roles.All(r => r.ID != roleID));

            // 在查询之后，排序和分页之前获取总记录数
            pagingInfo.RecordCount = q.Count();

            // 排列和数据库分页
            q = SortAndPage<User>(q, pagingInfo);

            return q.ToList();
        }

        #endregion

        #region Title

        // GET: Admin/Title
        [CheckPower(Name = "CoreTitleView")]
        public ActionResult Title()
        {
            return View(Title_LoadData());
        }

        private List<Title> Title_LoadData()
        {
            ViewBag.CoreTitleNew = CheckPower("CoreTitleNew");
            ViewBag.CoreTitleEdit = CheckPower("CoreTitleEdit");
            ViewBag.CoreTitleDelete = CheckPower("CoreTitleDelete");

            var pagingInfo = new PagingInfoViewModel
            {
                SortField = "Name",
                SortDirection = "DESC",
                PageIndex = 0,
                PageSize = ConfigHelper.PageSize
            };
            ViewBag.PagingInfo = pagingInfo;

            return Title_GetData(pagingInfo, String.Empty);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Title_DoPostBack(string[] Grid1_fields, int Grid1_pageIndex, string Grid1_sortField, string Grid1_sortDirection,
            string ttbSearchMessage, int ddlGridPageSize, string actionType, int? deletedRowID)
        {
            var ttbSearchMessageUI = UIHelper.TwinTriggerBox("ttbSearchMessage");
            if (actionType == "trigger1")
            {
                ttbSearchMessageUI.Text(String.Empty);
                ttbSearchMessageUI.ShowTrigger1(false);

                // 清空传入的搜索值
                ttbSearchMessage = String.Empty;
            }
            else if (actionType == "trigger2")
            {
                ttbSearchMessageUI.ShowTrigger1(true);
            }
            else if (actionType == "delete")
            {
                // 在操作之前进行权限检查
                if (!CheckPower("CoreTitleDelete"))
                {
                    CheckPowerFailWithAlert();
                    return UIHelper.Result();
                }

                int userCount = db.Users.Where(u => u.Titles.Any(r => r.ID == deletedRowID)).Count();
                if (userCount > 0)
                {
                    Alert.ShowInTop("删除失败！需要先清空属于此职称的用户！");
                    return UIHelper.Result();
                }

                // 执行数据库操作
                var Title = db.Titles.Where(m => m.ID == deletedRowID.Value).FirstOrDefault();
                db.Titles.Remove(Title);
                db.SaveChanges();
            }


            var grid1UI = UIHelper.Grid("Grid1");
            var pagingInfo = new PagingInfoViewModel
            {
                SortField = Grid1_sortField,
                SortDirection = Grid1_sortDirection,
                PageIndex = Grid1_pageIndex,
                PageSize = ddlGridPageSize
            };
            var titles = Title_GetData(pagingInfo, ttbSearchMessage);
            // 1. 设置总项数
            grid1UI.RecordCount(pagingInfo.RecordCount);
            // 2. 设置每页显示项数
            if (actionType == "changeGridPageSize")
            {
                grid1UI.PageSize(ddlGridPageSize);
            }
            // 3.设置分页数据
            grid1UI.DataSource(titles, Grid1_fields);

            return UIHelper.Result();
        }


        private List<Title> Title_GetData(PagingInfoViewModel pagingInfo, string ttbSearchMessage)
        {
            IQueryable<Title> q = db.Titles;

            // 表单搜索
            string searchText = ttbSearchMessage.Trim();
            if (!String.IsNullOrEmpty(searchText))
            {
                q = q.Where(p => p.Name.Contains(searchText));
            }

            // 在查询之后，排序和分页之前获取总记录数
            pagingInfo.RecordCount = q.Count();

            // 排列和数据库分页
            q = SortAndPage<Title>(q, pagingInfo);

            return q.ToList();
        }

        #endregion

        #region TitleEdit

        // GET: Admin/TitleEdit
        [CheckPower(Name = "CoreTitleEdit")]
        public ActionResult TitleEdit(int id)
        {
            Title current = db.Titles
                .Where(m => m.ID == id).FirstOrDefault();
            if (current == null)
            {
                return Content("无效参数！");
            }

            return View(current);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPower(Name = "CoreTitleEdit")]
        public ActionResult TitleEdit_btnSaveClose_Click([Bind(Include = "ID,Name,Remark")]Title Title)
        {
            if (ModelState.IsValid)
            {
                db.Entry(Title).State = EntityState.Modified;
                db.SaveChanges();

                // 关闭本窗体（触发窗体的关闭事件）
                ActiveWindow.HidePostBack();
            }

            return UIHelper.Result();
        }

        #endregion

        #region TitleNew

        // GET: Admin/TitleEdit
        [CheckPower(Name = "CoreTitleNew")]
        public ActionResult TitleNew()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPower(Name = "CoreTitleNew")]
        public ActionResult TitleNew_btnSaveClose_Click([Bind(Include = "Name,Remark")]Title Title)
        {
            if (ModelState.IsValid)
            {
                db.Titles.Add(Title);
                db.SaveChanges();

                // 关闭本窗体（触发窗体的关闭事件）
                ActiveWindow.HidePostBack();
            }

            return UIHelper.Result();
        }

        #endregion

        #region TitleUser

        // GET: Admin/TitleUser
        [CheckPower(Name = "CoreTitleUserView")]
        public ActionResult TitleUser()
        {
            ViewBag.CoreTitleUserNew = CheckPower("CoreTitleUserNew");
            ViewBag.CoreTitleUserDelete = CheckPower("CoreTitleUserDelete");

            // 表格1
            var grid1PagingInfo = new PagingInfoViewModel
            {
                SortField = "Name",
                SortDirection = "DESC"
            };
            var grid1DataSource = Sort<Title>(db.Titles, grid1PagingInfo).ToList();
            if (grid1DataSource.Count == 0)
            {
                // 没有职称数据
                return Content("请先添加职称！");
            }
            var grid1SelectedRowID = grid1DataSource[0].ID;

            ViewBag.Grid1SelectedRowID = grid1SelectedRowID.ToString();
            ViewBag.Grid1PagingInfo = grid1PagingInfo;
            ViewBag.Grid1DataSource = grid1DataSource;

            return View(TitleUser_LoadData(grid1SelectedRowID));
        }

        private List<User> TitleUser_LoadData(int grid1SelectedRowID)
        {
            // 表格2
            var grid2PagingInfo = new PagingInfoViewModel
            {
                SortField = "Name",
                SortDirection = "DESC",
                PageIndex = 0,
                PageSize = ConfigHelper.PageSize
            };
            ViewBag.Grid2PagingInfo = grid2PagingInfo;
            return TitleUser_GetData(grid2PagingInfo, grid1SelectedRowID, String.Empty);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TitleUser_Grid2_DoPostBack(string[] Grid2_fields, int Grid2_pageIndex, string Grid2_sortField, string Grid2_sortDirection,
            string ttbSearchMessage, int ddlGridPageSize, string actionType, int selectedTitleId, int[] deletedUserIDs)
        {
            var ttbSearchMessageUI = UIHelper.TwinTriggerBox("ttbSearchMessage");
            if (actionType == "trigger1")
            {
                ttbSearchMessageUI.Text(String.Empty);
                ttbSearchMessageUI.ShowTrigger1(false);

                // 清空传入的搜索值
                ttbSearchMessage = String.Empty;
            }
            else if (actionType == "trigger2")
            {
                ttbSearchMessageUI.ShowTrigger1(true);
            }
            else if (actionType == "delete")
            {
                // 在操作之前进行权限检查
                if (!CheckPower("CoreTitleUserDelete"))
                {
                    CheckPowerFailWithAlert();
                    return UIHelper.Result();
                }

                Title role = db.Titles.Include(r => r.Users)
                    .Where(r => r.ID == selectedTitleId)
                    .FirstOrDefault();

                //role.Users.Where(u => userIDs.Contains(u.ID)).ToList().ForEach(u => role.Users.Remove(u));
                foreach (int userID in deletedUserIDs)
                {
                    User user = role.Users.Where(u => u.ID == userID).FirstOrDefault();
                    if (user != null)
                    {
                        role.Users.Remove(user);
                    }
                }

                db.SaveChanges();
            }

            var grid2UI = UIHelper.Grid("Grid2");
            var pagingInfo = new PagingInfoViewModel
            {
                SortField = Grid2_sortField,
                SortDirection = Grid2_sortDirection,
                PageIndex = Grid2_pageIndex,
                PageSize = ddlGridPageSize
            };
            var titleUsers = TitleUser_GetData(pagingInfo, selectedTitleId, ttbSearchMessage);
            // 1. 设置总项数
            grid2UI.RecordCount(pagingInfo.RecordCount);
            // 2. 设置每页显示项数
            if (actionType == "changeGridPageSize")
            {
                grid2UI.PageSize(ddlGridPageSize);
            }
            // 3.设置分页数据
            grid2UI.DataSource(titleUsers, Grid2_fields);

            
            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TitleUser_Grid1_Sort(string[] Grid1_fields, string Grid1_sortField, string Grid1_sortDirection)
        {
            var grid1UI = UIHelper.Grid("Grid1");
            var pagingInfo = new PagingInfoViewModel
            {
                SortField = Grid1_sortField,
                SortDirection = Grid1_sortDirection
            };

            IQueryable<Title> q = db.Titles;
            grid1UI.DataSource(Sort<Title>(q, pagingInfo).ToList(), Grid1_fields, clearSelection: false);

            return UIHelper.Result();
        }

        private List<User> TitleUser_GetData(PagingInfoViewModel pagingInfo, int titleID, string ttbSearchMessage)
        {
            IQueryable<User> q = db.Users;

            // 在用户名称中搜索
            string searchText = ttbSearchMessage.Trim();
            if (!String.IsNullOrEmpty(searchText))
            {
                q = q.Where(u => u.Name.Contains(searchText) || u.ChineseName.Contains(searchText) || u.EnglishName.Contains(searchText));
            }

            q = q.Where(u => u.Name != "admin");

            // 过滤选中职称下的所有用户
            q = q.Where(u => u.Titles.Any(r => r.ID == titleID));

            // 在查询添加之后，排序和分页之前获取总记录数
            pagingInfo.RecordCount = q.Count();

            // 排列和分页
            q = SortAndPage<User>(q, pagingInfo);

            return q.ToList();
        }

        #endregion

        #region TitleUserNew

        // GET: Admin/TitleEdit
        [CheckPower(Name = "CoreTitleUserNew")]
        public ActionResult TitleUserNew(int titleID)
        {
            Title current = db.Titles.Find(titleID);
            if (current == null)
            {
                return Content("无效参数！");
            }

            return View(TitleUserNew_LoadData(titleID));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPower(Name = "CoreTitleUserNew")]
        public ActionResult TitleUserNew_btnSaveClose_Click(int titleID, int[] selectedRowIDs)
        {
            Title role = db.Titles.Include(r => r.Users)
                .Where(r => r.ID == titleID)
                .FirstOrDefault();

            foreach (int userID in selectedRowIDs)
            {
                User user = Attach<User>(userID);
                role.Users.Add(user);
            }
            db.SaveChanges();

            // 关闭本窗体（触发窗体的关闭事件）
            ActiveWindow.HidePostBack();

            return UIHelper.Result();
        }

        private List<User> TitleUserNew_LoadData(int titleID)
        {
            var pagingInfo = new PagingInfoViewModel
            {
                SortField = "Name",
                SortDirection = "DESC",
                PageIndex = 0,
                PageSize = ConfigHelper.PageSize
            };
            ViewBag.PagingInfo = pagingInfo;

            return TitleUserNew_GetData(pagingInfo, titleID, String.Empty);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TitleUserNew_DoPostBack(string[] Grid1_fields, int Grid1_pageIndex, string Grid1_sortField, string Grid1_sortDirection,
            string ttbSearchMessage, int ddlGridPageSize, string actionType, int titleID)
        {
            var ttbSearchMessageUI = UIHelper.TwinTriggerBox("ttbSearchMessage");
            if (actionType == "trigger1")
            {
                ttbSearchMessageUI.Text(String.Empty);
                ttbSearchMessageUI.ShowTrigger1(false);

                // 清空传入的搜索值
                ttbSearchMessage = String.Empty;
            }
            else if (actionType == "trigger2")
            {
                ttbSearchMessageUI.ShowTrigger1(true);
            }


            var grid1UI = UIHelper.Grid("Grid1");
            var pagingInfo = new PagingInfoViewModel
            {
                SortField = Grid1_sortField,
                SortDirection = Grid1_sortDirection,
                PageIndex = Grid1_pageIndex,
                PageSize = ddlGridPageSize
            };
            grid1UI.DataSource(TitleUserNew_GetData(pagingInfo, titleID, ttbSearchMessage), Grid1_fields, clearSelection: false);
            grid1UI.RecordCount(pagingInfo.RecordCount);

            return UIHelper.Result();
        }


        private List<User> TitleUserNew_GetData(PagingInfoViewModel pagingInfo, int titleID, string ttbSearchMessage)
        {
            IQueryable<User> q = db.Users;

            // 在名称中搜索
            string searchText = ttbSearchMessage.Trim();
            if (!String.IsNullOrEmpty(searchText))
            {
                q = q.Where(u => u.Name.Contains(searchText) || u.ChineseName.Contains(searchText) || u.EnglishName.Contains(searchText));
            }

            q = q.Where(u => u.Name != "admin");

            // 排除已经属于本职称的用户
            q = q.Where(u => u.Titles.All(r => r.ID != titleID));

            // 在查询之后，排序和分页之前获取总记录数
            pagingInfo.RecordCount = q.Count();

            // 排列和数据库分页
            q = SortAndPage<User>(q, pagingInfo);

            return q.ToList();
        }

        #endregion

        #region Dept

        // GET: Admin/Dept
        [CheckPower(Name = "CoreDeptView")]
        public ActionResult Dept()
        {
            return View(Dept_LoadData());
        }

        private List<Dept> Dept_LoadData()
        {
            ViewBag.CoreDeptNew = CheckPower("CoreDeptNew");
            ViewBag.CoreDeptEdit = CheckPower("CoreDeptEdit");
            ViewBag.CoreDeptDelete = CheckPower("CoreDeptDelete");

            return DeptHelper.Depts;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Dept_DoPostBack(string[] Grid1_fields, string actionType, int? deletedRowID)
        {
            if (actionType == "delete")
            {
                // 在操作之前进行权限检查
                if (!CheckPower("CoreDeptDelete"))
                {
                    CheckPowerFailWithAlert();
                    return UIHelper.Result();
                }

                int userCount = db.Users.Where(u => u.Dept.ID == deletedRowID.Value).Count();
                if (userCount > 0)
                {
                    Alert.ShowInTop("删除失败！需要先清空属于此部门的用户！");
                    return UIHelper.Result();
                }

                int childCount = db.Depts.Where(d => d.Parent.ID == deletedRowID.Value).Count();
                if (childCount > 0)
                {
                    Alert.ShowInTop("删除失败！请先删除子部门！");
                    return UIHelper.Result();
                }

                var dept = db.Depts.Where(d => d.ID == deletedRowID.Value).FirstOrDefault();
                db.Depts.Remove(dept);
                db.SaveChanges();
            }


            DeptHelper.Reload();
            UIHelper.Grid("Grid1").DataSource(DeptHelper.Depts, Grid1_fields);
            return UIHelper.Result();
        }

        #endregion

        #region DeptEdit

        // GET: Admin/DeptEdit
        [CheckPower(Name = "CoreDeptEdit")]
        public ActionResult DeptEdit(int id)
        {
            Dept current = db.Depts
                .Where(m => m.ID == id).FirstOrDefault();
            if (current == null)
            {
                return Content("无效参数！");
            }

            ViewBag.DeptDataSource = ResolveDDL<Dept>(DeptHelper.Depts, id).ToArray();


            return View(current);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPower(Name = "CoreDeptEdit")]
        public ActionResult DeptEdit_btnSaveClose_Click([Bind(Include = "ID,Name,ParentID,SortIndex,Remark")]Dept dept)
        {
            if (ModelState.IsValid)
            {
                // 下拉列表的顶级节点值为-1
                if (dept.ParentID == -1)
                {
                    dept.ParentID = null;
                }

                db.Entry(dept).State = EntityState.Modified;
                db.SaveChanges();

                // 关闭本窗体（触发窗体的关闭事件）
                ActiveWindow.HidePostBack();
            }

            return UIHelper.Result();
        }

        #endregion

        #region DeptNew

        // GET: Admin/DeptEdit
        [CheckPower(Name = "CoreDeptNew")]
        public ActionResult DeptNew()
        {
            ViewBag.DeptDataSource = ResolveDDL<Dept>(DeptHelper.Depts).ToArray();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPower(Name = "CoreDeptNew")]
        public ActionResult DeptNew_btnSaveClose_Click([Bind(Include = "Name,ParentID,SortIndex,Remark")]Dept dept)
        {
            if (ModelState.IsValid)
            {
                // 下拉列表的顶级节点值为-1
                if (dept.ParentID == -1)
                {
                    dept.ParentID = null;
                }

                db.Depts.Add(dept);
                db.SaveChanges();

                // 关闭本窗体（触发窗体的关闭事件）
                ActiveWindow.HidePostBack();
            }

            return UIHelper.Result();
        }

        #endregion

        #region DeptUser

        // GET: Admin/DeptUser
        [CheckPower(Name = "CoreDeptUserView")]
        public ActionResult DeptUser()
        {
            ViewBag.CoreDeptUserNew = CheckPower("CoreDeptUserNew");
            ViewBag.CoreDeptUserDelete = CheckPower("CoreDeptUserDelete");

            // 表格1
            ViewBag.Grid1DataSource = DeptHelper.Depts;
            if (DeptHelper.Depts.Count == 0)
            {
                // 没有部门数据
                return Content("请先添加部门！");
            }
            var grid1SelectedRowID = DeptHelper.Depts[0].ID;
            ViewBag.Grid1SelectedRowID = grid1SelectedRowID.ToString();

            return View(DeptUser_LoadData(grid1SelectedRowID));
        }

        private List<User> DeptUser_LoadData(int grid1SelectedRowID)
        {
            // 表格2
            var grid2PagingInfo = new PagingInfoViewModel
            {
                SortField = "Name",
                SortDirection = "DESC",
                PageIndex = 0,
                PageSize = ConfigHelper.PageSize
            };
            ViewBag.Grid2PagingInfo = grid2PagingInfo;
            return DeptUser_GetData(grid2PagingInfo, grid1SelectedRowID, String.Empty);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeptUser_Grid2_DoPostBack(string[] Grid2_fields, int Grid2_pageIndex, string Grid2_sortField, string Grid2_sortDirection,
            string ttbSearchMessage, int ddlGridPageSize, string actionType, int selectedDeptId, int[] deletedUserIDs)
        {
            var ttbSearchMessageUI = UIHelper.TwinTriggerBox("ttbSearchMessage");
            if (actionType == "trigger1")
            {
                ttbSearchMessageUI.Text(String.Empty);
                ttbSearchMessageUI.ShowTrigger1(false);

                // 清空传入的搜索值
                ttbSearchMessage = String.Empty;
            }
            else if (actionType == "trigger2")
            {
                ttbSearchMessageUI.ShowTrigger1(true);
            }
            else if (actionType == "delete")
            {
                // 在操作之前进行权限检查
                if (!CheckPower("CoreDeptUserDelete"))
                {
                    CheckPowerFailWithAlert();
                    return UIHelper.Result();
                }

                Dept role = db.Depts.Include(r => r.Users)
                    .Where(r => r.ID == selectedDeptId)
                    .FirstOrDefault();

                //role.Users.Where(u => userIDs.Contains(u.ID)).ToList().ForEach(u => role.Users.Remove(u));
                foreach (int userID in deletedUserIDs)
                {
                    User user = role.Users.Where(u => u.ID == userID).FirstOrDefault();
                    if (user != null)
                    {
                        role.Users.Remove(user);
                    }
                }

                db.SaveChanges();
            }

            var grid2UI = UIHelper.Grid("Grid2");
            var pagingInfo = new PagingInfoViewModel
            {
                SortField = Grid2_sortField,
                SortDirection = Grid2_sortDirection,
                PageIndex = Grid2_pageIndex,
                PageSize = ddlGridPageSize
            };
            var deptUsers = DeptUser_GetData(pagingInfo, selectedDeptId, ttbSearchMessage);
            // 1. 设置总项数
            grid2UI.RecordCount(pagingInfo.RecordCount);
            // 2. 设置每页显示项数
            if (actionType == "changeGridPageSize")
            {
                grid2UI.PageSize(ddlGridPageSize);
            }
            // 3.设置分页数据
            grid2UI.DataSource(deptUsers, Grid2_fields);

            
            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeptUser_Grid1_Sort(string[] Grid1_fields, string Grid1_sortField, string Grid1_sortDirection)
        {
            var grid1UI = UIHelper.Grid("Grid1");
            var pagingInfo = new PagingInfoViewModel
            {
                SortField = Grid1_sortField,
                SortDirection = Grid1_sortDirection
            };

            IQueryable<Dept> q = db.Depts;
            grid1UI.DataSource(Sort<Dept>(q, pagingInfo).ToList(), Grid1_fields, clearSelection: false);

            return UIHelper.Result();
        }

        private List<User> DeptUser_GetData(PagingInfoViewModel pagingInfo, int deptID, string ttbSearchMessage)
        {
            IQueryable<User> q = db.Users;

            // 在用户名称中搜索
            string searchText = ttbSearchMessage.Trim();
            if (!String.IsNullOrEmpty(searchText))
            {
                q = q.Where(u => u.Name.Contains(searchText) || u.ChineseName.Contains(searchText) || u.EnglishName.Contains(searchText));
            }

            q = q.Where(u => u.Name != "admin");

            // 过滤选中部门下的所有用户
            q = q.Where(u => u.Dept.ID == deptID);

            // 在查询添加之后，排序和分页之前获取总记录数
            pagingInfo.RecordCount = q.Count();

            // 排列和分页
            q = SortAndPage<User>(q, pagingInfo);

            return q.ToList();
        }

        #endregion

        #region DeptUserNew

        // GET: Admin/DeptEdit
        [CheckPower(Name = "CoreDeptUserNew")]
        public ActionResult DeptUserNew(int deptID)
        {
            Dept current = db.Depts.Find(deptID);
            if (current == null)
            {
                return Content("无效参数！");
            }

            return View(DeptUserNew_LoadData(deptID));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPower(Name = "CoreDeptUserNew")]
        public ActionResult DeptUserNew_btnSaveClose_Click(int deptID, int[] selectedRowIDs)
        {
            Dept dept = Attach<Dept>(deptID);

            List<int> selectedRowList = new List<int>(selectedRowIDs);
            db.Users.Where(u => selectedRowList.Contains(u.ID))
                .ToList()
                .ForEach(u => u.Dept = dept);

            db.SaveChanges();

            // 关闭本窗体（触发窗体的关闭事件）
            ActiveWindow.HidePostBack();

            return UIHelper.Result();
        }

        private List<User> DeptUserNew_LoadData(int deptID)
        {
            var pagingInfo = new PagingInfoViewModel
            {
                SortField = "Name",
                SortDirection = "DESC",
                PageIndex = 0,
                PageSize = ConfigHelper.PageSize
            };
            ViewBag.PagingInfo = pagingInfo;

            return DeptUserNew_GetData(pagingInfo, deptID, String.Empty);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeptUserNew_DoPostBack(string[] Grid1_fields, int Grid1_pageIndex, string Grid1_sortField, string Grid1_sortDirection,
            string ttbSearchMessage, int ddlGridPageSize, string actionType, int deptID)
        {
            var ttbSearchMessageUI = UIHelper.TwinTriggerBox("ttbSearchMessage");
            if (actionType == "trigger1")
            {
                ttbSearchMessageUI.Text(String.Empty);
                ttbSearchMessageUI.ShowTrigger1(false);

                // 清空传入的搜索值
                ttbSearchMessage = String.Empty;
            }
            else if (actionType == "trigger2")
            {
                ttbSearchMessageUI.ShowTrigger1(true);
            }


            var grid1UI = UIHelper.Grid("Grid1");
            var pagingInfo = new PagingInfoViewModel
            {
                SortField = Grid1_sortField,
                SortDirection = Grid1_sortDirection,
                PageIndex = Grid1_pageIndex,
                PageSize = ddlGridPageSize
            };
            grid1UI.DataSource(DeptUserNew_GetData(pagingInfo, deptID, ttbSearchMessage), Grid1_fields, clearSelection: false);
            grid1UI.RecordCount(pagingInfo.RecordCount);

            return UIHelper.Result();
        }


        private List<User> DeptUserNew_GetData(PagingInfoViewModel pagingInfo, int deptID, string ttbSearchMessage)
        {
            IQueryable<User> q = db.Users;

            // 在名称中搜索
            string searchText = ttbSearchMessage.Trim();
            if (!String.IsNullOrEmpty(searchText))
            {
                q = q.Where(u => u.Name.Contains(searchText) || u.ChineseName.Contains(searchText) || u.EnglishName.Contains(searchText));
            }

            q = q.Where(u => u.Name != "admin");

            // 排除所有已经属于某个部门的用户
            q = q.Where(u => u.Dept == null);

            // 在查询之后，排序和分页之前获取总记录数
            pagingInfo.RecordCount = q.Count();

            // 排列和数据库分页
            q = SortAndPage<User>(q, pagingInfo);

            return q.ToList();
        }

        #endregion

        #region UserList

        // GET: Admin/User
        [CheckPower(Name = "CoreUserView")]
        public ActionResult UserList()
        {
            return View(UserList_LoadData());
        }

        private List<User> UserList_LoadData()
        {
            ViewBag.CoreUserNew = CheckPower("CoreUserNew");
            ViewBag.CoreUserEdit = CheckPower("CoreUserEdit");
            ViewBag.CoreUserDelete = CheckPower("CoreUserDelete");
            ViewBag.CoreUserChangePassword = CheckPower("CoreUserChangePassword");

            var pagingInfo = new PagingInfoViewModel
            {
                SortField = "Name",
                SortDirection = "DESC",
                PageIndex = 0,
                PageSize = ConfigHelper.PageSize
            };
            ViewBag.PagingInfo = pagingInfo;

            return UserList_GetData(pagingInfo, String.Empty, "all");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UserList_DoPostBack(string[] Grid1_fields, int Grid1_pageIndex, string Grid1_sortField, string Grid1_sortDirection,
            string ttbSearchMessage, string rblEnableStatus, int ddlGridPageSize, string actionType, int[] deletedRowIDs)
        {
            List<int> ids = new List<int>();
            if (deletedRowIDs != null)
            {
                ids.AddRange(deletedRowIDs);
            }

            var ttbSearchMessageUI = UIHelper.TwinTriggerBox("ttbSearchMessage");
            if (actionType == "trigger1")
            {
                ttbSearchMessageUI.Text(String.Empty);
                ttbSearchMessageUI.ShowTrigger1(false);

                // 清空传入的搜索值
                ttbSearchMessage = String.Empty;
            }
            else if (actionType == "trigger2")
            {
                ttbSearchMessageUI.ShowTrigger1(true);
            }
            else if (actionType == "delete")
            {
                // 在操作之前进行权限检查
                if (!CheckPower("CoreUserDelete"))
                {
                    CheckPowerFailWithAlert();
                    return UIHelper.Result();
                }

                db.Users.Where(u => ids.Contains(u.ID)).ToList().ForEach(u => db.Users.Remove(u));
                db.SaveChanges();
            }
            else if (actionType == "enable")
            {
                // 在操作之前进行权限检查
                if (!CheckPower("CoreUserEdit"))
                {
                    CheckPowerFailWithAlert();
                    return UIHelper.Result();
                }

                db.Users.Where(u => ids.Contains(u.ID)).ToList().ForEach(u => u.Enabled = true);
                db.SaveChanges();
            }
            else if (actionType == "disable")
            {
                // 在操作之前进行权限检查
                if (!CheckPower("CoreUserEdit"))
                {
                    CheckPowerFailWithAlert();
                    return UIHelper.Result();
                }

                db.Users.Where(u => ids.Contains(u.ID)).ToList().ForEach(u => u.Enabled = false);
                db.SaveChanges();
            }


            var grid1UI = UIHelper.Grid("Grid1");
            var pagingInfo = new PagingInfoViewModel
            {
                SortField = Grid1_sortField,
                SortDirection = Grid1_sortDirection,
                PageIndex = Grid1_pageIndex,
                PageSize = ddlGridPageSize
            };

            var users = UserList_GetData(pagingInfo, ttbSearchMessage, rblEnableStatus);
            // 1. 设置总项数
            grid1UI.RecordCount(pagingInfo.RecordCount);
            // 2. 设置每页显示项数
            if (actionType == "changeGridPageSize")
            {
                grid1UI.PageSize(ddlGridPageSize);
            }
            // 3.设置分页数据
            grid1UI.DataSource(users, Grid1_fields);
            

            return UIHelper.Result();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TestBindGridOnClick(JObject gridItem)
        {
           
            IQueryable<User> q = db.Users;
            BindGird(q, "Grid1",gridItem);
            //var grid1UI = UIHelper.Grid("Grid1");
            //var gridItemC = gridItem.ToObject<ViewModel.GridConfig>();
            //var pagingInfo = new PagingInfoViewModel
            //{
            //    RecordCount = q.Count(),
            //    SortField = gridItemC.SortField,
            //    SortDirection = gridItemC.SortDirection,
            //    PageIndex = gridItemC.PageIndex,
            //    PageSize = gridItemC.PageSize
            //};
            //// 1. 设置总项数
            //grid1UI.RecordCount(pagingInfo.RecordCount);
            //// 2. 设置每页显示项数
            //grid1UI.PageSize(gridItemC.PageSize);
            //// 3.设置分页数据
            //q = SortAndPage<User>(q, pagingInfo);
            //grid1UI.DataSource(q, gridItemC.Fields);

            //Alert.Show(Newtonsoft.Json.JsonConvert.SerializeObject(gridItemC));
            return UIHelper.Result();
        }

        private List<User> UserList_GetData(PagingInfoViewModel pagingInfo, string ttbSearchMessage, string rblEnableStatus)
        {
            IQueryable<User> q = db.Users;

            // 在用户名称中搜索
            string searchText = ttbSearchMessage.Trim();
            if (!String.IsNullOrEmpty(searchText))
            {
                q = q.Where(u => u.Name.Contains(searchText) || u.ChineseName.Contains(searchText) || u.EnglishName.Contains(searchText));
            }

            if (GetIdentityName() != "admin")
            {
                q = q.Where(u => u.Name != "admin");
            }

            // 过滤启用状态
            if (rblEnableStatus != "all")
            {
                q = q.Where(u => u.Enabled == (rblEnableStatus == "enabled" ? true : false));
            }

            // 在查询之后，排序和分页之前获取总记录数
            pagingInfo.RecordCount = q.Count();

            // 排列和数据库分页
            q = SortAndPage<User>(q, pagingInfo);

            return q.ToList();
        }

        #endregion

        #region UserEdit

        // GET: Admin/UserEdit
        [CheckPower(Name = "CoreUserEdit")]
        public ActionResult UserEdit(int id)
        {
            User current = db.Users
                .Include(u => u.Dept)
                .Include(u => u.Roles)
                .Include(u => u.Titles)
                .Where(m => m.ID == id).FirstOrDefault();
            if (current == null)
            {
                return Content("无效参数！");
            }

            if (current.Name == "admin" && GetIdentityName() != "admin")
            {
                return Content("你无权编辑超级管理员！");
            }

            // 用户所属角色
            ViewBag.SelectedRoleNames = String.Join(",", current.Roles.Select(u => u.Name).ToArray());
            ViewBag.SelectedRoleIDs = String.Join(",", current.Roles.Select(u => u.ID).ToArray());

            // 用户拥有职称
            ViewBag.SelectedTitleNames = String.Join(",", current.Titles.Select(u => u.Name).ToArray()); ;
            ViewBag.SelectedTitleIDs = String.Join(",", current.Titles.Select(u => u.ID).ToArray()); ;

            // 用户所属部门
            ViewBag.SelectedDeptName = current.Dept == null ? "" : current.Dept.Name;
            ViewBag.SelectedDeptID = current.Dept == null ? "" : current.Dept.ID.ToString();


            return View(current);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPower(Name = "CoreUserEdit")]
        public ActionResult UserEdit_btnSaveClose_Click([Bind(Include = "ID,ChineseName,Gender,Enabled,Email,CompanyEmail,OfficePhone,OfficePhoneExt,HomePhone,CellPhone,Remark")]User user,
            string hfSelectedDept, string hfSelectedRole, string hfSelectedTitle)
        {
            // 不对 Name 和 Password 进行模型验证
            ModelState.Remove("Name");
            ModelState.Remove("Password");

            if (ModelState.IsValid)
            {
                // 更新部分字段（先从数据库检索用户，再覆盖用户输入值，注意没有更新Name，Password，CreateTime等字段）
                var _user = db.Users
                    .Include(u => u.Dept)
                    .Include(u => u.Roles)
                    .Include(u => u.Titles)
                    .Where(u => u.ID == user.ID).FirstOrDefault();
                _user.ChineseName = user.ChineseName;
                _user.Gender = user.Gender;
                _user.Enabled = user.Enabled;
                _user.Email = user.Email;
                _user.CompanyEmail = user.CompanyEmail;
                _user.OfficePhone = user.OfficePhone;
                _user.OfficePhoneExt = user.OfficePhoneExt;
                _user.HomePhone = user.HomePhone;
                _user.CellPhone = user.CellPhone;
                _user.Remark = user.Remark;


                int[] roleIDs = TestMVC.Models.StringUtil.GetIntArrayFromString(hfSelectedRole);
                ReplaceEntities<Role>(_user.Roles, roleIDs);

                int[] titleIDs = TestMVC.Models.StringUtil.GetIntArrayFromString(hfSelectedTitle);
                ReplaceEntities<Title>(_user.Titles, titleIDs);

                if (String.IsNullOrEmpty(hfSelectedDept))
                {
                    _user.Dept = null;
                }
                else
                {
                    int newDeptID = Convert.ToInt32(hfSelectedDept);
                    Dept dept = Attach<Dept>(newDeptID);
                    _user.Dept = dept;
                }


                db.SaveChanges();

                // 关闭本窗体（触发窗体的关闭事件）
                ActiveWindow.HidePostBack();
            }

            return UIHelper.Result();
        }

        #endregion

        #region UserNew

        // GET: Admin/UserEdit
        [CheckPower(Name = "CoreUserNew")]
        public ActionResult UserNew()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPower(Name = "CoreUserNew")]
        public ActionResult UserNew_btnSaveClose_Click([Bind(Include = "Name,Password,ChineseName,Gender,Enabled,Email,CompanyEmail,OfficePhone,OfficePhoneExt,HomePhone,CellPhone,Remark")]User user,
            string hfSelectedDept, string hfSelectedRole, string hfSelectedTitle)
        {
            if (ModelState.IsValid)
            {
                var _user = db.Users.Where(u => u.Name == user.Name).FirstOrDefault();
                if (_user != null)
                {
                    Alert.Show("用户 " + user.Name + " 已经存在！");
                    return UIHelper.Result();
                }

                // 创建保存到数据库的密码
                user.Password = PasswordUtil.CreateDbPassword(user.Password.Trim());
                user.CreateTime = DateTime.Now;


                // 添加所有部门
                if (!String.IsNullOrEmpty(hfSelectedDept))
                {
                    Dept dept = Attach<Dept>(Convert.ToInt32(hfSelectedDept));
                    user.Dept = dept;
                }

                // 添加所有角色
                if (!String.IsNullOrEmpty(hfSelectedRole))
                {
                    user.Roles = new List<Role>();
                    int[] roleIDs = TestMVC.Models.StringUtil.GetIntArrayFromString(hfSelectedRole);

                    AddEntities<Role>(user.Roles, roleIDs);
                }

                // 添加所有职称
                if (!String.IsNullOrEmpty(hfSelectedTitle))
                {
                    user.Titles = new List<Title>();
                    int[] titleIDs = TestMVC.Models.StringUtil.GetIntArrayFromString(hfSelectedTitle);

                    AddEntities<Title>(user.Titles, titleIDs);
                }


                db.Users.Add(user);
                db.SaveChanges();

                // 关闭本窗体（触发窗体的关闭事件）
                ActiveWindow.HidePostBack();
            }

            return UIHelper.Result();
        }

        #endregion

        #region UserView

        // GET: Admin/UserView
        public ActionResult UserView(int id)
        {
            User current = db.Users
                .Include(u => u.Roles)
                .Include(u => u.Dept)
                .Include(u => u.Titles)
                .Where(u => u.ID == id).FirstOrDefault();
            if (current == null)
            {
                return Content("无效参数！");
            }

            // 用户所属角色
            ViewBag.RoleText = String.Join(",", current.Roles.Select(r => r.Name).ToArray());

            // 用户的职称列表
            ViewBag.TitleText = String.Join(",", current.Titles.Select(t => t.Name).ToArray());

            // 用户所属的部门
            string deptText = String.Empty;
            if (current.Dept != null)
            {
                deptText = current.Dept.Name;
            }
            ViewBag.DeptText = deptText;

            return View(current);
        }

        #endregion

        #region UserChangePassword

        // GET: Admin/User
        [CheckPower(Name = "CoreUserChangePassword")]
        public ActionResult UserChangePassword(int id)
        {
            User current = db.Users
                .Where(m => m.ID == id).FirstOrDefault();
            if (current == null)
            {
                return Content("无效参数！");
            }

            if (current.Name == "admin" && GetIdentityName() != "admin")
            {
                return Content("你无权编辑超级管理员！");
            }

            return View(current);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPower(Name = "CoreUserChangePassword")]
        public ActionResult UserChangePassword_btnSaveClose_Click(int id, string tbxPassword)
        {
            var item = db.Users.Find(id);
            item.Password = PasswordUtil.CreateDbPassword(tbxPassword.Trim());
            db.SaveChanges();

            // 关闭本窗体（触发窗体的关闭事件）
            ActiveWindow.HidePostBack();

            return UIHelper.Result();
        }


        #endregion

        #region RolePower

        // GET: Admin/RolePower
        [CheckPower(Name = "CoreRolePowerView")]
        public ActionResult RolePower()
        {
            ViewBag.CoreRolePowerEdit = CheckPower("CoreRolePowerEdit");

            // 表格1
            var grid1PagingInfo = new PagingInfoViewModel
            {
                SortField = "Name",
                SortDirection = "DESC"
            };
            var grid1DataSource = Sort<Role>(db.Roles, grid1PagingInfo).ToList();
            if (grid1DataSource.Count == 0)
            {
                // 没有角色数据
                return Content("请先添加角色！");
            }
            var grid1SelectedRowID = grid1DataSource[0].ID;

            ViewBag.Grid1SelectedRowID = grid1SelectedRowID.ToString();
            ViewBag.Grid1PagingInfo = grid1PagingInfo;
            ViewBag.Grid1DataSource = grid1DataSource;

            return View(RolePower_LoadData(grid1SelectedRowID));
        }

        private List<GroupPowerViewModel> RolePower_LoadData(int grid1SelectedRowID)
        {
            // 当前选中角色拥有的权限列表
            ViewBag.RolePowerIds = RolePower_GetRolePowerIds(grid1SelectedRowID);

            // 表格2
            var grid2PagingInfo = new PagingInfoViewModel
            {
                SortField = "GroupName",
                SortDirection = "DESC",
            };
            ViewBag.Grid2PagingInfo = grid2PagingInfo;
            return RolePower_GetData(grid2PagingInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RolePower_Grid2_DoPostBack(string[] Grid2_fields, string Grid2_sortField, string Grid2_sortDirection,
            string actionType, int selectedRoleID, int[] selectedPowerIDs)
        {
            // 保存角色权限时，不需要重新加载表格数据
            if (actionType == "saveall")
            {
                // 在操作之前进行权限检查
                if (!CheckPower("CoreRolePowerEdit"))
                {
                    CheckPowerFailWithAlert();
                    return UIHelper.Result();
                }


                // 当前角色新的权限列表
                Role role = db.Roles.Include(r => r.Powers).Where(r => r.ID == selectedRoleID).FirstOrDefault();

                ReplaceEntities<Power>(role.Powers, selectedPowerIDs);

                db.SaveChanges();

                Alert.ShowInTop("当前角色的权限更新成功！");
            }
            else
            {
                var grid2UI = UIHelper.Grid("Grid2");
                var pagingInfo = new PagingInfoViewModel
                {
                    SortField = Grid2_sortField,
                    SortDirection = Grid2_sortDirection
                };
                grid2UI.DataSource(RolePower_GetData(pagingInfo), Grid2_fields);

                // 更新当前角色的权限
                PageContext.RegisterStartupScript("updateRolePowers(" + RolePower_GetRolePowerIds(selectedRoleID) + ");");
            }

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RolePower_Grid1_Sort(string[] Grid1_fields, string Grid1_sortField, string Grid1_sortDirection)
        {
            var grid1UI = UIHelper.Grid("Grid1");
            var pagingInfo = new PagingInfoViewModel
            {
                SortField = Grid1_sortField,
                SortDirection = Grid1_sortDirection
            };

            IQueryable<Role> q = db.Roles;
            grid1UI.DataSource(Sort<Role>(q, pagingInfo).ToList(), Grid1_fields, clearSelection: false);

            return UIHelper.Result();
        }


        private string RolePower_GetRolePowerIds(int grid1SelectedRowID)
        {
            // 当前选中角色拥有的权限列表
            Role role = db.Roles.Include(r => r.Powers).Where(r => r.ID == grid1SelectedRowID).FirstOrDefault();

            return new JArray(role.Powers.Select(p => p.ID)).ToString(Newtonsoft.Json.Formatting.None);
        }

        private List<GroupPowerViewModel> RolePower_GetData(PagingInfoViewModel pagingInfo)
        {
            // http://stackoverflow.com/questions/11393068/ef-4-3-using-local-cache-instead-of-re-fetching-from-database
            // In order to explicitly force a refetch use AsNoTracking().
            //var q = db.Powers.AsNoTracking().GroupBy(p => p.GroupName);

            var q = db.Powers.GroupBy(p => p.GroupName);
            if (pagingInfo.SortField == "GroupName")
            {
                if (pagingInfo.SortDirection == "ASC")
                {
                    q = q.OrderBy(g => g.Key);
                }
                else
                {
                    q = q.OrderByDescending(g => g.Key);
                }
            }

            var powers = q.ToList();

            List<GroupPowerViewModel> groupPowers = new List<GroupPowerViewModel>();
            foreach (var power in powers)
            {
                var groupPower = new GroupPowerViewModel();
                groupPower.GroupName = power.Key;

                JArray ja = new JArray();
                foreach (var powerItem in power.ToList())
                {
                    JObject jo = new JObject();
                    jo.Add("id", powerItem.ID);
                    jo.Add("name", powerItem.Name);
                    jo.Add("title", powerItem.Title);
                    ja.Add(jo);
                }
                groupPower.Powers = ja;

                groupPowers.Add(groupPower);
            }

            return groupPowers;
        }

        #endregion

        #region UserSelectRole

        // GET: Admin/UserSelectRole
        [CheckPower(Name = "CoreRoleView")]
        public ActionResult UserSelectRole(string ids)
        {
            ViewBag.RoleSelectedValueArray = ids.Split(',');

            return View(db.Roles.ToList());
        }

        #endregion

        #region UserSelectTitle

        // GET: Admin/UserSelectTitle
        [CheckPower(Name = "CoreTitleView")]
        public ActionResult UserSelectTitle(string ids)
        {
            ViewBag.TitleSelectedValueArray = ids.Split(',');

            return View(db.Titles.ToList());
        }

        #endregion

        #region UserSelectDept

        // GET: Admin/UserSelectDept
        [CheckPower(Name = "CoreDeptView")]
        public ActionResult UserSelectDept(string ids)
        {
            ViewBag.DeptSelectedRowID = ids;

            return View(DeptHelper.Depts);
        }

        #endregion

        
    }
}