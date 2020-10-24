﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Navigation;
using VndbSharp.Models.Common;
using VnManager.Models.Settings;

namespace VnManager.Helpers
{
    // ReSharper disable once InconsistentNaming
    public static class BBCodeHelper
    {
        private const char SplitChar = '\x205E'; //this is a unicode 4 vertical dots(⁞) that can be used as a splitter
        private static readonly TimeSpan RegexTimeout = new TimeSpan(0,0,0,0,500);
        public static List<Inline> Helper(string text)
        {
            string modifiedText = text;
            if (string.IsNullOrEmpty(modifiedText)) return new List<Inline>();
            modifiedText = ReplaceSpoilers(text);
            modifiedText = ReplaceVndbLocalUrls(modifiedText);
            modifiedText = StripUnneededBbCode(modifiedText);
            modifiedText = ReplaceUrls(modifiedText);


            

            var p = new Paragraph();
            var inlineList = Format(modifiedText, p.Inlines);


            return inlineList;
        }


        /// <summary>
        /// Replace any BBCode [Spoiler][/Spoiler] blocks with a content hidden message if your spoiler level settings don't allow major spoilers
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Replace local Vndb urls (/v92) with full urls (vndb.org/v92)
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Strip out all unwanted BBCode blocks (bold, italics, strikethrough,...)
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string StripUnneededBbCode(string text)
        {
            //Regex has nested capturing groups!!
            //0th block is full string [raw]sample[/raw]
            //First block: (\[url=) captures: [code], [raw], [s], [u], [i], [b]
            //Second block is bbcode start without brackets, such as 'raw'
            //Third block (.+?(?=\[) captures everything forward UNTIL the first '[', but doesn't include the '[', basically the inside text
            //Fourth block contains the ending bbcode block with the brackets, like [/raw]
            //Fifth block captures the ending bbcode, but without brackets, like 'raw'
            var rgx = new Regex(@"(\[(code|raw|s|u|i|b)\])(.+?(?=\[))(\[\/(code|raw|s|u|i|b)])", RegexOptions.IgnoreCase);
            var matches = rgx.Matches(text);
            var modString = text;
            foreach (Match match in matches)
            {
                var inputStr = match.Value;
                var outputStr = match.Groups[3].Value;
                modString = modString.Replace(inputStr, outputStr, false, CultureInfo.InvariantCulture);
            }
            return modString;
        }

        /// <summary>
        /// Strip bbcode url brackets, and split the url into the URL+Label
        /// So the resulting urls would be something like https://google.com⁞Google
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string ReplaceUrls(string text)
        {
            var modifiedText = text;
            //First block: (\[url=) captures: [url=
            //Second block (.+?(?=\])) captures everything forward UNTIL the first ']', but doesn't include the ']'
            //Third block (\]) captures JUST the ']'
            //Fourth block (.+?(?=\[) captures everything forward UNTIL the first '[', but doesn't include the '['
            //Fifth block (\[\/url]) captures the [/url]
            var rgx = new Regex(@"(\[url=)(.+?(?=\]))(\])(.+?(?=\[))(\[\/url])", RegexOptions.IgnoreCase, RegexTimeout);
            var matches = rgx.Matches(modifiedText);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    var originalStr = match.Groups[0].Value;
                    var url = match.Groups[2].Value;
                    var displayName = match.Groups[4].Value;
                    var newUrlAndName = url + SplitChar + displayName + SplitChar;
                    modifiedText = modifiedText.Replace(originalStr, newUrlAndName, false, CultureInfo.InvariantCulture);
                }
            }

