using System;
using System.Collections.Generic;
using System.Text;

namespace md2book.Models
{
    public class BuildContext
    {
        public string InputFolder { get; set; } = string.Empty;
        public string OutputFile { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string TitleFile { get; set; } = string.Empty;
        public uint TocLevel { get; set; }

        public List<MarkdownDocument> Documents { get; set; } = [];
        public List<TocEntry> Toc { get; set; } = [];
    }
}
