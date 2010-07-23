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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.Services.Protocols;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;

using msdnman.com.microsoft.msdn.services;
using msdnman.com.msn.search.soap;

namespace MsdnMan
{
    internal class Program
    {
        private enum Status : int
        {
            Success = 0,
            UsageError = 1,
            Exception = 2,
        }

        #region Constants
        private const string c_banner =
@"MSDN Manual Page Generator Tool (msdnman@wangdera.com)";
        private const string c_msnSearchAppId = @"B60652558A13B3332B28B565DEE0ACDF66740C53";
        private const string c_terseUsage =
@"msdnman
  Prints this (terse) help message.

msdnman -help 
msdnman -? 
  Print detailed help message.

msdnman IDENTIFIER
  Retrieves contents of topic IDENTIFIER.

msdnman -keyword KEYWORD
msdnman -k KEYWORD
  Searches for topics that are appropriate to KEYWORD.";
        private const string c_usage =
@"Usage: 
======
msdnman -help
msdnman -?
  Prints this message.

msdnman [-nologo] [-debug] [-url URL] [-loc LOCALE] [-ver VERSION] 
[-language LANGUAGE] [-nolink | -shortlink] [-xml] IDENTIFIER
  Retrieves the contents of topic IDENTIFIER. 

msdnman [-nologo] [-debug] [-url URL] -k KEYWORD [-r LIMIT]
msdnman [-nologo] [-debug] [-url URL] -keyword KEYWORD [-results LIMIT]
  Searches for topics that are appropriate to KEYWORD (experimental). 

Definitions
-----------
IDENTIFIER : The name or ID of a topic to retrieve. E.g. System.Xml.XmlReader 
             or ms123401. 
LOCALE     : The desired locale of the requested topic. Defaults to the current
             UI locale. E.g. de-de for German. 
VERSION    : The desired version of the requested topic. Defaults to the latest
             version. E.g. VS.80.
URL        : The URL of the MTPS Content Service. Defaults to 
       http://services.msdn.microsoft.com/ContentServices/ContentService.asmx 
             which should be correct for most situations.
KEYWORD    : A keyword to search for. E.g. Xml. 
LANGUAGE   : Specifies the programming language to filter for. The default is
             to show all programming languages. Legal values are: 
             C#        = C#, Csharp 
             C++       = C++, Cpp 
             VB        = VB, VisualBasic
             JScript   = JS, JScript
             J#        = J#, JSharp
LIMIT      : An integer that limits the number of search results returned. 
             Defaults to 10. 

Switches 
--------
-help      : Prints this message.
-?         : Equivalent to -help.
-nologo    : Suppress printing of the title banner. 
-loc       : Specifies the LOCALE (see below). Overrides the MSDNMAN_LOCALE
             environment variable. Defaults to the current UI culture. 
-ver       : Specifies the VERSION (see below). Defaults to the latest version.
-debug     : Specify this switch to break into a debugger on program startup.
-url       : Specifies the URL to use for the MTPS Content Service. For testing
             purposes; users will not generally specify this switch.
-xml       : Emits raw XML for the requested item rather than the transformed 
             view. For testing purposes; most users will not specify this 
             switch. 
-keyword   : Performs a keyword search. 
-k         : Equivalent to -keyword.
-language  : Filters out code samples, declarations, etc. except for the 
             specified programming language.
-nolink    : Sets the link format to 'bare'. See 'Links' below.
-shortlink : Sets the link format to 'short'. See 'Links' below.
-results   : Specifies that no more than LIMIT results should be returned from 
             a keyword search. Defaults to 10. 
-r         : Equivalent to -results.

Envioronment Variables
======================
The following environment variables affect the behavior of msdnman: 

MSDNMAN_PAGER : If set, a path to a program to which all output will be piped. 
                This could be any program, but the intent is to provide 
                page-at-a-time reading, so a paging program would be typical. 
                E.g. more.com. 
MSDNMAN_LANG  : Provides a default programming language for the -language 
                option, so that invocations of msdnman without a -language
                switch will still filter. If the -language switch is provided,
                it takes precedence over this environment variable.
MSDNMAN_LOCALE: Provides a default LOCALE (see above). If the -loc switch is 
                specified, it takes precedence over this environment. If 
                neither the -loc switch nor this environment variable is
                specified, the current UI culture is used. 

Output
======

Content
-------
When content is requested the program will output one or more of the following: 

1. The requested content formatted for the console. 
2. Identity information (identifiers, version, locale) for the located topic.
3. A list of alternate versions and locales also availble for the specified 
   identifier.
4. A list of the documents included in the requested content item. For 
   informational purposes only - msdnman currently retrieves and renders only 
   the XHTML content.
5. A list of navigation path information for the specified topic. 
6. An error message.

Navigation Path Information
---------------------------
When navigation path information is avilable, the program will output a listing 
of entries providing a navigation trail to the requested topic in the following 
format: 

""TITLE1"":[IDENTIFIER1] -> ""TITLE2"":[IDENTIFIER2] -> etc.

Where IDENTIFIERn is the ID of the content item for the nth TOC entry, and
TITLEn is the title of the nth item. The IDENTIFIERs returned can be passed
to subsequent invocations of msdnman.   

Keyword Search Results
----------------------
Keyword searches are currently experimental. 

When keyword search results are returned, the results are formatted as follows:

RANK1. TITLE1
URL1
ABSTRACT1

RANK2. TITLE2
URL2
ABSTRACT2

etc.

RANKn is the rank within the search results. TITLEn is the title of the 
matching document. URLn is the URL at which the document can be found. 
ABSTRACTn is a brief summary of the document. 

Note that you may or may not be able to pass the URL to msdnman as an 
IDENTIFIER - not all results returned are for content stored in MTPS.

Links
-----
Links in the output appear in one of three formts: full (the default), short, 
and bare. Full links are shown as 

""Link text"":[LinkTarget1 (LinkTarget2)]

where LinkTargetn is an IDENTIFIER appropriate to pass to another invocation of 
msdnman. LinkTarget1 may not appear. When present, it is an equivalent to 
LinkTarget2 that may be easier to type.

When short links are shown, they appear in the format

""Link text"":[LinkTarget]

where LinkTarget will be in the same format as LinkTarget2 in the full link 
format. 

When links are set to the 'bare' format, link text will appear without any 
decoration or indication that it is a link. 

";
        #endregion

