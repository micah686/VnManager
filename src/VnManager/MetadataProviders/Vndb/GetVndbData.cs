// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Sentry;
using VndbSharp;
using VndbSharp.Models;
using VndbSharp.Models.Character;
using VndbSharp.Models.Errors;
using VndbSharp.Models.VisualNovel;
using VnManager.Helpers.Vndb;
using VnManager.ViewModels;
using VnManager.ViewModels.UserControls;

namespace VnManager.MetadataProviders.Vndb
{
    public class GetVndbData
    {
        private readonly Stopwatch stopwatch = new Stopwatch();
        private readonly TimeSpan maxTime = TimeSpan.FromMinutes(3);
        private bool _didErrorOccur = false;
        
        /// <summary>
        /// Download main Vndb Data
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="isRepairing"></param>
        /// <returns></returns>
        public async Task GetDataAsync(int gameId, bool isRepairing)
        {
            uint vnId = (uint)gameId;
            try
            {
                const int max = 100;
                const int countWithTagTrait = 9;
                const int countWithoutTagTrait = 7;
                double increment;
                if (isRepairing || !App.DidDownloadTagTraitDump)
                {
                    increment = (double) max / countWithTagTrait;
                }
                else
                {
                    increment = (double) max / countWithoutTagTrait;
                }
                
                using (var client = new VndbSharp.Vndb(true))
                {
                    RootViewModel.StatusBarPage.IsWorking = true;
                    RootViewModel.StatusBarPage.StatusString = App.ResMan.GetString("Working");
                    double current = increment;

                    RootViewModel.StatusBarPage.IsProgressBarVisible = true;
                    RootViewModel.StatusBarPage.ProgressBarValue = 0;
                    RootViewModel.StatusBarPage.IsProgressBarInfinite = false;

                    RequestOptions ro = new RequestOptions { Count = 25 };
                    stopwatch.Start();
                    RootViewModel.StatusBarPage.InfoText = App.ResMan.GetString("DownVnInfo");
                    var visualNovel = await GetVisualNovelAsync(client, vnId);
                    current += increment;
                    RootViewModel.StatusBarPage.ProgressBarValue = current;



                    RootViewModel.StatusBarPage.InfoText = App.ResMan.GetString("DownCharacterInfo");
                    var characters = await GetCharactersAsync(client, vnId, ro);
                    current += increment;
                    RootViewModel.StatusBarPage.ProgressBarValue = current;

                    stopwatch.Stop();
                    stopwatch.Reset();

                    
                    if(_didErrorOccur)
                    {
                        App.Logger.Error("Failed to get all of the Vndb Info from the API, one of the items was null");
                        //stop the progressbar here, and force it to show an error icon
                        RootViewModel.StatusBarPage.IsWorking = false;
                        RootViewModel.StatusBarPage.InfoText = "";
                    }
                    else
                    {
                        //run code to add info to database
                        
                        await SaveVnDataToDb.SortVnInfoAsync(visualNovel, characters, increment, current, isRepairing);
                    }

                    

                }
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "An error occurred when trying to get the vndb data from the API");
                SentrySdk.CaptureException(ex);
                StatusBarViewModel.ResetValues();
                throw;
            }
        }

        /// <summary>
        /// Get Visual Novel data
        /// </summary>
        /// <param name="client"></param>
        /// <param name="vnid"></param>
        /// <returns></returns>
        internal async Task<VisualNovel> GetVisualNovelAsync(VndbSharp.Vndb client, uint vnid)
        {
            try
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
            catch (Exception e)
            {
                App.Logger.Warning(e, "Failed to Get Visual novel");
                SentrySdk.CaptureException(e);
                return null;
            }
        }

        /// <summary>
        /// Get character data
        /// </summary>
        /// <param name="client"></param>
        /// <param name="vnid"></param>
        /// <param name="ro"></param>
        /// <returns></returns>
        internal async Task<List<Character>> GetCharactersAsync(VndbSharp.Vndb client, uint vnid, RequestOptions ro)
        {
            try
            {
                stopwatch.Restart();

                int pageCount = 1;
                bool shouldContinue = true;
                List<Character> characterList = new List<Character>();
                while (shouldContinue)
                {
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
                            shouldContinue = characters.HasMore; //When false, it will exit the while loop
                            characterList.AddRange(characters.Items);
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
            catch (Exception e)
            {
                App.Logger.Warning(e, "Failed to Get Visual novel characters");
                SentrySdk.CaptureException(e);
                return null;
            }
        }

    }
}
