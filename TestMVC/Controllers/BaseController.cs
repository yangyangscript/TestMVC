using FineUIMvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Data.Entity;
using Newtonsoft.Json.Linq;
using TestMVC.Models;

namespace TestMVC.Controllers
{
    public class BaseController : Controller
    {
        protected AppBoxMvcContext db = new AppBoxMvcContext();

        #region 只读静态变量

        private static readonly string SK_ONLINE_UPDATE_TIME = "OnlineUpdateTime";

        public static readonly string CHECK_POWER_FAIL_PAGE_MESSAGE = "您无权访问此页面！";
        public static readonly string CHECK_POWER_FAIL_ACTION_MESSAGE = "您无权进行此操作！";

        #endregion

        #region OnActionExecuting

        /// <summary>
        /// 动作方法调用之前执行
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            // 如果用户已经登录，更新在线记录
            if (User.Identity.IsAuthenticated)
            {
                UpdateOnlineUser(User.Identity.Name);
            }
        }


        #endregion

        #region ShowNotify

        /// <summary>
        /// 显示通知对话框
        /// </summary>
        /// <param name="message"></param>
        public virtual void ShowNotify(string message)
        {
            ShowNotify(message, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 显示通知对话框
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageIcon"></param>
        public virtual void ShowNotify(string message, MessageBoxIcon messageIcon)
        {
            ShowNotify(message, messageIcon, Target.Top);
        }

        /// <summary>
        /// 显示通知对话框
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageIcon"></param>
        /// <param name="target"></param>
        public virtual void ShowNotify(string message, MessageBoxIcon messageIcon, Target target)
        {
            Notify n = new Notify();
            n.Target = target;
            n.Message = message;
            n.MessageBoxIcon = messageIcon;
            n.PositionX = Position.Center;
            n.PositionY = Position.Top;
            n.DisplayMilliseconds = 3000;
            n.ShowHeader = false;

            n.Show();
        }


        #endregion

        #region 在线用户相关

        protected void UpdateOnlineUser(string username)
        {
            DateTime now = DateTime.Now;
            object lastUpdateTime = Session[SK_ONLINE_UPDATE_TIME];
            if (lastUpdateTime == null || (Convert.ToDateTime(lastUpdateTime).AddMinutes(5) < now))
            {
                // 记录本次更新时间
                Session[SK_ONLINE_UPDATE_TIME] = now;

                Online online = db.Onlines.Where(o => o.User.Name == username).FirstOrDefault();
                if (online != null)
                {
                    online.UpdateTime = now;
                    db.SaveChanges();
                }
            }
        }

        protected void RegisterOnlineUser(User user)
        {
            Online online = db.Onlines.Where(o => o.User.ID == user.ID).FirstOrDefault();

            // 如果不存在，就创建一条新的记录
            if (online == null)
            {
                online = new Online();
                db.Onlines.Add(online);
            }
            DateTime now = DateTime.Now;
            online.User = user;
            online.IPAdddress = Request.UserHostAddress;
            online.LoginTime = now;
            online.UpdateTime = now;

            db.SaveChanges();

            // 记录本次更新时间
            Session[SK_ONLINE_UPDATE_TIME] = now;
        }

        /// <summary>
        /// 在线人数
        /// </summary>
        /// <returns></returns>
        protected int GetOnlineCount()
        {
            DateTime lastM = DateTime.Now.AddMinutes(-15);
            return db.Onlines.Where(o => o.UpdateTime > lastM).Count();
        }

        #endregion

        #region 当前登录用户信息

        // http://blog.163.com/zjlovety@126/blog/static/224186242010070024282/
        // http://www.cnblogs.com/gaoshuai/articles/1863231.html
        /// <summary>
        /// 当前登录用户的角色列表
        /// </summary>
        /// <returns></returns>
        protected List<int> GetIdentityRoleIDs()
        {
            return GetIdentityRoleIDs(HttpContext);
        }

        /// <summary>
        /// 当前登录用户名
        /// </summary>
        /// <returns></returns>
        protected string GetIdentityName()
        {
            return GetIdentityName(HttpContext);
        }


        /// <summary>
        /// 创建表单验证的票证并存储在客户端Cookie中
        /// </summary>
        /// <param name="userName">当前登录用户名</param>
        /// <param name="roleIDs">当前登录用户的角色ID列表</param>
        /// <param name="isPersistent">是否跨浏览器会话保存票证</param>
        /// <param name="expiration">过期时间</param>
        protected void CreateFormsAuthenticationTicket(string userName, string roleIDs, bool isPersistent, DateTime expiration)
        {
            // 创建Forms身份验证票据
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1,
                userName,                       // 与票证关联的用户名
                DateTime.Now,                   // 票证发出时间
                expiration,                     // 票证过期时间
                isPersistent,                   // 如果票证将存储在持久性 Cookie 中（跨浏览器会话保存），则为 true；否则为 false。
                roleIDs                         // 存储在票证中的用户特定的数据
             );

            // 对Forms身份验证票据进行加密，然后保存到客户端Cookie中
            string hashTicket = FormsAuthentication.Encrypt(ticket);
            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, hashTicket);
            cookie.HttpOnly = true;
            // 1. 关闭浏览器即删除（Session Cookie）：DateTime.MinValue
            // 2. 指定时间后删除：大于 DateTime.Now 的某个值
            // 3. 删除Cookie：小于 DateTime.Now 的某个值
            if (isPersistent)
            {
                cookie.Expires = expiration;
            }
            else
            {
                cookie.Expires = DateTime.MinValue;
            }
            Response.Cookies.Add(cookie);
        }

