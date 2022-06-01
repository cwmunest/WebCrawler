using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace WebCrawler
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //new WebCrawler.Parsers.DictAdmin().Start();
            //WebCrawler.Parsers.DictAdmin.hanZiSearch();
            //MessageBox.Show("OK！");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //    ThreadPool.QueueUserWorkItem((obj) =>
            //    {
            //        new WebCrawler.Parsers.DictAdmin().Start();
            //        //WebCrawler.Parsers.DictAdmin.hanZiSearch();
            //        MessageBox.Show("OK！");
            //    });

            //string startUrl = "https://www.liaoxuefeng.com/wiki/1252599548343744";
            //string saveDir = @"C:\WebSite";
            //string startUrl = "http://www.moralsoft.com/index.html";
            //string saveDir = @"C:\WebSite\Moralsoft";

            string startUrl = this.txtMainpage.Text;
            string saveDir = this.txtSavePath.Text;
            string pcPath = this.txtExcludepPath.Text;

            if (string.IsNullOrEmpty(startUrl))
            {
                MessageBox.Show("请输入爬取首页网址！");
                return;
            }
            if (string.IsNullOrEmpty(saveDir))
            {
                MessageBox.Show("请选择存放文件夹！");
                return;
            }
            ThreadPool.QueueUserWorkItem((obj) =>
            {
                WebCrawler.Parsers.WebSite.start(startUrl, saveDir, pcPath);
                MessageBox.Show("OK！");
            });

        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            WebCrawler.Parsers.WebSite.stop();
        }

        private void btnOpenSrcPath_Click(object sender, EventArgs e)
        {
            DialogResult result = fbDialog.ShowDialog();
            if (result == DialogResult.OK)
                this.txtSavePath.Text = fbDialog.SelectedPath;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
