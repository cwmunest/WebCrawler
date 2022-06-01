using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using JSONObject = Newtonsoft.Json.Linq.JObject;
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
using WebCrawler.Network;

using hcc.Tools;

namespace WebCrawler.Parsers
{
    public class WebSite : Parser
    {
        static string Protocol = "https";
        static string DN = "www.liaoxuefeng.com";
        static string DN_1 = "liaoxuefeng.com";
        static string DN_ALL = "https://www.liaoxuefeng.com";
        static string StartURL = "https://www.liaoxuefeng.com/wiki/1252599548343744";
        static string SaveRootDir = @"C:\WebSite";
        static bool stopped = false;
        static int totalPageCount = 0;

        public static List<string> ListDownHrefDict = new List<string>();
        public static List<string> ListExcludePath = new List<string>();

        public static string start(string startUrl, string saveDir,string excludePaths)
        {
            //string str = "<script src=\"/cache/lang_json_cn.js?1572521191\"></script>";
            //str = new Regex("(<script[^\\.]+src=[\\\\'\"]*)([^'\"]+)(['\"\\\\])").Replace(str, new MatchEvaluator(delegate (Match mc2)
            //{
            //    return "";
            //}));
            //return "";

            StartURL = startUrl;
            SaveRootDir = saveDir;
            Match match = Regex.Match(StartURL, "(http|https)://([\\w\\.]+)");
            DN_ALL = match.Groups[0].Value;
            Protocol = match.Groups[1].Value;
            DN = match.Groups[2].Value;
            DN_1 = DN.Substring(DN.IndexOf(".") + 1);

            ListExcludePath.Clear();
            if (!string.IsNullOrEmpty(excludePaths))
            {
                excludePaths = excludePaths.Replace("，", ",").Replace("；", ",").Replace(";", ",");
                string[] arrRet = excludePaths.Split(new string[2] { ",", "\r\n" }, StringSplitOptions.None);
                foreach(string str in arrRet)
                {
                    if(!string.IsNullOrEmpty(str)) ListExcludePath.Add(str);
                }
            }
            new WebCrawler.Parsers.WebSite().Start();
            return null;
        }
        public static void stop()
        {
            stopped = true;
        }

