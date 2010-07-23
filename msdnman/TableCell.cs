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
using System.Text;

namespace MsdnMan
{
    internal class TableCell
    {
        private string _contents;
        private bool _isHeader;
        private string[] _lines;
        private string _width;

        public string Contents
        {
            get { return _contents; }
            set
            {
                _contents = value.Replace("\r\n", "\n");
                _lines = value.Split(new string[] { "\n" }, StringSplitOptions.None);
            }
        }

        public bool IsHeader
        {
            get { return _isHeader; }
            set { _isHeader = value; }
        }

        public string[] Lines
        {
            get { return _lines; }
        }

        public int MaxLineWidth
        {
            get
            {
                int max = 0;
                foreach (string line in Lines)
                {
                    max = Math.Max(max, line.Length);
                }

                return max;
            }
        }

        public string Width
        {
            get { return _width; }
            set { _width = value; }
        }

        public int GetLineCount(int width)
        {
            int start = 0;
            int lineCount = 0;
            while (start < Contents.Length)
            {
                int nextNewline = SafeIndexOf(Contents, "\n", start, width);

                if (nextNewline == -1)
                {
                    start += width;
                }
                else
                {
                    start = nextNewline + 2;
                }
                lineCount++;
            }

            return lineCount;
        }

        public string GetSubset(int line, int width)
        {
            int start = 0;
            while (start < Contents.Length && line > 0)
            {
                int nextNewline = SafeIndexOf(Contents, "\n", start, width);

                if (nextNewline == -1)
                {
                    start += width;
                }
                else
                {
                    start = nextNewline + 2;
                }
                line--;
            }

            if (start >= Contents.Length)
            {
                return new string(' ', width);
            }

            int nextNewLine = SafeIndexOf(Contents, "\n", start, width);

            int length;
            if (nextNewLine == -1)
            {
                length = Math.Min(Contents.Length - start, width);
            }
            else
            {
                length = nextNewLine - start;
            }

            string subset = Contents.Substring(start, length);

            return subset.PadRight(width, ' ');

        }

        private static int SafeIndexOf(string s, string value, int start, int count)
        {
            if (start + count > s.Length)
            {
                count = s.Length - start;
            }

            return s.IndexOf(value, start, count);
        }


    }
}