            return modifiedText;
        }


        
        


        public struct UrlMatch
        {
            public int Offset;
            public string Text;
        }

        internal struct SplitUrl
        {
            internal string Url;
            internal string Label;
        }

        /// <summary>
        /// Tries to get a match to a valid url
        /// https://github.com/pointgaming/point-gaming-desktop/blob/f9bc16a69d66cab39e23a3ce3a05d06a3888071c/PointGaming/Chat/ChatCommon.cs
        /// </summary>
        /// <param name="mine"></param>
        /// <param name="startOffset"></param>
        /// <param name="urlMatch"></param>
        /// <returns></returns>
        public static bool TryGetMatch(string mine, int startOffset, out UrlMatch urlMatch)
        {
            //matches the https://... with a lookahead to '⁞', not capturing it
            //then capture everything after the ⁞, not including it
            //finally, capture the ⁞
            var regex = new Regex(@"http[s]?://[^\\s](.+?(?=\⁞))(.+?(?=\⁞))\⁞", RegexOptions.None, RegexTimeout);
            var matches = regex.Matches(mine, startOffset);
            if (matches.Count > 0 && matches[0].Success)
            {
                var result = matches[0].Value;
                if (result.EndsWith(".") || result.EndsWith("?") || result.EndsWith("!"))
                    result = result.Substring(0, result.Length - 1);
                urlMatch = new UrlMatch { Offset = matches[0].Index, Text = result };
                return true;
            }


            urlMatch = new UrlMatch();
            return false;
        }


        /// <summary>
        /// Create hyperlinks out of urls, and add each of the inlines to a list
        /// </summary>
        /// <param name="message"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static List<Inline> Format(string message, InlineCollection collection)
        {
            List<string> dupeList= new List<string>();
            string modifiedText = message;
            UrlMatch urlMatch;
            int cur = 0;
            while (TryGetMatch(modifiedText, cur, out urlMatch))
            {
                string before = modifiedText.Substring(cur, urlMatch.Offset - cur);
                if (before.Length > 0)
                {
                    DupeCheck(before, ref dupeList, ref collection);
                }
                
                var split = urlMatch.Text.Split(SplitChar);
                
                SplitUrl splitUrl= new SplitUrl {Url = split[0], Label = split[1]};
                modifiedText = modifiedText.Replace(urlMatch.Text, splitUrl.Label, false, CultureInfo.InvariantCulture);
                
                var matchRun = new Run(splitUrl.Label);
                try
                {
                    Hyperlink link = new Hyperlink(matchRun);
                    var isValid = Uri.IsWellFormedUriString(splitUrl.Url, UriKind.Absolute);
                    if (isValid)
                    {
                        link.NavigateUri = new Uri(splitUrl.Url);
                        link.RequestNavigate += Hyperlink_RequestNavigate;
                        collection.Add(link);
                    }
                    else
                    {
                        collection.Add(matchRun);
                    }
                    
                }
                catch
                {
                    collection.Add(matchRun);
                }

                cur = urlMatch.Offset + splitUrl.Label.Length;
                string ending = modifiedText.Substring(cur);
                if (ending.Length > 0)
                {
                    ending = LookAheadHttp(ending);
                    collection.Add(new Run(ending));
                    dupeList.Add(ending);
                }
                    
            }


            return collection.ToList();
        }

        /// <summary>
        /// Checks to see if the word that we want to add was already added, and if so, skip adding it
        /// </summary>
        /// <param name="before">text preparing to add</param>
        /// <param name="dupeList">Reference of List of duplicate entries</param>
        /// <param name="collection">Inline collection</param>
        private static void DupeCheck(string before, ref List<string> dupeList, ref InlineCollection collection)
        {
            dupeList.Add(before);
            if (dupeList.Count == 2)
            {
                string entry1 = string.Concat(dupeList[0].Where(c => !char.IsWhiteSpace(c)),CultureInfo.InvariantCulture).ToLower();
                string entry2 = string.Concat(dupeList[0].Where(c => !char.IsWhiteSpace(c)), CultureInfo.InvariantCulture).ToLower();
                if (!entry1.Equals(entry2))
                {
                    collection.Add(new Run(before));
                }
                
            }
            else
            {
                collection.Add(new Run(before));
            }
            dupeList.Clear();
        }

        /// <summary>
        /// Grabs the the text BEFORE the https://....
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string LookAheadHttp(string text)
        {
            string outputText = text;
            //captures everything forward UNTIL the first http://..., but not including the http://...
            var regex = new Regex("(.+?(?=http[s]?://[^\\s]*))", RegexOptions.None, RegexTimeout);
            var match = regex.Match(text);
            if (match.Success)
            {
                outputText = match.Groups[0].Value;
            }
            return outputText;
        }

        /// <summary>
        /// Method that adds the code to actually navigate to the link that was specified
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

    }
}
