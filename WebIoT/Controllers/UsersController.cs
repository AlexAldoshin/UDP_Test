using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebIoT.Models;
using System.Threading.Tasks;
using System.Drawing;

namespace WebIoT.Controllers
{
    [Authorize]
    public class UsersController : Controller
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

        // GET: Users
        public async Task<ActionResult> Index()
        {         
            var UserID = await UserManager.FindByNameAsync(User.Identity.Name);
            var UserData = db.Users.Where(p => p.Name == UserID.Id).FirstOrDefault();
            ViewBag.Shema = await GetShemaFromServerAsync(0);
            ViewBag.Types = new List<string> {"byte","float","ushort"};
            return View(UserData);
        }
                
        public async Task<JsonResult> ShemaGet()
        {
            var ret = await GetShemaFromServerAsync(0);
            return Json(ret, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> ChangeReadAPIKey()
        {
            var ret = new Dictionary<string, Guid>();
            var UserID = await UserManager.FindByNameAsync(User.Identity.Name);
            var UserData = db.Users.Where(p => p.Name == UserID.Id).FirstOrDefault();
            Guid NewReadAPI = Guid.NewGuid();
            UserData.ReadKeyAPI = NewReadAPI;
            db.Entry(UserData).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            ret.Add("ReadAPIKey", NewReadAPI);
            return Json(ret, JsonRequestBehavior.AllowGet);
        }


        private async Task<Dictionary<string, string>> GetShemaFromServerAsync(int dev_id)
        {
            var ret = new Dictionary<string, string>();           
            var UserID = await UserManager.FindByNameAsync(User.Identity.Name);
            var UserData = db.Users.Join(db.NBIoTCommands, p => p.Id, c => c.UserId, (p, c) => new { p.Name, c.IdDev, c.DataShema }).FirstOrDefault(d => d.Name == UserID.Id && d.IdDev == dev_id);
            
            var UserDataShema = UserData.DataShema;
            var DataShemaRows = UserDataShema.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string dataShemaRow in DataShemaRows)
            {
                var Type_Name = dataShemaRow.Split(new string[] { "\t" }, StringSplitOptions.None);
                var dataType = Type_Name[0];
                var dataName = Type_Name[1];
                ret.Add(dataName, dataType);
            }
            return ret;
        }

        
        public async Task<EmptyResult> ShemaSetAsync(int dev_id, string shema)
        {            
           // var UserID = ((System.Security.Claims.ClaimsPrincipal)User).Claims.ToArray()[0].Value; //Получим ID пользователя из AspNetUsers
           // var UserData = db.Users.Where(p => p.Name == UserID).FirstOrDefault();

            var UserID = await UserManager.FindByNameAsync(User.Identity.Name);
            var UserData = db.Users.FirstOrDefault(d => d.Name == UserID.Id);
            var DevComm =  db.NBIoTCommands.FirstOrDefault(p => p.UserId == UserData.Id && p.IdDev == dev_id);
            DevComm.DataShema = shema;
            db.Entry(DevComm).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return null;
        }


        [HttpPost]
        public JsonResult Upload()
        {
            int IdDev = 0;
            foreach (string file in Request.Files)
            {
                var upload = Request.Files[file];
                if (upload != null)
                {
                    // получаем файл. конвертим в jpg 400x300
                    string fileName = System.IO.Path.GetFileName(upload.FileName);                    
                    Bitmap bm = new Bitmap(Image.FromStream(upload.InputStream), 400, 300);
                    var memStream = new System.IO.MemoryStream();
                    bm.Save(memStream, System.Drawing.Imaging.ImageFormat.Jpeg);

                    //Получаем текущего юзера
                    var UserIDAsync = UserManager.FindByNameAsync(User.Identity.Name);
                    UserIDAsync.Wait();
                    var UserID = UserIDAsync.Result;

                    //Сохраним каритинку
                    var UserData = db.Users.FirstOrDefault(d => d.Name == UserID.Id);
                    var DevComm = db.NBIoTCommands.FirstOrDefault(p => p.UserId == UserData.Id && p.IdDev == IdDev);
                    DevComm.Image = memStream.ToArray();
                    db.Entry(DevComm).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
            }
            return Json("файл загружен");
        }

        public FileResult Img(int IdDev)
        {
            //Получим данные авторизов. юзера
            //var UserID = await UserManager.FindByNameAsync(User.Identity.Name);

            var UserIDAsync = UserManager.FindByNameAsync(User.Identity.Name);
            UserIDAsync.Wait();
            var UserID = UserIDAsync.Result;

            //Получим данные устройств этого юзера
            var DeviceImage = db.Users.
                Join(db.NBIoTCommands, pUser => pUser.Id, cDevice => cDevice.UserId, (pUser, cDevice) =>
                new 
                {
                    UserNameId = pUser.Name,                    
                    IdDev = cDevice.IdDev,                    
                    DeviceImage = cDevice.Image
                }).
                FirstOrDefault(d => d.UserNameId == UserID.Id && d.IdDev == IdDev).DeviceImage;
            return File(DeviceImage, "image/jpeg", "img" + IdDev + ".jpg");
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
