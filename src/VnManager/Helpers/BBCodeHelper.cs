using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Navigation;
using CodeKicker.BBCode;
using VndbSharp.Models.Common;
using VnManager.Models.Settings;

namespace VnManager.Helpers
{
    // ReSharper disable once InconsistentNaming
    public static class BBCodeHelper
    {

        public static List<Inline> Helper(string text)
        {
            string modifiedText = text;
            if (string.IsNullOrEmpty(modifiedText)) return new List<Inline>();
            modifiedText = ReplaceSpoilers(text);
            modifiedText = ReplaceVndbLocalUrls(modifiedText);

            var inlineList = ReplaceUrls(modifiedText);


            return inlineList;
        }



        private static string ReplaceSpoilers(string text)
        {
            try
            {
                List<string> spoilerList = new List<string>();
                string rawText = text;
                Regex regex = new Regex(@"\[spoiler\](.*)\[\/spoiler\]");
                foreach (Match match in regex.Matches(text))
                {
                    rawText = rawText.Replace(match.Groups[0].ToString(), match.Groups[1].ToString());
                    spoilerList.Add(match.Groups[1].ToString());
                }

                if (App.UserSettings.SettingsVndb.Spoiler < SpoilerLevel.Major)
                {
                    rawText = spoilerList.Aggregate(rawText,
                        (current, spoiler) => current.Replace(spoiler, "<Content hidden by spoiler setting>"));
                }

                return rawText;
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "failed to replace spoilers");
                throw;
            }
        }

        private static string ReplaceVndbLocalUrls(string text)
        {
            try
            {
                string rawText = text;
                //match the local url bracket
                Regex regex = new Regex(@"(\[\burl\b=\/[a-z][0-9]+\])");
                foreach (var segment in regex.Split(text))
                {
                    //matches vndb local url, like /c##, /v##,..., and eliminates empty strings, seperated into 4 groups
                    List<string> splitUrl = (new Regex(@"(\[)(\burl=)(\/[a-z][0-9]+)(\])",
                            RegexOptions.Compiled | RegexOptions.IgnoreCase).Split(segment))
                        .Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

                    if (splitUrl.Count != 4) continue;
                    //this should always be the case
                    if (splitUrl[1] != "url=") continue;
                    splitUrl[1] = "url=http://vndb.org";
                    string merged = string.Join("", splitUrl);
                    rawText = rawText.Replace(segment, merged);
                }
                return rawText;
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "failed to replace vndb local url");
                throw;
            }
        }

        private static List<Inline> ReplaceUrls(string text)
        {
            string rawText= text;
            List<BBReplacement> replacementList = SplitUrl(text);
            List<Inline> inlineList = new List<Inline>();
            var rgx = new Regex(@"(\[url=)(.+?(?=\]))(\])(.+?(?=\[))(\[\/url])", RegexOptions.IgnoreCase);
            var bbTags = new List<BBTag>()
            {
                new BBTag("b", "<strong>", "</strong>"),
                new BBTag("i", "<em>", "</em>"),
                new BBTag("u", "<span style=\"text-decoration: line-through\">", "</span>"),

                new BBTag("list", "<ul>", "</ul>") { SuppressFirstNewlineAfter = true },
                new BBTag("li", "<li>", "</li>", true, false),

                new BBTag("url", "<a href=\"${url}\">", "</a>",
                    new BBAttribute("url", "")),

                new BBTag("code", "<pre class=\"prettyprint\">", "</pre>")
                {
                    StopProcessing = true,
                    SuppressFirstNewlineAfter = true
                },
            };
            var parser = new BBCodeParser(bbTags);
            var split = parser.ParseSyntaxTree(rawText);

            foreach (var node in split.SubNodes)
            {
                
                if (rgx.IsMatch(node.ToString()))
                {
                    Match match = rgx.Matches(node.ToString()).FirstOrDefault();
                    int idx = replacementList.FindIndex(x =>  match != null && x.ToRemove == match.Groups[0].ToString());
                    var run = new Run(replacementList[idx].StringName);
                    var validUri = Uri.IsWellFormedUriString(replacementList[idx].Url, UriKind.Absolute);
                    if (validUri)
                    {
                        var hyperlink = new Hyperlink(run)
                        {
                            NavigateUri = new Uri(replacementList[idx].Url)
                        };
                        hyperlink.RequestNavigate += Hyperlink_RequestNavigate;
                        inlineList.Add(hyperlink);
                    }
                    else
                    {
                        inlineList.Add(run);
                    }

                }
                else
                {
                    var run = new Run(node.ToString());
                    inlineList.Add(run);
                }
            }

            return inlineList;
        }


        internal static void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var targetUrl = e.Uri.ToString();
            var psi = new ProcessStartInfo
            {
                FileName = targetUrl,
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        private static List<BBReplacement> SplitUrl(string text)
        {

            List<BBReplacement> replacementList = new List<BBReplacement>();
            //First block: (\[url=) captures: [url=
            //Second block (.+?(?=\])) captures everything forward UNTIL the first ']', but doesn't include the ']'
            //Third block (\]) captures JUST the ']'
            //Fourth block (.+?(?=\[) captures everything forward UNTIL the first ']', but doesn't include the '['
            //Fifth block (\[\/url]) captures the [/url]
            var rgx = new Regex(@"(\[url=)(.+?(?=\]))(\])(.+?(?=\[))(\[\/url])", RegexOptions.IgnoreCase);
            var matches = rgx.Matches(text);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    var groups = match.Groups;
                    var toDelete = groups[0].Value;
                    var url = groups[2].Value; 
                    var stringVal = groups[4].Value; //the name of the element between the two [url][/url] blocks

                    var replacement = new BBReplacement(){ToRemove = toDelete, Url = url, StringName = stringVal};
                    replacementList.Add(replacement);
                }
            }

            return replacementList;
        }

        private struct BBReplacement
        {
            internal string ToRemove;
            internal string Url;
            internal string StringName;

        }
    }
}
