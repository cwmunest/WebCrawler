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
    public class AiBaZiBook : Parser
    {
        private static string DN = "https://app.i8z.cn/articles";
        //1,易经五术;2,中医;3,诸子百家;4,童蒙养正;5,因果宗教;6,兵法谋略;7,史书典籍;8,四大名著;9,才子佳人;10,讽刺谴责;11,神魔志怪;12,古典侠义;13,历史公案;14,历史演义;15,笔记小说;16,辞赋鉴赏;17,文言文鉴赏
        //private static Dictionary<int, string[,]> dicKindUrl = new Dictionary<int, string[,]> { 
        //     //{ 2210101, new string[,] { {"神峰通考 卷一","https://app.i8z.cn/z8/articles/?collection=ff6b4a02-ef75-4e8e-9f54-cbd8e10adffd"}
        //     //,{"神峰通考 卷二","https://app.i8z.cn/z8/articles/?collection=d2f173fe-2a78-4c1b-a0ac-faf0abf51b44" }
        //     //,{"神峰通考 卷三","https://app.i8z.cn/z8/articles/?collection=3fbaa5fb-1dea-44a0-b50d-b844f27fe1fc" }
        //     //,{"神峰通考 卷四","https://app.i8z.cn/z8/articles/?collection=096e589a-7c7d-4a58-a932-6a29c109d5a4" }
        //     //,{"神峰通考 卷五","https://app.i8z.cn/z8/articles/?collection=7d5c3855-67a7-4341-9660-45be9691612a" }
        //     //,{"神峰通考 卷六","https://app.i8z.cn/z8/articles/?collection=d2bd06e4-62ac-4b2b-ad45-82fabeaed1c5" }}
        //     //{ 2112102, new string[,] { {"渊海子平卷一","https://app.i8z.cn/z8/articles/?collection=bff342f0-8377-4437-9038-00af4c3a0289"}
        //     //,{"渊海子平卷二","https://app.i8z.cn/z8/articles/?collection=d403b4ec-e491-419c-90ab-b2198c4c6a02" }
        //     //,{"渊海子平卷三","https://app.i8z.cn/z8/articles/?collection=ca51e39a-d295-4187-ae9a-5391adf21a7f" }
        //     //,{"渊海子平卷四","https://app.i8z.cn/z8/articles/?collection=6d2d6479-5297-428e-9d06-a9f12e777c29" }
        //     //,{"渊海子平卷五","https://app.i8z.cn/z8/articles/?collection=0c08a2b7-192f-451a-9d17-9f170e355606" }
        //     //,{"渊海子平卷六","https://app.i8z.cn/z8/articles/?collection=f9f2603c-a4d3-49e1-b54a-6be0a0868676" }
        //     //,{"渊海子平卷七","https://app.i8z.cn/z8/articles/?collection=04e0d390-c541-4979-a5b0-7ddc81e229a4" }}   
        //     { 2210100, new string[,] { {"三命通会 卷 1","https://app.i8z.cn/z8/articles/?collection=4f5fbc0b-91a2-4053-a60f-097a1f4b071a"}
        //     ,{"三命通会 卷 2","https://app.i8z.cn/z8/articles/?collection=941b134a-a5e2-4d9a-a0d0-161a0302ceca" }
        //     ,{"三命通会 卷 3","https://app.i8z.cn/z8/articles/?collection=9dda8d52-8ecc-4b1f-9933-2cfa12f794fa" }
        //     ,{"三命通会 卷 4","https://app.i8z.cn/z8/articles/?collection=02600165-21ca-41f9-978b-f9a8c4b35651" }
        //     ,{"三命通会 卷 5","https://app.i8z.cn/z8/articles/?collection=6a6f1e8f-116a-40eb-94cf-c0d369d383a5" }
        //     ,{"三命通会 卷 6","https://app.i8z.cn/z8/articles/?collection=8343b35a-55ba-4004-bb84-8466c63c080b" }
        //     ,{"三命通会 卷 7","https://app.i8z.cn/z8/articles/?collection=b84b9a89-dabe-4fd5-9839-0fcdf29f5486" }
        //     ,{"三命通会 卷 8","https://app.i8z.cn/z8/articles/?collection=23eb4237-c403-486d-ad64-c436793bd41e" }
        //     ,{"三命通会 卷 9","https://app.i8z.cn/z8/articles/?collection=140591cd-bd15-4561-a875-1497e06d48c9" }
        //     ,{"三命通会 卷 10","https://app.i8z.cn/z8/articles/?collection=8b31fc0f-6273-4a26-9ec2-a5b202e1ec64" }
        //     ,{"三命通会 卷 11","https://app.i8z.cn/z8/articles/?collection=e1081658-6d2f-430f-ad06-023fb4d34cf2" }
        //     ,{"三命通会 卷 12","https://app.i8z.cn/z8/articles/?collection=728daa0d-a4c9-466d-a1b3-18b5ede428ed" }}
        //    }
        private static Dictionary<int, string> dicKindUrl = new Dictionary<int, string> {
             //{ 2112102, "https://app.i8z.cn/z8/articles/?collection=3fbf3b6e-1555-44ce-86be-2aae44f7c77a"},
             { 2112103, "https://app.i8z.cn/z8/articles/?collection=b5dab4ea-473d-4a05-bcc4-878f717e5b2e"},
             { 2112104, "https://app.i8z.cn/z8/articles/?collection=7a4b10b4-ea97-45ed-bbc8-e5b02bc2f634"},
             { 2112105, "https://app.i8z.cn/z8/articles/?collection=edd70182-1105-4d3b-b76f-536b1e7f8815"},
             { 2112106, "https://app.i8z.cn/z8/articles/?collection=773ade00-0f66-4999-93f0-fb17ab1b3d9a"},
             { 2112107, "https://app.i8z.cn/z8/articles/?collection=7019f0b2-0b36-41fe-ba6e-fc1674fdd7db"},
             { 2112108, "https://app.i8z.cn/z8/articles/?collection=44930d5a-8d21-4939-9fda-b97307b9f509"},
        };

        protected int ParseWorkOrder0()
        {
            IList<JDBookContent> result = new List<JDBookContent>();
            //string sql = "";
            //string idPre = "21";
            //string saveDir = @"D:\Projects\WebCrawler\BookPics";
            //string savePath = "";
            //string saveDir2 = "";
            //bool ok = true;
            //int failNum = 0;
            //int okNum = 0;
            //DbOperator objDB = DbBridge.GetDbOperator(DBConnStr.DataConnStr);
            //try
            //{
            //    string[,] multiArray;
            //    int len1 = 0;
            //    foreach (var key in dicKindUrl.Keys)
            //    {
            //        multiArray = dicKindUrl[key];
            //        for (int i = 0; i <= multiArray.GetUpperBound(0); i++)
            //        {
            //            //for (int j = 0; j <= multiArray.GetUpperBound(multiArray.Rank - 1); j++)
            //            //{
            //            //    string arr = multiArray[i, j];
            //            //    Console.Write("multiArray[" + i + "][" + j + "] = " + arr + "\r\n");
            //            //}
            //            //break;
            //            sql = @"IF NOT EXISTS(SELECT 1 FROM ZJD_书籍分卷 WHERE 书籍=@书籍 AND ID=@ID)
            //              INSERT INTO ZJD_书籍分卷(ID,书籍,分卷名称,序号)VALUES(@ID,@书籍,@分卷名称,@序号)
            //              ELSE UPDATE ZJD_书籍分卷 SET 分卷名称=@分卷名称,序号=@序号 WHERE 书籍=@书籍 AND ID=@ID";
            //            SqlParameter[] parameters0 = {
            //                new SqlParameter("@ID", i+1),
            //                new SqlParameter("@书籍",key),
            //                new SqlParameter("@分卷名称",multiArray[i, 0]),
            //                new SqlParameter("@序号",i+1),
            //            };
            //            ok = objDB.ExecSqlCommandForAffectRow(sql, parameters0) > 0;
            //            if (!ok)
            //            {
            //                break;
            //            }
            //            string content = this.crawler.Get(multiArray[i, 1]).HttpGet_SSL(null);
            //            var body = JObject.Parse(content);
            //            len1 = 0;
            //            foreach (var row in body["results"])
            //            {
            //                len1++;
            //                sql = @"IF NOT EXISTS(SELECT 1 FROM ZJD_书籍内容 WHERE 书籍=@书籍 AND 章节名称=@章节名称)
            //                  INSERT INTO ZJD_书籍内容(书籍,卷号,章节名称,章节内容,翻译内容,序号)VALUES(@书籍,@卷号,@章节名称,@章节内容,@翻译内容,@序号)
            //                  ELSE UPDATE ZJD_书籍内容 SET 卷号=@卷号,章节内容=@章节内容,翻译内容=@翻译内容,序号=@序号 WHERE 书籍=@书籍 AND 章节名称=@章节名称";
            //                SqlParameter[] parameters2 = {
            //                        new SqlParameter("@书籍",key),
            //                        new SqlParameter("@章节名称",JSONValue(row, "title")),
            //                        new SqlParameter("@卷号",i+1),
            //                        new SqlParameter("@章节内容",JSONValue(row, "text")),
            //                        new SqlParameter("@翻译内容",null),
            //                        new SqlParameter("@序号",(i+1)*100+len1),
            //                    };
            //                ok = objDB.ExecSqlCommandForAffectRow(sql, parameters2) >= 0;
            //                if (!ok)
            //                {
            //                    failNum++;
            //                    break;
            //                }
            //                okNum++;
            //            }
            //            if (!ok)
            //            {
            //                break;
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    ok = false;
            //    //clxit.Common.Log.Info("UserAdmin.userRegister", ex.ToString());
            //}
            //finally { objDB.Dispose(); }

            ////SaveServiceGua(result);

            return result.Count;
        }

        protected override int ParseWorkOrder()
        {
            IList<JDBookContent> result = new List<JDBookContent>();
            string sql = "";
            string idPre = "22";
            string saveDir = @"D:\Projects\WebCrawler\BookPics";
            string savePath = "";
            string saveDir2 = "";
            bool ok = true;
            int failNum = 0;
            int okNum = 0;
            DbOperator objDB = DbBridge.GetDbOperator(DBConnStr.DataConnStr);
            try
            {
                int total = 0, pageCount = 0; ;
                int len1 = 0;
                string url = "";
                foreach (var key in dicKindUrl.Keys)
                {
                    url = dicKindUrl[key];
                    while (true)
                    {
                        string content = this.crawler.Get(url).HttpGet_SSL(null);
                        var body = JObject.Parse(content);
                        len1 = 0;
                        foreach (var row in body["results"])
                        {
                            len1++;
                            sql = @"IF NOT EXISTS(SELECT 1 FROM ZJD_书籍内容 WHERE 书籍=@书籍 AND 章节名称=@章节名称)
                              INSERT INTO ZJD_书籍内容(书籍,卷号,章节名称,章节内容,翻译内容,序号)VALUES(@书籍,@卷号,@章节名称,@章节内容,@翻译内容,@序号)
                              ELSE UPDATE ZJD_书籍内容 SET 卷号=@卷号,章节内容=@章节内容,翻译内容=@翻译内容,序号=@序号 WHERE 书籍=@书籍 AND 章节名称=@章节名称";
                            SqlParameter[] parameters2 = {
                                    new SqlParameter("@书籍",key),
                                    new SqlParameter("@章节名称",JSONValue(row, "title")),
                                    new SqlParameter("@卷号",null),
                                    new SqlParameter("@章节内容",JSONValue(row, "text")),
                                    new SqlParameter("@翻译内容",null),
                                    new SqlParameter("@序号",total+len1),
                                };
                            ok = objDB.ExecSqlCommandForAffectRow(sql, parameters2) >= 0;
                            if (!ok)
                            {
                                failNum++;
                                break;
                            }
                            okNum++;
                        }
                        total += len1;
                        pageCount++;
                        if (!ok)
                        {
                            break;
                        }
                        if (body["count"].ToObject<int>() > total) url = string.Format("{0}&page={1}", dicKindUrl[key], pageCount + 1);
                        else break;
                    }
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