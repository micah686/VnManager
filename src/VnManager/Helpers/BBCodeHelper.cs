using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Navigation;
using VndbSharp.Models.Common;

namespace VnManager.Helpers
{
    // ReSharper disable once InconsistentNaming
    public static class BBCodeHelper
    {
        private const char SplitChar = '\x205E'; //this is a unicode 4 vertical dots(⁞) that can be used as a splitter
        private const char StartChar = '\x25C4';
        private const char EndChar = '\x25BA';
        private static readonly TimeSpan RegexTimeout = new TimeSpan(0,0,0,0,500);
        /// <summary>
        /// Takes a BBCode string and converts it into a formats it into a usable way
        /// </summary>
        /// <param name="text">String of BBCode</param>
        /// <returns>List of InLines. This needs to be bound using the Textblock Inline Binding Extension</returns>
        public static Inline[] Helper(string text)
        {
            string modifiedText = text;
            if (string.IsNullOrEmpty(modifiedText))
            {
                return new Inline[] {new Span()};
            }
            modifiedText = ReplaceSpoilers(text);
            modifiedText = ReplaceVndbLocalUrls(modifiedText);
            modifiedText = StripUnneededBbCode(modifiedText);
            modifiedText = ReplaceUrls(modifiedText);
            
            var inlineList = FormatUrlsInLine(modifiedText);


            return inlineList.ToArray();
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
                Regex regex = new Regex(@"\[spoiler\](.*)\[\/spoiler\]", RegexOptions.IgnoreCase);
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
                const int splitGroupCount = 4;
                string rawText = text;
                //match the local url bracket
                Regex regex = new Regex(@"(\[\burl\b=\/[a-z][0-9]+\])");
                foreach (var segment in regex.Split(text))
                {
                    //matches vndb local url, like /c##, /v##,..., and eliminates empty strings, seperated into 4 groups
                    List<string> splitUrl = (new Regex(@"(\[)(\burl=)(\/[a-z][0-9]+)(\])",
                            RegexOptions.Compiled | RegexOptions.IgnoreCase).Split(segment))
                        .Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

                    if (splitUrl.Count != splitGroupCount)
                    {
                        continue;
                    }
                    //this should always be the case
                    if (splitUrl[1] != "url=")
                    {
                        continue;
                    }
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
        private static string StripUnneededBbCode(string text)
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
                    var newUrlAndName = StartChar + url + SplitChar + displayName + EndChar;
                    modifiedText = modifiedText.Replace(originalStr, newUrlAndName, false, CultureInfo.InvariantCulture);
                }
            }

            return modifiedText;
        }


        /// <summary>
        /// Takes a string, and creates an inline list out of it, creating hyperlinks
        /// adapted from https://stackoverflow.com/a/27742886
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static List<Inline> FormatUrlsInLine(string input)
        {
            const int splitTextValue = 2;
            List<Inline> inlineList = new List<Inline>();
            var rgx = new Regex(@"(\◄.+?\►)", RegexOptions.IgnoreCase);
            var str = rgx.Split(input);
            for (int i = 0; i < str.Length; i++)
            {
                if (i % splitTextValue == 0)
                {
                    inlineList.Add(new Run {Text = str[i]});
                }
                else
                {
                    var newText = str[i];
                    newText = newText.Replace($"{StartChar}", string.Empty);
                    newText = newText.Replace($"{EndChar}", string.Empty);
                    var split = newText.Split(SplitChar);
                    SplitUrl splitUrl = new SplitUrl {Url = split[0], Label = split[1]};
                    var run = new Run(splitUrl.Label);
                    Hyperlink link = new Hyperlink(run);


                    var isValid = Uri.IsWellFormedUriString(splitUrl.Url, UriKind.Absolute);
                    if (isValid)
                    {
                        link.NavigateUri = new Uri(splitUrl.Url);
                        link.RequestNavigate += Hyperlink_RequestNavigate;
                        inlineList.Add(link);
                    }
                    else
                    {
                        inlineList.Add(run);
                    }

                }
            }
            return inlineList;
        }
        
        internal struct SplitUrl: IEquatable<SplitUrl>
        {
            internal string Url;
            internal string Label;
            public bool Equals(SplitUrl other) =>
                (Url, Label) == (other.Url, other.Label);
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
