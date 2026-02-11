# md2book
Build eBook from several markdown files, add contents and title page

## Command line looks like:
```
md2book --output the-book.pdf --toclevel 2 --titlefile title.md ~/mybook
```

## Algorithm:
1. [ ] Read command line arguments
   - --output, -o *- option, default "book.pdf"*
   - --toclevel *- level of extracted headers, default 2*
   - --title *- text for title page, default the basename of input folder, ignored if --titlefile provided*
   - --titlefile *- markdown file containing title page for the book, optional*
   - input-folder *- argument, default "."*
2. [ ] Find markdown files in the input folder
3. [ ] Sort markdown files alphabeticaly
4. [ ] Extract headers of the desired level from markdown files, 0 mean "no headers"
5. [ ] Create a table of contents
6. [ ] Create the title page file
   - *If* --titlefile exists *then* Read --titlefile
   - *Else* Create title page from --title
7. [ ] Convert Markdown -> HTML -> PDF
8. [ ] Build PDF file
   *1. [ ] Page numbers, title page is zero*
   *2. [ ] Contents show page numbers*
   *3. [ ] Contents has links to articles*


