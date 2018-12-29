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

        public EmptyResult RelaySet(int relay, byte set)
        {
            using (var db = new Models.DBContext())
            {
                var sel = db.NBIoTCommands.Where(p => (p.UserId == 9 && p.IdDev == 0)).FirstOrDefault();
                var CurCommand = sel.Data;
                CurCommand[relay-1] = set;

                sel.Data = CurCommand;
                db.Entry(sel).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }

            return null;
        }

    }
}