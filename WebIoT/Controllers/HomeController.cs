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

        public JsonResult RelaySet(int relay, byte set)
        {
            var ret = new Dictionary<string, byte>();
            using (var db = new Models.DBContext())
            {
                var sel = db.NBIoTCommands.Where(p => (p.UserId == 9 && p.IdDev == 0)).FirstOrDefault();
                var CurCommand = sel.Data;
                CurCommand[relay-1] = set;

                sel.Data = CurCommand;
                db.Entry(sel).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                ret.Add("Rel1", CurCommand[0]);
                ret.Add("Rel2", CurCommand[1]);
            }
            return Json(ret, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RelayGet()
        {
            var ret = new Dictionary<string, byte>();
            using (var db = new Models.DBContext())
            {
                var sel = db.NBIoTCommands.Where(p => (p.UserId == 9 && p.IdDev == 0)).FirstOrDefault();
                var CurCommand = sel.Data;                
                ret.Add("Rel1", CurCommand[0]);
                ret.Add("Rel2", CurCommand[1]);
            }
            return Json(ret, JsonRequestBehavior.AllowGet);
        }        
        public JsonResult LastDataGet()
        {
            var ret = new Dictionary<string, byte[]>();
            using (var db = new Models.DBContext())
            {
                var sel = db.NBIoTDatas.Where(p => (p.UserId == 9 && p.IdDev == 0)).OrderByDescending(p=>p.Id).FirstOrDefault();
                var CurCommand = sel.Data;

                //ret.Add("BatLev", (float)(CurCommand[CurCommand.Length-3])/10);
                ret.Add("LastData", CurCommand);

            }
            return Json(ret, JsonRequestBehavior.AllowGet);
        }
    }
}