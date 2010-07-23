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
using System.Xml.Serialization;

namespace MsdnMan
{
    public class LinkItem
    {
        private string _aliasId;
        private string _assetId;
        private string _guidId;
        private string _shortId;

        [XmlElement("contentAlias", Namespace = "urn:mtpg-com:mtps/2004/1/key")]
        public string AliasId
        {
            get { return _aliasId; }
            set { _aliasId = value; }
        }
        [XmlElement("assetId", Namespace = "urn:msdn-com:public-content-syndication")]
        public string AssetId
        {
            get { return _assetId; }
            set { _assetId = value; }
        }
        [XmlElement("contentGuid", Namespace = "urn:mtpg-com:mtps/2004/1/key")]
        public string GuidId
        {
            get { return _guidId; }
            set { _guidId = value; }
        }
        [XmlElement("contentId", Namespace = "urn:mtpg-com:mtps/2004/1/key")]
        public string ShortId
        {
            get { return _shortId; }
            set { _shortId = value; }
        }
    }
}
