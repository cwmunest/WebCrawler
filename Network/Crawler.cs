using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using WebCrawler.Helper;

namespace WebCrawler.Network
{
    public class Crawler
    {
        private const int DEFAULT_REDIRECT_NUM = 50;

        public enum DataType { JSON, FORM };

        private const string DEFAULT_USER_AGENT = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.13; rv:57.0) Gecko/20100101 Firefox/57.0";

        private CookieContainer cookies = new CookieContainer();
        private Uri url;
        private string method;
        private DataType type = DataType.JSON;

        private Encoding dataEncoding = Encoding.UTF8;
        private IDictionary<string, string> headers = new Dictionary<string, string>();

        private IList<KeyValuePair<string, object>> data = new List<KeyValuePair<string, object>>();
        private string data_str = "";

        public Crawler ClearCookie()
        {
            this.cookies = new CookieContainer();
            return this;
        }

        public Crawler ClearHeader()
        {
            this.headers = new Dictionary<string, string>();
            return this;
        }

        private IList<KeyValuePair<string, object>> queries = new List<KeyValuePair<string, object>>();
        private IList<KeyValuePair<string, string>> files = new List<KeyValuePair<string, string>>();
        private int redirectNum = DEFAULT_REDIRECT_NUM;

        public Crawler()
        {

        }

        public string Host { get; set; }
        public string Cookies
        {
            get
            {
                if (this.url == null) return "";
                return cookies.GetCookieHeader(this.url);
            }
        }

        public Crawler Get(string url)
        {
            Clear();

            //this.url = ProcessUrl(url);
            this.url = new Uri(url);
            this.method = "GET";
            return this;
        }
        public Crawler Get(string url,bool isEscape)
        {
            Clear();

            if (isEscape) this.url = ProcessUrl(url);
            else this.url = new Uri(url);
            this.method = "GET";
            return this;
        }

        private void Clear()
        {
            this.data.Clear();
            this.data_str = "";
            this.queries.Clear();
            this.files.Clear();
            this.redirectNum = DEFAULT_REDIRECT_NUM;
        }


        public Crawler Post(string url)
        {
            Clear();

            this.url = ProcessUrl(url);
            this.method = "POST";
            return this;
        }

        public Crawler SetHeader(string key, string value)
        {
            this.headers[key] = value;
            return this;
        }

        public Crawler SetHeader(object header)
        {
            if (header == null) return this;
            if (header is System.Collections.IDictionary)
            {
                System.Collections.IDictionary kvs = header as System.Collections.IDictionary;
                foreach (var key in kvs.Keys)
                {
                    var value = kvs[key];
                    if (value is string)
                        this.headers[key.ToString()] = value.ToString();
                    else
                        this.headers[key.ToString()] = JsonConvert.SerializeObject(value);
                }
            }
            else
            {
                foreach (var pair in header.GetType().GetFields()
                    .Select(field => new { key = field.Name, value = field.GetValue(header).ToString() }))
                {
                    this.headers[pair.key] = pair.value;
                }
            }
            return this;
        }

        public Crawler RemoveHeader(string key)
        {
            this.headers.Remove(key);
            return this;
        }
        public Crawler Redirect(int value)
        {
            this.redirectNum = value;
            return this;
        }

        public Crawler Field(string key, object value)
        {
            this.data.Add(new KeyValuePair<string, object>(key, value));
            return this;
        }
        public Crawler FieldStr(string value)
        {
            this.data_str=value;
            return this;
        }

        public Crawler Attach(string key, string path)
        {
            this.files.Add(new KeyValuePair<string, string>(key, path));
            return this;
        }

        public void SetCookie(string key, string value, string path = null)
        {
            this.cookies.Add(new Uri(path == null ? this.url.Scheme + "://" + this.url.Host : path), new Cookie(key, value.Replace(",", "%2C")));
        }
        public Crawler SetCookieHeader(string cookieHeader)
        {
            this.cookies.SetCookies(this.url, cookieHeader.Replace(",", "%2C"));
            return this;
        }

