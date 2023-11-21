using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CPEA.Utilities.InterswitchClasses
{
    public class General
    {
        public static string strPath;
        public static string[] currentFileName;
        public static string[] incomingFolderName;
        public static string[] outgoingFolderName;
        public static string[] ExpectedFolderList;
        public static string FromFileLocation;
        public static string ToFilelocation;
        public static string ArchiveName;
        public static string expectedFolderList;
        public static string Server;
        public static string Subject;
        public static string MessageBody;
        public static string From;
        public static string To;
        public static string Cc;
        public static long Counter;
        public static string GeneralReportSummary;

        public static void CreateAndSendMessage(string server, string subject, string messageBody, string from, string to, string cc)
        {
            MailMessage message = new MailMessage(from, to);
            message.CC.Add(cc);
            message.Subject = subject;
            message.SubjectEncoding = Encoding.UTF8;
            message.Body = messageBody;
            message.BodyEncoding = Encoding.UTF8;
            SmtpClient smtpClient = new SmtpClient(server);
            smtpClient.Port = 25;
            smtpClient.UseDefaultCredentials = true;
            try
            {
                smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                General.LogToFile(ex);
            }
        }

        public static string GetCompleteExceptionMessage(Exception EX)
        {
            Exception exception = EX;
            string str = exception.Message;
            for (; exception.InnerException != null; exception = exception.InnerException)
                str = str + " because " + exception.InnerException.Message;
            return str;
        }

        protected static string BuildErrorMsg(Exception EX)
        {
            string str1 = "";
            string str2 = "";
            Exception baseException = EX.GetBaseException();
            if (EX == null)
                return "";
            string str3 = DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss");
            string exceptionMessage = General.GetCompleteExceptionMessage(EX);
            if (baseException.TargetSite != (MethodBase)null)
                str1 = baseException.TargetSite.Name;
            if (baseException.StackTrace != null)
                str2 = EX.GetBaseException().StackTrace;
            return string.Format("\r\n\r\n[{0}]\r\n Subject: \t{1}\r\n Page Request: \t{2}\r\n Stack Trace : \t{3}", (object)str3, (object)exceptionMessage, (object)str1, (object)str2);
        }

        protected static string BuildStringErrorMsg(string lenghtErrorText)
        {
            string str = lenghtErrorText;
            if (lenghtErrorText == null)
                return "";
            return string.Format("\r\n\r\n[{0}]\r\n Stack Trace : \t{1}", new object[2]
            {
        (object) DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss"),
        (object) str
            });
        }

        public static void LogToFile(Exception EX)
        {
            try
            {
               // File.AppendAllText(ConfigurationManager.AppSettings["ErrorLogFolder"] + DateTime.Now.ToString("dd-MMM-yyyy") + ".txt", General.BuildErrorMsg(EX));
            }
            catch (NullReferenceException ex)
            {
                LogToFileLenght(ex.ToString());
            }
            catch (FileNotFoundException ex)
            {
                LogToFileLenght(ex.ToString());
            }
            catch (Exception ex)
            {
                LogToFile(ex);
            }
        }

        public static void LogToFileLenght(string lenghtErrorText)
        {
            try
            {
               // File.AppendAllText(ConfigurationManager.AppSettings["ErrorLogFolder"] + DateTime.Now.ToString("dd-MMM-yyyy") + ".txt", General.BuildStringErrorMsg(lenghtErrorText));
            }
            catch (NullReferenceException ex)
            {
                LogToFile(ex);
            }
            catch (FileNotFoundException ex)
            {
                LogToFile(ex);
            }
            catch (Exception ex)
            {
                LogToFile(ex);
            }
        }

        public static string getLathPathName(string strPath)
        {
            General.currentFileName = strPath.Split('\\');
            return General.currentFileName[((IEnumerable<string>)General.currentFileName).Count<string>() - 1];
        }

        public static void InitializeFileLocations(string fromFileLocation, string toFilelocation, string expectedFolderList)
        {
            try
            {
                General.Counter = 0L;
                General.FromFileLocation = fromFileLocation;
                General.ToFilelocation = toFilelocation;
                if (string.IsNullOrEmpty(expectedFolderList))
                    return;
                General.ExpectedFolderList = expectedFolderList.Split(',');
            }
            catch (Exception ex)
            {
                General.CreateAndSendMessage(General.Server, General.Subject, ex.Message, General.From, General.To, General.Cc);
            }
        }

        public static bool WhitelistedIps(string hostIP)
        {
            try
            {
                //string[] Ips = ConfigurationManager.AppSettings["WhiteListedIPAddresses"].Split(',');
                //foreach (string x in Ips)
                //{
                //    if (hostIP.Contains(x))
                //    {
                //        return true;
                //    }
                //}

            }
            catch (NullReferenceException ex)
            {
                LogToFile(ex);
            }
            catch (FileNotFoundException ex)
            {
                LogToFile(ex);
            }
            catch (Exception ex)
            {
                LogToFile(ex);
            }
            return false;
        }
    }
}
