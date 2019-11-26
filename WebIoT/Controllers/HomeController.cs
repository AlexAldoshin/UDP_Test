using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebIoT.Models;
using System.Threading.Tasks;
using System.Data.Entity;

namespace WebIoT.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                if (_userManager == null)
                {
                    _userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                }
                return _userManager;
            }          
        }


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

        /// <summary>
        /// Получение сохраненных данных
        /// </summary>
        /// <param name="api_key">Read API Key</param>
        /// <param name="IdDev">Номер устройства</param>
        /// <param name="results">кол-во последних данных</param>
        /// <returns></returns>
        public JsonResult ReadData(string api_key, int IdDev, int results)
        {
            var ret = new Dictionary<string, object>();
            var UserData = db.Users.Join(db.NBIoTCommands, p => p.Id, c => c.UserId, (p, c) => new { p.Id, p.ReadKeyAPI, c.IdDev, c.DataShema }).FirstOrDefault(d => d.ReadKeyAPI == new Guid(api_key) && d.IdDev == IdDev);

            var sel = db.NBIoTDatas
                .Where(p => (p.UserId == UserData.Id && p.IdDev == IdDev))
                .OrderByDescending(dd => dd.DateTime)
                .Take(results).Select(ss => new { ss.DateTime, ss.Data }).ToList();

            var UserDataShema = UserData.DataShema;
            var DataShemaRows = UserDataShema.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            // Ось времени
            ret.Add("DateTime_msec", new List<long>());
            //Получаем имена полей
            foreach (string dataShemaRow in DataShemaRows)
            {
                var Type_Name = dataShemaRow.Split(new string[] { "\t" }, StringSplitOptions.None);
                var dataType = Type_Name[0];
                var dataName = Type_Name[1];
                switch (dataType)
                {
                    case "byte":
                        ret.Add(dataName, new List<byte>());
                        break;
                    case "ushort":
                        ret.Add(dataName, new List<ushort>());
                        break;
                    case "float":
                        ret.Add(dataName, new List<float>());
                        break;
                }
            }

            //Заполняем поля значениями
            foreach (var item in sel)
            {
                var CurCommand = item.Data;
                int CommandOfset = 0;
                ((List<long>)ret["DateTime_msec"]).Add((long)(item.DateTime.Ticks / TimeSpan.TicksPerMillisecond)); //переведем дату в мсек.
                foreach (string dataShemaRow in DataShemaRows)
                {
                    var Type_Name = dataShemaRow.Split(new string[] { "\t" }, StringSplitOptions.None);
                    var dataType = Type_Name[0];
                    var dataName = Type_Name[1];
                    switch (dataType)
                    {
                        case "byte":
                            ((List<byte>)ret[dataName]).Add(CurCommand[CommandOfset]);
                            CommandOfset++;
                            break;
                        case "ushort":
                            ((List<ushort>)ret[dataName]).Add(BitConverter.ToUInt16(CurCommand, CommandOfset));
                            CommandOfset += 2;
                            break;
                        case "float":
                            ((List<float>)ret[dataName]).Add(BitConverter.ToSingle(CurCommand, CommandOfset));
                            CommandOfset += 4;
                            break;
                    }
                }
            }
            return Json(ret, JsonRequestBehavior.AllowGet);
        }

        [Authorize]        
        public async Task<JsonResult> RelaySet(int IdDev, int relay, byte set)
        {
            var ret = new Dictionary<string, byte>();
            var UserID = await UserManager.FindByNameAsync(User.Identity.Name);
            var UserData = db.Users.Where(p => p.Name == UserID.Id).FirstOrDefault();
            var sel = db.NBIoTCommands.Where(p => (p.UserId == UserData.Id && p.IdDev == IdDev)).FirstOrDefault();

            var CurCommand = sel.Data;
            CurCommand[relay - 1] = set;

            sel.Data = CurCommand;
            db.Entry(sel).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            ret.Add("Rel1", CurCommand[0]);
            ret.Add("Rel2", CurCommand[1]);

            return Json(ret, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public async Task<JsonResult> RelayGet()
        {
            var ret = new Dictionary<string, byte>();
            var UserID = await UserManager.FindByNameAsync(User.Identity.Name);
            var UserData = db.Users.Where(p => p.Name == UserID.Id).FirstOrDefault();
            var sel = db.NBIoTCommands.Where(p => (p.UserId == UserData.Id && p.IdDev == 0)).FirstOrDefault();
            var CurCommand = sel.Data;
            ret.Add("Rel1", CurCommand[0]);
            ret.Add("Rel2", CurCommand[1]);
            return Json(ret, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public async Task<JsonResult> LastDataGet(int IdDev)
        {
            var ret = new Dictionary<string, object>();
            var UserID = await UserManager.FindByNameAsync(User.Identity.Name);
            var UserData = db.Users.Join(db.NBIoTCommands, p => p.Id, c => c.UserId, (p, c) => new { p.Id, p.Name, p.ReadKeyAPI, c.IdDev, c.DataShema }).FirstOrDefault(d => d.Name == UserID.Id && d.IdDev == IdDev);
            var sel = db.NBIoTDatas.OrderByDescending(c => c.DateTime).FirstOrDefault(p => (p.UserId == UserData.Id && p.IdDev == IdDev));
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
            return Json(ret, JsonRequestBehavior.AllowGet);
        }

        [Authorize]        
        public ActionResult DevicesPartial()
        {
            //Получим данные авторизов. юзера
            //var UserID = await UserManager.FindByNameAsync(User.Identity.Name);

            var UserIDAsync = UserManager.FindByNameAsync(User.Identity.Name);
            UserIDAsync.Wait();
            var UserID = UserIDAsync.Result;

            //Получим данные устройств этого юзера
            var UserDevices = db.Users.
                Join(db.NBIoTCommands, pUser => pUser.Id, cDevice => cDevice.UserId, (pUser, cDevice) => 
                new UserDevice
                { 
                    UserNameId = pUser.Name,
                    UserReadKeyAPI = pUser.ReadKeyAPI,
                    UserKeyAPI = pUser.KeyAPI,
                    IdDev = cDevice.IdDev,
                    DeviceName = cDevice.Name,
                    DeviceDescription = cDevice.Description,
                    DeviceLatitude = cDevice.Latitude,
                    DeviceLongitude = cDevice.Longitude,
                    DeviceElevation = cDevice.Elevation,
                    DeviceImage = cDevice.Image
                }).
                Where(d => d.UserNameId == UserID.Id);
            return PartialView(UserDevices);
        }

        [Authorize]
        public ActionResult NewDevice()
        {
            var UserIDAsync = UserManager.FindByNameAsync(User.Identity.Name);
            UserIDAsync.Wait();
            var UserID = UserIDAsync.Result;
            var cUser = db.Users.FirstOrDefault(p => p.Name == UserID.Id);
            var newIdDev = db.NBIoTCommands.Where(p => p.UserId == cUser.Id).Max(p => p.IdDev) + 1;
            NBIoTCommand UserCommand = new NBIoTCommand { IdDev = newIdDev, DataShema = "byte\tUserData", User = cUser, Data = new byte[] { 0, 0 } , Name = ("New Dev " + newIdDev), Description = ("New Description " + newIdDev), Latitude=0, Longitude=0, Elevation=0 };
            db.NBIoTCommands.Add(UserCommand);
            db.SaveChanges();
            return null;
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}