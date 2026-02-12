using System;
using System.Collections.Generic;
using System.Text;

namespace md2book.Models
{
    public class MarkdownDocument
    {
        public string FilePath { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<TocEntry> Headings { get; set; } = [];
        public string Html { get; set; } = string.Empty;
    }
}
