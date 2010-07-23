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
using System.IO;

namespace MsdnMan
{
    internal class CorrectingTextWriter : TextWriter
    {
        private CorrectionBehavior _correctionBehavior;
        private TextWriter _inner;
        private bool[] _lineHistory = new bool[3];

        internal CorrectingTextWriter(TextWriter inner)
        {
            _inner = inner;
        }

        public CorrectionBehavior CorrectionBehavior
        {
            get { return _correctionBehavior; }
            set { _correctionBehavior = value; }
        }
        public override System.Text.Encoding Encoding
        {
            get
            {
                return _inner.Encoding;
            }
        }
        public override IFormatProvider FormatProvider
        {
            get
            {
                return _inner.FormatProvider;
            }
        }

        public override void Close()
        {
            _inner.Close();
        }
        public override void Flush()
        {
            _inner.Flush();
        }
        public override string NewLine
        {
            get
            {
                return _inner.NewLine;
            }
            set
            {
                _inner.NewLine = value;
            }
        }
        public override void Write(bool value)
        {
            AddNonBlankLineContent();
            _inner.Write(value);
        }
        public override void Write(char value)
        {
            AddLineContent(value.ToString());
            _inner.Write(value);
        }
        public override void Write(char[] buffer)
        {
            AddNonBlankLineContent();
            _inner.Write(buffer);
        }
        public override void Write(char[] buffer, int index, int count)
        {
            AddNonBlankLineContent();
            _inner.Write(buffer, index, count);
        }
        public override void Write(decimal value)
        {
            AddNonBlankLineContent();
            _inner.Write(value);
        }
        public override void Write(double value)
        {
            AddNonBlankLineContent();
            _inner.Write(value);
        }
        public override void Write(float value)
        {
            AddNonBlankLineContent();
            _inner.Write(value);
        }
        public override void Write(int value)
        {
            AddNonBlankLineContent();
            _inner.Write(value);
        }
        public override void Write(long value)
        {
            AddNonBlankLineContent();
            _inner.Write(value);
        }
        public override void Write(object value)
        {
            AddLineContent(value.ToString());
            _inner.Write(value);
        }
        public override void Write(string format, object arg0)
        {
            AddLineContent(string.Format(format, arg0));
            _inner.Write(format, arg0);
        }
        public override void Write(string format, object arg0, object arg1)
        {
            AddLineContent(string.Format(format, arg0, arg1));
            _inner.Write(format, arg0, arg1);
        }
        public override void Write(string format, object arg0, object arg1, object arg2)
        {
            AddLineContent(string.Format(format, arg0, arg1, arg2));
            _inner.Write(format, arg0, arg1, arg2);
        }
        public override void Write(string format, params object[] arg)
        {
            AddLineContent(string.Format(format, arg));
            _inner.Write(format, arg);
        }
        public override void Write(string value)
        {
            AddLineContent(value);
            _inner.Write(value);
        }
        public override void Write(uint value)
        {
            AddNonBlankLineContent();
            _inner.Write(value);
        }
        public override void Write(ulong value)
        {
            AddNonBlankLineContent();
            _inner.Write(value);
        }
        public override void WriteLine()
        {
            AddNewline();

            if (!IsExtraNewline() || CorrectionBehavior == CorrectionBehavior.NoCorrection)
            {
                _inner.WriteLine();
            }
        }
        public override void WriteLine(bool value)
        {
            AddNonBlankLineContent();
            AddNewline();
            _inner.WriteLine(value);
        }
        public override void WriteLine(char value)
        {
            AddLineContent(value.ToString());
            AddNewline();
            _inner.WriteLine(value);
        }
        public override void WriteLine(char[] buffer)
        {
            AddNonBlankLineContent();
            AddNewline();
            _inner.WriteLine(buffer);
        }
        public override void WriteLine(char[] buffer, int index, int count)
        {
            AddNonBlankLineContent();
            AddNewline();
            _inner.WriteLine(buffer, index, count);
        }
        public override void WriteLine(decimal value)
        {
            AddNonBlankLineContent();
            AddNewline();
            _inner.WriteLine(value);
        }
        public override void WriteLine(double value)
        {
            AddNonBlankLineContent();
            AddNewline();
            _inner.WriteLine(value);
        }
        public override void WriteLine(float value)
        {
            AddNonBlankLineContent();
            AddNewline();
            _inner.WriteLine(value);
        }
        public override void WriteLine(int value)
        {
            AddNonBlankLineContent();
            AddNewline();
            _inner.WriteLine(value);
        }
        public override void WriteLine(long value)
        {
            AddNonBlankLineContent();
            AddNewline();
            _inner.WriteLine(value);
        }
        public override void WriteLine(object value)
        {
            AddLineContent(value.ToString());
            AddNewline();
            _inner.WriteLine(value);
        }
        public override void WriteLine(string format, object arg0)
        {
            AddLineContent(string.Format(format, arg0));
            AddNewline();
            _inner.WriteLine(format, arg0);
        }
        public override void WriteLine(string format, object arg0, object arg1)
        {
            AddLineContent(string.Format(format, arg0, arg1));
            AddNewline();
            _inner.WriteLine(format, arg0, arg1);
        }
        public override void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            AddLineContent(string.Format(format, arg0, arg1, arg2));
            AddNewline();
            _inner.WriteLine(format, arg0, arg1, arg2);
        }
        public override void WriteLine(string format, params object[] arg)
        {
            AddLineContent(string.Format(format, arg));
            AddNewline();
            _inner.WriteLine(format, arg);
        }
        public override void WriteLine(string value)
        {
            AddLineContent(value);
            AddNewline();
            _inner.WriteLine(value);
        }
        public override void WriteLine(uint value)
        {
            AddNonBlankLineContent();
            AddNewline();
            _inner.WriteLine(value);
        }
        public override void WriteLine(ulong value)
        {
            AddNonBlankLineContent();
            AddNewline();
            _inner.WriteLine(value);
        }

        private void AddLineContent(string content)
        {
            if (_lineHistory[0])
            {
                return;
            }

            if (content.Trim().Length > 0)
            {
                _lineHistory[0] = true;
            }
        }
        private void AddNewline()
        {
            for (int i = _lineHistory.Length - 1; i > 0; i--)
            {
                _lineHistory[i] = _lineHistory[i - 1];
            }
            _lineHistory[0] = false;
        }
        private void AddNonBlankLineContent()
        {
            _lineHistory[0] = true;
        }
        private bool IsExtraNewline()
        {
            return (!_lineHistory[0] && !_lineHistory[1] && !_lineHistory[2]);
        }
    }
}
