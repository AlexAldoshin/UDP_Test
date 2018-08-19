using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;

namespace UDP_Test.UDP
{
    class UDPEmail
    {
        private MailAddress point1;
        private MailAddress point2;
        private SmtpClient smtp;
        private MailMessage M;
        private string AddEmMain;
        private string AddPassMain;
        public UDPEmail(string Em1, string pass, string Em2, string name)
        {
            AddEmMain = Em1;
            AddPassMain = pass;
            point1 = new MailAddress(AddEmMain, name);
            point2 = new MailAddress(Em2);
            M = new MailMessage(point1, point2);
            // адрес smtp-сервера и порт, с которого будем отправлять письмо
            smtp = new SmtpClient("smtp.gmail.com", 587);
        }


        public void SendMessages(string mess)
        {
            M.Body = mess;
            // логин и пароль
            smtp.Credentials = new NetworkCredential(AddEmMain, AddPassMain);
            smtp.EnableSsl = true;
            smtp.Send(M);
        }

        public async Task SendAsicMessages(string mess)
        {
            M.Body = mess;
            // логин и пароль
            smtp.Credentials = new NetworkCredential(AddEmMain, AddPassMain);
            smtp.EnableSsl = true;
            await smtp.SendMailAsync(M);
        }
    }
}
