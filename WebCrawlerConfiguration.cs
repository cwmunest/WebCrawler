using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WebCrawler.Helper;

namespace WebCrawler
{
    class WebCrawlerConfiguration
    {
        private Configuration cfg;
        public static string StartupPath = "";

        internal static WebCrawlerConfiguration Instance { get; } = new WebCrawlerConfiguration();

        public WebCrawlerConfiguration()
        {
            StartupPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            StartupPath = StartupPath.Substring(0, StartupPath.LastIndexOf(@"\") + 1);

            string path = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            this.cfg = ConfigurationManager.OpenExeConfiguration(path);
        }

        internal string this[string key]
        {
           get
            {
                try
                {
                    return cfg.AppSettings.Settings[key].Value;
                } catch(Exception)
                {
                    return null;
                }
            }
        }

        public bool IsSingle
        {
            get
            {
                try
                {
                    return "single".Equals(cfg.AppSettings.Settings["type"].Value.ToLower());
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}
