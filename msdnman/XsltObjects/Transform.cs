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
using System.Reflection;
using System.Xml;
using System.Xml.XPath;


namespace MsdnMan.XsltObjects
{
    public class Transform
    {
        private XPathNavigator _context;
        private IXmlNamespaceResolver _namespaceResolver;
        private readonly XPathMethodInfoPairCollection _xpathMethodInfos = new XPathMethodInfoPairCollection();

        public Transform()
        {
        }

        protected IXmlNamespaceResolver NamespaceResolver
        {
            get { return _namespaceResolver; }
            set { _namespaceResolver = value; }
        }

        public void Execute(XPathNavigator context)
        {
            InitializeMethodMap(context);

            _context = context;

            ApplyTemplates("/");

        }

        protected void ApplyTemplates()
        {
            ApplyTemplates("*");
        }

        protected void ApplyTemplates(string xpath)
        {
            XPathNodeIterator nodes = _context.Select(Compile(_context, xpath));

            while (nodes.MoveNext())
            {
                XPathNavigator oldContext = _context;
                _context = nodes.Current;
                foreach (XPathMethodInfoPair pair in _xpathMethodInfos)
                {
                    if (_context.Matches(pair.Expression))
                    {
                        pair.Method.Invoke(this, new object[] { _context });
                        break;
                    }
                }
                _context = oldContext;
            }
        }

        [Match("*|/")]
        protected virtual void DefaultRule(XPathNavigator context)
        {
            ApplyTemplates("*");
        }

        private void InitializeMethodMap(XPathNavigator context)
        {
            foreach (MethodInfo method in GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance |
                BindingFlags.FlattenHierarchy | BindingFlags.NonPublic))
            {
                if (method.IsDefined(typeof(MatchAttribute), true))
                {
                    foreach (MatchAttribute match in method.GetCustomAttributes(typeof(MatchAttribute), false))
                    {
                        XPathExpression expression = Compile(context, match.XPath);
                        _xpathMethodInfos.Add(new XPathMethodInfoPair(expression, method));
                    }
                }
            }
        }

        private XPathExpression Compile(XPathNavigator context, string xpath)
        {
            XPathExpression expression = context.Compile(xpath);
            if (NamespaceResolver != null)
            {
                expression.SetContext(NamespaceResolver);
            }
            return expression;
        }
    }
}