        private static Options s_options;
        private static TextWriter s_output;

        private static Options Options
        {
            get { return s_options; }
        }

        static int Main(string[] args)
        {
            Process pagerProcess = null;
            try
            {

                string pager = Environment.GetEnvironmentVariable("MSDNMAN_PAGER");

                if (pager == null)
                {
                    s_output = Console.Out;
                }
                else
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo(pager);
                    startInfo.RedirectStandardInput = true;
                    startInfo.UseShellExecute = false;
                    try
                    {
                        pagerProcess = Process.Start(startInfo);
                    }
                    catch (Exception e)
                    {
                        throw new MsdnManException("Unable to start specified pager '" + pager + "'", e);
                    }

                    s_output = pagerProcess.StandardInput;
                }
                try
                {
                    ParseOptions(args);
                }
                catch (ParseOptionsException poe)
                {
                    s_output.WriteLine("Usage error: {0}", poe.Message);
                    s_output.WriteLine(c_usage);
                    return (int)Status.UsageError;
                }

                if (!Options.NoLogo)
                {
                    s_output.WriteLine(c_banner);
                    s_output.WriteLine();
                }

                if (Options.Debug)
                {
                    System.Diagnostics.Debugger.Break();
                }

                if (Options.Help)
                {
                    s_output.WriteLine(c_usage);
                }
                else if (Options.IsKeywordSearch)
                {
                    RenderKeywordSearch();
                }
                else if (Options.Identifier == null)
                {
                    s_output.WriteLine(c_terseUsage);
                }
                else
                {
                    RenderContent();
                }

                return (int)Status.Success;
            }
            catch (MsdnManException ex)
            {
                TextWriter writer = Console.Out;
                if (s_output != null)
                {
                    writer = s_output;
                }
                writer.WriteLine(ex.Message);
                return (int)Status.Exception;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return (int)Status.Exception;
            }
            finally
            {
                if (pagerProcess != null)
                {
                    s_output.Flush();
                    s_output.Close();
                    pagerProcess.WaitForExit();
                }
            }
        }

