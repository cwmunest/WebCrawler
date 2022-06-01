using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler.Entity
{
    [Table("ZJD_书籍内容")]
    public class JDBookContent
    {
        [Key]
        [Column]
        public int AI { get; set; }
        [Column]
        public int 书籍 { get; set; }
        [Column]
        public int 卷号 { get; set; }
        [Column]
        public string 章节名称 { get; set; }
        [Column]
        public string 章节内容 { get; set; }
        [Column]
        public int 序号 { get; set; }
    }
}
