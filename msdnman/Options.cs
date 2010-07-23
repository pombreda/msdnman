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
    internal class Options
    {
        private bool _debug;
        private bool _failsafe;
        private bool _help;
        private string _identifier;
        private bool _isKeywordSearch;
        private string _language = Environment.GetEnvironmentVariable("MSDNMAN_LANG");
        private LinkFormat _linkFormat = LinkFormat.Full;
        private string _locale;
        private bool _nologo;
        private string _root = "ms310241";
        private int _searchResultsLimit = 10;
        private string _url;
        private string _version;
        private bool _xml;

        internal bool Debug
        {
            get { return _debug; }
            set { _debug = value; }
        }
        internal bool Failsafe
        {
            get { return _failsafe; }
            set { _failsafe = value; }
        }
        internal bool Help
        {
            get { return _help; }
            set { _help = value; }
        }
        internal string Identifier
        {
            get { return _identifier; }
            set { _identifier = value; }
        }
        internal bool IsKeywordSearch
        {
            get { return _isKeywordSearch; }
            set { _isKeywordSearch = value; }
        }
        internal string Language
        {
            get { return _language; }
            set { _language = value; }
        }
        internal LinkFormat LinkFormat
        {
            get { return _linkFormat; }
            set { _linkFormat = value; }
        }
        internal string Locale
        {
            get
            {
                if (_locale != null)
                {
                    return _locale;
                }

                _locale = Environment.GetEnvironmentVariable("MSDNMAN_LOCALE");

                if (_locale == null)
                {
                    _locale = System.Globalization.CultureInfo.CurrentUICulture.ToString();
                }

                return _locale;
            }
            set { _locale = value; }
        }
        internal bool NoLogo
        {
            get { return _nologo; }
            set { _nologo = value; }
        }
        internal string Root
        {
            get { return _root; }
            set { _root = value; }
        }
        internal int SearchResultsLimit
        {
            get { return _searchResultsLimit; }
            set { _searchResultsLimit = value; }
        }
        internal string Url
        {
            get { return _url; }
            set { _url = value; }
        }
        internal string Version
        {
            get { return _version; }
            set { _version = value; }
        }
        internal bool Xml
        {
            get { return _xml; }
            set { _xml = value; }
        }

    }
}
