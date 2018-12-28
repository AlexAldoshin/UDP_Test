using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebIoT.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        //public ActionResult PushButton(int Rel, int OnOff)
        //{
        //    return View();
        //}
        public ActionResult About()
        {
            ViewBag.Message = "Портал управления Интернет-вещами.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "У вас есть вопросы или проблемы? Вы найдёте ответы после окончания тестирования.";

            return View();
        }
    }
}