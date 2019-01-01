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

        [Authorize]
        public JsonResult RelaySet(int relay, byte set)
        {
            var ret = new Dictionary<string, byte>();
            using (var db = new Models.DBContext())
            {
                var UserID = ((System.Security.Claims.ClaimsPrincipal)User).Claims.ToArray()[0].Value; //Получим ID пользователя из AspNetUsers
                var UserData = db.Users.Where(p => p.Name == UserID).FirstOrDefault();
                var sel = db.NBIoTCommands.Where(p => (p.UserId == UserData.Id && p.IdDev == 0)).FirstOrDefault();

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
        [Authorize]
        public JsonResult RelayGet()
        {
            var ret = new Dictionary<string, byte>();
            using (var db = new Models.DBContext())
            {
                var UserID = ((System.Security.Claims.ClaimsPrincipal)User).Claims.ToArray()[0].Value; //Получим ID пользователя из AspNetUsers
                var UserData = db.Users.Where(p => p.Name == UserID).FirstOrDefault();
                var sel = db.NBIoTCommands.Where(p => (p.UserId == UserData.Id && p.IdDev == 0)).FirstOrDefault();

                var CurCommand = sel.Data;                
                ret.Add("Rel1", CurCommand[0]);
                ret.Add("Rel2", CurCommand[1]);
            }
            return Json(ret, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult LastDataGet()
        {
            var ret = new Dictionary<string, object>();                        
            using (var db = new Models.DBContext())
            {
                var UserID = ((System.Security.Claims.ClaimsPrincipal)User).Claims.ToArray()[0].Value; //Получим ID пользователя из AspNetUsers
                var UserData = db.Users.Where(p => p.Name == UserID).FirstOrDefault();
                var sel = db.NBIoTDatas.Where(p => (p.UserId == UserData.Id && p.IdDev == 0)).OrderByDescending(p=>p.DateTime).FirstOrDefault();
                var UserDataShema = UserData.DataShema;
                var CurCommand = sel.Data;
                var DataShemaRows = UserDataShema.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                int CommandOfset = 0;
                foreach (string dataShemaRow in DataShemaRows)
                {
                    var Type_Name = dataShemaRow.Split(new string[] { "\t" }, StringSplitOptions.None);
                    var dataType = Type_Name[0];
                    var dataName = Type_Name[1];

                    switch (dataType)
                    {
                        case "byte":
                            ret.Add(dataName, CurCommand[CommandOfset]);
                            CommandOfset++;
                            break;
                        case "ushort":
                            ret.Add(dataName, BitConverter.ToUInt16(CurCommand, CommandOfset));
                            CommandOfset += 2;
                            break;
                        case "float":
                            ret.Add(dataName, BitConverter.ToSingle(CurCommand, CommandOfset));
                            CommandOfset += 4;
                            break;
                    }
                }
            }
            return Json(ret, JsonRequestBehavior.AllowGet);
        }      
    }
}