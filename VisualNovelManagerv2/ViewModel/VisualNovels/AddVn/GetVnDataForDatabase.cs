using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using VisualNovelManagerv2.CustomClasses;
using VisualNovelManagerv2.CustomClasses.TinyClasses;
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
        private async void GetData()
        {

            Globals.StatusBar.ProgressPercentage = 0;            
            Globals.StatusBar.IsWorkProcessing = true;
            Globals.StatusBar.ProgressText = "Retrieving Data";
            try
            {
                using (Vndb client = new Vndb(true))
                {
                    bool hasMore = true;
                    RequestOptions ro = new RequestOptions { Count = 25 };
                    int pageCount = 1;
                    int characterCount = 0;
                    int releasesCount = 0;


                    List<Character> characterList = new List<Character>();
                    while (hasMore)
                    {
                        ro.Page = pageCount;
                        VndbResponse<Character> characters = await client.GetCharacterAsync(VndbFilters.VisualNovel.Equals(_vnid), VndbFlags.FullCharacter, ro);
                        if (characters != null)
                        {
                            hasMore = characters.HasMore;
                            characterList.AddRange(characters.Items);
                            characterCount = characterCount + characters.Count;
                            pageCount++;
                        }
                        if (characters != null) continue;
                        HandleError.HandleErrors(client.GetLastError(), 0);
                        return;
                    }

                    //set progress percentage to a set value until I get the values for each, then I get the real double I need
                    //TODO: make it so the max percentage is 100, not over
                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage = 3;

                    hasMore = true;
                    pageCount = 1;
                    //do progress here

                    VndbResponse<VisualNovel> visualNovels = await client.GetVisualNovelAsync(VndbFilters.Id.Equals(_vnid), VndbFlags.FullVisualNovel);
                    if (visualNovels == null)
                    {
                        HandleError.HandleErrors(client.GetLastError(), 0);
                        return;
                    }
                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage = 6;


                    List<Release> releaseList = new List<Release>();
                    while (hasMore)
                    {
                        ro.Page = pageCount;
                        VndbResponse<Release> releases = await client.GetReleaseAsync(VndbFilters.VisualNovel.Equals(_vnid), VndbFlags.FullRelease, ro);
                        if (releases == null)
                        {
                            HandleError.HandleErrors(client.GetLastError(), 0);
                            break;
                        }
                        hasMore = releases.HasMore;
                        releaseList.AddRange(releases.Items);
                        releasesCount = releasesCount + releases.Count;
                        pageCount++;
                    }
                    if (Globals.StatusBar.ProgressPercentage != null)
                        Globals.StatusBar.ProgressPercentage = 9;


                    _progressIncrement = (double) 91 / (2 + characterCount + releasesCount);
                    //await save vn data here
                    Globals.StatusBar.ProgressText = "Processing Data";
                    Globals.StatusBar.IsDbProcessing = true;
                    await AddToDatabase(visualNovels, releaseList, characterList);
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