        protected override int ParseWorkOrder()
        {
            totalPageCount = 0;
            ListDownHrefDict.Clear();
            this.downPage(StartURL,true);
            File.AppendAllText(string.Format("{0}/down_stats.log", SaveRootDir), string.Format("{0}共下载{1}个页面\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), totalPageCount));
            return totalPageCount;
        }
        //是否在排除行列
        private bool _isExcludeUrl(string url)
        {
            foreach(string str in ListExcludePath)
            {
                if (url.IndexOf(str) != -1) return true;
            }
            return false;
        }
        private string _getSaveDir(string url)
        {
            ////"//img.zdic.net/kai/cn/5E7D.svg"
            //url = url.Substring(url.IndexOf("net/") + 4);
            //return url.Substring(0, url.LastIndexOf("/"));

            //<a href="/wiki/1252599548343744/1309138673991714">标题</a>
            if (string.IsNullOrEmpty(url)) return "";
            string str= MatchValue(url, "\\.[\\w]+/(.+)$");
            if (str == null)
            {
                str = url.Substring(0, url.LastIndexOf("/"));
                if (str.StartsWith("/")) str = str.Substring(1);
                return str;
            }
            else
            {
                if (str.IndexOf("/") != -1)
                    return str.Substring(0, str.LastIndexOf("/"));
                else
                    return "";
            }
        }
        private string _getFullRelPath(string curDir, string relUrl)
        {
            string ret = "";
            if (relUrl.StartsWith(".") == false)
            {
                //if (curDir == "") curDir = "/";
                ret = string.Format("{0}/{1}", curDir, relUrl);
            }
            else
            {
                var mcs = Regex.Matches(relUrl, "\\.\\./");
                if (mcs.Count == 0)
                {
                    if (relUrl.StartsWith("./")) ret = string.Format("{0}/{1}", curDir, relUrl.Substring(2));
                    else ret = relUrl;
                }
                else
                {
                    if (mcs.Count == 1) ret = string.Format("{0}/{1}", curDir.Substring(0, curDir.LastIndexOf("/")), relUrl.Substring(3));
                    else if (mcs.Count == 2)
                    {
                        curDir = curDir.Substring(0, curDir.LastIndexOf("/"));
                        if (curDir == "") return relUrl;
                        ret = string.Format("{0}/{1}", curDir.Substring(0, curDir.LastIndexOf("/")), relUrl.Substring(6));
                    }
                    else if (mcs.Count == 3)
                    {
                        curDir = curDir.Substring(0, curDir.LastIndexOf("/"));
                        curDir = curDir.Substring(0, curDir.LastIndexOf("/"));
                        if (curDir == "") return relUrl;
                        ret = string.Format("{0}/{1}", curDir.Substring(0, curDir.LastIndexOf("/")), relUrl.Substring(9));
                    }
                    else if (mcs.Count == 4)
                    {
                        curDir = curDir.Substring(0, curDir.LastIndexOf("/"));
                        curDir = curDir.Substring(0, curDir.LastIndexOf("/"));
                        curDir = curDir.Substring(0, curDir.LastIndexOf("/"));
                        if (curDir == "") return relUrl;
                        ret = string.Format("{0}/{1}", curDir.Substring(0, curDir.LastIndexOf("/")), relUrl.Substring(12));
                    }
                    else if (mcs.Count == 5)
                    {
                        curDir = curDir.Substring(0, curDir.LastIndexOf("/"));
                        curDir = curDir.Substring(0, curDir.LastIndexOf("/"));
                        curDir = curDir.Substring(0, curDir.LastIndexOf("/"));
                        curDir = curDir.Substring(0, curDir.LastIndexOf("/"));
                        if (curDir == "") return relUrl;
                        ret = string.Format("{0}/{1}", curDir.Substring(0, curDir.LastIndexOf("/")), relUrl.Substring(12));
                    }
                    else if (mcs.Count == 6)
                    {
                        curDir = curDir.Substring(0, curDir.LastIndexOf("/"));
                        curDir = curDir.Substring(0, curDir.LastIndexOf("/"));
                        curDir = curDir.Substring(0, curDir.LastIndexOf("/"));
                        curDir = curDir.Substring(0, curDir.LastIndexOf("/"));
                        ret = string.Format("{0}/{1}", curDir.Substring(0, curDir.LastIndexOf("/")), relUrl.Substring(12));
                    }

                }
            }
            if (ret == "/") ret = "";
            return ret;
        }
        private bool downPage(string url,bool isFirstPage)
        {
            if (totalPageCount > 10000)
            {
                stopped = true;
                return true;
            }
            string hrefUrl = "", cssUrl = "", picUrl = "", jsUrl = "";
            string hrefUrl2 = "", cssUrl2 = "", picUrl2 = "", jsUrl2 = "";
            string  css2Url = "", js2Url = "";
            string  css2Url2 = "", js2Url2 = "";
            string saveDir = "", savePath = "", fullDir = "", fileName = "";
            string saveDir2 = "", savePath2 = "", fullDir2 = "", fileName2 = "";
            string saveDir3 = "", savePath3 = "", fullDir3 = "", fileName3 = "";
            string content = "", cssContent = "", jsContent = "";
            Stream stream = null;
            Response response;
            Match match;
            int errNum = 0;
            bool ok = false;

            try
            {
                //if (url == "https://www.liaoxuefeng.com/wiki/1252599548343744")
                //    ok = false;

                //response = this.crawler.Get("https://www.zdic.net/hans/%E5%B9%BD")
                response = this.crawler.Get(url)
                    //.SetHeader(BASE_HEADER)
                    //.SetCookie("__gads=ID=d261c56e9aa01f30-222652cb87d200ad:T=1651389213:RT=1651389213:S=ALNI_MZIwpV5D-PTtpZuCz2AKiI90ruccg; Hm_lvt_2efddd14a5f2b304677462d06fb4f964=1651389210,1653617016; __gpi=UID=000005cb75412fa7:T=1653617016:RT=1653617016:S=ALNI_Ma7VLMGhZS1Y2D5KgKFnYrjmP3EhA; Hm_lpvt_2efddd14a5f2b304677462d06fb4f964=1653632293")
                    .End_SSL(false);
                if (response == null) return false;
                content = response.EncodeingText(Encoding.UTF8);
                if (string.IsNullOrEmpty(content))
                {
                    File.AppendAllText(string.Format("{0}/fail_down_url.log", SaveRootDir), url + "\r\n");
                    return false;
                }

                //if (url.StartsWith("http"))
                //{
                //    url = url.Replace("http://", "/").Replace("https://", "/");
                //}
                //if (url.StartsWith("//")) url = url.Replace("//", "/");

                //if (ListDownHrefDict.Contains(url))
                //    return true;

                saveDir = string.Format("/{0}", _getSaveDir(url));//不以/开头
                if (saveDir == "/") saveDir = "";
                fileName = url.Substring(url.LastIndexOf("/") + 1);
                if (fileName.IndexOf("?") != -1) fileName = fileName.Substring(0, fileName.IndexOf("?"));
                if (fileName.IndexOf(".") == -1) fileName += ".html";
                savePath = string.Format("{0}/{1}", saveDir, fileName);

                //<a href="/wiki/1252599548343744/1309138673991714">标题</a>
                //content = new Regex("<a[^/]+href=['\"]([/]{1,2}[^'\"]+)['\"]").Replace(content, new MatchEvaluator(delegate (Match mc)
                content = new Regex("(<a[^/]+href=['\"])([^'\"]+)(['\"])").Replace(content, new MatchEvaluator(delegate (Match mc)
                {
                    hrefUrl = mc.Groups[2].Value;
                    if (hrefUrl.StartsWith("http"))
                    {
                        if (hrefUrl.IndexOf(DN) == -1) return mc.Groups[0].Value;
                        hrefUrl = MatchValue(hrefUrl, "\\.[\\w]+(/.+)$");
                        return string.Format("{0}{1}{2}", mc.Groups[1].Value, hrefUrl, mc.Groups[3].Value);
                    }
                    else if (hrefUrl.IndexOf("{") != -1 || hrefUrl.IndexOf("javascript:") != -1 || hrefUrl.IndexOf("(") != -1) return mc.Groups[0].Value;
                    else if (hrefUrl.StartsWith("//"))
                    {
                        if (hrefUrl.IndexOf(DN) == -1) return mc.Groups[0].Value;
                        hrefUrl = MatchValue(hrefUrl, "\\.[\\w]+(/.+)$");
                        return string.Format("{0}{1}{2}", mc.Groups[1].Value, hrefUrl, mc.Groups[3].Value);
                    }
                    else if (hrefUrl.StartsWith("/"))
                    {
                        return mc.Groups[0].Value;
                    }
                    else
                    {
                        if(hrefUrl.EndsWith("/")) return mc.Groups[0].Value;
                        return string.Format("{0}{1}{2}", mc.Groups[1].Value, _getFullRelPath(saveDir, hrefUrl), mc.Groups[3].Value);
                    }
                }));

                //<link rel="stylesheet" href="/static/css/itranswarp.css?v=1.0.3-76ca094-2022-05-18-13-18">
                //<link rel="stylesheet" type="text/css" href="./public/ui/v2/static/css/basic.css?1572099357">
                content = new Regex("(<link[^\\.]+href=['\"])([^'\"]+)(['\"])").Replace(content, new MatchEvaluator(delegate (Match mc)
                {
                    cssUrl = mc.Groups[2].Value;
                    if(cssUrl.IndexOf(".css")==-1) return mc.Groups[0].Value;
                    if (cssUrl.StartsWith("http"))
                    {
                        if (cssUrl.IndexOf(DN) == -1) return mc.Groups[0].Value;
                        cssUrl2 = MatchValue(cssUrl, "\\.[\\w]+(/.+)$");
                        cssUrl = cssUrl2;
                    }
                    else if (cssUrl.StartsWith("//"))
                    {
                        if (cssUrl.IndexOf(DN) == -1) return mc.Groups[0].Value;
                        cssUrl2 = MatchValue(cssUrl, "\\.[\\w]+(/.+)$");
                        cssUrl = cssUrl2;
                    }
                    else if (cssUrl.StartsWith("/"))
                    {
                        cssUrl2 = cssUrl;
                    }
                    else
                    {
                           cssUrl2 = _getFullRelPath(saveDir, cssUrl);
                    }
                    saveDir2 = string.Format("/{0}", _getSaveDir(cssUrl2.Replace("/./","/")));//不以/开头
                    if (saveDir2 == "/") saveDir2 = "";
                    fileName2 = cssUrl2.Substring(cssUrl2.LastIndexOf("/") + 1);
                    if (fileName2.IndexOf("?") != -1) fileName2 = fileName2.Substring(0, fileName2.IndexOf("?"));
                    if (fileName2.IndexOf(".") == -1) fileName2 += ".css";
                    savePath2 = string.Format("{0}/{1}", saveDir2, fileName2);
                    if (!File.Exists(string.Format("{0}/{1}", SaveRootDir, savePath2)))
                    {
                        fullDir2 = string.Format("{0}{1}", SaveRootDir, saveDir2);
                        if (!System.IO.Directory.Exists(fullDir2)) System.IO.Directory.CreateDirectory(fullDir2);
                        response = this.crawler.Get(string.Format("{0}{1}", DN_ALL, cssUrl2.Replace("/./", "/")))
                        //.SetHeader(BASE_HEADER_SVG)
                        //.SetCookie("_ga=GA1.2.259231690.1648030505; __gads=ID=208119bd7c2f1ad5-2244bc4819d1003f:T=1648030505:RT=1648030505:S=ALNI_MaT8PIJ8CTa69qbYeb-RjpNWGcrZw; _gid=GA1.2.1884383594.1653092217; __gpi=UID=0000059c79f7ff91:T=1653092216:RT=1653355698:S=ALNI_Mb_FHMAjGill44Gs91jdSUqdnTiNw; _gat_gtag_UA_161009_3=1")
                        .End(false);
                        if (response != null)
                        {
                            cssContent = response.Text;
                            if (!string.IsNullOrEmpty(cssContent))
                            {
                                //url(/images/z/pyjs_h.png)
                                cssContent = new Regex("url\\(['|\"]*([^\\)]+\\.(?:png|gif|jpg|jpeg))['|\"]*\\)", RegexOptions.IgnoreCase).Replace(cssContent, new MatchEvaluator(delegate (Match mc2)
                                {
                                    picUrl = mc2.Groups[1].Value;
                                    if (picUrl.StartsWith("http"))
                                    {
                                        if (picUrl.IndexOf(DN) == -1) return mc2.Groups[0].Value;
                                        picUrl2 = MatchValue(picUrl, "\\.[\\w]+(/.+)$");
                                        picUrl = picUrl2;
                                    }
                                    else if (picUrl.StartsWith("//"))
                                    {
                                        if (picUrl.IndexOf(DN) == -1) return mc2.Groups[0].Value;
                                        picUrl2 = MatchValue(picUrl, "\\.[\\w]+(/.+)$");
                                        picUrl = picUrl2;
                                    }
                                    else if (picUrl.StartsWith("/"))
                                    {
                                        picUrl2 = picUrl;
                                    }
                                    else
                                    {
                                        picUrl2 = _getFullRelPath(saveDir2, picUrl);
                                    }
                                    saveDir3 = string.Format("/{0}", _getSaveDir(picUrl2.Replace("/./", "/")));//不以/开头
                                    if (saveDir3 == "/") saveDir3 = "";
                                    fileName3 = picUrl2.Substring(picUrl2.LastIndexOf("/") + 1);
                                    if (fileName3.IndexOf("?") != -1) fileName3 = fileName3.Substring(0, fileName3.IndexOf("?"));
                                    savePath3 = string.Format("{0}/{1}", saveDir3, fileName3);
                                    if (!File.Exists(string.Format("{0}/{1}", SaveRootDir, savePath3)))
                                    {
                                        try
                                        {
                                            fullDir3 = string.Format("{0}{1}", SaveRootDir, saveDir3);
                                            if (!System.IO.Directory.Exists(fullDir3)) System.IO.Directory.CreateDirectory(fullDir3);

                                            //stream = this.crawler.Get(string.Format("{0}{1}", DN_ALL, picUrl2))
                                            ////.SetHeader(BASE_HEADER_GIF)
                                            ////.SetHeader("path", headerPath)
                                            ////.SetCookie("_ga=GA1.2.259231690.1648030505; __gads=ID=208119bd7c2f1ad5-2244bc4819d1003f:T=1648030505:RT=1648030505:S=ALNI_MaT8PIJ8CTa69qbYeb-RjpNWGcrZw; _gid=GA1.2.1884383594.1653092217; __gpi=UID=0000059c79f7ff91:T=1653092216:RT=1653355698:S=ALNI_Mb_FHMAjGill44Gs91jdSUqdnTiNw; _gat_gtag_UA_161009_3=1")
                                            //.End(true).Stream;
                                            //System.Drawing.Image.FromStream(stream).Save(string.Format("{0}/{1}", SaveRootDir, savePath3), System.Drawing.Imaging.ImageFormat.Gif);
                                            //stream.Close();
                                            byte[] data = this.crawler.Get(string.Format("{0}{1}", DN_ALL, picUrl2.Replace("/./", "/")))
                                                           //.SetHeader(BASE_HEADER_MP3)
                                                           //.SetHeader("path", headerPath)
                                                           //.SetCookie("_ga=GA1.2.259231690.1648030505; __gads=ID=208119bd7c2f1ad5-2244bc4819d1003f:T=1648030505:RT=1648030505:S=ALNI_MaT8PIJ8CTa69qbYeb-RjpNWGcrZw; _gid=GA1.2.1884383594.1653092217; __gpi=UID=0000059c79f7ff91:T=1653092216:RT=1653355698:S=ALNI_Mb_FHMAjGill44Gs91jdSUqdnTiNw; _gat_gtag_UA_161009_3=1")
                                                           .End(true).Data;
                                            FileStream fs = File.Create(string.Format("{0}/{1}", fullDir3, fileName3));
                                            fs.Write(data, 0, data.Length);
                                            fs.Close();
                                        }
                                        catch
                                        {
                                            File.AppendAllText(string.Format("{0}/err_down_pic.log", SaveRootDir), picUrl2 + "\r\n");
                                        }
                                    }
                                    return string.Format("url({0})", picUrl);
                                }));
                                //字体文件：src:url('../../../public/ui/v2/static/plugin/slick/./fonts/slick.eot');
                                if (fileName2.IndexOf("index_cn") != -1)
                                    ok = true;
                                cssContent = new Regex("url\\(['|\"]*([^\\)'\"]+)['|\"]*\\)", RegexOptions.IgnoreCase).Replace(cssContent, new MatchEvaluator(delegate (Match mc2)
                                {
                                    picUrl = mc2.Groups[1].Value;
                                    if (picUrl.StartsWith("http"))
                                    {
                                        if (picUrl.IndexOf(DN) == -1) return mc2.Groups[0].Value;
                                        picUrl2 = MatchValue(picUrl, "\\.[\\w]+(/.+)$");
                                        picUrl = picUrl2;
                                    }
                                    else if (picUrl.StartsWith("//"))
                                    {
                                        if (picUrl.IndexOf(DN) == -1) return mc2.Groups[0].Value;
                                        picUrl2 = MatchValue(picUrl, "\\.[\\w]+(/.+)$");
                                        picUrl = picUrl2;
                                    }
                                    else if (picUrl.StartsWith("/"))
                                    {
                                        picUrl2 = picUrl;
                                    }
                                    else
                                    {
                                        picUrl2 = _getFullRelPath(saveDir2, picUrl);
                                    }
                                    saveDir3 = string.Format("/{0}", _getSaveDir(picUrl2.Replace("/./", "/")));//不以/开头
                                    if (saveDir3 == "/") saveDir3 = "";
                                    fileName3 = picUrl2.Substring(picUrl2.LastIndexOf("/") + 1);
                                    if (fileName3.IndexOf("?") != -1) fileName3 = fileName3.Substring(0, fileName3.IndexOf("?"));
                                    savePath3 = string.Format("{0}/{1}", saveDir3, fileName3);
                                    if (!File.Exists(string.Format("{0}/{1}", SaveRootDir, savePath3)))
                                    {
                                        try
                                        {
                                            fullDir3 = string.Format("{0}{1}", SaveRootDir, saveDir3);
                                            if (!System.IO.Directory.Exists(fullDir3)) System.IO.Directory.CreateDirectory(fullDir3);

                                            byte[] data = this.crawler.Get(string.Format("{0}{1}", DN_ALL, picUrl2.Replace("/./", "/")))
                                                           //.SetHeader(BASE_HEADER_MP3)
                                                           //.SetHeader("path", headerPath)
                                                           //.SetCookie("_ga=GA1.2.259231690.1648030505; __gads=ID=208119bd7c2f1ad5-2244bc4819d1003f:T=1648030505:RT=1648030505:S=ALNI_MaT8PIJ8CTa69qbYeb-RjpNWGcrZw; _gid=GA1.2.1884383594.1653092217; __gpi=UID=0000059c79f7ff91:T=1653092216:RT=1653355698:S=ALNI_Mb_FHMAjGill44Gs91jdSUqdnTiNw; _gat_gtag_UA_161009_3=1")
                                                           .End(true).Data;
                                            FileStream fs = File.Create(string.Format("{0}/{1}", fullDir3, fileName3));
                                            fs.Write(data, 0, data.Length);
                                            fs.Close();
                                        }
                                        catch
                                        {
                                            File.AppendAllText(string.Format("{0}/err_down_css_other.log", SaveRootDir), picUrl2 + "\r\n");
                                        }
                                    }
                                    return string.Format("url({0})", picUrl);
                                }));

                                File.WriteAllText(string.Format("{0}/{1}", fullDir2, fileName2), cssContent, Encoding.UTF8);
                            }
                        }
                    }
                    return string.Format("{0}{1}{2}", mc.Groups[1].Value, cssUrl, mc.Groups[3].Value);
                }));

                //<script src="/static/js/shared.js?v=1.0.3-76ca094-2022-05-18-13-18"></script>
                content = new Regex("(<script[^\\.]+src=['\"])([^'\"]+)(['\"])").Replace(content, new MatchEvaluator(delegate (Match mc)
                {
                    jsUrl = mc.Groups[2].Value;
                    if (jsUrl.IndexOf(".js") == -1) return mc.Groups[0].Value;
                    if (jsUrl.StartsWith("http"))
                    {
                        if (jsUrl.IndexOf(DN) == -1) return mc.Groups[0].Value;
                        jsUrl2 = MatchValue(jsUrl, "\\.[\\w]+(/.+)$");
                        jsUrl = jsUrl2;
                    }
                    else if (jsUrl.StartsWith("//"))
                    {
                        if (jsUrl.IndexOf(DN) == -1) return mc.Groups[0].Value;
                        jsUrl2 = MatchValue(jsUrl, "\\.[\\w]+(/.+)$");
                        jsUrl = jsUrl2;
                    }
                    else if (jsUrl.StartsWith("/"))
                    {
                        jsUrl2 = jsUrl;
                    }
                    else
                    {
                        jsUrl2 = _getFullRelPath(saveDir, jsUrl);
                    }
                    saveDir2 = string.Format("/{0}", _getSaveDir(jsUrl2.Replace("/./", "/")));//不以/开头
                    if (saveDir2 == "/") saveDir2 = "";
                    fileName2 = jsUrl2.Substring(jsUrl2.LastIndexOf("/") + 1);
                    if (fileName2.IndexOf("?") != -1) fileName2 = fileName2.Substring(0, fileName2.IndexOf("?"));
                    if (fileName2.IndexOf(".") == -1) fileName2 += ".js";
                    savePath2 = string.Format("{0}/{1}", saveDir2, fileName2);
                    if (!File.Exists(string.Format("{0}/{1}", SaveRootDir, savePath2)))
                    {
                        fullDir2 = string.Format("{0}{1}", SaveRootDir, saveDir2);
                        if (!System.IO.Directory.Exists(fullDir2)) System.IO.Directory.CreateDirectory(fullDir2);
                        response = this.crawler.Get(string.Format("{0}{1}", DN_ALL, jsUrl2.Replace("/./", "/")))
                        //.SetHeader(BASE_HEADER_SVG)
                        //.SetCookie("_ga=GA1.2.259231690.1648030505; __gads=ID=208119bd7c2f1ad5-2244bc4819d1003f:T=1648030505:RT=1648030505:S=ALNI_MaT8PIJ8CTa69qbYeb-RjpNWGcrZw; _gid=GA1.2.1884383594.1653092217; __gpi=UID=0000059c79f7ff91:T=1653092216:RT=1653355698:S=ALNI_Mb_FHMAjGill44Gs91jdSUqdnTiNw; _gat_gtag_UA_161009_3=1")
                        .End(false);
                        if (response != null)
                        {
                            jsContent = response.Text;
                            if (!string.IsNullOrEmpty(jsContent))
                            {
                                //js文件中：<script src=\"/cache/lang_json_cn.js?1572521191\"></script>"
                                jsContent = new Regex("(<script[^\\.]+src=[\\\\'\"]+)([^'\"\\\\]+)(['\"\\\\]+(?: |>))").Replace(jsContent, new MatchEvaluator(delegate (Match mc2)
                                {
                                    js2Url = mc2.Groups[2].Value;
                                    if (js2Url.IndexOf(".js") == -1) return mc2.Groups[0].Value;
                                    if (js2Url.StartsWith("http"))
                                    {
                                        if (js2Url.IndexOf(DN) == -1) return mc2.Groups[0].Value;
                                        js2Url2 = MatchValue(js2Url, "\\.[\\w]+(/.+)$");
                                        js2Url = js2Url2;
                                    }
                                    else if (js2Url.StartsWith("//"))
                                    {
                                        if (js2Url.IndexOf(DN) == -1) return mc2.Groups[0].Value;
                                        js2Url2 = MatchValue(js2Url, "\\.[\\w]+(/.+)$");
                                        js2Url = js2Url2;
                                    }
                                    else if (js2Url.StartsWith("/"))
                                    {
                                        js2Url2 = js2Url;
                                    }
                                    else
                                    {
                                        js2Url2 = _getFullRelPath(saveDir, js2Url);
                                    }
                                    saveDir3 = string.Format("/{0}", _getSaveDir(js2Url2.Replace("/./", "/")));//不以/开头
                                    if (saveDir3 == "/") saveDir3 = "";
                                    fileName3 = js2Url2.Substring(js2Url2.LastIndexOf("/") + 1);
                                    if (fileName3.IndexOf("?") != -1) fileName3 = fileName3.Substring(0, fileName3.IndexOf("?"));
                                    if (fileName3.IndexOf(".") == -1) fileName3 += ".js";
                                    savePath3 = string.Format("{0}/{1}", saveDir3, fileName3);
                                    if (!File.Exists(string.Format("{0}/{1}", SaveRootDir, savePath3)))
                                    {
                                        fullDir3 = string.Format("{0}{1}", SaveRootDir, saveDir3);
                                        if (!System.IO.Directory.Exists(fullDir3)) System.IO.Directory.CreateDirectory(fullDir3);
                                        response = this.crawler.Get(string.Format("{0}{1}", DN_ALL, js2Url2.Replace("/./", "/")))
                                        //.SetHeader(BASE_HEADER_SVG)
                                        //.SetCookie("_ga=GA1.2.259231690.1648030505; __gads=ID=208119bd7c2f1ad5-2244bc4819d1003f:T=1648030505:RT=1648030505:S=ALNI_MaT8PIJ8CTa69qbYeb-RjpNWGcrZw; _gid=GA1.2.1884383594.1653092217; __gpi=UID=0000059c79f7ff91:T=1653092216:RT=1653355698:S=ALNI_Mb_FHMAjGill44Gs91jdSUqdnTiNw; _gat_gtag_UA_161009_3=1")
                                        .End(false);
                                        if (response != null)
                                        {
                                            jsContent = response.Text;
                                            if (!string.IsNullOrEmpty(jsContent))
                                            {
                                                File.WriteAllText(string.Format("{0}/{1}", fullDir3, fileName3), jsContent, Encoding.UTF8);
                                            }
                                        }
                                    }
                                    return string.Format("{0}{1}{2}", mc2.Groups[1].Value, js2Url, mc2.Groups[3].Value);
                                }));
                                jsContent = new Regex("(<link[^\\.]+href=[\\\\'\"]+)([^'\"\\\\]+)(['\"\\\\]+)").Replace(jsContent, new MatchEvaluator(delegate (Match mc2)
                                {
                                    css2Url = mc2.Groups[2].Value;
                                    if (css2Url.IndexOf(".css") == -1) return mc2.Groups[0].Value;
                                    if (css2Url.StartsWith("http"))
                                    {
                                        if (css2Url.IndexOf(DN) == -1) return mc2.Groups[0].Value;
                                        css2Url2 = MatchValue(css2Url, "\\.[\\w]+(/.+)$");
                                        css2Url = css2Url2;
                                    }
                                    else if (css2Url.StartsWith("//"))
                                    {
                                        if (css2Url.IndexOf(DN) == -1) return mc2.Groups[0].Value;
                                        css2Url2 = MatchValue(css2Url, "\\.[\\w]+(/.+)$");
                                        css2Url = css2Url2;
                                    }
                                    else if (css2Url.StartsWith("/"))
                                    {
                                        css2Url2 = css2Url;
                                    }
                                    else
                                    {
                                        css2Url2 = _getFullRelPath(saveDir, css2Url);
                                    }
                                    saveDir3 = string.Format("/{0}", _getSaveDir(css2Url2.Replace("/./", "/")));//不以/开头
                                    if (saveDir3 == "/") saveDir3 = "";
                                    fileName3 = css2Url2.Substring(css2Url2.LastIndexOf("/") + 1);
                                    if (fileName3.IndexOf("?") != -1) fileName3 = fileName3.Substring(0, fileName3.IndexOf("?"));
                                    if (fileName3.IndexOf(".") == -1) fileName3 += ".css";
                                    savePath3 = string.Format("{0}/{1}", saveDir3, fileName3);
                                    if (!File.Exists(string.Format("{0}/{1}", SaveRootDir, savePath3)))
                                    {
                                        fullDir3 = string.Format("{0}{1}", SaveRootDir, saveDir3);
                                        if (!System.IO.Directory.Exists(fullDir3)) System.IO.Directory.CreateDirectory(fullDir3);
                                        response = this.crawler.Get(string.Format("{0}{1}", DN_ALL, css2Url2.Replace("/./", "/")))
                                        //.SetHeader(BASE_HEADER_SVG)
                                        //.SetCookie("_ga=GA1.2.259231690.1648030505; __gads=ID=208119bd7c2f1ad5-2244bc4819d1003f:T=1648030505:RT=1648030505:S=ALNI_MaT8PIJ8CTa69qbYeb-RjpNWGcrZw; _gid=GA1.2.1884383594.1653092217; __gpi=UID=0000059c79f7ff91:T=1653092216:RT=1653355698:S=ALNI_Mb_FHMAjGill44Gs91jdSUqdnTiNw; _gat_gtag_UA_161009_3=1")
                                        .End(false);
                                        if (response != null)
                                        {
                                            cssContent = response.Text;
                                            if (!string.IsNullOrEmpty(cssContent))
                                            {
                                                File.WriteAllText(string.Format("{0}/{1}", fullDir3, fileName3), cssContent, Encoding.UTF8);
                                            }
                                        }
                                    }
                                    return string.Format("{0}{1}{2}", mc2.Groups[1].Value, css2Url, mc2.Groups[3].Value);
                                }));
                                jsContent = new Regex("(<img[^\\.]+src=['\"\\\\]+)([^'\"]+\\.(?:png|gif|jpg|jpeg))(['|\"\\\\]+)", RegexOptions.IgnoreCase).Replace(jsContent, new MatchEvaluator(delegate (Match mc2)
                                {
                                    picUrl = mc2.Groups[2].Value;
                                    if (picUrl.StartsWith("http"))
                                    {
                                        if (picUrl.IndexOf(DN) == -1) return mc2.Groups[0].Value;
                                        picUrl2 = MatchValue(picUrl, "\\.[\\w]+(/.+)$");
                                        picUrl = picUrl2;
                                    }
                                    else if (picUrl.StartsWith("//"))
                                    {
                                        if (picUrl.IndexOf(DN) == -1) return mc2.Groups[0].Value;
                                        picUrl2 = MatchValue(picUrl, "\\.[\\w]+(/.+)$");
                                        picUrl = picUrl2;
                                    }
                                    else if (picUrl.StartsWith("/"))
                                    {
                                        picUrl2 = picUrl;
                                    }
                                    else
                                    {
                                        picUrl2 = _getFullRelPath(saveDir, picUrl);
                                    }
                                    saveDir3 = string.Format("/{0}", _getSaveDir(picUrl2.Replace("/./", "/")));//不以/开头
                                    if (saveDir3 == "/") saveDir3 = "";
                                    fileName3 = picUrl2.Substring(picUrl2.LastIndexOf("/") + 1);
                                    if (fileName3.IndexOf("?") != -1) fileName3 = fileName3.Substring(0, fileName3.IndexOf("?"));
                                    savePath3 = string.Format("{0}/{1}", saveDir3, fileName3);
                                    if (!File.Exists(string.Format("{0}/{1}", SaveRootDir, savePath3)))
                                    {
                                        try
                                        {
                                            fullDir3 = string.Format("{0}{1}", SaveRootDir, saveDir3);
                                            if (!System.IO.Directory.Exists(fullDir3)) System.IO.Directory.CreateDirectory(fullDir3);

                                            //stream = this.crawler.Get(string.Format("{0}{1}", DN_ALL, picUrl2))
                                            ////.SetHeader(BASE_HEADER_GIF)
                                            ////.SetHeader("path", headerPath)
                                            ////.SetCookie("_ga=GA1.2.259231690.1648030505; __gads=ID=208119bd7c2f1ad5-2244bc4819d1003f:T=1648030505:RT=1648030505:S=ALNI_MaT8PIJ8CTa69qbYeb-RjpNWGcrZw; _gid=GA1.2.1884383594.1653092217; __gpi=UID=0000059c79f7ff91:T=1653092216:RT=1653355698:S=ALNI_Mb_FHMAjGill44Gs91jdSUqdnTiNw; _gat_gtag_UA_161009_3=1")
                                            //.End(true).Stream;
                                            //System.Drawing.Image.FromStream(stream).Save(string.Format("{0}/{1}", SaveRootDir, savePath3), System.Drawing.Imaging.ImageFormat.Gif);
                                            //stream.Close();
                                            byte[] data = this.crawler.Get(string.Format("{0}{1}", DN_ALL, picUrl2.Replace("/./", "/")))
                                                           //.SetHeader(BASE_HEADER_MP3)
                                                           //.SetHeader("path", headerPath)
                                                           //.SetCookie("_ga=GA1.2.259231690.1648030505; __gads=ID=208119bd7c2f1ad5-2244bc4819d1003f:T=1648030505:RT=1648030505:S=ALNI_MaT8PIJ8CTa69qbYeb-RjpNWGcrZw; _gid=GA1.2.1884383594.1653092217; __gpi=UID=0000059c79f7ff91:T=1653092216:RT=1653355698:S=ALNI_Mb_FHMAjGill44Gs91jdSUqdnTiNw; _gat_gtag_UA_161009_3=1")
                                                           .End(true).Data;
                                            FileStream fs = File.Create(string.Format("{0}/{1}", fullDir3, fileName3));
                                            fs.Write(data, 0, data.Length);
                                            fs.Close();
                                        }
                                        catch
                                        {
                                            File.AppendAllText(string.Format("{0}/err_down_pic.log", SaveRootDir), picUrl2 + "\r\n");
                                        }
                                    }
                                    return string.Format("{0}{1}{2}", mc2.Groups[1].Value, picUrl, mc2.Groups[3].Value);
                                }));

                                File.WriteAllText(string.Format("{0}/{1}", fullDir2, fileName2), jsContent, Encoding.UTF8);
                            }
                        }
                    }
                    return string.Format("{0}{1}{2}", mc.Groups[1].Value, jsUrl, mc.Groups[3].Value);
                }));
                content = new Regex("(url=['\"])([^'\"\\+]+)(['\"])").Replace(content, new MatchEvaluator(delegate (Match mc)
                {
                    jsUrl = mc.Groups[2].Value;
                    if(!Path.HasExtension(jsUrl)) return mc.Groups[0].Value;
                    if (jsUrl.StartsWith("http"))
                    {
                        if (jsUrl.IndexOf(DN) == -1) return mc.Groups[0].Value;
                        jsUrl2 = MatchValue(jsUrl, "\\.[\\w]+(/.+)$");
                        jsUrl = jsUrl2;
                    }
                    else if (jsUrl.StartsWith("//"))
                    {
                        if (jsUrl.IndexOf(DN) == -1) return mc.Groups[0].Value;
                        jsUrl2 = MatchValue(jsUrl, "\\.[\\w]+(/.+)$");
                        jsUrl = jsUrl2;
                    }
                    else if (jsUrl.StartsWith("/"))
                    {
                        jsUrl2 = jsUrl;
                    }
                    else
                    {
                        jsUrl2 = _getFullRelPath(saveDir, jsUrl);
                    }
                    saveDir2 = string.Format("/{0}", _getSaveDir(jsUrl2.Replace("/./", "/")));//不以/开头
                    if (saveDir2 == "/") saveDir2 = "";
                    fileName2 = jsUrl2.Substring(jsUrl2.LastIndexOf("/") + 1);
                    if (fileName2.IndexOf("?") != -1) fileName2 = fileName2.Substring(0, fileName2.IndexOf("?"));
                    //if (fileName2.IndexOf(".") == -1) fileName2 += ".js";
                    savePath2 = string.Format("{0}/{1}", saveDir2, fileName2);
                    if (!File.Exists(string.Format("{0}/{1}", SaveRootDir, savePath2)))
                    {
                        fullDir2 = string.Format("{0}{1}", SaveRootDir, saveDir2);
                        if (!System.IO.Directory.Exists(fullDir2)) System.IO.Directory.CreateDirectory(fullDir2);
                        response = this.crawler.Get(string.Format("{0}{1}", DN_ALL, jsUrl2.Replace("/./", "/")))
                        //.SetHeader(BASE_HEADER_SVG)
                        //.SetCookie("_ga=GA1.2.259231690.1648030505; __gads=ID=208119bd7c2f1ad5-2244bc4819d1003f:T=1648030505:RT=1648030505:S=ALNI_MaT8PIJ8CTa69qbYeb-RjpNWGcrZw; _gid=GA1.2.1884383594.1653092217; __gpi=UID=0000059c79f7ff91:T=1653092216:RT=1653355698:S=ALNI_Mb_FHMAjGill44Gs91jdSUqdnTiNw; _gat_gtag_UA_161009_3=1")
                        .End(false);
                        if (response != null)
                        {
                            jsContent = response.Text;
                            if (!string.IsNullOrEmpty(jsContent))
                            {
                                //js文件中：<script src=\"/cache/lang_json_cn.js?1572521191\"></script>"
                                jsContent = new Regex("(<script[^\\.]+src=[\\\\'\"]+)([^'\"\\\\]+)(['\"\\\\]+(?: |>))").Replace(jsContent, new MatchEvaluator(delegate (Match mc2)
                                {
                                    js2Url = mc2.Groups[2].Value;
                                    if (js2Url.IndexOf(".js") == -1) return mc2.Groups[0].Value;
                                    if (js2Url.StartsWith("http"))
                                    {
                                        if (js2Url.IndexOf(DN) == -1) return mc2.Groups[0].Value;
                                        js2Url2 = MatchValue(js2Url, "\\.[\\w]+(/.+)$");
                                        js2Url = js2Url2;
                                    }
                                    else if (js2Url.StartsWith("//"))
                                    {
                                        if (js2Url.IndexOf(DN) == -1) return mc2.Groups[0].Value;
                                        js2Url2 = MatchValue(js2Url, "\\.[\\w]+(/.+)$");
                                        js2Url = js2Url2;
                                    }
                                    else if (js2Url.StartsWith("/"))
                                    {
                                        js2Url2 = js2Url;
                                    }
                                    else
                                    {
                                        js2Url2 = _getFullRelPath(saveDir, js2Url);
                                    }
                                    saveDir3 = string.Format("/{0}", _getSaveDir(js2Url2.Replace("/./", "/")));//不以/开头
                                    if (saveDir3 == "/") saveDir3 = "";
                                    fileName3 = js2Url2.Substring(js2Url2.LastIndexOf("/") + 1);
                                    if (fileName3.IndexOf("?") != -1) fileName3 = fileName3.Substring(0, fileName3.IndexOf("?"));
                                    if (fileName3.IndexOf(".") == -1) fileName3 += ".js";
                                    savePath3 = string.Format("{0}/{1}", saveDir3, fileName3);
                                    if (!File.Exists(string.Format("{0}/{1}", SaveRootDir, savePath3)))
                                    {
                                        fullDir3 = string.Format("{0}{1}", SaveRootDir, saveDir3);
                                        if (!System.IO.Directory.Exists(fullDir3)) System.IO.Directory.CreateDirectory(fullDir3);
                                        response = this.crawler.Get(string.Format("{0}{1}", DN_ALL, js2Url2.Replace("/./", "/")))
                                        //.SetHeader(BASE_HEADER_SVG)
                                        //.SetCookie("_ga=GA1.2.259231690.1648030505; __gads=ID=208119bd7c2f1ad5-2244bc4819d1003f:T=1648030505:RT=1648030505:S=ALNI_MaT8PIJ8CTa69qbYeb-RjpNWGcrZw; _gid=GA1.2.1884383594.1653092217; __gpi=UID=0000059c79f7ff91:T=1653092216:RT=1653355698:S=ALNI_Mb_FHMAjGill44Gs91jdSUqdnTiNw; _gat_gtag_UA_161009_3=1")
                                        .End(false);
                                        if (response != null)
                                        {
                                            jsContent = response.Text;
                                            if (!string.IsNullOrEmpty(jsContent))
                                            {
                                                File.WriteAllText(string.Format("{0}/{1}", fullDir3, fileName3), jsContent, Encoding.UTF8);
                                            }
                                        }
                                    }
                                    return string.Format("{0}{1}{2}", mc2.Groups[1].Value, js2Url, mc2.Groups[3].Value);
                                }));
                                jsContent = new Regex("(<link[^\\.]+href=[\\\\'\"]+)([^'\"\\\\]+)(['\"\\\\]+)").Replace(jsContent, new MatchEvaluator(delegate (Match mc2)
                                {
                                    css2Url = mc2.Groups[2].Value;
                                    if (css2Url.IndexOf(".css") == -1) return mc2.Groups[0].Value;
                                    if (css2Url.StartsWith("http"))
                                    {
                                        if (css2Url.IndexOf(DN) == -1) return mc2.Groups[0].Value;
                                        css2Url2 = MatchValue(css2Url, "\\.[\\w]+(/.+)$");
                                        css2Url = css2Url2;
                                    }
                                    else if (css2Url.StartsWith("//"))
                                    {
                                        if (css2Url.IndexOf(DN) == -1) return mc2.Groups[0].Value;
                                        css2Url2 = MatchValue(css2Url, "\\.[\\w]+(/.+)$");
                                        css2Url = css2Url2;
                                    }
                                    else if (css2Url.StartsWith("/"))
                                    {
                                        css2Url2 = css2Url;
                                    }
                                    else
                                    {
                                        css2Url2 = _getFullRelPath(saveDir, css2Url);
                                    }
                                    saveDir3 = string.Format("/{0}", _getSaveDir(css2Url2.Replace("/./", "/")));//不以/开头
                                    if (saveDir3 == "/") saveDir3 = "";
                                    fileName3 = css2Url2.Substring(css2Url2.LastIndexOf("/") + 1);
                                    if (fileName3.IndexOf("?") != -1) fileName3 = fileName3.Substring(0, fileName3.IndexOf("?"));
                                    if (fileName3.IndexOf(".") == -1) fileName3 += ".css";
                                    savePath3 = string.Format("{0}/{1}", saveDir3, fileName3);
                                    if (!File.Exists(string.Format("{0}/{1}", SaveRootDir, savePath3)))
                                    {
                                        fullDir3 = string.Format("{0}{1}", SaveRootDir, saveDir3);
                                        if (!System.IO.Directory.Exists(fullDir3)) System.IO.Directory.CreateDirectory(fullDir3);
                                        response = this.crawler.Get(string.Format("{0}{1}", DN_ALL, css2Url2.Replace("/./", "/")))
                                        //.SetHeader(BASE_HEADER_SVG)
                                        //.SetCookie("_ga=GA1.2.259231690.1648030505; __gads=ID=208119bd7c2f1ad5-2244bc4819d1003f:T=1648030505:RT=1648030505:S=ALNI_MaT8PIJ8CTa69qbYeb-RjpNWGcrZw; _gid=GA1.2.1884383594.1653092217; __gpi=UID=0000059c79f7ff91:T=1653092216:RT=1653355698:S=ALNI_Mb_FHMAjGill44Gs91jdSUqdnTiNw; _gat_gtag_UA_161009_3=1")
                                        .End(false);
                                        if (response != null)
                                        {
                                            cssContent = response.Text;
                                            if (!string.IsNullOrEmpty(cssContent))
                                            {
                                                File.WriteAllText(string.Format("{0}/{1}", fullDir3, fileName3), cssContent, Encoding.UTF8);
                                            }
                                        }
                                    }
                                    return string.Format("{0}{1}{2}", mc2.Groups[1].Value, css2Url, mc2.Groups[3].Value);
                                }));

                                jsContent = new Regex("(<img[^\\.]+src=['\"\\\\]+)([^'\"]+\\.(?:png|gif|jpg|jpeg))(['|\"\\\\]+)", RegexOptions.IgnoreCase).Replace(jsContent, new MatchEvaluator(delegate (Match mc2)
                                {
                                    picUrl = mc2.Groups[2].Value;
                                    if (picUrl.StartsWith("http"))
                                    {
                                        if (picUrl.IndexOf(DN) == -1) return mc2.Groups[0].Value;
                                        picUrl2 = MatchValue(picUrl, "\\.[\\w]+(/.+)$");
                                        picUrl = picUrl2;
                                    }
                                    else if (picUrl.StartsWith("//"))
                                    {
                                        if (picUrl.IndexOf(DN) == -1) return mc2.Groups[0].Value;
                                        picUrl2 = MatchValue(picUrl, "\\.[\\w]+(/.+)$");
                                        picUrl = picUrl2;
                                    }
                                    else if (picUrl.StartsWith("/"))
                                    {
                                        picUrl2 = picUrl;
                                    }
                                    else
                                    {
                                        picUrl2 = _getFullRelPath(saveDir, picUrl);
                                    }
                                    saveDir3 = string.Format("/{0}", _getSaveDir(picUrl2.Replace("/./", "/")));//不以/开头
                                    if (saveDir3 == "/") saveDir3 = "";
                                    fileName3 = picUrl2.Substring(picUrl2.LastIndexOf("/") + 1);
                                    if (fileName3.IndexOf("?") != -1) fileName3 = fileName3.Substring(0, fileName3.IndexOf("?"));
                                    savePath3 = string.Format("{0}/{1}", saveDir3, fileName3);
                                    if (!File.Exists(string.Format("{0}/{1}", SaveRootDir, savePath3)))
                                    {
                                        try
                                        {
                                            fullDir3 = string.Format("{0}{1}", SaveRootDir, saveDir3);
                                            if (!System.IO.Directory.Exists(fullDir3)) System.IO.Directory.CreateDirectory(fullDir3);

                                            //stream = this.crawler.Get(string.Format("{0}{1}", DN_ALL, picUrl2))
                                            ////.SetHeader(BASE_HEADER_GIF)
                                            ////.SetHeader("path", headerPath)
                                            ////.SetCookie("_ga=GA1.2.259231690.1648030505; __gads=ID=208119bd7c2f1ad5-2244bc4819d1003f:T=1648030505:RT=1648030505:S=ALNI_MaT8PIJ8CTa69qbYeb-RjpNWGcrZw; _gid=GA1.2.1884383594.1653092217; __gpi=UID=0000059c79f7ff91:T=1653092216:RT=1653355698:S=ALNI_Mb_FHMAjGill44Gs91jdSUqdnTiNw; _gat_gtag_UA_161009_3=1")
                                            //.End(true).Stream;
                                            //System.Drawing.Image.FromStream(stream).Save(string.Format("{0}/{1}", SaveRootDir, savePath3), System.Drawing.Imaging.ImageFormat.Gif);
                                            //stream.Close();
                                            byte[] data = this.crawler.Get(string.Format("{0}{1}", DN_ALL, picUrl2.Replace("/./", "/")))
                                                           //.SetHeader(BASE_HEADER_MP3)
                                                           //.SetHeader("path", headerPath)
                                                           //.SetCookie("_ga=GA1.2.259231690.1648030505; __gads=ID=208119bd7c2f1ad5-2244bc4819d1003f:T=1648030505:RT=1648030505:S=ALNI_MaT8PIJ8CTa69qbYeb-RjpNWGcrZw; _gid=GA1.2.1884383594.1653092217; __gpi=UID=0000059c79f7ff91:T=1653092216:RT=1653355698:S=ALNI_Mb_FHMAjGill44Gs91jdSUqdnTiNw; _gat_gtag_UA_161009_3=1")
                                                           .End(true).Data;
                                            FileStream fs = File.Create(string.Format("{0}/{1}", fullDir3, fileName3));
                                            fs.Write(data, 0, data.Length);
                                            fs.Close();
                                        }
                                        catch
                                        {
                                            File.AppendAllText(string.Format("{0}/err_down_pic.log", SaveRootDir), picUrl2 + "\r\n");
                                        }
                                    }
                                    return string.Format("{0}{1}{2}", mc2.Groups[1].Value, picUrl, mc2.Groups[3].Value);
                                }));

                                File.WriteAllText(string.Format("{0}/{1}", fullDir2, fileName2), jsContent, Encoding.UTF8);
                            }
                        }
                    }
                    return string.Format("{0}{1}{2}", mc.Groups[1].Value, jsUrl, mc.Groups[3].Value);
                }));

                //<img class="cover-image" src="/images/banner/banner0_all.jpg"/>
                content = new Regex("(<img[^\\.]+src=['\"])([^'\"]+\\.(?:png|gif|jpg|jpeg))(['|\"])", RegexOptions.IgnoreCase).Replace(content, new MatchEvaluator(delegate (Match mc)
                {
                    picUrl = mc.Groups[2].Value;
                    if (picUrl.StartsWith("http"))
                    {
                        if (picUrl.IndexOf(DN) == -1) return mc.Groups[0].Value;
                        picUrl2 = MatchValue(picUrl, "\\.[\\w]+(/.+)$");
                        picUrl = picUrl2;
                    }
                    else if (picUrl.StartsWith("//"))
                    {
                        if (picUrl.IndexOf(DN) == -1) return mc.Groups[0].Value;
                        picUrl2 = MatchValue(picUrl, "\\.[\\w]+(/.+)$");
                        picUrl = picUrl2;
                    }
                    else if (picUrl.StartsWith("/"))
                    {
                        picUrl2 = picUrl;
                    }
                    else
                    {
                        picUrl2 = _getFullRelPath(saveDir, picUrl);
                    }
                    saveDir3 = string.Format("/{0}", _getSaveDir(picUrl2.Replace("/./", "/")));//不以/开头
                    if (saveDir3 == "/") saveDir3 = "";
                    fileName3 = picUrl2.Substring(picUrl2.LastIndexOf("/") + 1);
                    if (fileName3.IndexOf("?") != -1) fileName3 = fileName3.Substring(0, fileName3.IndexOf("?"));
                    savePath3 = string.Format("{0}/{1}", saveDir3, fileName3);
                    if (!File.Exists(string.Format("{0}/{1}", SaveRootDir, savePath3)))
                    {
                        try
                        {
                            fullDir3 = string.Format("{0}{1}", SaveRootDir, saveDir3);
                            if (!System.IO.Directory.Exists(fullDir3)) System.IO.Directory.CreateDirectory(fullDir3);

                            //stream = this.crawler.Get(string.Format("{0}{1}", DN_ALL, picUrl2))
                            ////.SetHeader(BASE_HEADER_GIF)
                            ////.SetHeader("path", headerPath)
                            ////.SetCookie("_ga=GA1.2.259231690.1648030505; __gads=ID=208119bd7c2f1ad5-2244bc4819d1003f:T=1648030505:RT=1648030505:S=ALNI_MaT8PIJ8CTa69qbYeb-RjpNWGcrZw; _gid=GA1.2.1884383594.1653092217; __gpi=UID=0000059c79f7ff91:T=1653092216:RT=1653355698:S=ALNI_Mb_FHMAjGill44Gs91jdSUqdnTiNw; _gat_gtag_UA_161009_3=1")
                            //.End(true).Stream;
                            //System.Drawing.Image.FromStream(stream).Save(string.Format("{0}/{1}", SaveRootDir, savePath3), System.Drawing.Imaging.ImageFormat.Gif);
                            //stream.Close();
                            byte[] data = this.crawler.Get(string.Format("{0}{1}", DN_ALL, picUrl2.Replace("/./", "/")))
                                           //.SetHeader(BASE_HEADER_MP3)
                                           //.SetHeader("path", headerPath)
                                           //.SetCookie("_ga=GA1.2.259231690.1648030505; __gads=ID=208119bd7c2f1ad5-2244bc4819d1003f:T=1648030505:RT=1648030505:S=ALNI_MaT8PIJ8CTa69qbYeb-RjpNWGcrZw; _gid=GA1.2.1884383594.1653092217; __gpi=UID=0000059c79f7ff91:T=1653092216:RT=1653355698:S=ALNI_Mb_FHMAjGill44Gs91jdSUqdnTiNw; _gat_gtag_UA_161009_3=1")
                                           .End(true).Data;
                            FileStream fs = File.Create(string.Format("{0}/{1}", fullDir3, fileName3));
                            fs.Write(data, 0, data.Length);
                            fs.Close();
                        }
                        catch
                        {
                            File.AppendAllText(string.Format("{0}/err_down_pic.log", SaveRootDir), picUrl2 + "\r\n");
                        }
                    }
                    return string.Format("{0}{1}{2}", mc.Groups[1].Value, picUrl, mc.Groups[3].Value);
                }));
                content = new Regex("(<img[^\\.]+src=['\"])([^'\"\\.\\:}]+)(['|\"])", RegexOptions.IgnoreCase).Replace(content, new MatchEvaluator(delegate (Match mc)
                {
                    picUrl = mc.Groups[2].Value;
                    if (picUrl.StartsWith("http"))
                    {
                        if (picUrl.IndexOf(DN) == -1) return mc.Groups[0].Value;
                        picUrl2 = MatchValue(picUrl, "\\.[\\w]+(/.+)$");
                        picUrl = picUrl2;
                    }
                    else if (picUrl.StartsWith("//"))
                    {
                        if (picUrl.IndexOf(DN) == -1) return mc.Groups[0].Value;
                        picUrl2 = MatchValue(picUrl, "\\.[\\w]+(/.+)$");
                        picUrl = picUrl2;
                    }
                    else if (picUrl.StartsWith("/"))
                    {
                        picUrl2 = picUrl;
                    }
                    else
                    {
                        picUrl2 = _getFullRelPath(saveDir, picUrl);
                    }
                    saveDir3 = string.Format("/{0}", _getSaveDir(picUrl2.Replace("/./", "/")));//不以/开头
                    if (saveDir3 == "/") saveDir3 = "";
                    fileName3 = picUrl2.Substring(picUrl2.LastIndexOf("/") + 1);
                    if (fileName3.IndexOf("?") != -1) fileName3 = fileName3.Substring(0, fileName3.IndexOf("?"));
                    savePath3 = string.Format("{0}/{1}", saveDir3, fileName3);
                    if (!File.Exists(string.Format("{0}/{1}", SaveRootDir, savePath3)))
                    {
                        try
                        {
                            fullDir3 = string.Format("{0}{1}", SaveRootDir, saveDir3);
                            if (!System.IO.Directory.Exists(fullDir3)) System.IO.Directory.CreateDirectory(fullDir3);

                            //stream = this.crawler.Get(string.Format("{0}{1}", DN_ALL, picUrl2))
                            ////.SetHeader(BASE_HEADER_GIF)
                            ////.SetHeader("path", headerPath)
                            ////.SetCookie("_ga=GA1.2.259231690.1648030505; __gads=ID=208119bd7c2f1ad5-2244bc4819d1003f:T=1648030505:RT=1648030505:S=ALNI_MaT8PIJ8CTa69qbYeb-RjpNWGcrZw; _gid=GA1.2.1884383594.1653092217; __gpi=UID=0000059c79f7ff91:T=1653092216:RT=1653355698:S=ALNI_Mb_FHMAjGill44Gs91jdSUqdnTiNw; _gat_gtag_UA_161009_3=1")
                            //.End(true).Stream;
                            //System.Drawing.Image.FromStream(stream).Save(string.Format("{0}/{1}", SaveRootDir, savePath3), System.Drawing.Imaging.ImageFormat.Gif);
                            //stream.Close();
                            byte[] data = this.crawler.Get(string.Format("{0}{1}", DN_ALL, picUrl2.Replace("/./", "/")))
                                           //.SetHeader(BASE_HEADER_MP3)
                                           //.SetHeader("path", headerPath)
                                           //.SetCookie("_ga=GA1.2.259231690.1648030505; __gads=ID=208119bd7c2f1ad5-2244bc4819d1003f:T=1648030505:RT=1648030505:S=ALNI_MaT8PIJ8CTa69qbYeb-RjpNWGcrZw; _gid=GA1.2.1884383594.1653092217; __gpi=UID=0000059c79f7ff91:T=1653092216:RT=1653355698:S=ALNI_Mb_FHMAjGill44Gs91jdSUqdnTiNw; _gat_gtag_UA_161009_3=1")
                                           .End(true).Data;
                            FileStream fs = File.Create(string.Format("{0}/{1}", fullDir3, fileName3));
                            fs.Write(data, 0, data.Length);
                            fs.Close();
                        }
                        catch
                        {
                            File.AppendAllText(string.Format("{0}/err_down_pic.log", SaveRootDir), picUrl2 + "\r\n");
                        }
                    }
                    return string.Format("{0}{1}{2}", mc.Groups[1].Value, picUrl, mc.Groups[3].Value);
                }));

                // <img data-original="/images/common/bar-two-ic1.png" alt="开源">
                content = new Regex("(data-original=['\"])([^'\"]+)(['\"])").Replace(content, new MatchEvaluator(delegate (Match mc)
                {
                    picUrl = mc.Groups[2].Value;
                    if (!Path.HasExtension(jsUrl)) return mc.Groups[0].Value;
                    if (picUrl.StartsWith("http"))
                    {
                        if (picUrl.IndexOf(DN) == -1) return mc.Groups[0].Value;
                        picUrl2 = MatchValue(picUrl, "\\.[\\w]+(/.+)$");
                        picUrl = picUrl2;
                    }
                    else if (picUrl.StartsWith("//"))
                    {
                        if (picUrl.IndexOf(DN) == -1) return mc.Groups[0].Value;
                        picUrl2 = MatchValue(picUrl, "\\.[\\w]+(/.+)$");
                        picUrl = picUrl2;
                    }
                    else if (picUrl.StartsWith("/"))
                    {
                        picUrl2 = picUrl;
                    }
                    else
                    {
                        picUrl2 = _getFullRelPath(saveDir, picUrl);
                    }
                    saveDir2 = string.Format("/{0}", _getSaveDir(picUrl2.Replace("/./", "/")));//不以/开头
                    if (saveDir2 == "/") saveDir2 = "";
                    fileName2 = picUrl2.Substring(picUrl2.LastIndexOf("/") + 1);
                    if (fileName2.IndexOf("?") != -1) fileName2 = fileName2.Substring(0, fileName2.IndexOf("?"));
                    if (fileName2.IndexOf(".") == -1) fileName2 += ".js";
                    savePath2 = string.Format("{0}/{1}", saveDir2, fileName2);
                    if (!File.Exists(string.Format("{0}/{1}", SaveRootDir, savePath2)))
                    {
                        try
                        {
                            fullDir2 = string.Format("{0}{1}", SaveRootDir, saveDir2);
                            if (!System.IO.Directory.Exists(fullDir2)) System.IO.Directory.CreateDirectory(fullDir2);
                            byte[] data = this.crawler.Get(string.Format("{0}{1}", DN_ALL, picUrl2.Replace("/./", "/")))
                                           //.SetHeader(BASE_HEADER_MP3)
                                           //.SetHeader("path", headerPath)
                                           //.SetCookie("_ga=GA1.2.259231690.1648030505; __gads=ID=208119bd7c2f1ad5-2244bc4819d1003f:T=1648030505:RT=1648030505:S=ALNI_MaT8PIJ8CTa69qbYeb-RjpNWGcrZw; _gid=GA1.2.1884383594.1653092217; __gpi=UID=0000059c79f7ff91:T=1653092216:RT=1653355698:S=ALNI_Mb_FHMAjGill44Gs91jdSUqdnTiNw; _gat_gtag_UA_161009_3=1")
                                           .End(true).Data;
                            FileStream fs = File.Create(string.Format("{0}/{1}", fullDir2, fileName2));
                            fs.Write(data, 0, data.Length);
                            fs.Close();
                        }
                        catch
                        {
                            File.AppendAllText(string.Format("{0}/err_down_pic.log", SaveRootDir), picUrl2 + "\r\n");
                        }
                    }
                    return string.Format("{0}{1}{2}", mc.Groups[1].Value, picUrl, mc.Groups[3].Value);
                }));

                if (!File.Exists(string.Format("{0}/{1}", SaveRootDir, savePath)))
                {
                    fullDir = string.Format("{0}{1}", SaveRootDir, saveDir);
                    if (!System.IO.Directory.Exists(fullDir)) System.IO.Directory.CreateDirectory(fullDir);
                    File.WriteAllText(string.Format("{0}/{1}", fullDir, fileName), content, Encoding.UTF8);
                }

                var matches = Regex.Matches(content, "<a[^/]+href=['\"]([^'\"]+)['\"]");
                foreach (Match mc in matches)
                {
                    if (stopped) break;
                    hrefUrl = mc.Groups[1].Value;
                    if (hrefUrl.IndexOf("?") != -1) hrefUrl = hrefUrl.Substring(0, hrefUrl.IndexOf("?"));
                    if (hrefUrl.IndexOf("{") != -1 || hrefUrl.IndexOf("javascript:") != -1 || hrefUrl.IndexOf("(") != -1 || hrefUrl.IndexOf("#") != -1) continue;
                    else if (hrefUrl.StartsWith("http")) continue;
                    else if (hrefUrl.EndsWith("/")) continue;
                    else
                    {
                        if (hrefUrl.StartsWith("//"))
                        {
                            if (hrefUrl.IndexOf(DN) == -1) continue;
                            hrefUrl = MatchValue(hrefUrl, "\\.[\\w]+(/.+)$");
                        }
                        if (hrefUrl.StartsWith("/"))
                        {
                            if (hrefUrl.IndexOf("?") != -1) hrefUrl = hrefUrl.Substring(0, hrefUrl.IndexOf("?"));
                        }
                        else if (hrefUrl.StartsWith("."))
                        {
                            hrefUrl = _getFullRelPath(saveDir, hrefUrl);
                        }
                        if (ListDownHrefDict.Contains(hrefUrl)) continue;
                        else ListDownHrefDict.Add(hrefUrl);
                        if(_isExcludeUrl(hrefUrl)) continue;
                        downPage(string.Format("{0}{1}", DN_ALL, hrefUrl), false);
                    }
                }

                if (isFirstPage && savePath!= "/index.html")
                {
                    //在根目录下创建index.html页
                    //fileName = "index.html";
                    content = string.Format("<script>location.href='{0}';</script>",savePath);
                    File.AppendAllText(string.Format("{0}/index.html", SaveRootDir), content);
                }

                totalPageCount++;
                //ListDownHrefDict.Add(url);
                return true;
            }
            catch (Exception ex)
            {
                File.AppendAllText(string.Format("{0}/fail_down_url.log", SaveRootDir), url + "\r\n");
                File.AppendAllText(string.Format("{0}/fail_down_url.log", SaveRootDir), "系统错误：" + ex.ToString() + "\r\n");
                return false;
            }
        }

        private static IDictionary<string, string> BASE_HEADER = new Dictionary<string, string>
        {
            ["authority"] = "www.liaoxuefeng.com",
            ["method"] = "GET",
            ["path"] = "/wiki/1252599548343744",
            ["scheme"] = "https",
            ["accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9",
            ["accept-encoding"] = "gzip, deflate, br",
            ["accept-language"] = "zh-CN,zh;q=0.9,en;q=0.8",
            ["cache-control"] = "max-age=0",
            //["connection"] = "keep-alive",
            //["Content-Type"] = "text/xml;charset=UTF-8",
            ["sec-ch-ua"] = "\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"100\", \"Google Chrome\";v=\"100\"",
            //["Host"] = "www.chaziwang.com",
            //["Host"] = "www.chaziwang.com",
            ["upgrade-insecure-requests"] = "1",
            //["Origin"] = "http://www.quanxue.cn/CT_RuJia/ZhouYi/ZhouYi07.html",
            //["Referer"] = "https://www.zdic.net/",
            //["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.159 Safari/537.36",
            ["user-agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.88 Safari/537.36",
        };

        private static IDictionary<string, string> BASE_HEADER_SVG = new Dictionary<string, string>
        {
            ["authority"] = "www.liaoxuefeng.com",
            ["method"] = "GET",
            //["path"] = "/wiki/1252599548343744",
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
            //["Referer"] = "https://www.zdic.net/",
            //["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.159 Safari/537.36",
            ["User-Agent"] = "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.88 Mobile Safari/537.36",
        };
        private static IDictionary<string, string> BASE_HEADER_GIF = new Dictionary<string, string>
        {
            ["authority"] = "img.zdic.net",
            ["method"] = "GET",
            //["path"] = "/kai/jbh/4E1A.gif",
            ["scheme"] = "https",
            ["Accept"] = "mage/avif,image/webp,image/apng,image/svg+xml,image/*,*/*;q=0.8",
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
        private static IDictionary<string, string> BASE_HEADER_MP3 = new Dictionary<string, string>
        {
            ["authority"] = "img.zdic.net",
            ["method"] = "GET",
            //["path"] = "/kai/jbh/5E7D.gif",
            ["scheme"] = "https",
            ["accept"] = "*/*",
            ["accept-encoding"] = "identity;q=1, *;q=0",
            ["accept-language"] = "zh-CN,zh;q=0.9",
            //["range"] = "0-",
            //["Cache-Control"] = "max-age=0",
            //["Connection"] = "keep-alive",
            //["Content-Type"] = "text/xml;charset=UTF-8",
            //["Host"] = "www.chaziwang.com",
            //["Upgrade-Insecure-Requests"] = "1",
            //["Origin"] = "http://www.quanxue.cn/CT_RuJia/ZhouYi/ZhouYi07.html",
            ["referer"] = "https://www.zdic.net/",
            //["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.159 Safari/537.36",
            ["user-agent"] = "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.88 Mobile Safari/537.36",
        };
    }
}
