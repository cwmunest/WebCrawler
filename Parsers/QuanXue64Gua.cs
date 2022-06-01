using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using WebCrawler.Entity;
using WebCrawler.Helper;
using WebCrawler.Context;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Data.SqlClient;
using WebCrawler;
using WebCrawler.Network;

namespace WebCrawler.Parsers
{
    public class QuanXue64Gua : Parser
    {
        public new static string[] BrandPart
        {
            get
            {
                return new string[] { "14_" };
            }
        }

        public QuanXue64Gua(ParserConfig config) : base(config)
        {
        }

        protected override bool Login()
        {
            return true;
        }

        protected override bool NeedLogin()
        {
            return false;
        }

        protected override int ParseWorkOrder()
        {
            IList<Gua64> result = new List<Gua64>();

            for (int j = 3; j <= 64; j++)
            {
                var response = this.crawler.Get(string.Format("http://www.quanxue.cn/CT_RuJia/ZhouYi/ZhouYi{0}.html", j.ToString("00")))
                    .SetHeader(BASE_HEADER)
                    .SetCookie("zh_choose=s;Hm_lvt_da94247e5b70c927a6e6933bccb11e29=1630902793;Hm_lpvt_da94247e5b70c927a6e6933bccb11e29=1631170713").End();
                result.Add(this.ToServiceGua(j, response.EncodeingText(Encoding.UTF8)));
            }

            SaveServiceGua(result);

            return result.Count;
        }
        private static Dictionary<string, int> dicJingGua = new Dictionary<string, int> { { "乾", 1 }, { "兑", 2 }, { "离", 3 }, { "震", 4 }, { "巽", 5 }, { "坎", 6 }, { "艮", 7 }, { "坤", 8 } };
        private Gua64 ToServiceGua(int guaId, string content)
        {
            Gua64 gua = new Gua64();
            Regex r = new Regex(@"<[^>]+>");
            gua.ID =guaId;
            gua.序号 = gua.ID;
            gua.封名 = this.MatchValue(content, "<span class=\"title3\">([^<]+)</span>");
            gua.简称 = gua.封名.Substring(2);
            gua.上卦 = dicJingGua[this.MatchValue(content, "<span class=\"title4\">(.?)上<br>")]; 
            gua.下卦 = dicJingGua[this.MatchValue(content, "<span class=\"title4\">.?上<br>(.?)下[\\s]*</span>")];
            gua.卦辞 = r.Replace(this.MatchValue(content, "<div class=\"guaci\">(?:|\r\n|\n)(.+)(?:|\r\n|\n)*</div>"), "").Replace(" ","").Trim();

            string yaoci = this.MatchValue(content, "<pre class=\"yaoci\">([\\W\\w]+)</pre>");
            string[] arrYaoCi = yaoci.Trim().Split(new string[1] { "\r\n" }, StringSplitOptions.None);
            gua.爻辞1 = r.Replace(arrYaoCi[0],"");
            gua.爻辞2 = r.Replace(arrYaoCi[1], "");
            gua.爻辞3 = r.Replace(arrYaoCi[2], "");
            gua.爻辞4 = r.Replace(arrYaoCi[3], "");
            gua.爻辞5 = r.Replace(arrYaoCi[4], "");
            gua.爻辞6 = r.Replace(arrYaoCi[5], "");
            //gua.彖辞 = this.MatchValue(content, "<p class=\"tuan\"><big>(彖曰</big>[^<]+)(?:|</span>)</p>").Replace("</big>", "：").Replace("\r\n", "").Replace(" ", "").Trim();
            gua.彖辞 = r.Replace(this.MatchValue(content, "<p class=\"tuan\"><big>(彖曰</big>(?:|\r\n)[\\w\\W]+)</p>[\\s]*<p class=\"xiang\"><big>象曰").Replace("</big>", "："),"").Replace("\r\n", "").Replace(" ", "").Trim();
            gua.卦象辞 = this.MatchValue(content, "<p class=\"xiang\"><big>(象曰</big>[\\s]*.+)[\\s]*</p>[\\s]*<p class=\"xiang\"><big>初").Replace("</big>", "：").Replace("\r\n", "").Replace(" ", "").Trim();

            var mcs = Regex.Matches(content, "<p class=\"xiang\"><big>[^<]+</big>[\\s]*<span class=\"yin\">.+</span><br>[\\s]*<big>象曰</big>([^<]+)<br>[\\s]*</p>");
            for (int i= 1;i <= mcs.Count;i++)
            {
                if (i == 1) gua.爻象辞1 = r.Replace(mcs[i - 1].Groups[1].Value,"").Replace(" ", "").Trim();
                else if (i == 2) gua.爻象辞2 = r.Replace(mcs[i - 1].Groups[1].Value, "").Replace(" ", "").Trim();
                else if (i == 3) gua.爻象辞3 = r.Replace(mcs[i - 1].Groups[1].Value, "").Replace(" ", "").Trim();
                else if (i == 4) gua.爻象辞4 = r.Replace(mcs[i - 1].Groups[1].Value, "").Replace(" ", "").Trim();
                else if (i == 5) gua.爻象辞5 = r.Replace(mcs[i - 1].Groups[1].Value, "").Replace(" ", "").Trim();
                else if (i == 6) gua.爻象辞6 = r.Replace(mcs[i - 1].Groups[1].Value, "").Replace(" ", "").Trim();
            }
            return gua;
        }

