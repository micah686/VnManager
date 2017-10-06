using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmValidation;
using VisualNovelManagerv2.CustomClasses;
using VisualNovelManagerv2.CustomClasses.TinyClasses;
using VisualNovelManagerv2.CustomClasses.Vndb;
using VndbSharp;
using VndbSharp.Models.Common;

namespace VisualNovelManagerv2.ViewModel.VisualNovels.VnListViewModel
{
    //Set on Vndb Code
    public partial class VnListViewModel
    {
        public ICommand UpdateCommand => new GalaSoft.MvvmLight.Command.RelayCommand(SetUpdateData);


        private async void SetVoteList()
        {
            try
            {
                bool didErrorOccur = false;
                IsUserInputEnabled = false;
                if (VoteDropDownSelected == "No Change")
                {
                    return;
                }
                if (_vnId == 0)
                {
                    return;
                }
                using (Vndb client = new Vndb(Username, Password))
                {
                    var check = await client.GetDatabaseStatsAsync();
                    if (check == null)
                    {
                        HandleError.HandleErrors(client.GetLastError(), 0);
                        didErrorOccur = true;
                    }
                    if (didErrorOccur == false)
                    {
                        if (VoteDropDownSelected == "Clear Entry")
                        {
                            if (_vnId > 0)
                            {
                                await client.SetVoteListAsync(_vnId, null);
                            }
                        }
                        if (VoteDropDownSelected == "Add/Update Vote")
                        {
                            SetValidationRules();
                            Validator.ResultChanged += OnValidationResultChanged;
                            await ValidateAsync();
                            if (IsValid == true)
                            {
                                await client.SetVoteListAsync(_vnId, Convert.ToByte(VotelistVote.Replace(".", String.Empty)));
                            }
                        }

                    }
                }
                IsUserInputEnabled = true;
            }
            catch (Exception exception)
            {
                Globals.Logger.Error(exception);
                throw;
            }
        }

        private async void SetVnList()
        {
            try
            {
                bool didErrorOccur = false;
                IsUserInputEnabled = false;
                if (VnListStatus == "No Change")
                {
                    return;
                }
                if (_vnId == 0)
                {
                    return;
                }
                using (Vndb client = new Vndb(Username, Password))
                {
                    var check = await client.GetDatabaseStatsAsync();
                    if (check == null)
                    {
                        HandleError.HandleErrors(client.GetLastError(), 0);
                        didErrorOccur = true;
                    }
                    if (didErrorOccur == false)
                    {
                        switch (VnListStatus)
                        {
                            case "Clear Entry":
                                if (_vnId > 0 && NoteEnabled == true && string.IsNullOrEmpty(VnListNote))
                                {
                                    await client.SetVisualNovelListAsync(_vnId, null, null);
                                }
                                if (_vnId > 0)
                                {
                                    await client.SetVisualNovelListAsync(_vnId, (Status?)null);
                                }
                                break;
                            case "Playing":
                                await client.SetVisualNovelListAsync(_vnId, Status.Playing);
                                break;
                            case "Finished":
                                await client.SetVisualNovelListAsync(_vnId, Status.Finished);
                                break;
                            case "Stalled":
                                await client.SetVisualNovelListAsync(_vnId, Status.Stalled);
                                break;
                            case "Dropped":
                                await client.SetVisualNovelListAsync(_vnId, Status.Dropped);
                                break;
                            case "Unknown":
                                await client.SetVisualNovelListAsync(_vnId, Status.Unknown);
                                break;
                        }
                        if (NoteEnabled == true && !string.IsNullOrEmpty(VnListNote))
                        {
                            await client.SetVisualNovelListAsync(_vnId, VnListNote);
                        }
                    }
                }
                IsUserInputEnabled = true;
            }
            catch (Exception exception)
            {
                Globals.Logger.Error(exception);
                throw;
            }
        }

        private async void SetWishlist()
        {
            try
            {
                bool didErrorOccur = false;
                IsUserInputEnabled = false;
                if (WishlistPriority == "No Change")
                {
                    return;
                }
                if (_vnId == 0)
                {
                    return;
                }
                using (Vndb client = new Vndb(Username, Password))
                {
                    var check = await client.GetDatabaseStatsAsync();
                    if (check == null)
                    {
                        HandleError.HandleErrors(client.GetLastError(), 0);
                        didErrorOccur = true;
                    }
                    if (didErrorOccur == false)
                    {
                        if (WishlistPriority == "Clear Entry")
                        {
                            if (_vnId > 0)
                            {
                                await client.SetWishlistAsync(_vnId, null);
                            }
                        }
                        switch (WishlistPriority)
                        {
                            case "Clear Entry":
                                await client.SetWishlistAsync(_vnId, null);
                                break;
                            case "High":
                                await client.SetWishlistAsync(_vnId, Priority.High);
                                break;
                            case "Medium":
                                await client.SetWishlistAsync(_vnId, Priority.Medium);
                                break;
                            case "Low":
                                await client.SetWishlistAsync(_vnId, Priority.Low);
                                break;
                            case "Blacklist":
                                await client.SetWishlistAsync(_vnId, Priority.Blacklist);
                                break;
                        }
                    }
                }
                IsUserInputEnabled = true;
            }
            catch (Exception exception)
            {
                Globals.Logger.Error(exception);
                throw;
            }
        }

        private void SetUpdateData()
        {
            if (ConnectionTest.VndbTcpSocketTest() == false)
            {
                Globals.Logger.Warn("Could not connect to Vndb API over SSL");
                return;
            }
            SetVoteList();
            SetVnList();
            SetWishlist();
        }

        #region Validation
        private void SetValidationRules()
        {
            Validator.AddRule(nameof(VotelistVote),
                () =>
                {
                    //matches 10 or 1-9, or 1.1 up to 9.9
                    Regex regex = new Regex(@"^(10|[1-9]{1,2}){1}(\.[0-9]{1,2})?$");
                    return RuleResult.Assert(regex.IsMatch(VotelistVote), "Not a valid vote");
                });
        }        
        #endregion
    }
}
