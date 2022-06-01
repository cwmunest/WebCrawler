using System;
using System.Diagnostics;
using System.IO;
using WebCrawler.Entity;

namespace WebCrawler.Helper
{
    public class Log : TraceListener
    {
        private static TraceSource instance = null;
        public static TraceSource Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TraceSource("WorkOrderCrawler", SourceLevels.Verbose);
                    instance.Listeners.Remove("Default");
                    instance.Listeners.Add(new Log());
                }
                return instance;
            }
        }

        private string LOG_FILE = "WorkCrawlerLogs.txt";

        private Log() : base("WorkOrderCrawler")
        {
            var startupPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            startupPath = startupPath.Substring(0, startupPath.LastIndexOf(@"\"));
            LOG_FILE = Path.Combine(startupPath, LOG_FILE);

        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            if (eventType != TraceEventType.Information && args != null && args.Length > 0)
            {
                //using (var db = DB.Instance.GetDataContext(int.Parse(args[0].ToString())))
                //{
                //    if (args.Length != 3 || Convert.ToBoolean(args[2]) != false)//第三个参数为0时不写日志表
                //    {
                //        db.TempWorkOrderLogs.Add(new TempWorkOrderLog()
                //        {
                //            CorpID = int.Parse(args[0].ToString()),
                //            Detail = format,
                //            LogType = eventType == TraceEventType.Warning ? 2 : 4
                //        });
                //        db.SaveChanges();
                //    }

                //    if (eventType == TraceEventType.Critical && args.Length >= 2)
                //    {
                //        db.Database.ExecuteSqlCommand($"update SBC_厂商系统信息 set 自动采集 = 0,采集状态=2, 错误提示 = '{format}' where AI = {args[1].ToString()}");
                //        SendStopTipToAdmin(int.Parse(args[0].ToString()),$"原因:{format}");
                //    }
                //}
            }
            base.TraceEvent(eventCache, source, eventType, id, format, args);
        }

        public override void Write(string message)
        {
            innerLog(message);
        }

        public override void WriteLine(string message)
        {
            innerLog(message);
        }

        private void innerLog(string message)
        {
            if (message.IndexOf("WorkOrderCrawler") != -1) return;
            message = DateTime.Now.ToString() + " " + message + " ";
            //#if DEBUG
            Console.WriteLine(message);
            //#else
            File.AppendAllLines(LOG_FILE, new String[] { message });
            //#endif
        }
        //发送暂停采集提醒给管理员
        public void SendStopTipToAdmin(int corpID, string msg)
        {
           
        }
    }
}
