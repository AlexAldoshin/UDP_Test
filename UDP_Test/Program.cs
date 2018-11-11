using System;
using System.Linq;
using System.Threading;
using UDP_Test.Act;
using UDP_Test.DB;

namespace UDP_Test
{
    class Program
    {
        static void Main()
        {
            int ii = 0;

            while (true)
            {
                using (UserContext db = new UserContext())
                {
                    db.Configuration.AutoDetectChangesEnabled = false;
                    db.Configuration.ValidateOnSaveEnabled = false;

                    // создаем User
                    for (int i = 0; i < 100; i++)
                    {
                        db.Users.Add(new User { Name = "Tom", Age = 33 });
                        db.Musers.Add(new Muser { Name = "Tom", Age = 33 });

                    }
                    //добавляем в бд
                    db.SaveChanges();
                    Console.WriteLine("Объекты успешно сохранены" + ii++);

                    //// получаем объекты из бд и выводим на консоль
                    //var users = db.Users;
                    //Console.WriteLine("Список объектов:");
                    //foreach(User u in users)
                    //{
                    //    Console.WriteLine("{0}.{1} - {2}", u.Id, u.Name, u.Age);
                    //}
                    //Thread.Sleep(10);
                }
            }



            ActWithServer A = new ActWithServer();
            A.run();
        }
    }
}