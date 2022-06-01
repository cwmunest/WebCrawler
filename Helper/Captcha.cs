using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCrawler;
using WebCrawler.Network;

namespace WebCrawler.Helper
{
    public static class Captcha
    {
        private static string IMAGE_PATH = WebCrawlerConfiguration.Instance["ImagePath"];
        private static string USER = WebCrawlerConfiguration.Instance["CaptchaUser"];
        private static string PWD = WebCrawlerConfiguration.Instance["CaptchaPwd"];
        private static string SOFTID = WebCrawlerConfiguration.Instance["CaptchaSoftID"];
        private static string CODETYPE = WebCrawlerConfiguration.Instance["CaptchaCodeType"];

        public static CrackInfo Crack(string url, Crawler crawler)
        {
            if (string.IsNullOrEmpty(USER) || string.IsNullOrEmpty(PWD)) throw new CaptchaNoAccountException(USER);
            if (!Directory.Exists(WebCrawlerConfiguration.StartupPath+IMAGE_PATH)) Directory.CreateDirectory(WebCrawlerConfiguration.StartupPath + IMAGE_PATH);

            //string filename = Path.Combine(IMAGE_PATH, DateTime.UtcNow.Ticks + ".gif");
            string filename = Path.Combine(WebCrawlerConfiguration.StartupPath + IMAGE_PATH, DateTime.UtcNow.Ticks + "");
            crawler.Get(url).End().ToFile(filename);
            //return null;
            var response = new Crawler().Post("http://upload.chaojiying.net/Upload/Processing.php")
                        .Type(Crawler.DataType.FORM)
                        .Field("user", USER).Field("pass", PWD)
                        .Field("softid", SOFTID).Field("codetype", CODETYPE)
                        .Attach("userfile", filename).End();

            var body = response.Body;
            File.Delete(filename);
            if (body["err_no"].ToObject<int>() == -1005) throw new CaptchaNeedMoneyException(USER);
            else
                return new CrackInfo()
                {
                    Id = body["pic_id"].ToString(),
                    Result = body["pic_str"].ToString()
                };
        }
        public static CrackInfo Crack2(string dataImageStr, Crawler crawler)
        {
            if (!Directory.Exists(WebCrawlerConfiguration.StartupPath + IMAGE_PATH)) Directory.CreateDirectory(WebCrawlerConfiguration.StartupPath + IMAGE_PATH);
            if (dataImageStr.IndexOf("data:image") != -1) dataImageStr = dataImageStr.Substring(dataImageStr.IndexOf(",") + 1);
            string filename = Path.Combine(WebCrawlerConfiguration.StartupPath + IMAGE_PATH, DateTime.UtcNow.Ticks + "");
            Stream inputStream = new System.IO.MemoryStream(Convert.FromBase64String(dataImageStr));
            using (FileStream storeStream = new FileStream(filename, FileMode.OpenOrCreate))
            {
                inputStream.CopyTo(storeStream);
            }
            //return null;
            var response = new Crawler().Post("http://upload.chaojiying.net/Upload/Processing.php")
                        .Type(Crawler.DataType.FORM)
                        .Field("user", USER).Field("pass", PWD)
                        .Field("softid", SOFTID).Field("codetype", CODETYPE)
                        .Attach("userfile", filename).End();

            var body = response.Body;
            File.Delete(filename);
            if (body["err_no"].ToObject<int>() == -1005) throw new CaptchaNeedMoneyException(USER);
            else
            {
                return new CrackInfo()
                {
                    Id = body["pic_id"].ToString(),
                    Result = body["pic_str"].ToString()
                };
            }
        }

        public static void ReportError(string id)
        {
            new Crawler().Post("http://upload.chaojiying.net/Upload/ReportError.php")
                .Field("user", USER).Field("pass", PWD).Field("softid", SOFTID)
                .Field("id", id).End();
        }
    }

    public class CrackInfo
    {
        public string Id { get; set; }
        public string Result { get; set; }
    }
}
