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
    public class ZiDian : Parser
    {
        //private static string DN1 = "https://www.zdic.net/hans/%E5%B9%BD";
        //private static string DN2 = "http://www.chaziwang.com/?q=%E5%B9%BD";
        protected override int ParseWorkOrder()
        {
            IList<JDBookContent> result = new List<JDBookContent>();
            try
            {
                Response response;
                Match match;
                string style1 = "", style2 = "", style3 = "";
                string content1 = "", content2 = "", content3 = "";
                string html1 = "", html2 = "", html3 = "";
                string baseInfo1 = "", baseInfo2 = "", baseExplain = "", detialExplain = "";

                //response = this.crawler.Get("https://www.zdic.net/hans/%E5%B9%BD")
                //    .SetHeader(BASE_HEADER)
                //    .SetCookie("GA1.2.259231690.1648030505; __gads=ID=208119bd7c2f1ad5-2244bc4819d1003f:T=1648030505:RT=1648030505:S=ALNI_MaT8PIJ8CTa69qbYeb-RjpNWGcrZw; _gid=GA1.2.1884383594.1653092217; __gpi=UID=0000059c79f7ff91:T=1653092216:RT=1653268532:S=ALNI_Mb_FHMAjGill44Gs91jdSUqdnTiNw; _gat_gtag_UA_161009_3=1")
                //    .End(false);
                //string content = response.EncodeingText(Encoding.UTF8);

                response = this.crawler.Get("https://img.zdic.net/song/cn/212E2.svg")
                    .SetHeader(BASE_HEADER_SVG)
                    .SetCookie("GA1.2.259231690.1648030505; __gads=ID=208119bd7c2f1ad5-2244bc4819d1003f:T=1648030505:RT=1648030505:S=ALNI_MaT8PIJ8CTa69qbYeb-RjpNWGcrZw; _gid=GA1.2.1884383594.1653092217; __gpi=UID=0000059c79f7ff91:T=1653092216:RT=1653268532:S=ALNI_Mb_FHMAjGill44Gs91jdSUqdnTiNw")
                    .End(false);
                string content = response.EncodeingText(Encoding.UTF8);

                return 0;

                response = this.crawler.Get("http://www.chaziwang.com/?q=%E5%B9%BD")
                   .SetHeader(BASE_HEADER).SetHeader("Host", "www.chaziwang.com").SetHeader("Referer", "www.chaziwang.com")
                   //.SetCookie("PHPSESSID=b8lbp1loa0cac26u4l4g5nerj2; Hm_lvt_5cf8bbc5bce0139dfebe6b9e12d8e939=1653092839; Hm_lpvt_5cf8bbc5bce0139dfebe6b9e12d8e939=1653099434")
                   .End(false);
                //bool bb = response.Status == System.Net.HttpStatusCode.Redirect;
                html1 = response.EncodeingText(Encoding.UTF8);
                if (html1.StartsWith("<script>")){
                    //cookie="C3VK=d95b71;
                    match = Regex.Match(html1, "cookie=\"([^;]+;)");
                    response = this.crawler.Get("http://www.chaziwang.com/?q=%E5%B9%BD")
                    .SetHeader(BASE_HEADER).SetHeader("Host", "www.chaziwang.com").SetHeader("Referer", "www.chaziwang.com")
                    .SetCookie(match.Groups[1].Value)
                    .End(false);
                    html1 = response.EncodeingText(Encoding.UTF8);
                }

                match = Regex.Match(html1, "<style>([\\w\\W]+)</style>");
                if (match.Success) style1 = match.Groups[1].Value;
                match = Regex.Match(html1, "<div id=\"container\">(<div class=\"content[\\w\\W]+</div><div class=\"clearfix mt10\"></div>)(<div class=\"content[\\w\\W]+</div></div><div class=\"clearfix mt10\"></div>)<div class=\"content[\\w\\W]+(<div class=\"content mt10\"><h2 class=\"title f16\"><span>.基本解释[\\w\\W]+)(<div class=\"content mt10\"><h2 class=\"title f16\"><span>.详细解释</span>[\\w\\W]+)</div>﻿<div id=\"footer\">");
                if (match.Success)
                {
                    baseInfo1 = match.Groups[1].Value;
                    baseInfo2 = match.Groups[2].Value;
                    baseExplain = match.Groups[3].Value;
                    detialExplain = match.Groups[4].Value;
                }

                response = this.crawler.Get("http://shuowen.chaziwang.com/?q=%E5%B9%BD")
                    .SetHeader(BASE_HEADER).SetHeader("Host", "shuowen.chaziwang.com").SetHeader("Referer", "shuowen.chaziwang.com")
                    //.SetCookie("C3VK=7da701; PHPSESSID=b8lbp1loa0cac26u4l4g5nerj2; Hm_lvt_5cf8bbc5bce0139dfebe6b9e12d8e939=1653092839; Hm_lpvt_5cf8bbc5bce0139dfebe6b9e12d8e939=1653099434")
                    .End(false);
                //bool bb = response.Status == System.Net.HttpStatusCode.Redirect;
                html2 = response.EncodeingText(Encoding.UTF8);
                if (html2.StartsWith("<script>"))
                {
                    //cookie="C3VK=d95b71;
                    match = Regex.Match(html2, "cookie=\"([^;]+;)");
                    response = this.crawler.Get("http://shuowen.chaziwang.com/?q=%E5%B9%BD")
                    .SetHeader(BASE_HEADER).SetHeader("Host", "shuowen.chaziwang.com").SetHeader("Referer", "shuowen.chaziwang.com")
                    .SetCookie(match.Groups[1].Value)
                    .End(false);
                    html2 = response.EncodeingText(Encoding.UTF8);
                }
                match = Regex.Match(html2, "<style>([\\w\\W]+)</style>");
                if (match.Success) style2 = match.Groups[1].Value;
                match = Regex.Match(html2, "(<div id=\"container\">[\\w\\W]+)<div id=\"footer\">");
                if (match.Success) content2 = match.Groups[1].Value;

                response = this.crawler.Get("http://kangxi.chaziwang.com/?q=%E5%B9%BD")
                   .SetHeader(BASE_HEADER).SetHeader("Host", "kangxi.chaziwang.com").SetHeader("Referer", "kangxi.chaziwang.com")
                   //.SetCookie("PHPSESSID=b8lbp1loa0cac26u4l4g5nerj2; Hm_lvt_5cf8bbc5bce0139dfebe6b9e12d8e939=1653092839; Hm_lpvt_5cf8bbc5bce0139dfebe6b9e12d8e939=1653099434")
                   .End(false);
                //bool bb = response.Status == System.Net.HttpStatusCode.Redirect;
                html3 = response.EncodeingText(Encoding.UTF8);
                if (html3.StartsWith("<script>"))
                {
                    //cookie="C3VK=d95b71;
                    match = Regex.Match(html3, "cookie=\"([^;]+;)");
                    response = this.crawler.Get("http://kangxi.chaziwang.com/?q=%E5%B9%BD")
                    .SetHeader(BASE_HEADER).SetHeader("Host", "shuowen.chaziwang.com").SetHeader("Referer", "shuowen.chaziwang.com")
                    .SetCookie(match.Groups[1].Value)
                    .End(false);
                    html3 = response.EncodeingText(Encoding.UTF8);
                }
                match = Regex.Match(html3, "<style>([\\w\\W]+)</style>");
                if (match.Success) style3 = match.Groups[1].Value;
                match = Regex.Match(html3, "(<div id=\"container\">[\\w\\W]+)<div id=\"footer\">");
                if (match.Success) content3 = match.Groups[1].Value;


                //string content = hcc.Tools.HttpHelper.HttpGet("https://www.zdic.net/hans/%E5%B9%BD");
                //string content = hcc.Tools.HttpHelper.HttpGet("http://www.chaziwang.com/?q=%E5%B9%BD");
                //string content = hcc.Tools.HttpHelper.HttpGet("http://www.chaziwang.com/?q=%E9%83%A8");
                //string content = hcc.Tools.HttpHelper.HttpGet("http://shuowen.chaziwang.com/?q=%E5%B9%BD");


            }
            catch (Exception ex)
            {
            }
            finally {  }

            return result.Count;
        }


        private IDictionary<string, string> BASE_HEADER = new Dictionary<string, string>
        {
            ["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9",
            ["Accept-Encoding"] = "gzip, deflate, br",
            ["Accept-Language"] = "zh-CN,zh;q=0.9,en;q=0.8",
            ["Cache-Control"] = "max-age=0",
            ["Connection"] = "keep-alive",
            //["Content-Type"] = "text/xml;charset=UTF-8",
            //["Host"] = "www.chaziwang.com",
            ["Upgrade-Insecure-Requests"] = "1",
            //["Origin"] = "http://www.quanxue.cn/CT_RuJia/ZhouYi/ZhouYi07.html",
            //["Referer"] = "http://shuowen.chaziwang.com/",
            //["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.159 Safari/537.36",
            ["User-Agent"] = "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.88 Mobile Safari/537.36",
        };

        private IDictionary<string, string> BASE_HEADER_SVG = new Dictionary<string, string>
        {
            ["authority"] = "img.zdic.net",
            ["method"] = "GET",
            ["path"] = "/kai/kr/5E7D.svg",
            ["scheme"] = "https",
            ["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9",
            ["Accept-Encoding"] = "gzip, deflate, br",
            ["Accept-Language"] = "zh-CN,zh;q=0.9,en;q=0.8",
            ["Cache-Control"] = "max-age=0",
            ["Connection"] = "keep-alive",
            //["Content-Type"] = "text/xml;charset=UTF-8",
            //["Host"] = "www.chaziwang.com",
            //["Upgrade-Insecure-Requests"] = "1",
            //["Origin"] = "http://www.quanxue.cn/CT_RuJia/ZhouYi/ZhouYi07.html",
            ["Referer"] = "https://www.zdic.net/",
            //["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.159 Safari/537.36",
            ["User-Agent"] = "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.88 Mobile Safari/537.36",
        };
    }
}