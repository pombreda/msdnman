#region License Statement
/*
Copyright (c) 2006 Wangdera Corporation

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using MsdnMan.XsltObjects;

namespace MsdnMan
{
    public class DefaultTransform : Transform
    {
        private Table _currentTable;
        private TableRow _currentTableRow;
        private string _language;
        private LinkFormat _linkFormat;
        private LinkMap _linkMap;
        //private bool _hadText = true; 
        private CorrectingTextWriter _output;
        private int _width;

        internal DefaultTransform(TextWriter output, XmlNameTable nameTable, int width, LinkMap linkMap,
            string language, LinkFormat linkFormat)
        {
            _output = new CorrectingTextWriter(output);
            _width = width;
            _linkMap = linkMap;
            _language = language;
            _linkFormat = linkFormat;

            XmlNamespaceManager nsManager = new XmlNamespaceManager(nameTable);
            nsManager.AddNamespace("xhtml", "http://www.w3.org/1999/xhtml");
            nsManager.AddNamespace("mtps", "http://msdn2.microsoft.com/mtps");
            this.NamespaceResolver = nsManager;
        }


        [Match("xhtml:a[text()]")]
        public void Anchor(XPathNavigator context)
        {
            string href = (string)context.Evaluate("string(@href)");

            href = MapLink(href);

            WriteLinkPrefix();
            ApplyTemplates("*|text()");
            WriteLinkSuffix(href);
        }

        [Match("xhtml:b")]
        public void Bold(XPathNavigator context)
        {
            _output.Write(" ");
            ApplyTemplates("*|text()");
            _output.Write(" ");
        }

        [Match("xhtml:br")]
        public void Break(XPathNavigator context)
        {
            _output.WriteLine();
        }

        [Match("mtps:CodeSnippet")]
        public void CodeSnippet(XPathNavigator context)
        {
            string snippetLanguage = (string)context.Evaluate("string(@Language)");

            if (_language == null || snippetLanguage == "None" || snippetLanguage.StartsWith(_language))
            {
                WriteHeader(context, "@DisplayLanguage", '-');

                CorrectionBehavior oldBehavior = _output.CorrectionBehavior;
                _output.CorrectionBehavior = CorrectionBehavior.NoCorrection;
                _output.WriteLine(context.Value);
                _output.CorrectionBehavior = oldBehavior;
            }
        }

        [Match("mtps:CollapsibleArea")]
        public void CollapsibleArea(XPathNavigator context)
        {
            WriteHeader(context, "@Title", '-');
            ApplyTemplates("*|text()");
        }

        [Match("xhtml:div[not(@class='title')]|xhtml:blockquote")]
        public void DivsEtc(XPathNavigator context)
        {
            _output.WriteLine();
            _output.WriteLine();
            _output.WriteLine();
            ApplyTemplates("*|text()");
            _output.WriteLine();
            _output.WriteLine();
        }

        [Match("xhtml:dl")]
        public void DefinitionList(XPathNavigator context)
        {
            _output.WriteLine();
            _output.WriteLine();
            ApplyTemplates("*|text()");
            _output.WriteLine();
            _output.WriteLine();
        }

        [Match("xhtml:dt")]
        public void DefinitionTerm(XPathNavigator context)
        {
            ApplyTemplates("*|text()");
            _output.WriteLine();
        }

        [Match("xhtml:h1|xhtml:h2|xhtml:h3|xhtml:h4|xhtml:h5|xhtml:div[@class='title']")]
        public void Headings(XPathNavigator context)
        {
            _output.WriteLine();
            _output.WriteLine();
            _output.WriteLine();

            CorrectingTextWriter oldOutput = _output;
            StringWriter newOutput = new StringWriter();
            _output = new CorrectingTextWriter(newOutput);

            ApplyTemplates("*|text()");

            _output = oldOutput;

            string contents = newOutput.ToString();
            string divider = new string('-', contents.Length);

            _output.WriteLine(contents);
            _output.WriteLine(divider);

            _output.WriteLine();
            _output.WriteLine();
        }

        [Match("mtps:InstrumentedLink")]
        public void InstrumentedLink(XPathNavigator context)
        {
            string href = (string)context.Evaluate("string(@NavigateUrl)");

            href = MapLink(href);

            WriteLinkPrefix();
            ApplyTemplates("*|text()");
            WriteLinkSuffix(href);

        }

        [Match("xhtml:i")]
        public void Italic(XPathNavigator context)
        {
            _output.Write(" ");
            ApplyTemplates("*|text()");
            _output.Write(" ");
        }

        [Match("mtps:KTableControl")]
        public void KTableControl(XPathNavigator context)
        {
            // Ignore it - not sure what it's supposed to do. 
        }

        [Match("xhtml:li")]
        public void ListItem(XPathNavigator context)
        {
            _output.Write("  * ");
            ApplyTemplates("*|text()");
            _output.WriteLine();
        }

        [Match("mtps:MemberLink")]
        public void MemberLink(XPathNavigator context)
        {
            string target = (string)context.Evaluate("string(@Target)");
            string text = (string)context.Evaluate("string(@Text)");

            target = MapLink(target);

            WriteLinkPrefix();
            _output.Write(text);
            WriteLinkSuffix(target);
        }

        [Match("xhtml:p")]
        public void Paragraph(XPathNavigator context)
        {
            ApplyTemplates("*|text()");

            _output.WriteLine();
            _output.WriteLine();

        }

        [Match("xhtml:pre")]
        public void Preformatted(XPathNavigator context)
        {
            CorrectionBehavior oldBehavior = _output.CorrectionBehavior;
            _output.CorrectionBehavior = CorrectionBehavior.NoCorrection;
            _output.WriteLine(context.Value);
            _output.CorrectionBehavior = oldBehavior;
        }

        [Match("xhtml:span")]
        public void Span(XPathNavigator context)
        {
            _output.Write(" ");
            ApplyTemplates("*|text()");
            _output.Write(" ");
        }

        [Match("xhtml:table")]
        public void Table(XPathNavigator context)
        {
            Table oldTable = _currentTable;
            _currentTable = new Table();
            ApplyTemplates("xhtml:tr");

            int[] maxColumnWidths = GetMaxColumnWidths(_currentTable);

            int[] columnWidths = new int[maxColumnWidths.Length];

            int numColumns = columnWidths.Length;
            int remainingWidth = _width - 2;
            for (int i = 0; i < numColumns; i++)
            {
                int actualWidth = maxColumnWidths[i] + 1;
                int maxAllowedWidth = (remainingWidth / (numColumns - i)) - 1;
                int columnWidth = Math.Min(actualWidth, maxAllowedWidth);
                columnWidths[i] = columnWidth;
                remainingWidth = remainingWidth - columnWidths[i] - 1;
            }

            WriteRowDivider(columnWidths, numColumns, '-', '-');

            for (int rowNumber = 0; rowNumber < _currentTable.Rows.Count; ++rowNumber)
            {
                TableRow row = _currentTable.Rows[rowNumber];
                int maxCellLength = GetMaxCellLength(row);
                int lines = GetMaxLineCount(row, columnWidths);

                for (int line = 0; line < lines; line++)
                {
                    _output.Write("|");
                    for (int column = 0; column < numColumns; column++)
                    {
                        string segment = GetCellSubset(row, line, column, columnWidths[column]);
                        _output.Write(segment);
                        _output.Write("|");
                    }
                    _output.WriteLine();
                }

                char rowDivider = '-';

                if (row.Cells[0].IsHeader)
                {
                    rowDivider = '=';
                }

                char columnDivider = '|';

                if (rowNumber == _currentTable.Rows.Count - 1)
                {
                    columnDivider = rowDivider;
                }

                WriteRowDivider(columnWidths, numColumns, rowDivider, columnDivider);

            }

            _currentTable = oldTable;
        }

        [Match("xhtml:th|xhtml:td")]
        public void TableCell(XPathNavigator context)
        {
            CorrectingTextWriter oldOutput = _output;
            StringWriter newOutput = new StringWriter();
            _output = new CorrectingTextWriter(newOutput);

            ApplyTemplates("*|text()");

            TableCell cell = new TableCell();
            newOutput.Flush();
            cell.Contents = newOutput.ToString();
            cell.Width = (string)context.Evaluate("string(@width)");
            cell.IsHeader = (bool)context.Evaluate("boolean(local-name(.) = 'th')", NamespaceResolver);

            _currentTableRow.Cells.Add(cell);

            _output = oldOutput;
        }

        [Match("xhtml:tr")]
        public void TableRow(XPathNavigator context)
        {
            _currentTableRow = new TableRow();
            ApplyTemplates("xhtml:th|xhtml:td");
            _currentTable.Rows.Add(_currentTableRow);
        }

        [Match("text()")]
        public void Text(XPathNavigator context)
        {
            // Condense whitespace to a single space. 
            string content = context.Value.Trim();

            bool lastWasWhitespace = false;
            foreach (char c in content)
            {
                if (Char.IsWhiteSpace(c))
                {
                    if (!lastWasWhitespace)
                    {
                        _output.Write(" ");
                    }
                    lastWasWhitespace = true;
                }
                else
                {
                    _output.Write(c);
                    lastWasWhitespace = false;
                }
            }

        }

        [Match("xhtml:ul")]
        public void UnorderedList(XPathNavigator context)
        {
            ApplyTemplates("xhtml:li");
            _output.WriteLine();
            _output.WriteLine();
        }

        [Match("mtps:*")]
        public void MtpsCatchAll(XPathNavigator context)
        {
            _output.WriteLine();
            _output.WriteLine();
            _output.WriteLine();

            _output.WriteLine("****Unrecognized mtps element {0}****", context.Name);

            _output.WriteLine();
            _output.WriteLine();
            _output.WriteLine();
        }

        private string GetCellSubset(TableRow row, int line, int column, int width)
        {
            if (row.Cells.Count < column)
            {
                return new string(' ', width);
            }

            TableCell cell = row.Cells[column];

            return cell.GetSubset(line, width);

        }
        private int GetMaxCellLength(TableRow row)
        {
            int maxCellLength = 0;

            foreach (TableCell cell in row.Cells)
            {
                if (cell.MaxLineWidth > maxCellLength)
                {
                    maxCellLength = cell.MaxLineWidth;
                }
            }

            return maxCellLength;
        }
        private int[] GetMaxColumnWidths(Table table)
        {
            List<int> columnWidths = new List<int>();

            foreach (TableRow row in table.Rows)
            {
                int columnNum = 0;
                foreach (TableCell cell in row.Cells)
                {
                    if (columnNum >= columnWidths.Count)
                    {
                        columnWidths.Add(0);
                    }

                    if (cell.MaxLineWidth > columnWidths[columnNum])
                    {
                        columnWidths[columnNum] = cell.MaxLineWidth;
                    }

                    ++columnNum;
                }
            }

            return columnWidths.ToArray();
        }
        private int GetMaxLineCount(TableRow row, int[] columnWidths)
        {
            int maxLineCount = 1;
            for (int i = 0; i < row.Cells.Count; ++i)
            {
                int lines = row.Cells[i].GetLineCount(columnWidths[i]);

                maxLineCount = Math.Max(lines, maxLineCount);
            }

            return maxLineCount;
        }
        private bool IsNullOrEmpty(string s)
        {
            if (s == null)
            {
                return true;
            }

            if (s.Trim().Length == 0)
            {
                return true;
            }

            return false;
        }
        private bool IsWhitespaceOrNewline(char c)
        {
            if (char.IsWhiteSpace(c))
            {
                return true;
            }

            if (c == '\r')
            {
                return true;
            }

            if (c == '\n')
            {
                return true;
            }

            return false;
        }
        private string MapLink(string href)
        {
            string lowerHref = href.ToLower();
            if (_linkMap.Contains(lowerHref))
            {
                LinkItem info = _linkMap[lowerHref];

                if (!IsNullOrEmpty(info.AliasId) && _linkFormat == LinkFormat.Full)
                {
                    return info.AliasId + " (" + info.ShortId + ")";
                }
                else
                {
                    return info.ShortId;
                }
            }
            else
            {
                return href;
            }
        }
        private string TrimTrailingWhitespace(string input)
        {
            int lastNonWhitespace = input.Length;
            for (int i = input.Length - 1; i >= 0; i--, lastNonWhitespace--)
            {
                if (!IsWhitespaceOrNewline(input[i]))
                {
                    break;
                }
            }

            return input.Substring(0, lastNonWhitespace);
        }
        private void WriteHeader(XPathNavigator context, string xpath, char divider)
        {
            _output.WriteLine();
            _output.WriteLine();
            _output.WriteLine();

            xpath = string.Format("string({0})", xpath);
            string heading = (string)context.Evaluate(xpath);

            _output.WriteLine(heading);
            _output.WriteLine(new string(divider, heading.Length));

            _output.WriteLine();
            _output.WriteLine();
            _output.WriteLine();
        }
        private void WriteLinkPrefix()
        {
            if (_linkFormat != LinkFormat.Bare)
            {
                _output.Write(" \"");
            }
        }
        private void WriteLinkSuffix(string href)
        {
            if (_linkFormat != LinkFormat.Bare)
            {
                _output.Write("\":[{0}] ", href);
            }
        }
        private void WriteRowDivider(int[] columnWidths, int numColumns, char rowDivider, char columnDivider)
        {
            _output.Write(columnDivider);
            for (int column = 0; column < numColumns; column++)
            {
                _output.Write(new string(rowDivider, columnWidths[column]));
                _output.Write(columnDivider);
            }
            _output.WriteLine();
        }


    }
}
