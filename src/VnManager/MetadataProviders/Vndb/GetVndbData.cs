using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VndbSharp;
using VndbSharp.Models;
using VndbSharp.Models.Character;
using VndbSharp.Models.Errors;
using VndbSharp.Models.Producer;
using VndbSharp.Models.Release;
using VndbSharp.Models.Staff;
using VndbSharp.Models.VisualNovel;
using VnManager.Helpers.Vndb;
using VnManager.ViewModels.UserControls;

namespace VnManager.MetadataProviders.Vndb
{
    public class GetVndbData
    {
        private readonly Stopwatch stopwatch = new Stopwatch();
        private readonly TimeSpan maxTime = TimeSpan.FromMinutes(3);
        private bool _didErrorOccur = false;
        public async Task GetDataAsync(int gameId)
        {
            uint vnId = (uint)gameId;
            try
            {
                using (var client = new VndbSharp.Vndb(true))
                {
                    App.StatusBar.IsWorking = true;
                    App.StatusBar.StatusString = App.ResMan.GetString("Working");
                    const double increment = (double)100 / 7;
                    double current = increment;

                    App.StatusBar.IsProgressBarVisible = true;
                    App.StatusBar.ProgressBarValue = 0;
                    App.StatusBar.IsProgressBarInfinite = false;

                    RequestOptions ro = new RequestOptions { Count = 25 };
                    stopwatch.Start();
                    App.StatusBar.InfoText = App.ResMan.GetString("DownVnInfo");
                    var visualNovel = await GetVisualNovelAsync(client, vnId);
                    current += increment;
                    App.StatusBar.ProgressBarValue = current;

                    App.StatusBar.InfoText = App.ResMan.GetString("DownReleasesInfo");
                    var releases = await GetReleasesAsync(client, vnId, ro);
                    current += increment;
                    App.StatusBar.ProgressBarValue = current;

                    uint[] producerIds = releases.SelectMany(x => x.Producers.Select(y => y.Id)).Distinct().ToArray();
                    App.StatusBar.InfoText = App.ResMan.GetString("DownProducersInfo");
                    var producers = await GetProducersAsync(client, producerIds, ro);
                    current += increment;
                    App.StatusBar.ProgressBarValue = current;

                    App.StatusBar.InfoText = App.ResMan.GetString("DownCharacterInfo");
                    var characters = await GetCharactersAsync(client, vnId, ro);
                    current += increment;
                    App.StatusBar.ProgressBarValue = current;

                    uint[] staffIds = visualNovel.Staff.Select(x => x.StaffId).Distinct().ToArray();
                    App.StatusBar.InfoText = App.ResMan.GetString("DownStaffInfo");
                    var staff = await GetStaffAsync(client, staffIds, ro);
                    current += increment;
                    App.StatusBar.ProgressBarValue = current;

                    stopwatch.Stop();
                    stopwatch.Reset();

                    
                    if(_didErrorOccur)
                    {
                        App.Logger.Error("Failed to get all of the Vndb Info from the API, one of the items was null");
                        //stop the progressbar here, and force it to show an error icon
                        App.StatusBar.IsWorking = false;
                        App.StatusBar.InfoText = "";
                    }
                    else
                    {
                        //run code to add info to database
                        
                        await SaveVnDataToDb.SortVnInfoAsync(visualNovel, releases, producers, characters, staff, current);
                    }

                    

                }
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "An error occurred when trying to get the vndb data from the API");
                StatusBarViewModel.ResetValues();
                throw;
            }
        }

        internal async Task<VisualNovel> GetVisualNovelAsync(VndbSharp.Vndb client, uint vnid)
        {
            stopwatch.Restart();
            while (true)
            {
                if (stopwatch.Elapsed > maxTime)
                {
                    return null;
                }
                VndbResponse<VisualNovel> visualNovels = await client.GetVisualNovelAsync(VndbFilters.Id.Equals(vnid), VndbFlags.FullVisualNovel);


                switch (visualNovels)
                {
                    case null when client.GetLastError().Type == ErrorType.Throttled:
                        await HandleVndbErrors.ThrottledWaitAsync((ThrottledError)client.GetLastError(), 0);
                        break;
                    case null:
                        HandleVndbErrors.HandleErrors(client.GetLastError());
                        _didErrorOccur = true;
                        return null;
                    default:
                        return visualNovels.First();
                }
                
            }
        }

