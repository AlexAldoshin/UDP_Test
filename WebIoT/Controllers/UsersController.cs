using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebIoT.Models;

namespace WebIoT.Controllers
{
    public class UsersController : Controller
    {
        private DBContext db = new DBContext();

        // GET: Users
        public ActionResult Index()
        {
            var UserID = ((System.Security.Claims.ClaimsPrincipal)User).Claims.ToArray()[0].Value; //Получим ID пользователя из AspNetUsers
            var UserData = db.Users.Where(p => p.Name == UserID).FirstOrDefault();
            ViewBag.Shema = GetShemaFromServer();
            ViewBag.Types = new List<string> {"byte","float","ushort"};
            return View(UserData);
        }

        // GET: Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,KeyAPI,DataShema,ReadKeyAPI")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,KeyAPI,DataShema,ReadKeyAPI")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        [Authorize]
        public JsonResult ShemaGet()
        {
            Dictionary<string, string> ret = GetShemaFromServer();
            return Json(ret, JsonRequestBehavior.AllowGet);
        }

        private Dictionary<string, string> GetShemaFromServer()
        {
            var ret = new Dictionary<string, string>();
            var UserID = ((System.Security.Claims.ClaimsPrincipal)User).Claims.ToArray()[0].Value; //Получим ID пользователя из AspNetUsers
            var UserData = db.Users.Where(p => p.Name == UserID).FirstOrDefault();
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

        [Authorize]
        public EmptyResult ShemaSet(string shema)
        {            
            var UserID = ((System.Security.Claims.ClaimsPrincipal)User).Claims.ToArray()[0].Value; //Получим ID пользователя из AspNetUsers
            var UserData = db.Users.Where(p => p.Name == UserID).FirstOrDefault();
            UserData.DataShema = shema;
            db.Entry(UserData).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
