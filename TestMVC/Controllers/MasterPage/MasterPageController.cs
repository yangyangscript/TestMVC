using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestMVC.ViewModel;

namespace TestMVC.Controllers
{
    public partial class MasterPageController : Controller
    {
        private List<ViewModel.MasterInfoView> GetDatas(int num)
        {
            var now = DateTime.Now;
            var ret = new List<ViewModel.MasterInfoView>();
            for (int i = 1; i < num; i++)
            {
                var nu1 = i * 2;
                var nu2 = i * 3;
                var nu3 = i * 5;
                var nu4 = i * 7;               
                ret.Add(new MasterInfoView()
                {
                      Name1 = $"Name_{nu1}",
                      Name2  = $"Name_{nu2}",
                        Name3 = $"Name_{nu3}",
                        Name4 = $"Name_{nu4}",
                      Time1 = now.AddMinutes(-nu1),
                    Time2 = now.AddMinutes(-nu2),
                    Time3 = now.AddMinutes(-nu3),
                    Time4 = now.AddMinutes(-nu4),
                    Number1 = nu1,
                    Number2 = nu2,
                    Number3 = nu3,
                    Number4 = nu4,
                });
            }
            return ret;
        }
    }
}