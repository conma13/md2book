# md2book
Build eBook from several Markdown files, add contents and title page

## Command line looks like:
```
md2book --output the-book.pdf --toclevel 2 --titlefile title.md ~/mybook
```

## Algorithm:
1. [ ] Read command line options
   - --input, -i *- input-directory, default: "."*
   - --output, -o *- output file name, default: "book"*
   - --title *- text for the title page, ignored if --titlefile is provided, default: basename of the input directory*
   - --titlefile *- path to a Markdown file containing the book's title page, optional*
   - --toclevel *- Markdown header level used to generate the TOC, default: 2, disable: 0*
2. [ ] Find Markdown files in the input folder
3. [ ] Sort Markdown files alphabeticaly
4. [ ] Extract headers of the desired level from Markdown files
5. [ ] Create a table of contents
6. [ ] Create the title page file
   - *If* --titlefile exists *then* Read --titlefile
   - *Else* Create title page from --title
7. [ ] Convert Markdown -> HTML -> PDF
8. [ ] Build PDF file
   *1. [ ] Page numbers, title page is zero*
   *2. [ ] Contents show page numbers*
   *3. [ ] Contents has links to articles*


