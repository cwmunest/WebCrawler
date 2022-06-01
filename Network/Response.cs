using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

namespace WebCrawler
{
    public class Response
    {
        private byte[] responseResult = null;
        //private string responseText = null;
        private Encoding encoding;

        internal Response(HttpWebResponse httpWebResponse)
        {
            try
            {
                this.Status = httpWebResponse.StatusCode;
                this.StatusDesc = httpWebResponse.StatusDescription;
                this.Headers = httpWebResponse.Headers;

                #region HttpWebRequest开启gzip压缩
                //if (httpWebResponse.ContentEncoding.ToLower().Contains("gzip"))
                //{
                //    using (GZipStream stream = new GZipStream(httpWebResponse.GetResponseStream(), CompressionMode.Decompress))
                //    {
                //        using (StreamReader reader = new StreamReader(stream))
                //        {
                //            responseText = reader.ReadToEnd();
                //        }
                //    }
                //}
                //else if (httpWebResponse.ContentEncoding.ToLower().Contains("deflate"))
                //{
                //    using (DeflateStream stream = new DeflateStream(
                //        httpWebResponse.GetResponseStream(), CompressionMode.Decompress))
                //    {
                //        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                //        {
                //            responseText = reader.ReadToEnd();
                //        }
                //    }
                //}
                //else
                //{
                //    using (Stream stream = httpWebResponse.GetResponseStream())
                //    {
                //        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                //        {
                //            responseText = reader.ReadToEnd();
                //        }
                //    }
                //}
                #endregion
                using (MemoryStream ms = new MemoryStream())
                {
                    //if (Headers["Content-Encoding"] == "gzip")
                    if (httpWebResponse.ContentEncoding.ToLower().Contains("gzip"))
                    {
                        new GZipStream(httpWebResponse.GetResponseStream(), CompressionMode.Decompress).CopyTo(ms);
                    }
                    else if (httpWebResponse.ContentEncoding.ToLower().Contains("deflate"))
                    {
                        new DeflateStream(httpWebResponse.GetResponseStream(), CompressionMode.Decompress).CopyTo(ms);
                    }
                    else
                    {
                        httpWebResponse.GetResponseStream().CopyTo(ms);
                    }
                    this.responseResult = ms.ToArray();
                }

                if (!string.IsNullOrEmpty(httpWebResponse.CharacterSet))
                    this.encoding = Encoding.GetEncoding(httpWebResponse.CharacterSet);
                else
                    this.encoding = Encoding.UTF8;
            }
            finally
            {
                httpWebResponse.Close();
            }
        }

        public HttpStatusCode Status { get; private set; }

        public string StatusDesc { get; private set; }

        public WebHeaderCollection Headers { get; private set; }

        public string Text
        {
            get
            {
                if (this.responseResult == null) return null;
                else return this.encoding.GetString(this.responseResult);
            }
        }

        public System.IO.Stream Stream
        {
            get
            {
                if (this.responseResult == null) return null;
                else return new MemoryStream(this.responseResult);
            }
        }
        public byte[] Data
        {
            get
            {
                if (this.responseResult == null) return null;
                else return this.responseResult;
            }
        }
        public string EncodeingText(Encoding enc)
        {
                if (this.responseResult == null) return null;
                else return enc.GetString(this.responseResult);
                   }

        public Response Chartset(string value)
        {
            this.encoding = Encoding.GetEncoding(value);
            return this;
        }

        public JObject Body
        {
            get
            {
                try
                {
                    return JObject.Parse(this.Text);
                }
                catch (JsonReaderException)
                {
                    return null;
                }
            }
        }
        public JObject Body2
        {
            get
            {
                try
                {
                    return JObject.Parse(this.Text.Substring(this.Text.IndexOf("{")));
                }
                catch (JsonReaderException)
                {
                    return null;
                }
            }
        }

        public string ToFile(string filename)
        {
            using (FileStream fs = File.OpenWrite(filename))
            {
                string txt = this.Text;
                if (txt.IndexOf("data:image") == 1)
                {
                    txt = txt.Substring(txt.IndexOf(",") + 1, txt.Length - txt.IndexOf(",") - 2);
                    txt = new System.Text.RegularExpressions.Regex(@"\\r\\n").Replace(txt, "\r\n");
                    new System.IO.MemoryStream(Convert.FromBase64String(txt)).CopyTo(fs);
                    ////过滤特殊字符即可   
                    //string dummyData = txt.Trim().Replace("%", "").Replace(",", "").Replace(" ", "+");
                    //if (dummyData.Length % 4 > 0)
                    //{
                    //    dummyData = dummyData.PadRight(dummyData.Length + 4 - dummyData.Length % 4, '=');
                    //}
                    //new System.IO.MemoryStream(Convert.FromBase64String(dummyData)).CopyTo(fs);
                }
                else
                {
                    fs.Write(responseResult, 0, responseResult.Length);
                }
                fs.Flush();
            }
            return filename;
        }
    }
}
