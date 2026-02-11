# Golang

**Copilot drafts. Not tested or reviewed.**

## Project structure
```
/mdpdf
   main.go
   cmd/
      build.go
   pipeline/
      pipeline.go
      load_files.go
      sort_files.go
      extract_headings.go
      build_toc.go
      convert_markdown.go
      build_pdf.go
   services/
      markdown_parser.go
      html_renderer.go
      pdf_builder.go
   models/
      context.go
      document.go
      toc.go
```

## main.go — entry point
```golang
package main

import (
    "mdpdf/cmd"
)

func main() {
    cmd.Execute()
}
```

## cmd/build.go — CLI command (cobra)
```golang
package cmd

import (
    "github.com/spf13/cobra"
    "mdpdf/models"
    "mdpdf/pipeline"
)

var (
    input     string
    output    string
    title     string
    titleFile string
    level     int
)

var buildCmd = &cobra.Command{
    Use:   "build",
    Short: "Build PDF from markdown files",
    Run: func(cmd *cobra.Command, args []string) {
        ctx := &models.BuildContext{
            InputFolder: input,
            OutputPdf:   output,
            Title:       title,
            TocLevel:    level,
            TitleFile:   titleFile,
        }

        pipeline.NewBuildPipeline().Run(ctx)
    },
}

func init() {
    buildCmd.Flags().StringVar(&input, "input", "", "Input folder")
    buildCmd.Flags().StringVar(&output, "output", "output.pdf", "Output PDF")
    buildCmd.Flags().StringVar(&title, "title", "Document", "Title")
    buildCmd.Flags().StringVar(&titleFile, "title-file", "", "Title markdown file")
    buildCmd.Flags().IntVar(&level, "level", 2, "Heading level for TOC")

    buildCmd.MarkFlagRequired("input")
}

func Execute() {
    root := &cobra.Command{Use: "mdpdf"}
    root.AddCommand(buildCmd)
    root.Execute()
}
```

## models/context.go
```golang
package models

type BuildContext struct {
    InputFolder string
    OutputPdf   string
    Title       string
    TocLevel    int
    TitleFile   string

    TitleDocument *MarkdownDocument
    Documents     []*MarkdownDocument
    Toc           []*TocEntry
}
```

## models/document.go
```golang
package models

type MarkdownDocument struct {
    FilePath string
    Content  string
    Html     string
    Headings []*TocEntry
}
```

## models/toc.go
```golang
package models

type TocEntry struct {
    Text  string
    Level int
    Page  int
    AnchorID int
}
```

## pipeline/pipeline.go
```golang
package pipeline

import "mdpdf/models"

type Step interface {
    Execute(ctx *models.BuildContext)
}

type BuildPipeline struct {
    steps []Step
}

func NewBuildPipeline() *BuildPipeline {
    return &BuildPipeline{
        steps: []Step{
            &LoadFilesStep{},
            &SortFilesStep{},
            &ExtractHeadingsStep{},
            &BuildTocStep{},
            &ConvertMarkdownStep{},
            &BuildPdfStep{},
        },
    }
}

func (p *BuildPipeline) Run(ctx *models.BuildContext) {
    for _, step := range p.steps {
        step.Execute(ctx)
    }
}
```

## pipeline/load_files.go — load files + title file
```golang
package pipeline

import (
    "io/ioutil"
    "mdpdf/models"
    "path/filepath"
)

type LoadFilesStep struct{}

func (s *LoadFilesStep) Execute(ctx *models.BuildContext) {
    files, _ := filepath.Glob(filepath.Join(ctx.InputFolder, "*.md"))

    for _, f := range files {
        contentBytes, _ := ioutil.ReadFile(f)
        content := string(contentBytes)

        doc := &models.MarkdownDocument{
            FilePath: f,
            Content:  content,
        }

        if filepath.Base(f) == ctx.TitleFile {
            ctx.TitleDocument = doc
        } else {
            ctx.Documents = append(ctx.Documents, doc)
        }
    }
}
```

## pipeline/sort_files.go
```golang
package pipeline

import (
    "mdpdf/models"
    "sort"
)

type SortFilesStep struct{}

func (s *SortFilesStep) Execute(ctx *models.BuildContext) {
    sort.Slice(ctx.Documents, func(i, j int) bool {
        return ctx.Documents[i].FilePath < ctx.Documents[j].FilePath
    })
}
```

## services/markdown_parser.go — extract headings
Use github.com/gomarkdown/markdown:
```golang
package services

import (
    "mdpdf/models"
    "regexp"
    "strings"
)

type MarkdownParser struct{}

func (p *MarkdownParser) ExtractHeadings(md string, level int) []*models.TocEntry {
    pattern := "^" + strings.Repeat("#", level) + "\\s+(.*)"
    re := regexp.MustCompile(pattern)

    var result []*models.TocEntry

    for _, line := range strings.Split(md, "\n") {
        if m := re.FindStringSubmatch(line); m != nil {
            result = append(result, &models.TocEntry{
                Text:  m[1],
                Level: level,
            })
        }
    }

    return result
}
```