        protected virtual void SaveServiceGua(IEnumerable<Gua64> guas)
        {
            if (guas.Count() > 0)
            {
                Gua64 lastGua = null;
                try
                {
                    using (var db = DB.Instance.GetDataContext(this.corpId.Value))
                    {
                        int ret = 0;
                        string sql = string.Empty;
                        foreach (Gua64 gua in guas)
                        {
                            lastGua = gua;
                            sql = SAVE_GUA_SQL;
                            ret = db.Database.ExecuteSqlCommand(sql, DB.MakeParam("@ID", gua.ID),
                               DB.MakeParam("@封名", gua.封名)
                               , DB.MakeParam("@简称", gua.简称)
                               , DB.MakeParam("@上卦", gua.上卦)
                               , DB.MakeParam("@下卦", gua.下卦)
                               , DB.MakeParam("@卦辞", gua.卦辞)
                               , DB.MakeParam("@爻辞1", gua.爻辞1)
                               , DB.MakeParam("@爻辞2", gua.爻辞2)
                               , DB.MakeParam("@爻辞3", gua.爻辞3)
                               , DB.MakeParam("@爻辞4", gua.爻辞4)
                               , DB.MakeParam("@爻辞5", gua.爻辞5)
                               , DB.MakeParam("@爻辞6", gua.爻辞6)
                               , DB.MakeParam("@用爻辞", gua.用爻辞)
                               , DB.MakeParam("@彖辞", gua.彖辞)
                               , DB.MakeParam("@卦象辞", gua.卦象辞)
                               , DB.MakeParam("@爻象辞1", gua.爻象辞1)
                               , DB.MakeParam("@爻象辞2", gua.爻象辞2)
                               , DB.MakeParam("@爻象辞3", gua.爻象辞3)
                               , DB.MakeParam("@爻象辞4", gua.爻象辞4)
                               , DB.MakeParam("@爻象辞5", gua.爻象辞5)
                               , DB.MakeParam("@爻象辞6", gua.爻象辞6)
                               , DB.MakeParam("@用爻象辞", gua.用爻象辞)
                               , DB.MakeParam("@序号", gua.序号));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Instance.TraceInformation(JsonConvert.SerializeObject(lastGua));
                    throw ex;
                }
            }
        }
        

        private static string SAVE_GUA_SQL = @"IF NOT EXISTS(SELECT 1 FROM ZY_别卦 WHERE ID =@ID)
                          INSERT INTO ZY_别卦(ID,封名,简称,上卦,下卦,卦辞,爻辞1,爻辞2,爻辞3,爻辞4,爻辞5,爻辞6,用爻辞,彖辞,卦象辞,爻象辞1,爻象辞2,爻象辞3,爻象辞4,爻象辞5,爻象辞6,用爻象辞,序号) VALUES(@ID,@封名,@简称,@上卦,@下卦,@卦辞,@爻辞1,@爻辞2,@爻辞3,@爻辞4,@爻辞5,@爻辞6,@用爻辞,@彖辞,@卦象辞,@爻象辞1,@爻象辞2,@爻象辞3,@爻象辞4,@爻象辞5,@爻象辞6,@用爻象辞,@序号)
                          ELSE UPDATE ZY_别卦 SET 封名=@封名,简称=@简称,上卦=@上卦,下卦=@下卦,卦辞=@卦辞,爻辞1=@爻辞1,爻辞2=@爻辞2,爻辞3=@爻辞3,爻辞4=@爻辞4,爻辞5=@爻辞5,爻辞6=@爻辞6,用爻辞=@用爻辞,彖辞=@彖辞,卦象辞=@卦象辞,爻象辞1=@爻象辞1,爻象辞2=@爻象辞2,爻象辞3=@爻象辞3,爻象辞4=@爻象辞4,爻象辞5=@爻象辞5,爻象辞6=@爻象辞6,用爻象辞=@用爻象辞,序号=@序号 WHERE ID =@ID";


        private IDictionary<string, string> BASE_HEADER = new Dictionary<string, string>
        {
            ["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9",
            ["Accept-Encoding"] = "gzip, deflate",
            ["Accept-Language"] = "zh-CN,zh;q=0.9,en;q=0.8",
            ["Cache-Control"] = "max-age=0",
            ["Connection"] = "keep-alive",
            ["Content-Type"] = "text/xml;charset=UTF-8",
            ["Host"] = "www.quanxue.cn",
            ["Upgrade-Insecure-Requests"]="1",
            //["Origin"] = "http://www.quanxue.cn/CT_RuJia/ZhouYi/ZhouYi07.html",
            ["Referer"] = "http://www.quanxue.cn/CT_RuJia/ZhouYi/ZhouYi07.html",
            ["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.159 Safari/537.36",
        };
    }

}
