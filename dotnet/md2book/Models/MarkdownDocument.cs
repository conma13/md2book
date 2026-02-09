using System;
using System.Collections.Generic;
using System.Text;

namespace md2book.Models
{
    public class MarkdownDocument
    {
        public string FilePath { get; set; }
        public string Content { get; set; }
        public List<TocEntry> Headings { get; set; } = [];
        public string Html { get; set; }
    }
}
