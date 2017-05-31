using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using VisualNovelManagerv2.CustomClasses;

namespace VisualNovelManagerv2.Converters
{
    public class ConvertRichTextDocument
    {
        public static FlowDocument ConvertToFlowDocument(string text)
        {
            try
            {                                
                List<string> websiteList = new List<string>();

                websiteList = FindUrls(text, websiteList);
                return CreateHyperlinks(text, websiteList);
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
        }

        private static List<string> FindUrls(string text, List<string>websiteList)
        {
            try
            {
                //matches the first part of bbcode url([url=website]
                var startBBregex = new Regex(@"(\[url=.+?\])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var matchStartBBregex = startBBregex.Matches(text).Cast<Match>().Select(m => m.Value).ToList();

                foreach (string segment in startBBregex.Split(text))
                {
                    if (matchStartBBregex.Contains(segment))
                    {
                        //matches the local url, but with the url tag, to eliminate bad values
                        var vndbRegex = new Regex(@"(\[\burl\b=\/[a-z][0-9]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

                        foreach (string url in vndbRegex.Split(segment))
                        {
                            //matches vndb local url, like /c##, /v##,...
                            var localRegex = new Regex(@"(\/[a-z][0-9]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                            var localRegexmatch = localRegex.Match(segment).ToString();
                            if (localRegexmatch != "" && !websiteList.Contains("http://vndb.org" + localRegexmatch))
                            {
                                websiteList.Add("http://vndb.org" + localRegexmatch);
                            }
                        }

                        //matches the url within the bbcode. So everything after url= and NOT including the last bracket
                        var regexURL = new Regex(@"(?:\[url=)(.+?)(?:\])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        var urlList = regexURL.Split(segment);

                        foreach (string url in urlList)
                        {
                            if (!string.IsNullOrEmpty(url) && !websiteList.Contains(url))
                            {
                                List<Regex> RegexList = new List<Regex>();
                                //used http://blog.mattheworiordan.com/post/13174566389/url-regular-expression-for-links-with-or-without for the first two regex
                                RegexList.Add(new Regex(@"(([A-Za-z]{3,9}:(?:\/\/)?)[A-Za-z0-9\.\-]+|(?:www\.)[A-Za-z0-9\.\-]+)((?:\/[\+~%\/\.\w\-]*)?\??(?:[\-\+=&;%@\.\w]*)#?(?:[\.\!\/\\\w]*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase));
                                RegexList.Add(new Regex(@"/((([A-Za-z]{3,9}:(?:\/\/)?)(?:[\-;:&=\+\$,\w]+@)?[A-Za-z0-9\.\-]+|(?:www\.|[\-;:&=\+\$,\w]+@)[A-Za-z0-9\.\-]+)((?:\/[\+~%\/\.\w\-_]*)?\??(?:[\-\+=&;%@\.\w_]*)#?(?:[\.\!\/\\\w]*))?)/", RegexOptions.Compiled | RegexOptions.IgnoreCase));
                                RegexList.Add(new Regex(@"(?i)\b((?:[a-z][\w-]+:(?:/{1,3}|[a-z0-9%])|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'"".,<>?«»“”‘’]))", RegexOptions.Compiled | RegexOptions.IgnoreCase));
                                RegexList.Add(new Regex(@"(?i)\b((?:https?:(?:/{1,3}|[a-z0-9%])|[a-z0-9.\-]+[.](?:com|net|org|edu|gov|mil|aero|asia|biz|cat|coop|info|int|jobs|mobi|museum|name|post|pro|tel|travel|xxx|ac|ad|ae|af|ag|ai|al|am|an|ao|aq|ar|as|at|au|aw|ax|az|ba|bb|bd|be|bf|bg|bh|bi|bj|bm|bn|bo|br|bs|bt|bv|bw|by|bz|ca|cc|cd|cf|cg|ch|ci|ck|cl|cm|cn|co|cr|cs|cu|cv|cx|cy|cz|dd|de|dj|dk|dm|do|dz|ec|ee|eg|eh|er|es|et|eu|fi|fj|fk|fm|fo|fr|ga|gb|gd|ge|gf|gg|gh|gi|gl|gm|gn|gp|gq|gr|gs|gt|gu|gw|gy|hk|hm|hn|hr|ht|hu|id|ie|il|im|in|io|iq|ir|is|it|je|jm|jo|jp|ke|kg|kh|ki|km|kn|kp|kr|kw|ky|kz|la|lb|lc|li|lk|lr|ls|lt|lu|lv|ly|ma|mc|md|me|mg|mh|mk|ml|mm|mn|mo|mp|mq|mr|ms|mt|mu|mv|mw|mx|my|mz|na|nc|ne|nf|ng|ni|nl|no|np|nr|nu|nz|om|pa|pe|pf|pg|ph|pk|pl|pm|pn|pr|ps|pt|pw|py|qa|re|ro|rs|ru|rw|sa|sb|sc|sd|se|sg|sh|si|sj|Ja|sk|sl|sm|sn|so|sr|ss|st|su|sv|sx|sy|sz|tc|td|tf|tg|th|tj|tk|tl|tm|tn|to|tp|tr|tt|tv|tw|tz|ua|ug|uk|us|uy|uz|va|vc|ve|vg|vi|vn|vu|wf|ws|ye|yt|yu|za|zm|zw)/)(?:[^\s()<>{}\[\]]+|\([^\s()]*?\([^\s()]+\)[^\s()]*?\)|\([^\s]+?\))+(?:\([^\s()]*?\([^\s()]+\)[^\s()]*?\)|\([^\s]+?\)|[^\s`!()\[\]{};:'"".,<>?«»“”‘’])|(?:(?<!@)[a-z0-9]+(?:[.\-][a-z0-9]+)*[.](?:com|net|org|edu|gov|mil|aero|asia|biz|cat|coop|info|int|jobs|mobi|museum|name|post|pro|tel|travel|xxx|ac|ad|ae|af|ag|ai|al|am|an|ao|aq|ar|as|at|au|aw|ax|az|ba|bb|bd|be|bf|bg|bh|bi|bj|bm|bn|bo|br|bs|bt|bv|bw|by|bz|ca|cc|cd|cf|cg|ch|ci|ck|cl|cm|cn|co|cr|cs|cu|cv|cx|cy|cz|dd|de|dj|dk|dm|do|dz|ec|ee|eg|eh|er|es|et|eu|fi|fj|fk|fm|fo|fr|ga|gb|gd|ge|gf|gg|gh|gi|gl|gm|gn|gp|gq|gr|gs|gt|gu|gw|gy|hk|hm|hn|hr|ht|hu|id|ie|il|im|in|io|iq|ir|is|it|je|jm|jo|jp|ke|kg|kh|ki|km|kn|kp|kr|kw|ky|kz|la|lb|lc|li|lk|lr|ls|lt|lu|lv|ly|ma|mc|md|me|mg|mh|mk|ml|mm|mn|mo|mp|mq|mr|ms|mt|mu|mv|mw|mx|my|mz|na|nc|ne|nf|ng|ni|nl|no|np|nr|nu|nz|om|pa|pe|pf|pg|ph|pk|pl|pm|pn|pr|ps|pt|pw|py|qa|re|ro|rs|ru|rw|sa|sb|sc|sd|se|sg|sh|si|sj|Ja|sk|sl|sm|sn|so|sr|ss|st|su|sv|sx|sy|sz|tc|td|tf|tg|th|tj|tk|tl|tm|tn|to|tp|tr|tt|tv|tw|tz|ua|ug|uk|us|uy|uz|va|vc|ve|vg|vi|vn|vu|wf|ws|ye|yt|yu|za|zm|zw)\b/?(?!@)))", RegexOptions.Compiled | RegexOptions.IgnoreCase));

                                foreach (Regex regx in RegexList)
                                {
                                    if (url == regx.Match(url).ToString())
                                    {
                                        websiteList.Add(url);
                                        break;
                                    }
                                }
                            }
                        }

                    }
                }
                return websiteList;
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
            
        }

        private static FlowDocument CreateHyperlinks(string text, List<string> websiteList)
        {
            try
            {
                var flowDocument = new FlowDocument();

                var fullBBcodeRegex = new Regex(@"(\[url=.+?\].+?\[\/url\])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var centerwordBbRegex = new Regex(@"(?:\[url=.+?\])(.+?)(?:\[\/url\])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var matches = fullBBcodeRegex.Matches(text).Cast<Match>().Select(m => m.Value).ToList();

                var paragraph = new Paragraph();
                flowDocument.Blocks.Add(paragraph);
                int i = 0;
                foreach (var segment in fullBBcodeRegex.Split(text))
                {
                    string centerword = null;

                    if (matches.Contains(segment))
                    {
                        foreach (var center in centerwordBbRegex.Split(segment))
                        {
                            if (!string.IsNullOrEmpty(center))
                            {
                                centerword = center;
                            }
                        }
                        //add about:blank to prevent issues if a url wan't found
                        if (websiteList.Count < 1) { websiteList.Add("about:blank"); }

                        var hyperlink = new Hyperlink(new Run(centerword))
                        {
                            NavigateUri = new Uri(websiteList[i])
                        };
                        int count = i;
                        hyperlink.RequestNavigate += (sender, args) => Process.Start(websiteList[count]);
                        paragraph.Inlines.Add(hyperlink);
                        i++;
                    }
                    else
                    {
                        paragraph.Inlines.Add(new Run(segment));
                    }
                }

                return flowDocument;
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }            
        }

    }
}
