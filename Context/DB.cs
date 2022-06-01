using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler.Context
{
    [System.Runtime.Remoting.Contexts.Synchronization]
    public class DB : System.ContextBoundObject
    {
        private const string COMMON_CNN_STR = "CommonConnStr";
        private const string DATA_CNN_STR = "DataConnStr";
        public const string DATA_CNN_STR_YY = "DataConnStr_YY";
        private const string CNN_STR_TEMPLATE = @"data source={0},{1};initial catalog=shfw_data;uid={2};pwd={3};Connect Timeout=900";
        public const string DATA_CNN_STR_OTHER = "DataConnStr_OTHER";
        private bool inited = false;
        private IDictionary<int, ServerInfo> corpCnns = new Dictionary<int, ServerInfo>();
        private IDictionary<int, string> dataCnns = new Dictionary<int, string>();

        public static DB Instance { get; private set; } = new DB();

        private DB()
        {
            init();
        }

        private void init()
        {
            if (this.inited) return;
            try
            {
                using (var commonDb = this.CommonContext)
                {
                    var ServerInfos = commonDb.Database.SqlQuery<ServerInfo>(@"select es.id, ecc.corpid, es.ip, es.dbport, es.dbsa, es.dbpwd from EAE_Server es
                                                join EAE_DNServer eds on eds.sid = es.id
                                                right outer join EAE_CorpConfig ecc on ecc.dnserver = eds.id
                                                where es.type = 0 or es.type = 3");
                    this.corpCnns.Clear();
                    foreach (var info in ServerInfos)
                        this.corpCnns.Add(info.corpid, info);

                    this.dataCnns.Clear();
                    if (WebCrawlerConfiguration.Instance.IsSingle)
                        this.dataCnns.Add(0, WebCrawlerConfiguration.Instance[DATA_CNN_STR]);
                    else
                    {
                        var EAEServers = commonDb.Database.SqlQuery<EAEServer>(@"select id, name, ip, dbport, dbsa, dbpwd from EAE_Server where ip is not null and dbsa is not null and dbpwd is not null and (type = 3 or type = 0)");
                        foreach (var server in EAEServers)
                            this.dataCnns.Add(server.id, string.Format(CNN_STR_TEMPLATE, server.ip, server.dbport, server.dbsa, server.dbpwd));
                    }
                }
                this.inited = true;
            }
            catch
            {
                this.inited = false;
            }
        }
        public static void Reset()
        {
            Instance.inited = false;
        }
        public CommonContext CommonContext
        {
            get
            {
                return new CommonContext(WebCrawlerConfiguration.Instance[COMMON_CNN_STR]);
            }
        }

        public ICollection<string> DataCnnStrList
        {
            get
            {
                //this.inited = false;
                this.init();
                return this.dataCnns.Values.ToArray();
            }
        }

        public DataContext GetDataContext(int CorpID)
        {
            ServerInfo info; string result;
            var found = this.corpCnns.TryGetValue(CorpID, out info);
            
            if (found && !WebCrawlerConfiguration.Instance.IsSingle)
            {
                var found2 = this.dataCnns.TryGetValue(info.id, out result);
                if (!found2)
                {
                    result = string.Format(CNN_STR_TEMPLATE, info.ip, info.dbport, info.dbsa, info.dbpwd);
                    this.dataCnns.Add(info.id, result);
                }
            }
            else
                result = WebCrawlerConfiguration.Instance[DATA_CNN_STR];

            return new DataContext(result);
        }

        public string GetCommonDBConnStr()
        {
            return WebCrawlerConfiguration.Instance[COMMON_CNN_STR];
        }
        public string GetDBConnStr(int CorpID)
        {
            ServerInfo info; string result;
            var found = this.corpCnns.TryGetValue(CorpID, out info);

            if (found && !WebCrawlerConfiguration.Instance.IsSingle)
            {
                var found2 = this.dataCnns.TryGetValue(info.id, out result);
                if (!found2)
                {
                    result = string.Format(CNN_STR_TEMPLATE, info.ip, info.dbport, info.dbsa, info.dbpwd);
                    this.dataCnns.Add(info.id, result);
                }
            }
            else
                result = WebCrawlerConfiguration.Instance[DATA_CNN_STR];

            return result;
        }

        public DataContext GetDataContext(string cnn)
        {
            return new DataContext(cnn);
        }
        public DataContext GetDataContext_YY()
        {
            return new DataContext(WebCrawlerConfiguration.Instance[DATA_CNN_STR_YY]);
        }

        internal class EAEServer
        {
            public int id { get; set; }
            public string name { get; set; }
            public string ip { get; set; }
            public int? dbport { get; set; }
            public string dbsa { get; set; }
            public string dbpwd { get; set; }
        }

        internal class ServerInfo
        {
            public int id { get; set; }
            public int corpid { get; set; }
            public string ip { get; set; }
            public int? dbport { get; set; }
            public string dbsa { get; set; }
            public string dbpwd { get; set; }
        }

        public static SqlParameter MakeParam(string name, object value)
        {
            return new SqlParameter(name, value == null ? DBNull.Value : value);
        }
    }
}
