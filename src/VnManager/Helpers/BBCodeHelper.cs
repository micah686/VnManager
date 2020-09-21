using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using VndbSharp.Models.Common;
using VnManager.Models.Settings;

namespace VnManager.Helpers
{
    // ReSharper disable once InconsistentNaming
    public static class BBCodeHelper
    {
        public static string Helper(string text)
        {
            string modifiedText = text;
            if (string.IsNullOrEmpty(modifiedText)) return string.Empty;
            modifiedText = ReplaceSpoilers(text);
            modifiedText = ReplaceVndbLocalUrls(modifiedText);



            return modifiedText;
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

        private static string ReplaceUrls(string text)
        {
            string rawText= String.Empty;
            var replacementList = SplitUrl(text);

            return rawText;
        }



        private static List<BBReplacement> SplitUrl(string text)
        {

            List<BBReplacement> replacementList = new List<BBReplacement>();
            //First block: (\[url=) captures: [url=
            //Second block (https?:\/\/) captures http:// or https://
            //Third block (.+?(?=\])) captures everything forward UNTIL the first ']', but doesn't include the ']'
            //Fourth block (\]) captures JUST the ']'
            //Fifth block (.+?(?=\[) captures everything forward UNTIL the first ']', but doesn't include the '['
            //Sixth block (\[\/url]) captures the [/url]
            var rgx = new Regex(@"(\[url=)(https?:\/\/)(.+?(?=\]))(\])(.+?(?=\[))(\[\/url])", RegexOptions.IgnoreCase);
            var matches = rgx.Matches(text);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    var groups = match.Groups;
                    var toDelete = groups[0].Value;
                    var url = string.Concat(groups[2].Value, groups[3].Value); //combines the https:// with the actual url
                    var stringVal = groups[5].Value; //the name of the element between the two [url][/url] blocks

                    var replacement = new BBReplacement(){ToRemove = toDelete, Url = url, StringName = stringVal};
                    replacementList.Add(replacement);
                }
            }

            return replacementList;
        }

        private static void StripUrls(string original, List<BBReplacement> replacements)
        {
            //matches the bbcode brackets, but not anything between
            //for example, [color=orange-red]TEST[/color]
            //will match [color=orange-red] and [/color], but NOT TEST
            var rgx = new Regex(@"\[\/?(?:.+)*?.*?\]", RegexOptions.IgnoreCase);
            var output = rgx.Replace(original, "");
        }

        private struct BBReplacement
        {
            internal string ToRemove;
            internal string Url;
            internal string StringName;

        }
    }
}
