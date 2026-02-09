using System;
using System.Collections.Generic;
using System.Text;

namespace md2book.Models
{
    public class BuildContext
    {
        public string InputFolder { get; set; }
        public string OutputFile { get; set; }
        public string Title { get; set; }
        public int TocLevel { get; set; }

        public List<MarkdownDocument> Documents { get; set; } = [];
        public List<TocEntry> Toc { get; set; } = [];
    }
}
