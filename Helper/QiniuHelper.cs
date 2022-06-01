using System;
using System.Collections.Generic;
using System.Text;

using Qiniu.Common;
using Qiniu.IO;
using Qiniu.IO.Model;
using Qiniu.Http;
using Qiniu.Util;
using Qiniu.RS;

namespace clx.Tools
{
    public class QiniuHelper
    {
        static string zoneId = System.Configuration.ConfigurationManager.AppSettings["ZoneId"];
        static bool useHTTPs = System.Configuration.ConfigurationManager.AppSettings["UseHTTPs"] == "1";
        public static ZoneID GetZoneId()
        {
            switch (zoneId)
            {
                case "ZONE_CN_East":
                    return ZoneID.CN_East;
                case "ZONE_CN_North":
                    return ZoneID.CN_North;
                case "ZONE_CN_South":
                    return ZoneID.CN_South;
                case "ZONE_US_North":
                    return ZoneID.US_North;
                case "ZONE_AS_Singapore":
                    return ZoneID.Invalid;
                default:
                    return ZoneID.Default;
            }
        }
        /// <summary>
        /// 简单上传-上传小文件
        /// </summary>
        public static bool UploadFile(string saveKey, string localFile)
        {
            // 生成(上传)凭证时需要使用此Mac
            // 这个示例单独使用了一个Settings类，其中包含AccessKey和SecretKey
            // 实际应用中，请自行设置您的AccessKey和SecretKey
            Mac mac = new Mac(hcc.Settings.AK, hcc.Settings.SK);
            string bucket = hcc.Settings.Bucket;
            //string saveKey = "1.png";
            //string localFile = "D:\\QFL\\1.png";
            // 上传策略，参见 
            // https://developer.qiniu.com/kodo/manual/put-policy
            PutPolicy putPolicy = new PutPolicy();
            // 如果需要设置为"覆盖"上传(如果云端已有同名文件则覆盖)，请使用 SCOPE = "BUCKET:KEY"
            putPolicy.Scope = bucket + ":" + saveKey;
            // putPolicy.Scope = bucket;
            // 上传策略有效期(对应于生成的凭证的有效期)          
            putPolicy.SetExpires(3600);
            // 上传到云端多少天后自动删除该文件，如果不设置（即保持默认默认）则不删除
            //putPolicy.DeleteAfterDays = 1;
            // 生成上传凭证，参见
            // https://developer.qiniu.com/kodo/manual/upload-token            
            string jstr = putPolicy.ToJsonString();
            string token = Auth.CreateUploadToken(mac, jstr);
            UploadManager um = new UploadManager();
            HttpResult result = um.UploadFile(localFile, saveKey, token);
            //Console.WriteLine(result);
            return true;
        }

        /// <summary>
        /// 简单上传-上传字节数据
        /// </summary>
        public static bool UploadData(string saveKey, byte[] data)
        {
            // 生成(上传)凭证时需要使用此Mac
            // 这个示例单独使用了一个Settings类，其中包含AccessKey和SecretKey
            // 实际应用中，请自行设置您的AccessKey和SecretKey
            Mac mac = new Mac(hcc.Settings.AK, hcc.Settings.SK);
            string bucket = hcc.Settings.Bucket;
            //string saveKey = "myfile";
            //byte[] data = System.IO.File.ReadAllBytes("D:/QFL/1.mp3");
            //byte[] data = System.Text.Encoding.UTF8.GetBytes("Hello World!");
            // 上传策略，参见 
            // https://developer.qiniu.com/kodo/manual/put-policy
            PutPolicy putPolicy = new PutPolicy();
            // 如果需要设置为"覆盖"上传(如果云端已有同名文件则覆盖)，请使用 SCOPE = "BUCKET:KEY"
            putPolicy.Scope = bucket + ":" + saveKey;
            //putPolicy.Scope = bucket;
            // 上传策略有效期(对应于生成的凭证的有效期)          
            putPolicy.SetExpires(3600);
            // 上传到云端多少天后自动删除该文件，如果不设置（即保持默认默认）则不删除
            //putPolicy.DeleteAfterDays = 1;
            // 生成上传凭证，参见
            // https://developer.qiniu.com/kodo/manual/upload-token            
            string jstr = putPolicy.ToJsonString();
            string token = Auth.CreateUploadToken(mac, jstr);

            Config.SetZone(GetZoneId(), useHTTPs);
            FormUploader fu = new FormUploader();
            HttpResult result = fu.UploadData(data, saveKey, token);
            //Console.WriteLine(result);
            return true;
        }

        /// <summary>
        /// 上传（来自网络回复的）数据流
        /// </summary>
        public static bool UploadStream(string saveKey, System.IO.Stream stream)
        {
            // 生成(上传)凭证时需要使用此Mac
            // 这个示例单独使用了一个Settings类，其中包含AccessKey和SecretKey
            // 实际应用中，请自行设置您的AccessKey和SecretKey
            Mac mac = new Mac(hcc.Settings.AK, hcc.Settings.SK);
            string bucket = hcc.Settings.Bucket;
            //string saveKey = "1.jpg";
            // 上传策略，参见 
            // https://developer.qiniu.com/kodo/manual/put-policy
            PutPolicy putPolicy = new PutPolicy();
            // 如果需要设置为"覆盖"上传(如果云端已有同名文件则覆盖)，请使用 SCOPE = "BUCKET:KEY"
            putPolicy.Scope = bucket + ":" + saveKey;
            // putPolicy.Scope = bucket;
            // 上传策略有效期(对应于生成的凭证的有效期)          
            putPolicy.SetExpires(3600);
            // 上传到云端多少天后自动删除该文件，如果不设置（即保持默认默认）则不删除
            //putPolicy.DeleteAfterDays = 1;
            // 生成上传凭证，参见
            // https://developer.qiniu.com/kodo/manual/upload-token            
            string jstr = putPolicy.ToJsonString();
            string token = Auth.CreateUploadToken(mac, jstr);
            //try
            //{
            //string url = "http://img.ivsky.com/img/tupian/pre/201610/09/beifang_shanlin_xuejing-001.jpg";
            //var wReq = System.Net.WebRequest.Create(url) as System.Net.HttpWebRequest;
            //var resp = wReq.GetResponse() as System.Net.HttpWebResponse;
            //using (var stream = resp.GetResponseStream())
            //{
            // 请不要使用UploadManager的UploadStream方法，因为此流不支持查找(无法获取Stream.Length)
            // 请使用FormUploader或者ResumableUploader的UploadStream方法
            Config.SetZone(GetZoneId(), useHTTPs);
            FormUploader fu = new FormUploader();
            var result = fu.UploadStream(stream, saveKey, token);
            return true;
            //Console.WriteLine(result);
            //}
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex);
            //}
        }

        //下载文件
        public static void DownloadFile(string url, string localFileFullName)
        {
            //文件链接地址:http://oio2cxdal.bkt.clouddn.com/1/20170213231810.jpg
            DownloadManager.Download(url, localFileFullName);
        }


        //删除文件
        public static void DeleteFile(string saveKey)
        {
            Mac mac = new Mac(hcc.Settings.AK, hcc.Settings.SK);
            BucketManager bm = new BucketManager(mac);

            bm.DeleteOp(hcc.Settings.Bucket, saveKey);
        }
    }
}