        public Crawler SetCookie(string value)
        {
            if (string.IsNullOrEmpty(value)) return this;
            string[] cookies = value.Split(';');
            foreach (var cookie in cookies)
            {
                string tmp = cookie.Trim();
                int equalIndex = tmp.IndexOf("=");
                if (equalIndex == -1) continue;// throw new ArgumentOutOfRangeException();
                string k = tmp.Substring(0, equalIndex);
                string v = tmp.Substring(equalIndex + 1);
                this.SetCookie(k, v);
            }
            return this;
        }

        public Response End()
        {
            return End(true);
        }
        public Response End(bool isProcessUrl)
        {
            //var request = (HttpWebRequest)HttpWebRequest.Create(ProcessUrl(this.url.OriginalString));
            var request = (HttpWebRequest)HttpWebRequest.Create(isProcessUrl?ProcessUrl(this.url.OriginalString): this.url);
            request.CookieContainer = this.cookies;
            request.Method = this.method;
            SetHeader(request);
            ProcessPostData(request);
            if (this.redirectNum <= 0) request.AllowAutoRedirect = false;
            else
            {
                request.AllowAutoRedirect = true;
                request.MaximumAutomaticRedirections = this.redirectNum;
            }

            //HttpWebResponse response;
            //try
            //{
            //    response = (HttpWebResponse)request.GetResponse();
            //}
            //catch (WebException e)
            //{
            //    response = (HttpWebResponse)e.Response;
            //}
            try
            {
                return new Response((HttpWebResponse)request.GetResponse());
            }
            catch(Exception ex)
            {
                //if (!string.IsNullOrEmpty(this.url.OriginalString))
                //    throw new ConnectionException(this.url.ToString());
                //else
                    return null;
            }
        }
        public Response End_SSL(bool isProcessUrl)
        {
            //ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3
                                   | SecurityProtocolType.Tls
                                   | (SecurityProtocolType)0x300 //Tls11
                                   | (SecurityProtocolType)0xC00; //Tls12
                                                                  //ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
                                                                  //var request = (HttpWebRequest)HttpWebRequest.Create(ProcessUrl(this.url.OriginalString));
            var request = (HttpWebRequest)HttpWebRequest.Create(isProcessUrl ? ProcessUrl(this.url.OriginalString) : this.url);
            request.CookieContainer = this.cookies;
            request.Method = this.method;
            SetHeader(request);
            ProcessPostData(request);
            if (this.redirectNum <= 0) request.AllowAutoRedirect = false;
            else
            {
                request.AllowAutoRedirect = true;
                request.MaximumAutomaticRedirections = this.redirectNum;
            }
            try
            {
                return new Response((HttpWebResponse)request.GetResponse());
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public HttpWebResponse EndOri(bool isProcessUrl)
        {
            //var request = (HttpWebRequest)HttpWebRequest.Create(ProcessUrl(this.url.OriginalString));
            var request = (HttpWebRequest)HttpWebRequest.Create(isProcessUrl ? ProcessUrl(this.url.OriginalString) : this.url);
            request.CookieContainer = this.cookies;
            request.Method = this.method;
            SetHeader(request);
            ProcessPostData(request);
            if (this.redirectNum <= 0) request.AllowAutoRedirect = false;
            else
            {
                request.AllowAutoRedirect = true;
                request.MaximumAutomaticRedirections = this.redirectNum;
            }

            //HttpWebResponse response;
            //try
            //{
            //    response = (HttpWebResponse)request.GetResponse();
            //}
            //catch (WebException e)
            //{
            //    response = (HttpWebResponse)e.Response;
            //}
            try
            {
                return (HttpWebResponse)request.GetResponse();
            }
            catch (Exception ex)
            {
                    return null;
            }
        }

        public Response End2()
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(ProcessUrl(this.url.OriginalString));
            request.CookieContainer = this.cookies;
            request.Method = this.method;
            SetHeader(request);
            ProcessPostData(request);

            if (this.redirectNum <= 0) request.AllowAutoRedirect = false;
            else
            {
                request.AllowAutoRedirect = true;
                request.MaximumAutomaticRedirections = this.redirectNum;
            }
            try
            {
                return new Response((HttpWebResponse)request.GetResponse());
            }
            catch(Exception ex)
            {
                Console.Write(ex.ToString());
                //连接出错可以忽略
                return null;
            }
        }
        /// <summary>  
        /// GET请求与获取结果  
        /// </summary>  
        public string HttpGet_SSL(string contentType)
        {
            try
            {
                //ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3
                                       | SecurityProtocolType.Tls
                                       | (SecurityProtocolType)0x300 //Tls11
                                       | (SecurityProtocolType)0xC00; //Tls12
                //ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ProcessUrl(this.url.OriginalString));
                request.CookieContainer = this.cookies;
                request.Method = this.method;
                SetHeader(request);
                ProcessPostData(request);
                if (this.redirectNum <= 0) request.AllowAutoRedirect = false;
                else
                {
                    request.AllowAutoRedirect = true;
                    request.MaximumAutomaticRedirections = this.redirectNum;
                }
                request.ContentType = contentType ?? "application/json";

                WebResponse response = request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream, Encoding.UTF8);
                string responseFromServer = reader.ReadToEnd();
                reader.Close();

                dataStream.Close();
                response.Close();

                return responseFromServer;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public Crawler Type(DataType value)
        {
            this.type = value;
            return this;
        }

        public Crawler Charset(string value)
        {
            this.dataEncoding = Encoding.GetEncoding(value);
            return this;
        }

        private void ProcessPostData(HttpWebRequest request)
        {
            if (this.method == "GET") return;

            if (this.type == DataType.JSON) ProcessJSONData(request);
            else ProcessFormData(request);
        }

        private void ProcessFormData(HttpWebRequest request)
        {
            if (this.files.Count > 0) ProcessMultipartFormData(request);
            else ProcessNormalFormData(request);
        }

        private void ProcessNormalFormData(HttpWebRequest request)
        {
            if (this.data.Count == 0) return;

            if (request.ContentType == null || !request.ContentType.Contains("text/plain"))
                request.ContentType = "application/x-www-form-urlencoded;";

            var sb = new List<string>();
            if (!string.IsNullOrEmpty(this.data_str)) sb.Add(this.data_str);
            foreach (var kv in this.data)
            {
                if (kv.Value is string)
                     sb.Add(string.Format("{0}={1}", Uri.EscapeDataString(kv.Key), Uri.EscapeDataString(kv.Value.ToString())));
                    //sb.Add(string.Format("{0}={1}", kv.Key, kv.Value));
                else
                    sb.Add(string.Format("{0}={1}", Uri.EscapeDataString(kv.Key), Uri.EscapeDataString(JsonConvert.SerializeObject(kv.Value))));
            }

            byte[] bytes = this.dataEncoding.GetBytes(string.Join("&", sb));
            //byte[] bytes = this.dataEncoding.GetBytes("grant_type=password&username=5770006&password=SGhYpCOowAZyJwNdQIxKmTbydrnVz7QB3c3FdMaMHTd7U%2FK3KqN%2FlXUaJpT8PgzzXWrxE6mPwFd8BR8C%2F4TJKk7mZLuwNrKGKBMHiuinkIpjQTw2LbuiP4I4w1qW3t0FrqK%2BT%2BLJhTuqdkJOR48DmQ%2F1X4BeiRpiMkDtZ2kcUzk%3D&captcha1=W6P6&captcha2=4527873703461265C79EB78F3A31BD00CF98C35F806E58D0A68E256A7A1EB830D9AC5C37167AAD9C");
            request.ContentLength = bytes.Length;
            using (Stream rs = request.GetRequestStream())
            {
                rs.Write(bytes, 0, bytes.Length);
            }
        }

        private void ProcessMultipartFormData(HttpWebRequest request)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundaryBytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            request.ContentType = "multipart/form-data; boundary=" + boundary;

            using (Stream rs = request.GetRequestStream())
            {
                string formDataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
                foreach (var kv in this.data)
                {
                    rs.Write(boundaryBytes, 0, boundaryBytes.Length);
                    byte[] formData = Encoding.UTF8.GetBytes(string.Format(formDataTemplate, kv.Key, kv.Value));
                    rs.Write(formData, 0, formData.Length);
                }


                string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: application/octet-stream\r\n\r\n";
                foreach (var kv in this.files)
                {
                    if (!File.Exists(kv.Value)) continue;

                    rs.Write(boundaryBytes, 0, boundaryBytes.Length);
                    byte[] headerBytes = Encoding.UTF8.GetBytes(string.Format(headerTemplate, kv.Key, kv.Value));
                    rs.Write(headerBytes, 0, headerBytes.Length);
                    using (FileStream fs = File.OpenRead(kv.Value))
                    {
                        fs.CopyTo(rs);
                    }
                }

                byte[] tailer = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
                rs.Write(tailer, 0, tailer.Length);
            }
        }

        public Crawler Query(string key, object value)
        {
            this.queries.Add(new KeyValuePair<string, object>(key, value));
            return this;
        }

        private void ProcessJSONData(HttpWebRequest request)
        {
            if (this.data.Count == 0) return;

            request.ContentType = "application/json";

            var json = new Dictionary<string, object>();
            foreach (var kv in this.data)
                json.Add(kv.Key, kv.Value);

            byte[] bytes = this.dataEncoding.GetBytes(JsonConvert.SerializeObject(json));

            request.ContentLength = bytes.Length;
            using (Stream rs = request.GetRequestStream())
            {
                rs.Write(bytes, 0, bytes.Length);
            }
        }

        private Uri ProcessUrl(string url)
        {
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                url = "http://" + url;

            var sb = new List<string>();
            foreach (var query in this.queries)
            {
                sb.Add(string.Format("{0}={1}", query.Key, query.Value));
            }
            if (sb.Count > 0)
            {
                if (url.EndsWith("?") || url.EndsWith("&")) url = url + string.Join("&", sb);
                else if (url.Contains("?")) url = url + "&" + string.Join("&", sb);
                else url = url + "?" + string.Join("&", sb);
            }
            url = Uri.EscapeUriString(url);
            return new Uri(url);
        }

        private void SetHeader(HttpWebRequest request)
        {
            foreach (var pair in this.headers)
            {
                switch (pair.Key.ToLowerInvariant())
                {
                    case "accept":
                        request.Accept = pair.Value;
                        break;
                    case "connection":
                        if (string.Compare(pair.Value, "keep-alive", true) == 0)
                            request.KeepAlive = true;
                        else
                            request.Connection = pair.Value;
                        break;
                    case "contenttype":
                    case "content-type":
                        request.ContentType = pair.Value;
                        break;
                    case "contentlength":
                    case "content-length":
                        request.ContentLength = long.Parse(pair.Value);
                        break;
                    case "expect":
                        request.Expect = pair.Value;
                        break;
                    case "date":
                        request.Date = DateTime.Parse(pair.Value);
                        break;
                    case "host":
                        request.Host = pair.Value;
                        break;
                    case "ifmodifiedsince":
                        request.IfModifiedSince = DateTime.Parse(pair.Value);
                        break;
                    case "range":
                        string[] fromto = pair.Value.Split(',');
                        if (fromto.Length == 1)
                            request.AddRange(long.Parse(fromto[0]));
                        else
                            request.AddRange(long.Parse(fromto[0]), long.Parse(fromto[1]));
                        break;
                    case "referer":
                        request.Referer = pair.Value;
                        break;
                    case "transferencoding":
                        request.TransferEncoding = pair.Value;
                        break;
                    case "useragent":
                    case "user-agent":
                        request.UserAgent = pair.Value;
                        break;
                    default:
                        request.Headers.Set(pair.Key, pair.Value);
                        break;
                }
            }


            if (string.IsNullOrEmpty(request.Host) && !string.IsNullOrEmpty(this.Host))
                request.Host = this.Host;
            if (string.IsNullOrEmpty(request.UserAgent))
                request.UserAgent = DEFAULT_USER_AGENT;

            SetHeader("referer", this.url.OriginalString);

            if (string.IsNullOrEmpty(this.Host))
                this.Host = this.url.Host;
        }


    }
}
