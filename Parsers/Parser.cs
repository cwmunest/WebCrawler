using System;
using System.Runtime.Caching;
using WebCrawler;
using WebCrawler.Entity;
using System.Linq;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using Newtonsoft.Json.Linq;
using WebCrawler.Network;
using WebCrawler.Helper;

namespace WebCrawler.Parsers
{
    public abstract class Parser
    {
        protected Crawler crawler;

        protected Parser()
        {
            this.crawler = new Crawler();
        }

        public void Start()
        {

            this.StartWorkOrderData();
            //System.Threading.Thread.Sleep(60 * 1000);
        }

        public virtual void StartWorkOrderData()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            int count = this.ParseWorkOrder();
            watch.Stop();
        }

        protected virtual void SaveCookie()
        {
        }
        public string GetCookiesValue(string key)
        {
            //string[] cookies = this.oldCookies[0].Split(';');
            string[] cookies = this.GetCookies()[0].Split(';');
            foreach (var cookie in cookies)
            {
                string tmp = cookie.Trim();
                int equalIndex = tmp.IndexOf("=");
                if (equalIndex == -1) continue;

                if (key == tmp.Substring(0, equalIndex).Trim())
                    return tmp.Substring(equalIndex + 1).Trim();
            }
            return null;
        }
        public string GetCookieValue(string values, string key)
        {
            //string[] cookies = this.oldCookies[0].Split(';');
            string[] cookies = values.Split(';');
            foreach (var cookie in cookies)
            {
                string tmp = cookie.Trim();
                int equalIndex = tmp.IndexOf("=");
                if (equalIndex == -1) continue;

                if (key == tmp.Substring(0, equalIndex).Trim())
                    return tmp.Substring(equalIndex + 1).Trim();
            }
            return null;
        }

        protected virtual void SaveHeader()
        {
        }
        protected virtual IList<string> GetCookies()
        {
            return new List<string>()
            {
                this.crawler.Cookies
            };
        }

        protected abstract int ParseWorkOrder();
        
        public virtual void Stop()
        {

        }


        protected DateTime? ParseTime(string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value)) return null;
                return DateTime.Parse(value);
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected int? ParseInt(string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value)) return null;
                return int.Parse(value);
            }
            catch (Exception)
            {
                return null;
            }
        }
        protected long? ParseLong(string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value)) return null;
                return long.Parse(value);
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected Nullable<T> JSONValue<T>(JToken token, string name) where T : struct
        {
            if (token == null) return null;
            var value = token[name];
            if (value == null || value.ToObject<string>() == "") return null;
            else
            {
                try
                {
                    return token[name].ToObject<Nullable<T>>();
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        protected string JSONValue(JToken token, string name)
        {
            if (token == null) return null;
            var value = token[name];
            if (value == null) return null;
            else return token[name].ToString();
        }

        protected virtual string MatchValue(string target, string regstr = VALUE_REG, int? index = null)
        {
            if (string.IsNullOrEmpty(target)) return null;
            var match = Regex.Match(target, regstr);
            if (!match.Success) return null;
            if (index.HasValue) return match.Groups[index.Value].Value;
            else if (string.IsNullOrEmpty(match.Groups[1].Value)) return match.Groups[2].Value;
            else return match.Groups[1].Value;
        }
        protected virtual string MatchValues(string target, string regstr = VALUE_REG, int? index = null)
        {
            if (string.IsNullOrEmpty(target)) return null;
            List<string> list = new List<string>();
            var mcs = Regex.Matches(target, regstr);
            foreach (Match mc in mcs)
            {
                if (index.HasValue) list.Add(mc.Groups[index.Value].Value);
                else list.Add(mc.Groups[1].Value);
            }
            return string.Join(";", list.ToArray());
        }

        protected string MatchInput(string target, string id)
        {
            return MatchValue(MatchValue(target, string.Format("<input[^>]*id=\"{0}\"[^>]*>", id), 0));
        }

        protected string MatchTextArea(string target, string id)
        {
            return MatchValue(target, string.Format("<textarea[^>]*id=\"{0}\"[^>]*>([^<]*)<\\/textarea>", id));
        }

        protected string MatchSelected(string target, string id)
        {
            return MatchValue(MatchValue(target, string.Format("<select[^>]*id=\"{0}\"[^>]*>((?:.|\\s)*?)<\\/select>", id)),
                "<option[^>]*selected=\"selected\"[^>]*>([^<]*)");
        }

        protected string MatchElement(string target, string tagName, string id)
        {
            return MatchValue(target, string.Format("<{1}[^>]*id=\"{0}\"[^>]*>((?:.|\\s)*?)<\\/{1}>", id, tagName));
        }

        protected IEnumerable<KeyValuePair<string, string>> GetViewState(string content)
        {
            for (var match = Regex.Match(content, "name=\"(__[^\"]*)\".*?value=\"([^\"]*)\""); match.Success; match = match.NextMatch())
                yield return new KeyValuePair<string, string>(match.Groups[1].Value, match.Groups[2].Value);
        }

        protected long GetTime()
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            return (long)(DateTime.Now - startTime).TotalMilliseconds; // 相差毫秒数
        }
        protected System.DateTime? ConvertLongDateTime(long? d)
        {
            if (d == null) return null;
            System.DateTime time = System.DateTime.MinValue;
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            time = startTime.AddMilliseconds(d.Value);
            return time;
        }

        protected const string VALUE_REG = "value=(?:'([^']*)'|\"([^\"]*)\")";
    }
}