using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using WebCrawler.Entity;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Web;
using System.Net;
using System.Data;
using System.Data.SqlClient;
using WebCrawler;
using hcc.Database;

namespace WebCrawler.Parsers
{
    public class MSHYBook : Parser
    {
        private static string DN = "http://www.7278.org";
        //1,易经五术;2,中医;3,诸子百家;4,童蒙养正;5,因果宗教;6,兵法谋略;7,史书典籍;8,四大名著;9,才子佳人;10,讽刺谴责;11,神魔志怪;12,古典侠义;13,历史公案;14,历史演义;15,笔记小说;16,辞赋鉴赏;17,文言文鉴赏
        private static Dictionary<int, string> dicKindUrl = new Dictionary<int, string> { { 0,""}
            //, { 1, "http://www.7278.org/yjsj/" }
            //, { 2, "http://www.7278.org/yjsj/" }
            //, { 3, "http://www.7278.org/zzbj/" }
            //, { 4, "http://www.7278.org/mxsj/" }
            //, { 5, "http://www.7278.org/fjdj/" }
            //, { 6, "http://www.7278.org/bsdj/" }
            //, { 7, "http://www.7278.org/ssdj/" }
            //, { 8, "http://www.7278.org/sdmz/" }
            //, { 9, "http://www.7278.org/czjr/" }
            //, { 10, "http://www.7278.org/fcqz/" }
            //, { 11, "http://www.7278.org/smzg/" }
            //, { 12, "http://www.7278.org/gdxy/" }
            //, { 13, "http://www.7278.org/ga/" }
            //, { 14, "http://www.7278.org/lsyy/" }
            //, { 15, "http://www.7278.org/bj/" }
            //, { 16, "http://www.7278.org/cfjs/" }
            //, { 17, "http://www.7278.org/wyw/" }
        };

