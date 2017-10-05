using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VisualNovelManagerv2.CustomClasses.ConfigSettings;
using VisualNovelManagerv2.CustomClasses.TinyClasses;
using VisualNovelManagerv2.Design.Settings;

namespace VisualNovelManagerv2.Converters
{
    public class ConvertTextBBcode
    {
        public static string ConvertText(string text)
        {
            string modifiedText = text;
            if (string.IsNullOrEmpty(modifiedText)) return string.Empty;
            modifiedText = ReplaceSpoilers(text);
            modifiedText = ReplaceVndbLocalUrls(modifiedText);
            modifiedText = EscapeBbCode(modifiedText);
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

                UserSettings settings = ModifyUserSettings.LoadUserSettings();
                if (settings.VnSetting != null)
                {
                    if (!settings.VnSetting.Id.Equals(Globals.VnId) || settings.VnSetting.Spoiler < 2)
                    {
                        //intentionally left blank
                    }
                }
                else
                {
                    rawText = spoilerList.Aggregate(rawText,
                        (current, spoiler) => current.Replace(spoiler, "<Content hidden by spoiler setting>"));
                }
                return rawText;
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex);
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
                Globals.Logger.Error(ex);
                throw;
            }            
        }

        private static string EscapeBbCode(string text)
        {
            string rawText = text;
            //matches ALL instances of [ $something here ]
            var allBracket = new Regex(@"(\[)(.+?)(\])", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            List<string> abMatch = allBracket.Matches(rawText).Cast<Match>().Select(m => m.Value).ToList();
            //matches only valid bbcode brackets
            Regex bbCodeBracket = new Regex(@"/([\r\n])|(?:\[([a-z]{1,16})(?:=([^\x00-\x1F""'\(\)<>\[\]]{1,256}))?\])|(?:\[/([a-z]{1,16})\])", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            List<string> bbMatch = bbCodeBracket.Matches(rawText).Cast<Match>().Select(m => m.Value).ToList();

            List<string> invalidBbCodeList = abMatch.Except(bbMatch).ToList();
            foreach (string invalid in invalidBbCodeList)
            {
                //invalid should be something like [notBBcode text here....
                //escaped adds a '\' right after the first bracket, which is the currently defined escape character, so the BBCode parser doesn't parse that bracket
                string escaped = invalid.Insert(1, "\\");
                rawText = rawText.Replace(invalid, escaped);
            }        
            return rawText;
        }
    }
}