        #endregion

        #region 权限相关

        /// <summary>
        /// 检查当前用户是否拥有某个权限
        /// </summary>
        /// <param name="powerType"></param>
        /// <returns></returns>
        protected bool CheckPower(string powerName)
        {
            return CheckPower(HttpContext, powerName);
        }

        /// <summary>
        /// 获取当前登录用户拥有的全部权限列表
        /// </summary>
        /// <param name="roleIDs"></param>
        /// <returns></returns>
        protected List<string> GetRolePowerNames()
        {
            return GetRolePowerNames(HttpContext);
        }

        /// <summary>
        /// 检查权限失败（页面第一次加载）
        /// </summary>
        public static void CheckPowerFailWithPage(HttpContextBase context)
        {
            context.Response.Write(CHECK_POWER_FAIL_PAGE_MESSAGE);
            context.Response.End();
        }

        /// <summary>
        /// 检查权限失败（页面回发）
        /// </summary>
        public static void CheckPowerFailWithAlert()
        {
            PageContext.RegisterStartupScript(Alert.GetShowInTopReference(CHECK_POWER_FAIL_ACTION_MESSAGE));
        }

        /// <summary>
        /// 检查当前用户是否拥有某个权限
        /// </summary>
        /// <param name="context"></param>
        /// <param name="powerName"></param>
        /// <returns></returns>
        public static bool CheckPower(HttpContextBase context, string powerName)
        {
            // 如果权限名为空，则放行
            if (String.IsNullOrEmpty(powerName))
            {
                return true;
            }

            // 当前登陆用户的权限列表
            List<string> rolePowerNames = GetRolePowerNames(context);
            if (rolePowerNames.Contains(powerName))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取当前登录用户拥有的全部权限列表
        /// </summary>
        /// <param name="roleIDs"></param>
        /// <returns></returns>
        public static List<string> GetRolePowerNames(HttpContextBase context)
        {
            using (var db = new AppBoxMvcContext())
            {
                // 将用户拥有的权限列表保存在Session中，这样就避免每个请求多次查询数据库
                if (context.Session["UserPowerList"] == null)
                {
                    List<string> rolePowerNames = new List<string>();

                    // 超级管理员拥有所有权限
                    if (GetIdentityName(context) == "admin")
                    {
                        rolePowerNames = db.Powers.Select(p => p.Name).ToList();
                    }
                    else
                    {
                        List<int> roleIDs = GetIdentityRoleIDs(context);

                        foreach (var role in db.Roles.Include(r => r.Powers).Where(r => roleIDs.Contains(r.ID)))
                        {
                            foreach (var power in role.Powers)
                            {
                                if (!rolePowerNames.Contains(power.Name))
                                {
                                    rolePowerNames.Add(power.Name);
                                }
                            }
                        }
                    }

                    context.Session["UserPowerList"] = rolePowerNames;
                }
            }

            return (List<string>)context.Session["UserPowerList"];
        }

        // http://blog.163.com/zjlovety@126/blog/static/224186242010070024282/
        // http://www.cnblogs.com/gaoshuai/articles/1863231.html
        /// <summary>
        /// 当前登录用户的角色列表
        /// </summary>
        /// <returns></returns>
        public static List<int> GetIdentityRoleIDs(HttpContextBase context)
        {
            List<int> roleIDs = new List<int>();

            if (context.User.Identity.IsAuthenticated)
            {
                FormsAuthenticationTicket ticket = ((FormsIdentity)context.User.Identity).Ticket;
                string userData = ticket.UserData;

                foreach (string roleID in userData.Split(','))
                {
                    if (!String.IsNullOrEmpty(roleID))
                    {
                        roleIDs.Add(Convert.ToInt32(roleID));
                    }
                }
            }

            return roleIDs;
        }

        /// <summary>
        /// 当前登录用户名
        /// </summary>
        /// <returns></returns>
        public static string GetIdentityName(HttpContextBase context)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                return context.User.Identity.Name;
            }
            return String.Empty;
        }

        #endregion

        #region EF相关

        protected IQueryable<T> Sort<T>(IQueryable<T> q, PagingInfoViewModel pagingInfo)
        {
            return q.SortBy(pagingInfo.SortField + " " + pagingInfo.SortDirection);
        }

        // 排序
        protected IQueryable<T> Sort<T>(IQueryable<T> q, string sortField, string sortDirection)
        {
            return q.SortBy(sortField + " " + sortDirection);
        }

        //// 排序后分页
        //protected IQueryable<T> SortAndPage<T>(IQueryable<T> q, int recordCount, FormCollection values, string girdName)
        //{
        //    int pageIndex = Convert.ToInt32(values[girdName + "_pageIndex"]);
        //    int pageSize = Convert.ToInt32(values[girdName + "_pageSize"]);
        //    string sortField = values[girdName + "_sortField"];
        //    string sortDirection = values[girdName + "_sortDirection"];

        //    return SortAndPage(q, pageIndex, pageSize, recordCount, sortField, sortDirection);
        //}

        protected IQueryable<T> SortAndPage<T>(IQueryable<T> q, PagingInfoViewModel pagingInfo)
        {
            return SortAndPage(q, pagingInfo.PageIndex, pagingInfo.PageSize, pagingInfo.RecordCount, pagingInfo.SortField, pagingInfo.SortDirection);
        }

        // 排序后分页
        protected IQueryable<T> SortAndPage<T>(IQueryable<T> q, int pageIndex, int pageSize, int recordCount, string sortField, string sortDirection)
        {
            //// 对传入的 pageIndex 进行有效性验证//////////////
            int pageCount = recordCount / pageSize;
            if (recordCount % pageSize != 0)
            {
                pageCount++;
            }
            if (pageIndex > pageCount - 1)
            {
                pageIndex = pageCount - 1;
            }
            if (pageIndex < 0)
            {
                pageIndex = 0;
            }
            ///////////////////////////////////////////////

            return Sort(q, sortField, sortDirection).Skip(pageIndex * pageSize).Take(pageSize);
        }


        // 附加实体到数据库上下文中（首先在Local中查找实体是否存在，不存在才Attach，否则会报错）
        // http://patrickdesjardins.com/blog/entity-framework-4-3-an-object-with-the-same-key-already-exists-in-the-objectstatemanager
        protected T Attach<T>(int keyID) where T : class, IKeyID, new()
        {
            T t = db.Set<T>().Local.Where(x => x.ID == keyID).FirstOrDefault();
            if (t == null)
            {
                t = new T { ID = keyID };
                db.Set<T>().Attach(t);
            }
            return t;
        }

        // 向现有实体集合中添加新项
        protected void AddEntities<T>(ICollection<T> existItems, int[] newItemIDs) where T : class,  IKeyID, new()
        {
            foreach (int roleID in newItemIDs)
            {
                T t = Attach<T>(roleID);
                existItems.Add(t);
            }
        }

        // 替换现有实体集合中的所有项
        // http://stackoverflow.com/questions/2789113/entity-framework-update-entity-along-with-child-entities-add-update-as-necessar
        protected void ReplaceEntities<T>(ICollection<T> existEntities, int[] newEntityIDs) where T : class,  IKeyID, new()
        {
            if (newEntityIDs.Length == 0)
            {
                existEntities.Clear();
            }
            else
            {
                int[] tobeAdded = newEntityIDs.Except(existEntities.Select(x => x.ID)).ToArray();
                int[] tobeRemoved = existEntities.Select(x => x.ID).Except(newEntityIDs).ToArray();

                AddEntities<T>(existEntities, tobeAdded);

                existEntities.Where(x => tobeRemoved.Contains(x.ID)).ToList().ForEach(e => existEntities.Remove(e));
            }
        }

        // http://patrickdesjardins.com/blog/validation-failed-for-one-or-more-entities-see-entityvalidationerrors-property-for-more-details-2
        // ((System.Data.Entity.Validation.DbEntityValidationException)$exception).EntityValidationErrors

        #endregion

        #region 模拟树的下拉列表

        protected List<T> ResolveDDL<T>(List<T> mys) where T : ICustomTree, ICloneable, IKeyID, new()
        {
            return ResolveDDL<T>(mys, -1, true);
        }

        protected List<T> ResolveDDL<T>(List<T> mys, int currentId) where T : ICustomTree, ICloneable, IKeyID, new()
        {
            return ResolveDDL<T>(mys, currentId, true);
        }


        // 将一个树型结构放在一个下列列表中可供选择
        protected List<T> ResolveDDL<T>(List<T> source, int currentID, bool addRootNode) where T : ICustomTree, ICloneable, IKeyID, new()
        {
            List<T> result = new List<T>();

            if (addRootNode)
            {
                // 添加根节点
                T root = new T();
                root.Name = "--根节点--";
                root.ID = -1;
                root.TreeLevel = 0;
                root.Enabled = true;
                result.Add(root);
            }

            foreach (T item in source)
            {
                T newT = (T)item.Clone();
                result.Add(newT);

                // 所有节点的TreeLevel加一
                if (addRootNode)
                {
                    newT.TreeLevel++;
                }
            }

            // currentId==-1表示当前节点不存在
            if (currentID != -1)
            {
                // 本节点不可点击（也就是说当前节点不可能是当前节点的父节点）
                // 并且本节点的所有子节点也不可点击，你想如果当前节点跑到子节点的子节点，那么这些子节点就从树上消失了
                bool startChileNode = false;
                int startTreeLevel = 0;
                foreach (T my in result)
                {
                    if (my.ID == currentID)
                    {
                        startTreeLevel = my.TreeLevel;
                        my.Enabled = false;
                        startChileNode = true;
                    }
                    else
                    {
                        if (startChileNode)
                        {
                            if (my.TreeLevel > startTreeLevel)
                            {
                                my.Enabled = false;
                            }
                            else
                            {
                                startChileNode = false;
                            }
                        }
                    }
                }
            }

            return result;
        }

        #endregion

        #region InvalidModelState

        protected void InvalidModelState(ModelStateDictionary state)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("<ul>");
            foreach (var key in state.Keys)
            {
                //将错误描述添加到sb中
                foreach (var error in state[key].Errors)
                {
                    sb.AppendFormat("<li>{0}</li>", error.ErrorMessage);
                }
            }
            sb.Append("</ul>");

            Alert.Show(sb.ToString());
        }

        #endregion

        #region GetProductVersion

        protected string GetProductVersion()
        {
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            return String.Format("{0}.{1}.{2}", v.Major, v.Minor, v.Build);
        }

        #endregion

        #region Dispose

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion



        #region Grid相关

        protected void BindGird<T>(IQueryable<T> q,string gird,JObject gridItem)
        {
            var grid1UI = UIHelper.Grid("Grid1");
            var gridItemC = gridItem.ToObject<ViewModel.GridConfig>();
            if (gridItemC.IsPaging)
            {
                var pagingInfo = new PagingInfoViewModel
                {
                    RecordCount = q.Count(),
                    SortField = gridItemC.SortField,
                    SortDirection = gridItemC.SortDirection,
                    PageIndex = gridItemC.PageIndex,
                    PageSize = gridItemC.PageSize
                };
                // 1. 设置总项数
                grid1UI.RecordCount(pagingInfo.RecordCount);
                // 2. 设置每页显示项数
                grid1UI.PageSize(gridItemC.PageSize);
                // 3.设置分页数据
                q = SortAndPage<T>(q, pagingInfo);             
            }
            grid1UI.DataSource(q, gridItemC.Fields);
        }


        protected void BindGird<T>(List<T> items, string gird, JObject gridItem)
        {
            var grid1UI = UIHelper.Grid("Grid1");
            var gridItemC = gridItem.ToObject<ViewModel.GridConfig>();
            grid1UI.DataSource(items, gridItemC.Fields);
        }

        #endregion
    }
}