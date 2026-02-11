# powershell

**Copilot drafts. Not tested or reviewed.**

## Pattern
Module + Function set + Orchestrator script

## Project structure
```
/md2book
   md2book.psm1
      Get-MarkdownFiles
      Get-TitleDocument
      Convert-MarkdownToHtml
      Get-Toc
      New-Pdf
   md2book.ps1
```

### MarkdownPdfTool.psm1
```powershell
function Get-MarkdownFiles {
    param(
        [string]$Path,
        [string]$TitleFile
    )

    $files = Get-ChildItem -Path $Path -Filter *.md

    $title = $null
    $docs = @()

    foreach ($f in $files) {
        $content = Get-Content $f.FullName -Raw

        if ($TitleFile -and $f.Name -eq $TitleFile) {
            $title = [PSCustomObject]@{
                Name = $f.Name
                Content = $content
            }
        }
        else {
            $docs += [PSCustomObject]@{
                Name = $f.Name
                Content = $content
            }
        }
    }

    return [PSCustomObject]@{
        Title = $title
        Documents = $docs | Sort-Object Name
    }
}
```

```powershell
function Convert-MarkdownToHtml {
    param([string]$Markdown)

    # Используем pandoc (должен быть установлен)
    $temp = New-TemporaryFile
    Set-Content $temp $Markdown

    $html = pandoc $temp -f markdown -t html
    Remove-Item $temp

    return $html
}
```

```powershell
function Get-Toc {
    param(
        [array]$Documents,
        [int]$Level
    )

    $toc = @()

    foreach ($doc in $Documents) {
        $lines = $doc.Content -split "`n"

        foreach ($line in $lines) {
            if ($line -match "^#{${Level}}\s+(.*)") {
                $toc += [PSCustomObject]@{
                    Text = $Matches[1]
                    Level = $Level
                }
            }
        }
    }

    return $toc
}
```

```powershell
function New-Pdf {
    param(
        [string]$Output,
        [string]$TitleHtml,
        [array]$Toc,
        [array]$HtmlDocuments
    )

    # Генерируем HTML-файл
    $html = "<html><body>"

    $html += "<h1>$TitleHtml</h1>"
    $html += "<h2>Оглавление</h2><ul>"

    foreach ($entry in $Toc) {
        $html += "<li>$($entry.Text)</li>"
    }

    $html += "</ul>"

    foreach ($doc in $HtmlDocuments) {
        $html += $doc
    }

    $html += "</body></html>"

    $tempHtml = New-TemporaryFile
    Set-Content $tempHtml $html

    # Используем wkhtmltopdf
    wkhtmltopdf $tempHtml $Output

    Remove-Item $tempHtml
}
```

### Orchestrator build.ps1
```powershell
param(
    [string]$Input,
    [string]$Output = "output.pdf",
    [string]$Title = "Document",
    [string]$TitleFile,
    [int]$Level = 2
)

Import-Module "$PSScriptRoot/MarkdownPdfTool.psm1"

$files = Get-MarkdownFiles -Path $Input -TitleFile $TitleFile

$titleHtml = if ($files.Title) {
    Convert-MarkdownToHtml -Markdown $files.Title.Content
} else {
    "<h1>$Title</h1>"
}

$htmlDocs = @()
foreach ($doc in $files.Documents) {
    $htmlDocs += Convert-MarkdownToHtml -Markdown $doc.Content
}

$toc = Get-Toc -Documents $files.Documents -Level $Level

New-Pdf -Output $Output -TitleHtml $titleHtml -Toc $toc -HtmlDocuments $htmlDocs
```