        internal async Task<List<Release>> GetReleasesAsync(VndbSharp.Vndb client, uint vnid, RequestOptions ro)
        {
            stopwatch.Restart();
            bool hasMore = true;
            int pageCount = 1;
            int releasesCount = 0;
            List<Release> releaseList = new List<Release>();
            bool shouldContinue = true;
            while (hasMore && shouldContinue)
            {
                shouldContinue = true;
                ro.Page = pageCount;
                VndbResponse<Release> releases = await client.GetReleaseAsync(VndbFilters.VisualNovel.Equals(vnid), VndbFlags.FullRelease, ro);

                switch (releases)
                {
                    case null when client.GetLastError().Type == ErrorType.Throttled:
                        await HandleVndbErrors.ThrottledWaitAsync((ThrottledError)client.GetLastError(), 0);
                        break;
                    case null:
                        HandleVndbErrors.HandleErrors(client.GetLastError());
                        _didErrorOccur = true;
                        return null;
                    default:
                    {
                        shouldContinue = false;
                        hasMore = releases.HasMore;
                        releaseList.AddRange(releases.Items);
                        releasesCount = releasesCount + releases.Count;
                        pageCount++;
                        if (stopwatch.Elapsed > maxTime)
                        {
                            return null;
                        }
                        break;
                    }
                }
                
            }
            return releaseList;

        }

        internal async Task<List<Character>> GetCharactersAsync(VndbSharp.Vndb client, uint vnid, RequestOptions ro)
        {
            stopwatch.Restart();
            bool hasMore = true;
            int pageCount = 1;
            int characterCount = 0;
            List<Character> characterList = new List<Character>();
            bool shouldContinue = true;
            while (hasMore && shouldContinue)
            {
                shouldContinue = true;
                ro.Page = pageCount;
                VndbResponse<Character> characters = await client.GetCharacterAsync(VndbFilters.VisualNovel.Equals(vnid), VndbFlags.FullCharacter, ro);

                switch (characters)
                {
                    case null when client.GetLastError().Type == ErrorType.Throttled:
                        await HandleVndbErrors.ThrottledWaitAsync((ThrottledError)client.GetLastError(), 0);
                        break;
                    case null:
                        HandleVndbErrors.HandleErrors(client.GetLastError());
                        _didErrorOccur = true;
                        return null;
                    default:
                    {
                        shouldContinue = false;
                        hasMore = characters.HasMore;
                        characterList.AddRange(characters.Items);
                        characterCount = characterCount + characters.Count;
                        pageCount++;
                        if (stopwatch.Elapsed > maxTime)
                        {
                            return null;
                        }
                        break;
                    }
                }
                
            }
            return characterList;
        }

        internal async Task<List<Producer>> GetProducersAsync(VndbSharp.Vndb client, uint[] producerIdList, RequestOptions ro)
        {
            stopwatch.Restart();
            bool hasMore = true;
            int pageCount = 1;
            int producerCount = 0;
            List<Producer> producerList = new List<Producer>();
            bool shouldContinue = true;
            while (hasMore && shouldContinue)
            {
                shouldContinue = true;
                ro.Page = pageCount;
                var producers = await client.GetProducerAsync(VndbFilters.Id.Equals(producerIdList), VndbFlags.FullProducer, ro);

                if (producers == null && client.GetLastError().Type == ErrorType.Throttled)
                {
                    await HandleVndbErrors.ThrottledWaitAsync((ThrottledError)client.GetLastError(), 0);
                }
                else if (producers == null)
                {
                    HandleVndbErrors.HandleErrors(client.GetLastError());
                    _didErrorOccur = true;
                    return null;
                }
                else
                {
                    shouldContinue = false;
                    hasMore = producers.HasMore;
                    producerList.AddRange(producers.Items);
                    producerCount = producerCount + producers.Count;
                    pageCount++;
                    if (stopwatch.Elapsed > maxTime)
                    {
                        return null;
                    }
                }
                
            }
            return producerList;
        }

        internal async Task<List<Staff>> GetStaffAsync(VndbSharp.Vndb client, uint[] staffId, RequestOptions ro)
        {
            stopwatch.Restart();
            bool hasMore = true;
            int pageCount = 1;
            int staffCount = 0;
            List<Staff> staffList = new List<Staff>();
            bool shouldContinue = true;
            while (hasMore && shouldContinue)
            {
                shouldContinue = true;
                ro.Page = pageCount;
                VndbResponse<Staff> staff = await client.GetStaffAsync(VndbFilters.Id.Equals(staffId), VndbFlags.FullStaff, ro);

                switch (staff)
                {
                    case null when client.GetLastError().Type == ErrorType.Throttled:
                        await HandleVndbErrors.ThrottledWaitAsync((ThrottledError)client.GetLastError(), 0);
                        break;
                    case null:
                        HandleVndbErrors.HandleErrors(client.GetLastError());
                        _didErrorOccur = true;
                        return null;
                    default:
                    {
                        shouldContinue = false;
                        hasMore = staff.HasMore;
                        staffList.AddRange(staff.Items);
                        staffCount = staffCount + staff.Count;
                        pageCount++;
                        if (stopwatch.Elapsed > maxTime)
                        {
                            return null;
                        }
                        break;
                    }
                }
                
            }
            return staffList;

        }
        
    }
}
