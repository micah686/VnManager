using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using VisualNovelManagerv2.CustomClasses;
using VisualNovelManagerv2.CustomClasses.Vndb;
using VndbSharp;
using VndbSharp.Models;
using VndbSharp.Models.Character;
using VndbSharp.Models.Release;
using VndbSharp.Models.VisualNovel;

namespace VisualNovelManagerv2.ViewModel.VisualNovels.AddVn
{
    public partial class AddVnViewModel
    {
        private async Task GetData()
        {
            double progressIncrement = 0;

            Globals.StatusBar.ProgressPercentage = 0;
            Globals.StatusBar.IsDbProcessing = true;
            Globals.StatusBar.IsWorkProcessing = true;
            Globals.StatusBar.ProgressText = "Processing";
            try
            {
                using (Vndb client = new Vndb(true))
                {
                    bool hasMore = true;
                    RequestOptions ro = new RequestOptions { Count = 25 };
                    int pageCount = 1;
                    int characterCount = 0;


                    List<Character> characterList = new List<Character>();
                    while (hasMore)
                    {
                        ro.Page = pageCount;
                        VndbResponse<Character> characters = await client.GetCharacterAsync(VndbFilters.VisualNovel.Equals(_vnid), VndbFlags.FullCharacter, ro);
                        hasMore = characters.HasMore;
                        characterList.AddRange(characters.Items);
                        characterCount = characterCount + characters.Count;
                        pageCount++;
                    }
                    //TODO: See why I added 10 to the value
                    progressIncrement = (double) 100 / (10 + characterCount);

                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage = (double)Globals.StatusBar.ProgressPercentage + progressIncrement;

                    hasMore = true;
                    pageCount = 1;
                    //do progress here

                    VndbResponse<VisualNovel> visualNovels = await client.GetVisualNovelAsync(VndbFilters.Id.Equals(_vnid), VndbFlags.FullVisualNovel);
                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage = (double) Globals.StatusBar.ProgressPercentage + progressIncrement;


                    List<Release> releaseList = new List<Release>();
                    while (hasMore)
                    {
                        ro.Page = pageCount;
                        VndbResponse<Release> releases = await client.GetReleaseAsync(VndbFilters.VisualNovel.Equals(_vnid), VndbFlags.FullRelease, ro);
                        hasMore = releases.HasMore;
                        releaseList.AddRange(releases.Items);
                        pageCount++;
                    }
                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage = (double)Globals.StatusBar.ProgressPercentage + progressIncrement;

                    //await save vn data here
                }

            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                Globals.StatusBar.ProgressStatus = new BitmapImage(new Uri($@"{Globals.DirectoryPath}\Data\res\icons\statusbar\error.png"));
                Globals.StatusBar.ProgressText = "An Error Occured! Check log for details";
                throw;
            }
            
        }


    }
}