## pipeline/extract_headings.go
```golang
package pipeline

import (
    "mdpdf/models"
    "mdpdf/services"
)

type ExtractHeadingsStep struct{}

func (s *ExtractHeadingsStep) Execute(ctx *models.BuildContext) {
    parser := &services.MarkdownParser{}

    for _, doc := range ctx.Documents {
        doc.Headings = parser.ExtractHeadings(doc.Content, ctx.TocLevel)
    }
}
```

## pipeline/build_toc.go — create table of contents
```golang
package pipeline

import "mdpdf/models"

type BuildTocStep struct{}

func (s *BuildTocStep) Execute(ctx *models.BuildContext) {
    page := 1
    anchor := 1
    var toc []*models.TocEntry

    for _, doc := range ctx.Documents {
        for _, h := range doc.Headings {
            h.Page = page
            h.AnchorID = anchor
            toc = append(toc, h)
            anchor++
        }
        page++
    }

    ctx.Toc = toc
}
```

## services/html_renderer.go — Markdown -> HTML
Use github.com/gomarkdown/markdown:
```golang
package services

import (
    "github.com/gomarkdown/markdown"
)

type HtmlRenderer struct{}

func (r *HtmlRenderer) Render(md string) string {
    html := markdown.ToHTML([]byte(md), nil, nil)
    return string(html)
}
```

## pipeline/convert_markdown.go
```golang
package pipeline

import (
    "mdpdf/models"
    "mdpdf/services"
)

type ConvertMarkdownStep struct{}

func (s *ConvertMarkdownStep) Execute(ctx *models.BuildContext) {
    renderer := &services.HtmlRenderer{}

    for _, doc := range ctx.Documents {
        doc.Html = renderer.Render(doc.Content)
    }

    if ctx.TitleDocument != nil {
        ctx.TitleDocument.Html = renderer.Render(ctx.TitleDocument.Content)
    }
}
```

## services/pdf_builder.go — HTML -> PDF + clickable links + comments
Go has no internal HTML to PDF renderer, so use wkhtmltopdf instead:
```golang
package services

import (
    "fmt"
    "io/ioutil"
    "mdpdf/models"
    "os"
    "os/exec"
    "strings"
)

// PdfBuilder отвечает за генерацию PDF.
// Мы используем wkhtmltopdf, потому что он поддерживает:
// - HTML
// - CSS
// - кликабельные ссылки
// - якоря (#anchor)
// - оглавление
type PdfBuilder struct{}

// Build собирает HTML и конвертирует его в PDF.
func (b *PdfBuilder) Build(ctx *models.BuildContext) {
    // Начинаем собирать HTML-документ
    var html strings.Builder

    html.WriteString("<html><body style='font-family: sans-serif;'>")

    // --- ТИТУЛЬНЫЙ ЛИСТ ---
    if ctx.TitleDocument != nil {
        html.WriteString(ctx.TitleDocument.Html)
    } else {
        html.WriteString(fmt.Sprintf("<h1>%s</h1>", ctx.Title))
    }

    // --- ОГЛАВЛЕНИЕ ---
    html.WriteString("<h2>Оглавление</h2><ul>")

    for _, entry := range ctx.Toc {
        // Каждому заголовку присваиваем anchor-id
        anchor := fmt.Sprintf("section-%d", entry.AnchorID)

        // Делаем кликабельную ссылку
        html.WriteString(fmt.Sprintf(
            "<li style='margin-left:%dpx'><a href='#%s'>%s</a></li>",
            (entry.Level-1)*20, // отступы по уровню
            anchor,
            entry.Text,
        ))
    }

    html.WriteString("</ul>")

    // --- КОНТЕНТ ---
    for _, doc := range ctx.Documents {
        // Вставляем HTML документа
        // Но перед этим добавляем якоря для заголовков
        processed := insertAnchors(doc.Html, doc.Headings)
        html.WriteString(processed)
    }

    html.WriteString("</body></html>")

    // --- СОХРАНЯЕМ ВРЕМЕННЫЙ HTML ---
    tmp, _ := ioutil.TempFile("", "*.html")
    tmp.Write([]byte(html.String()))
    tmp.Close()

    // --- КОНВЕРТИРУЕМ В PDF ---
    exec.Command("wkhtmltopdf", tmp.Name(), ctx.OutputPdf).Run()

    os.Remove(tmp.Name())
}

// insertAnchors вставляет <a id="..."></a> перед заголовками.
// Это позволяет wkhtmltopdf делать кликабельные ссылки.
func insertAnchors(html string, headings []*models.TocEntry) string {
    for _, h := range headings {
        anchor := fmt.Sprintf("section-%d", h.AnchorID)

        // Ищем заголовок в HTML и добавляем якорь перед ним
        html = strings.Replace(html,
            fmt.Sprintf("<h%d>", h.Level),
            fmt.Sprintf("<a id='%s'></a><h%d>", anchor, h.Level),
            1,
        )
    }
    return html
}
```

## pipeline/build_pdf.go
```golang
package pipeline

import (
    "mdpdf/models"
    "mdpdf/services"
)

type BuildPdfStep struct{}

func (s *BuildPdfStep) Execute(ctx *models.BuildContext) {
    builder := &services.PdfBuilder{}
    builder.Build(ctx)
}
```