        private static void AssertFalse(bool option, string message)
        {
            if (option)
            {
                throw new ParseOptionsException(message);
            }
        }
        private static void AssertNotNull(string option, string message)
        {
            if (option == null)
            {
                throw new ParseOptionsException(message);
            }
        }
        private static void AssertNull(string option, string message)
        {
            if (option != null)
            {
                throw new ParseOptionsException(message);
            }
        }
        private static int GetConsoleWidth()
        {
            int width = 80;
            try
            {
                width = Console.WindowWidth;
            }
            catch (System.IO.IOException)
            {
                // Issue 478: If run from within an emacs window, the system
                // can't get the console width, and throws an exception. 
            }

            return width;
        }
        private static string GetFaultValue(XmlNode detail, string xpath)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(detail.OwnerDocument.NameTable);
            nsmgr.AddNamespace("mtps", "urn:msdn-com:public-content-syndication");

            XmlNode node = detail.SelectSingleNode(xpath, nsmgr);

            if (node == null)
            {
                return null;
            }

            return node.Value;

        }
        private static LinkMap GetLinks(XmlElement linkDocument)
        {
            if (linkDocument == null)
            {
                return new LinkMap();
            }

            XmlNodeReader reader = new XmlNodeReader(linkDocument);
            XmlSerializer serializer = new XmlSerializer(typeof(LinkInfo));
            LinkInfo linkInfo = (LinkInfo)serializer.Deserialize(reader);

            return linkInfo.Links;

        }
        private static XmlElement GetLinksDocument(getContentResponse response)
        {
            XmlElement linkDocument = null;
            foreach (common document in response.commonDocuments)
            {
                if (document.commonFormat.ToLower() == "mtps.links")
                {
                    linkDocument = document.Any[0];
                    break;
                }
            }
            return linkDocument;
        }
        private static XmlElement GetXhtmlPrimaryDocument(getContentResponse response)
        {
            XmlElement xhtml = null;
            XmlElement failsafe = null;
            foreach (primary primary in response.primaryDocuments)
            {
                if (primary.primaryFormat == "Mtps.Xhtml")
                {
                    xhtml = primary.Any;
                }

                if (primary.primaryFormat == "Mtps.Failsafe")
                {
                    failsafe = primary.Any;
                }
            }

            if (Options.Failsafe)
            {
                if (failsafe == null)
                {
                    return xhtml;
                }

                return failsafe;
            }
            else
            {
                return xhtml;
            }
        }
        private static string Limit(string text, int length)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }

            if (text.Length > length)
            {
                return text.Substring(0, length - 3) + "...";
            }

            return text;
        }
        private static string NormalizeLanguage(string language)
        {
            if (language == null)
            {
                return null;
            }

            Dictionary<string, string> mappings = new Dictionary<string, string>();
            mappings.Add("c#", "CSharp");
            mappings.Add("csharp", "CSharp");
            mappings.Add("c++", "ManagedCPlusPlus");
            mappings.Add("cpp", "ManagedCPlusPlus");
            mappings.Add("vb", "VisualBasic");
            mappings.Add("VisualBasic", "VisualBasic");
            mappings.Add("js", "JScript");
            mappings.Add("jscript", "JScript");
            mappings.Add("j#", "JSharp");
            mappings.Add("jsharp", "JSharp");

            language = language.ToLower();

            if (mappings.ContainsKey(language))
            {
                return mappings[language];
            }

            return null;
        }
        private static void ParseOptions(string[] args)
        {
            Options options = new Options();

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i].ToLower();

                if (arg.StartsWith("-") || arg.StartsWith("/"))
                {
                    arg = arg.Substring(1);
                    if (arg == "loc")
                    {
                        options.Locale = args[++i];
                    }
                    else if (arg == "ver")
                    {
                        options.Version = args[++i];
                    }
                    else if (arg == "help" || arg == "?")
                    {
                        options.Help = true;
                    }
                    else if (arg == "nologo")
                    {
                        options.NoLogo = true;
                    }
                    else if (arg == "debug")
                    {
                        options.Debug = true;
                    }
                    else if (arg == "url")
                    {
                        options.Url = args[++i];
                    }
                    else if (arg == "xml")
                    {
                        options.Xml = true;
                    }
                    else if (arg == "k" || arg == "keyword")
                    {
                        options.IsKeywordSearch = true;
                    }
                    else if (arg == "failsafe")
                    {
                        options.Failsafe = true;
                    }
                    else if (arg == "root")
                    {
                        options.Root = args[++i];
                    }
                    else if (arg == "language")
                    {
                        options.Language = args[++i];
                    }
                    else if (arg == "nolink")
                    {
                        options.LinkFormat = LinkFormat.Bare;
                    }
                    else if (arg == "shortlink")
                    {
                        options.LinkFormat = LinkFormat.Short;
                    }
                    else if (arg == "r" || arg == "results")
                    {
                        options.SearchResultsLimit = Convert.ToInt32(args[++i]);
                    }
                }
                else if (options.Identifier == null)
                {
                    options.Identifier = arg;
                }
                else
                {
                    throw new ParseOptionsException("Unable to parse argument " + arg);
                }
            }

            if (!options.Help && args.Length > 0)
            {
                AssertNotNull(options.Identifier, "Identifier/keyword must be specified");
            }

            options.Language = NormalizeLanguage(options.Language);

            s_options = options;
        }
        private static void RenderAlternates(availableVersionAndLocale[] availableVersionsAndLocales)
        {
            s_output.WriteLine();
            s_output.WriteLine("== Alternates =======");
            s_output.WriteLine();

            if (availableVersionsAndLocales.Length > 0)
            {
                s_output.WriteLine("The following alternate representations of this topic are available:");
            }
            else
            {
                s_output.WriteLine("No information about alternate representations is available.");
            }
            s_output.WriteLine();

            foreach (availableVersionAndLocale available in availableVersionsAndLocales)
            {
                s_output.WriteLine("* Version: {0} Locale: {1}", available.version, available.locale);
            }
        }
        private static void RenderContent()
        {
            ContentService proxy = new ContentService();
            if (Options.Url != null)
            {
                proxy.Url = Options.Url;
            }

            getContentRequest request = new getContentRequest();
            request.contentIdentifier = Options.Identifier;

            request.locale = Options.Locale;
            request.version = Options.Version;

            if (Options.Failsafe)
            {
                request.requestedDocuments = new requestedDocument[3];
            }
            else
            {
                request.requestedDocuments = new requestedDocument[2];
            }

            int i = 0;
            request.requestedDocuments[i] = new requestedDocument();
            request.requestedDocuments[i].type = documentTypes.primary;
            request.requestedDocuments[i++].selector = "Mtps.Xhtml";

            if (Options.Failsafe)
            {
                request.requestedDocuments[i] = new requestedDocument();
                request.requestedDocuments[i].type = documentTypes.primary;
                request.requestedDocuments[i++].selector = "Mtps.Failsafe";
            }

            request.requestedDocuments[i] = new requestedDocument();
            request.requestedDocuments[i].type = documentTypes.common;
            request.requestedDocuments[i++].selector = "Mtps.Links";

            getContentResponse response = null;
            try
            {
                response = proxy.GetContent(request);
            }
            catch (SoapException e)
            {
                throw TranslateSoapException(e);
            }

            RenderContent(response);
            RenderIdentifiers(response);
            RenderAlternates(response.availableVersionsAndLocales);
            RenderDocuments(response);
            RenderNavigationPaths(response.contentId, response.locale, response.version);

        }
        private static void RenderContent(getContentResponse response)
        {
            s_output.WriteLine();
            s_output.WriteLine("== Content ==========");
            s_output.WriteLine();

            XmlElement xhtml = GetXhtmlPrimaryDocument(response);

            if (xhtml == null)
            {
                s_output.WriteLine("No XHTML content could be found for the topic identifier {0}.", s_options.Identifier);

                if (response.availableVersionsAndLocales.Length > 0)
                {
                    s_output.WriteLine("It is possible that none is available for the {0} locale.", s_options.Locale);
                    s_output.WriteLine("You may wish to retry your request specifying the -loc option");
                    s_output.WriteLine("or the MSDNMAN_LOCALE environment variable. Run msdnman -? for details.");
                }
                return;
            }

            XmlElement linksDocument = GetLinksDocument(response);
            LinkMap links = GetLinks(linksDocument);

            // The -xml option causes us just to dump the untransformed XML. 
            if (Options.Xml)
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = "  ";
                using (XmlWriter writer = XmlWriter.Create(s_output, settings))
                {
                    writer.WriteStartElement("response");
                    writer.WriteStartElement("primary");

                    XmlNodeReader xhtmlNodeReader = new XmlNodeReader(xhtml);
                    writer.WriteNode(xhtmlNodeReader, false);
                    xhtmlNodeReader.Close();

                    writer.WriteEndElement(); // primary

                    writer.WriteStartElement("links");

                    XmlNodeReader linksNodeReader = new XmlNodeReader(linksDocument);
                    writer.WriteNode(linksNodeReader, false);
                    linksNodeReader.Close();

                    writer.WriteEndElement(); // links

                    writer.WriteEndElement(); // response
                }
            }
            else
            {
                // This should totally work, but doesn't. The reason appears to be that
                // the element returned by the web service proxy has no owning document
                // or has an owning document that's not quite right. As a result, there's 
                // no match for the root, which means the transform fails. 
                //XPathNavigator nav = xhtml.CreateNavigator();

                // So instead I came up with this lame hack, which does
                XmlDocument doc = new XmlDocument();
                XmlNodeReader reader = new XmlNodeReader(xhtml);
                doc.Load(reader);
                XPathNavigator nav = doc.CreateNavigator();
                DefaultTransform transform = new DefaultTransform(s_output, nav.NameTable, GetConsoleWidth(), links,
                    Options.Language, Options.LinkFormat);
                transform.Execute(nav);

                // Required by MTPS terms of use http://www.microsoft.com/info/cpyright.mspx
                s_output.WriteLine();
                s_output.WriteLine("Copyright (c) 2004 Microsoft Corporation, One Microsoft Way, Redmond, Washington 98052-6399 U.S.A. All rights reserved.");
            }
        }
        private static void RenderDocuments(getContentResponse response)
        {
            s_output.WriteLine();
            s_output.WriteLine("== Documents ========");
            s_output.WriteLine();

            foreach (primary primary in response.primaryDocuments)
            {
                s_output.WriteLine("{0} (primary)", primary.primaryFormat);
            }

            foreach (common common in response.commonDocuments)
            {
                s_output.WriteLine("{0} (common)", common.commonFormat);
            }

            foreach (image image in response.imageDocuments)
            {
                s_output.WriteLine("{0}.{1} (image)", image.name, image.imageFormat);
            }

            foreach (feature feature in response.featureDocuments)
            {
                s_output.WriteLine("{0} (feature)", feature.featureFormat);
            }
        }
        private static void RenderIdentifiers(getContentResponse response)
        {
            s_output.WriteLine();
            s_output.WriteLine("== Identity Info ====");
            s_output.WriteLine();

            s_output.WriteLine("Short ID : " + response.contentId);
            s_output.WriteLine("GUID     : " + response.contentGuid);
            s_output.WriteLine("Alias    : " + response.contentAlias);
            s_output.WriteLine("Locale   : " + response.locale);
            s_output.WriteLine("Version  : " + response.version);
        }
        private static void RenderKeywordSearch()
        {
            MSNSearchService proxy = new MSNSearchService();

            SearchRequest request = new SearchRequest();

            request.AppID = c_msnSearchAppId;

            // Currently, the only supported culture is en-US. 
            // TODO: Update this if the search ever comes out of beta. 
            request.CultureInfo = "en-US";

            SourceRequest[] sources = new SourceRequest[1];
            SourceRequest source = new SourceRequest();
            sources[0] = source;
            source.Source = SourceType.Web;
            source.ResultFields = ResultFieldMask.Description | ResultFieldMask.Title | ResultFieldMask.Url;
            //source.ResultFields = ResultFieldMask.All;
            source.Count = Options.SearchResultsLimit;

            request.Requests = sources;

            System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo(Options.Locale);
            //request.Query = string.Format("{0} language:{1} site:lab.msdn.microsoft.com OR site:winfx.msdn.microsoft.com OR site:windowssdk.msdn.microsoft.com OR site:whidbey.msdn.microsoft.com OR site:msdn2.microsoft.com",
            //    Options.Identifier, culture.TwoLetterISOLanguageName);
            request.Query = string.Format("{0} language:{1} site:msdn2.microsoft.com",
                Options.Identifier, culture.TwoLetterISOLanguageName);

            try
            {
                SearchResponse results = proxy.Search(request);

                s_output.WriteLine();
                s_output.WriteLine("== Results =====");
                s_output.WriteLine();

                if (results.Responses.Length > 0)
                {
                    SourceResponse response = results.Responses[0];

                    if (response.Total == Options.SearchResultsLimit)
                    {
                        s_output.WriteLine("Only the first {0} items are being displayed - there may be more.", response.Total);
                        s_output.WriteLine("To display more results, use the -results command line option.");
                        s_output.WriteLine();
                    }

                    int resultNumber = 1;
                    foreach (Result result in response.Results)
                    {
                        s_output.WriteLine("{0}. {1}\n{2}\nAbstract: {3}",
                            resultNumber,
                            result.Title,
                            result.Url,
                            Limit(result.Description, 250));

                        s_output.WriteLine();
                        ++resultNumber;
                    }
                }
                else
                {
                    s_output.WriteLine("No results were found for search: " + request.Query);
                }
            }
            catch (SoapException e)
            {
                s_output.WriteLine("The service reported an error:" + e.ToString());
                if (e.Detail != null)
                {
                    s_output.WriteLine(e.Detail.InnerXml);
                }
            }
        }
        private static void RenderNavigationPaths(string contentId, string locale, string version)
        {
            // If we don't have a version or a locale, there's no point in even trying, since
            // the web service call will fail. 

            if (version == null || version.Length == 0)
            {
                return;
            }

            if (locale == null || locale.Length == 0)
            {
                return;
            }

            ContentService proxy = new ContentService();
            if (Options.Url != null)
            {
                proxy.Url = Options.Url;
            }
            getNavigationPathsRequest request = new getNavigationPathsRequest();
            request.root = new navigationKey();
            request.root.contentId = Options.Root;
            request.root.locale = Options.Locale;
            request.root.version = "MSDN.10";
            request.target = new navigationKey();
            request.target.contentId = contentId;
            request.target.locale = locale;
            request.target.version = version;

            getNavigationPathsResponse response = proxy.GetNavigationPaths(request);

            s_output.WriteLine();
            s_output.WriteLine("== Navigation Info =");
            s_output.WriteLine();

            if (response.navigationPaths.Length > 0)
            {
                for (int pathNumber = 0; pathNumber < response.navigationPaths.Length; pathNumber++)
                {
                    navigationPath path = response.navigationPaths[pathNumber];

                    if (response.navigationPaths.Length > 1)
                    {
                        s_output.Write("{0}. ", pathNumber + 1);
                    }

                    for (int i = 0; i < path.navigationPathNodes.Length; i++)
                    {
                        navigationPathNode node = path.navigationPathNodes[i];
                        s_output.Write("\"{0}\":[{1}]", node.title, node.contentNodeKey.contentId);

                        if (i != path.navigationPathNodes.Length - 1)
                        {
                            s_output.Write(" -> ");
                        }
                    }
                    s_output.WriteLine();
                }
            }
            else
            {
                s_output.WriteLine("No navigation path information is available.");
            }

            s_output.WriteLine();

        }
        private static MsdnManException TranslateSoapException(SoapException e)
        {
            XmlNode detail = e.Detail;

            string eventId = GetFaultValue(detail, "mtps:mtpsFaultDetail/mtps:eventId/text()");
            string helpLink = GetFaultValue(detail, "mtps:mtpsFaultDetail/mtps:helpLink/text()");
            string source = GetFaultValue(detail, "mtps:mtpsFaultDetail/mtps:source/text()");

            string message = "ERROR: The MSDN web service reported the following error: " + e.Message;

            if (eventId != null && eventId.Length > 0)
            {
                message += "\nThe event ID was " + eventId;
            }
            else
            {
                message += "\nNo event ID was reported.";
            }

            if (helpLink != null && helpLink.Length > 0)
            {
                message += "\nFor more infomation, see " + helpLink;
            }
            return new MsdnManException(message, e);
        }


    }
}