        protected int ParseWorkOrder0()
        {
            IList<JDBookContent> result = new List<JDBookContent>();
            string sql = "";
            string idPre = "21";
            string saveDir = @"D:\Projects\WebCrawler\BookPics";
            string savePath = "";
            string saveDir2 = "";
            bool ok = true;
            DbOperator objDB = DbBridge.GetDbOperator(DBConnStr.DataConnStr);
            try
            {
                foreach (var key in dicKindUrl.Keys)
                {
                    if (string.IsNullOrEmpty(dicKindUrl[key])) continue;
                    var response = this.crawler.Get(dicKindUrl[key])
                        //.SetHeader(BASE_HEADER)
                        //.SetCookie("zh_choose=s;Hm_lvt_da94247e5b70c927a6e6933bccb11e29=1630902793;Hm_lpvt_da94247e5b70c927a6e6933bccb11e29=1631170713")
                        .End();
                    string content = response.EncodeingText(Encoding.UTF8);
                    var matches = Regex.Matches(content, "<a href=\"([^\"]+)\"[\\s]+alt=\"([^\"]+)\">[^<]*<img[\\s]+alt=\"[^\"]+\"[\\s]+src=\"([^\"]+)\">[\\s]*</a>");
                    int len0 = 0, len1 = 0, len2 = 0;
                    foreach (Match mc in matches)
                    {
                        len0++;
                        string bookHref = mc.Groups[1].Value;
                        string bookName = mc.Groups[2].Value;
                        string bookPic = mc.Groups[3].Value;

                        string resHTML = this.crawler.Get(DN + bookHref).End().EncodeingText(Encoding.UTF8);

                        string author = MatchValue(resHTML, "<p>作者：([^<]*)<a");
                        string intro = MatchValue(resHTML, "<div class=\"describe-html\"><p>([^<]*)</p></div>");

                        //System.Drawing.Image image = System.Drawing.Image.FromStream(WebRequest.Create(DN + bookPic).GetResponse().GetResponseStream());
                        savePath = string.Format("/upload/BookPics/{0}", bookPic.Replace("/d/file/", ""));
                        savePath = saveDir + savePath.Replace("/upload/BookPics", "").Replace("/", "\\");
                        saveDir2 = savePath.Substring(0, savePath.LastIndexOf("\\"));
                        if (!System.IO.Directory.Exists(saveDir2)) System.IO.Directory.CreateDirectory(saveDir2);
                        try
                        {
                            System.Drawing.Image.FromStream(WebRequest.Create(DN + bookPic).GetResponse().GetResponseStream()).Save(savePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                            //break;
                        }
                        catch { }

                        int bookID = Convert.ToInt32(idPre + key + "0000") + len0;
                        sql = @"IF NOT EXISTS(SELECT 1 FROM ZJD_经典书籍 WHERE ID=@ID)
                          INSERT INTO ZJD_经典书籍(ID,分类,名称,作者,内容简介,封面,URL,序号,来自)VALUES(@ID,@分类,@名称,@作者,@内容简介,@封面,@URL,@序号,1)
                          ELSE UPDATE ZJD_经典书籍 SET 分类=@分类,名称=@名称,作者=@作者,内容简介=@内容简介,封面=@封面,URL=@URL,序号=@序号 WHERE ID=@ID";
                        SqlParameter[] parameters0 = {
                            new SqlParameter("@ID", bookID),
                            new SqlParameter("@分类",key),
                            new SqlParameter("@名称",bookName),
                            new SqlParameter("@作者",author),
                            new SqlParameter("@内容简介",intro),
                            new SqlParameter("@封面",savePath),
                            new SqlParameter("@URL",DN + bookHref),
                            new SqlParameter("@序号",len0),
                        };
                        ok = objDB.ExecSqlCommandForAffectRow(sql, parameters0) > 0;
                        if (!ok)
                        {
                            break;
                        }
                        var matches2 = Regex.Matches(resHTML, "<li><a href=\"([^\"]+)\" title=\"[^\"]+\">([^<]+)</a></li>");
                        len1 = 0;
                        foreach (Match mc2 in matches2)
                        {
                            len1++;
                            string bookHref2 = mc2.Groups[1].Value;
                            string chapterName = mc2.Groups[2].Value;
                            string resHTML2 = this.crawler.Get(DN + bookHref2).End().EncodeingText(Encoding.UTF8);
                            string content2 = MatchValue(resHTML2, "<div id=\"nr1\" class=\"mb2\">[\\s]+<p>([\\w\\W]+)</p>[\\s]+</div>[\\s]+<script");

                            sql = @"IF NOT EXISTS(SELECT 1 FROM ZJD_书籍内容 WHERE 书籍=@书籍 AND 章节名称=@章节名称)
                              INSERT INTO ZJD_书籍内容(书籍,卷号,章节名称,章节内容,翻译内容,序号)VALUES(@书籍,@卷号,@章节名称,@章节内容,@翻译内容,@序号)";
                              //ELSE UPDATE ZJD_书籍内容 SET 卷号=@卷号,章节内容=@章节内容,翻译内容=@翻译内容,序号=@序号 WHERE 书籍=@书籍 AND 章节名称=@章节名称";
                            SqlParameter[] parameters2 = {
                                    new SqlParameter("@书籍",bookID),
                                    new SqlParameter("@章节名称",chapterName),
                                    new SqlParameter("@卷号",null),
                                    new SqlParameter("@章节内容",content2),
                                    new SqlParameter("@翻译内容",null),
                                    new SqlParameter("@序号",len1),
                                };
                            ok = objDB.ExecSqlCommandForAffectRow(sql, parameters2) >= 0;
                            if (!ok)
                            {
                                break;
                            }
                        }
                        if (!ok)
                        {
                            break;
                        }
                        //break;
                    }
                    //break;
                }
            }
            catch (Exception ex)
            {
                ok = false;
                //clxit.Common.Log.Info("UserAdmin.userRegister", ex.ToString());
            }
            finally { objDB.Dispose(); }

            //SaveServiceGua(result);

            return result.Count;
        }

        protected override int ParseWorkOrder()
        {
            IList<JDBookContent> result = new List<JDBookContent>();
            string sql = "";
            string idPre = "21";
            string saveDir = @"D:\Projects\WebCrawler\BookPics";
            string savePath = "";
            string saveDir2 = "";
            bool ok = true;
            int failNum = 0;
            DbOperator objDB = DbBridge.GetDbOperator(DBConnStr.DataConnStr);
            try
            {
                int len0 = 0, len1 = 0, len2 = 0;
                DataTable dtBook = objDB.ExecSqlForTable(string.Format("SELECT ID,名称,DURL FROM ZJD_经典书籍 WHERE ID IN(SELECT DISTINCT 书籍 FROM ZJD_书籍内容 WHERE 章节内容 IS NULL)"));
                DataTable dtChapter = objDB.ExecSqlForTable(string.Format("SELECT 书籍,章节名称 FROM ZJD_书籍内容 WHERE 章节内容 IS NULL"));
                dtChapter.PrimaryKey = new DataColumn[2] { dtChapter.Columns[0], dtChapter.Columns[1] };

                foreach (DataRow row in dtBook.Rows)
                {
                    //if (row["ID"].ToString() != "2110022") continue;
                    string bookName = row["名称"].ToString();
                    string resHTML = this.crawler.Get(row["DURL"].ToString()).End().EncodeingText(Encoding.UTF8);
                    var matches2 = Regex.Matches(resHTML, "<li><a href=\"([^\"]+)\" title=\"[^\"]+\">([^<]+)</a></li>");
                    len1 = 0;
                    foreach (Match mc2 in matches2)
                    {
                        len1++;
                        string bookHref2 = mc2.Groups[1].Value;
                        string chapterName = mc2.Groups[2].Value;
                        if (dtChapter.Rows.Find(new object[2] { row["ID"], chapterName }) == null) continue;
                        string resHTML2 = this.crawler.Get(DN + bookHref2).End().EncodeingText(Encoding.UTF8);
                        string content2 = MatchValue(resHTML2, "<div id=\"nr1\" class=\"mb2\">[\\s]+([\\w\\W]+)[\\s]+</div>[\\s]+<script");
                        if (string.IsNullOrEmpty(content2))
                        {
                            failNum++;
                            //break;
                        }

                        sql = @"IF NOT EXISTS(SELECT 1 FROM ZJD_书籍内容 WHERE 书籍=@书籍 AND 章节名称=@章节名称)
                              INSERT INTO ZJD_书籍内容(书籍,卷号,章节名称,章节内容,翻译内容,序号)VALUES(@书籍,@卷号,@章节名称,@章节内容,@翻译内容,@序号)
                              ELSE UPDATE ZJD_书籍内容 SET 章节内容=@章节内容 WHERE 书籍=@书籍 AND 章节名称=@章节名称";
                        SqlParameter[] parameters2 = {
                                    new SqlParameter("@书籍",row["ID"]),
                                    new SqlParameter("@章节名称",chapterName),
                                    new SqlParameter("@卷号",null),
                                    new SqlParameter("@章节内容",content2),
                                    new SqlParameter("@翻译内容",null),
                                    new SqlParameter("@序号",len1),
                                };
                        ok = objDB.ExecSqlCommandForAffectRow(sql, parameters2) >= 0;
                        if (!ok)
                        {
                            break;
                        }
                        //break;
                    }
                    //break;
                }
            }
            catch (Exception ex)
            {
                ok = false;
                //clxit.Common.Log.Info("UserAdmin.userRegister", ex.ToString());
            }
            finally { objDB.Dispose(); }

            //SaveServiceGua(result);

            return result.Count;
        }
        private JDBookContent ToServiceGua(int guaId, string content)
        {
            JDBookContent chapter = new JDBookContent();
            //Regex r = new Regex(@"<[^>]+>");
            //chapter.ID = guaId;
            //chapter.序号 = chapter.ID;
            //chapter.封名 = this.MatchValue(content, "<span class=\"title3\">([^<]+)</span>");
            //chapter.简称 = chapter.封名.Substring(2);
            //chapter.上卦 = dicJingGua[this.MatchValue(content, "<span class=\"title4\">(.?)上<br>")];
            //chapter.下卦 = dicJingGua[this.MatchValue(content, "<span class=\"title4\">.?上<br>(.?)下[\\s]*</span>")];
            //chapter.卦辞 = r.Replace(this.MatchValue(content, "<div class=\"guaci\">(?:|\r\n|\n)(.+)(?:|\r\n|\n)*</div>"), "").Replace(" ", "").Trim();

            //string yaoci = this.MatchValue(content, "<pre class=\"yaoci\">([\\W\\w]+)</pre>");
            //string[] arrYaoCi = yaoci.Trim().Split(new string[1] { "\r\n" }, StringSplitOptions.None);
            //chapter.爻辞1 = r.Replace(arrYaoCi[0], "");
            //chapter.爻辞2 = r.Replace(arrYaoCi[1], "");
            //chapter.爻辞3 = r.Replace(arrYaoCi[2], "");
            //chapter.爻辞4 = r.Replace(arrYaoCi[3], "");
            //chapter.爻辞5 = r.Replace(arrYaoCi[4], "");
            //chapter.爻辞6 = r.Replace(arrYaoCi[5], "");
            ////chapter.彖辞 = this.MatchValue(content, "<p class=\"tuan\"><big>(彖曰</big>[^<]+)(?:|</span>)</p>").Replace("</big>", "：").Replace("\r\n", "").Replace(" ", "").Trim();
            //chapter.彖辞 = r.Replace(this.MatchValue(content, "<p class=\"tuan\"><big>(彖曰</big>(?:|\r\n)[\\w\\W]+)</p>[\\s]*<p class=\"xiang\"><big>象曰").Replace("</big>", "："), "").Replace("\r\n", "").Replace(" ", "").Trim();
            //chapter.卦象辞 = this.MatchValue(content, "<p class=\"xiang\"><big>(象曰</big>[\\s]*.+)[\\s]*</p>[\\s]*<p class=\"xiang\"><big>初").Replace("</big>", "：").Replace("\r\n", "").Replace(" ", "").Trim();

            //var mcs = Regex.Matches(content, "<p class=\"xiang\"><big>[^<]+</big>[\\s]*<span class=\"yin\">.+</span><br>[\\s]*<big>象曰</big>([^<]+)<br>[\\s]*</p>");
            //for (int i = 1; i <= mcs.Count; i++)
            //{
            //    if (i == 1) chapter.爻象辞1 = r.Replace(mcs[i - 1].Groups[1].Value, "").Replace(" ", "").Trim();
            //    else if (i == 2) chapter.爻象辞2 = r.Replace(mcs[i - 1].Groups[1].Value, "").Replace(" ", "").Trim();
            //    else if (i == 3) chapter.爻象辞3 = r.Replace(mcs[i - 1].Groups[1].Value, "").Replace(" ", "").Trim();
            //    else if (i == 4) chapter.爻象辞4 = r.Replace(mcs[i - 1].Groups[1].Value, "").Replace(" ", "").Trim();
            //    else if (i == 5) chapter.爻象辞5 = r.Replace(mcs[i - 1].Groups[1].Value, "").Replace(" ", "").Trim();
            //    else if (i == 6) chapter.爻象辞6 = r.Replace(mcs[i - 1].Groups[1].Value, "").Replace(" ", "").Trim();
            //}
            return chapter;
        }

        protected virtual void SaveServiceGua(IEnumerable<JDBookContent> guas)
        {
            //if (guas.Count() > 0)
            //{
            //    JDBookContent lastGua = null;
            //    try
            //    {
            //        using (var db = DB.Instance.GetDataContext(this.corpId.Value))
            //        {
            //            int ret = 0;
            //            string sql = string.Empty;
            //            foreach (JDBookContent chapter in guas)
            //            {
            //                lastGua = chapter;
            //                sql = SAVE_BOOKCONTENT_SQL;
            //                ret = db.Database.ExecuteSqlCommand(sql, DB.MakeParam("@书籍", chapter.书籍),
            //                   DB.MakeParam("@卷号", chapter.卷号)
            //                   , DB.MakeParam("@章节名称", chapter.章节名称)
            //                   , DB.MakeParam("@章节内容", chapter.章节内容)
            //                   , DB.MakeParam("@序号", chapter.序号));
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Log.Instance.TraceInformation(JsonConvert.SerializeObject(lastGua));
            //        throw ex;
            //    }
        }


        private static string SAVE_BOOKCONTENT_SQL = @"IF NOT EXISTS(SELECT 1 FROM ZJD_书籍内容 WHERE 书籍=@书籍 AND 章节名称=@章节名称)
                          INSERT INTO ZJD_书籍内容(书籍,卷号,章节名称,章节内容,翻译内容,序号)VALUES(@书籍,@卷号,@章节名称,@章节内容,@翻译内容,@序号)
                          ELSE UPDATE ZJD_书籍内容 SET 卷号=@卷号,章节内容=@章节内容,翻译内容=@翻译内容,序号=@序号 WHERE 书籍=@书籍 AND 章节名称=@章节名称";


        private IDictionary<string, string> BASE_HEADER = new Dictionary<string, string>
        {
            ["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9",
            ["Accept-Encoding"] = "gzip, deflate",
            ["Accept-Language"] = "zh-CN,zh;q=0.9,en;q=0.8",
            ["Cache-Control"] = "max-age=0",
            ["Connection"] = "keep-alive",
            ["Content-Type"] = "text/xml;charset=UTF-8",
            ["Host"] = "www.quanxue.cn",
            ["Upgrade-Insecure-Requests"] = "1",
            //["Origin"] = "http://www.quanxue.cn/CT_RuJia/ZhouYi/ZhouYi07.html",
            ["Referer"] = "http://www.quanxue.cn/CT_RuJia/ZhouYi/ZhouYi07.html",
            ["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.159 Safari/537.36",
        };
    }
}