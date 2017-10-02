using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.EntityFrameworkCore;
using MvvmValidation;
using VisualNovelManagerv2.CustomClasses;
using VisualNovelManagerv2.CustomClasses.TinyClasses;
using VisualNovelManagerv2.CustomClasses.Vndb;
using VisualNovelManagerv2.Design.VisualNovel;
using VisualNovelManagerv2.EF.Context;
using VisualNovelManagerv2.EF.Entity.VnInfo;
using VisualNovelManagerv2.EF.Entity.VnOther;
using VisualNovelManagerv2.EF.Entity.VnUserList;
using VisualNovelManagerv2.Infrastructure;
using VndbSharp;
using VndbSharp.Models;
using VndbSharp.Models.Common;
using VndbSharp.Models.User;
using VndbSharp.Models.VisualNovel;
using ValidationResult = MvvmValidation.ValidationResult;
// ReSharper disable ExplicitCallerInfoArgument

namespace VisualNovelManagerv2.ViewModel.VisualNovels.VnListViewModel
{
    public partial class VnListViewModel: ValidatableViewModelBase
    {

        public ICommand LoginCommand => new GalaSoft.MvvmLight.Command.RelayCommand(Login);
        
        public VnListViewModel()
        {
            _vnLinksModel = new VnLinksModel();

            VoteCollection.Add("No Change");
            VoteCollection.Add("Clear Entry");
            VoteCollection.Add("Add/Update Vote");

            VnListCollection.Add("No Change");
            VnListCollection.Add("Clear Entry");            
            VnListCollection.Add("Playing");
            VnListCollection.Add("Finished");
            VnListCollection.Add("Stalled");
            VnListCollection.Add("Dropped");
            VnListCollection.Add("Unknown");

            WishlistCollection.Add("No Change");
            WishlistCollection.Add("Clear Entry");
            WishlistCollection.Add("High");
            WishlistCollection.Add("Medium");
            WishlistCollection.Add("Low");
            WishlistCollection.Add("Blacklist");
        }
        
        private async void Login()
        {
            IsUserInputEnabled = false;
            bool didErrorOccur = false;
            using (Vndb client = new Vndb(Username, Password))
            {
                var users = await client.GetUserAsync(VndbFilters.Username.Equals(Username));
                if (users != null)
                {
                    _userId = users.Items[0].Id;
                }
                if (users == null)
                {
                    HandleError.HandleErrors(client.GetLastError(), 0);
                    didErrorOccur = true;
                    IsUserInputEnabled = true;
                }
            }
            if (didErrorOccur != true)
            {
                _userListCollection.Clear();
                //Set a userID here for testing
                //_userId = 7887;

                if (IsVoteListSelected)
                    GetVoteList();
                else if (IsVnListSelected)
                    GetVisualNovelList();
                else if (IsWishlistSelected)
                    GetWishlist();
            }
        }

        private async void GetVoteList()
        {
            if (string.IsNullOrEmpty(_username) || _password.Length <= 0) return;
            Globals.StatusBar.IsDbProcessing = true;
            Globals.StatusBar.IsWorkProcessing = true;
            Globals.StatusBar.ProgressText = "Loading Data...";
            UInt32[] dbIdList = { };
            using (var context = new DatabaseContext())
            {
                //gets a list of all vnids that are in the VnVoteList where the userId is the logged in user
                List<uint> idEntry = (from v in context.VnVoteList where v.UserId.Equals(_userId) select v.VnId).ToList();
                List<uint> idList = idEntry.ToList();

                //gets a list of titles of all items in VnIdList which contain an id from the above vnlist,
                //which is any item in the votelist table
                List<string> entry = (from first in context.VnIdList
                    join second in idList on first.VnId equals second
                    select first.Title).ToList();

                if (entry.Count > 0)
                    _userListCollection.InsertRange(entry);
                if (idList.Count > 0)
                {
                    dbIdList = idList.ToArray();
                }
            }
            List<Tuple<uint, string>> dbItemsToAdd = new List<Tuple<uint, string>>();
            List<VnVoteList>voteListItems = new List<VnVoteList>();
            bool removeItems = false;
            using (Vndb client = new Vndb(Username, Password))
            {
                bool hasMore = true;
                RequestOptions ro = new RequestOptions();
                int page = 1;
                List<UInt32> idList = new List<uint>();
                //get the list of all ids on the votelist
                int errorCounter = 0;
                while (hasMore)
                {
                    ro.Page = page;
                    ro.Count = 100;
                    try
                    {
                        if (dbIdList.Length > 0)
                        {
                            var voteList = await client.GetVoteListAsync(VndbFilters.UserId.Equals(_userId) & VndbFilters.VisualNovel.NotEquals(dbIdList), VndbFlags.FullVotelist, ro);

                            if (voteList != null && voteList.Count > 0)
                            {
                                hasMore = voteList.HasMore;
                                idList.AddRange(voteList.Select(vote => vote.VisualNovelId));
                                voteListItems.AddRange(voteList.Select(item => new VnVoteList()
                                {
                                    UserId = item.UserId,
                                    VnId = item.VisualNovelId,
                                    Vote = item.Vote,
                                    Added = item.AddedOn.ToString(CultureInfo.InvariantCulture)
                                }));
                                page++;
                            }
                            if (voteList != null && voteList.Count == 0)
                            {
                                voteList = await client.GetVoteListAsync(VndbFilters.UserId.Equals(_userId), VndbFlags.FullVotelist, ro);
                                if (voteList != null)
                                {
                                    hasMore = voteList.HasMore;
                                    idList.AddRange(voteList.Select(vote => vote.VisualNovelId));
                                    voteListItems.AddRange(voteList.Select(item => new VnVoteList()
                                    {
                                        UserId = item.UserId,
                                        VnId = item.VisualNovelId,
                                        Vote = item.Vote,
                                        Added = item.AddedOn.ToString(CultureInfo.InvariantCulture)
                                    }));
                                    page++;
                                    removeItems = true;
                                }
                            }
                            else
                            {
                                HandleError.HandleErrors(client.GetLastError(), errorCounter);
                                errorCounter++;
                            }
                        }
                        else
                        {
                            var voteList = await client.GetVoteListAsync(VndbFilters.UserId.Equals(_userId), VndbFlags.FullVotelist, ro);
                            if (voteList != null)
                            {
                                hasMore = voteList.HasMore;
                                idList.AddRange(voteList.Select(wish => wish.VisualNovelId));
                                //dbWishlistToAdd.AddRange(votelist);
                                voteListItems.AddRange(voteList.Select(item => new VnVoteList()
                                {
                                    UserId = item.UserId,
                                    VnId = item.VisualNovelId,
                                    Vote = item.Vote,
                                    Added = item.AddedOn.ToString(CultureInfo.InvariantCulture)
                                }));
                                page++;
                                removeItems = true;
                            }
                            else
                            {
                                HandleError.HandleErrors(client.GetLastError(), errorCounter);
                                errorCounter++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        DebugLogging.WriteDebugLog(ex);
                        throw;
                    }
                                        
                }
                //get names from ids on votelist, and add them to ObservableCollection
                hasMore = true;
                page = 1;
                while (hasMore)
                {
                    ro.Count = 25;
                    ro.Page = page;
                    try
                    {
                        var data = await client.GetVisualNovelAsync(VndbFilters.Id.Equals(idList.ToArray()), VndbFlags.Basic, ro);
                        if (data != null)
                        {
                            hasMore = data.HasMore;
                            foreach (var item in data)
                            {
                                _userListCollection.Add(item.Name);
                                dbItemsToAdd.Add(new Tuple<uint, string>(item.Id, item.Name));
                            }
                            page++;
                        }
                        else
                        {
                            HandleError.HandleErrors(client.GetLastError(), errorCounter);
                            errorCounter++;
                        }
                    }
                    catch (Exception ex)
                    {
                        DebugLogging.WriteDebugLog(ex);
                        throw;
                    }                                        
                }
                client.Dispose();
            }
            AddToIdListDb(dbItemsToAdd);
            AddVotelistToDb(voteListItems, removeItems);

            Globals.StatusBar.ProgressText = "Done";
            Globals.StatusBar.ProgressStatus = new BitmapImage(new Uri($@"{Globals.DirectoryPath}\Data\res\icons\statusbar\ok.png"));
            Globals.StatusBar.IsDbProcessing = false;
            Globals.StatusBar.IsWorkProcessing = false;
            await Task.Delay(1500);
            Globals.StatusBar.ProgressText = string.Empty;
            Globals.StatusBar.ProgressStatus = null;
            IsUserInputEnabled = true;
        }

        private async void GetWishlist()
        {
            if (!string.IsNullOrEmpty(_username) && _password.Length > 0)
            {
                Globals.StatusBar.IsDbProcessing = true;
                Globals.StatusBar.IsWorkProcessing = true;
                Globals.StatusBar.ProgressText = "Loading Data...";
                UInt32[] dbIdList = { };
                using (var context = new DatabaseContext())
                {                   
                    //gets a list of all vnids that are in the VnWishlist where the userId is the logged in user
                    List<uint> idEntry = (from v in context.VnWishList where v.UserId.Equals(_userId) select v.VnId).ToList();
                    List<uint> idList= idEntry.ToList();

                    //gets a list of titles of all items in VnIdList which contain an id from the above vnlist,
                    //which is any item in the wishlist table
                    List<string> entry = (from first in context.VnIdList
                        join second in idList on first.VnId equals second
                        select first.Title).ToList();

                    if(entry.Count >0)
                        _userListCollection.InsertRange(entry);
                    if (idList.Count > 0)
                    {
                        dbIdList = idList.ToArray();
                    }
                }
                
                List<Tuple<uint, string>> dbItemsToAdd = new List<Tuple<uint, string>>();
                List<VnWishList> wishListItems= new List<VnWishList>();
                bool removeItems = false;
                using (Vndb client = new Vndb(Username, Password))
                {
                    bool hasMore = true;
                    RequestOptions ro = new RequestOptions();
                    int page = 1;
                    List<UInt32> idList = new List<uint>();
                    //get the list of all ids on the wishlist
                    int errorCounter = 0;
                    while (hasMore)
                    {
                        ro.Page = page;
                        ro.Count = 100;
                        try
                        {
                            if (dbIdList.Length > 0)
                            {
                                VndbResponse<Wishlist> wishlist = await client.GetWishlistAsync(VndbFilters.UserId.Equals(_userId) & VndbFilters.VisualNovel.NotEquals(dbIdList), VndbFlags.FullWishlist, ro);

                                if (wishlist != null && wishlist.Count >0)
                                {
                                    hasMore = wishlist.HasMore;
                                    idList.AddRange(wishlist.Select(wish => wish.VisualNovelId));
                                    //dbWishlistToAdd.AddRange(wishlist);
                                    wishListItems.AddRange(wishlist.Select(item => new VnWishList()
                                    {
                                        UserId = item.UserId,
                                        VnId = item.VisualNovelId,
                                        Priority = item.Priority.ToString(),
                                        Added = item.AddedOn.ToString(CultureInfo.InvariantCulture)
                                    }));
                                    page++;                                    
                                }
                                else if (wishlist != null && wishlist.Count == 0)
                                {
                                    wishlist = await client.GetWishlistAsync(VndbFilters.UserId.Equals(_userId), VndbFlags.FullWishlist, ro);
                                    if (wishlist != null)
                                    {
                                        hasMore = wishlist.HasMore;
                                        idList.AddRange(wishlist.Select(wish => wish.VisualNovelId));
                                        //dbWishlistToAdd.AddRange(wishlist);
                                        wishListItems.AddRange(wishlist.Select(item => new VnWishList()
                                        {
                                            UserId = item.UserId,
                                            VnId = item.VisualNovelId,
                                            Priority = item.Priority.ToString(),
                                            Added = item.AddedOn.ToString(CultureInfo.InvariantCulture)
                                        }));
                                        page++;
                                        removeItems = true;
                                    }
                                }
                                else
                                {
                                    HandleError.HandleErrors(client.GetLastError(), errorCounter);
                                    errorCounter++;
                                }
                            }
                            else
                            {
                                var wishlist = await client.GetWishlistAsync(VndbFilters.UserId.Equals(_userId), VndbFlags.FullWishlist, ro);
                                if (wishlist != null)
                                {
                                    hasMore = wishlist.HasMore;
                                    idList.AddRange(wishlist.Select(wish => wish.VisualNovelId));
                                    //dbWishlistToAdd.AddRange(wishlist);
                                    wishListItems.AddRange(wishlist.Select(item => new VnWishList()
                                    {
                                        UserId = item.UserId,
                                        VnId = item.VisualNovelId,
                                        Priority = item.Priority.ToString(),
                                        Added = item.AddedOn.ToString(CultureInfo.InvariantCulture)
                                    }));
                                    page++;
                                }
                                else
                                {
                                    HandleError.HandleErrors(client.GetLastError(), errorCounter);
                                    errorCounter++;
                                }
                            }
                                                        
                        }
                        catch (Exception ex)
                        {
                            DebugLogging.WriteDebugLog(ex);
                            throw;
                        }
                    }
                    //get names from ids on wishlist, and add them to ObservableCollection
                    hasMore = true;
                    page = 1;
                    while (hasMore)
                    {
                        ro.Page = page;
                        ro.Count = 25;
                        try
                        {
                            var data = await client.GetVisualNovelAsync(VndbFilters.Id.Equals(idList.ToArray()), VndbFlags.Basic, ro);
                            if (data != null)
                            {
                                hasMore = data.HasMore;
                                foreach (var item in data)
                                {
                                    _userListCollection.Add(item.Name);
                                    dbItemsToAdd.Add(new Tuple<uint, string>(item.Id, item.Name));
                                }
                                page++;
                            }
                            else
                            {
                                HandleError.HandleErrors(client.GetLastError(), errorCounter);
                                errorCounter++;
                            }
                        }
                        catch (Exception ex)
                        {
                            DebugLogging.WriteDebugLog(ex);
                            throw;
                        }
                    }
                    client.Dispose();
                }
                AddToIdListDb(dbItemsToAdd);
                AddWishlistToDb(wishListItems, removeItems);

                Globals.StatusBar.ProgressText = "Done";
                Globals.StatusBar.ProgressStatus = new BitmapImage(new Uri($@"{Globals.DirectoryPath}\Data\res\icons\statusbar\ok.png"));
                Globals.StatusBar.IsDbProcessing = false;
                Globals.StatusBar.IsWorkProcessing = false;
                await Task.Delay(1500);
                Globals.StatusBar.ProgressText = string.Empty;
                Globals.StatusBar.ProgressStatus = null;
                IsUserInputEnabled = true;
            }
        }

        private async void GetVisualNovelList()
        {
            if (string.IsNullOrEmpty(_username) || _password.Length <= 0) return;
            Globals.StatusBar.IsDbProcessing = true;
            Globals.StatusBar.IsWorkProcessing = true;
            Globals.StatusBar.ProgressText = "Loading Data...";

            UInt32[] dbIdList = { };
            using (var context = new DatabaseContext())
            {
                //gets a list of all vnids that are in the VnVisualNovelList where the userId is the logged in user
                List<uint> idEntry = (from v in context.VnVisualNovelList where v.UserId.Equals(_userId) select v.VnId).ToList();
                List<uint> idList = idEntry.ToList();

                //gets a list of titles of all items in VnIdList which contain an id from the above vnlist,
                //which is any item in the visualnovelList table
                List<string> entry = (from first in context.VnIdList
                    join second in idList on first.VnId equals second
                    select first.Title).ToList();

                if (entry.Count > 0)
                    _userListCollection.InsertRange(entry);
                if (idList.Count > 0)
                {
                    dbIdList = idList.ToArray();
                }
            }
            List<Tuple<uint, string>> dbItemsToAdd = new List<Tuple<uint, string>>();
            List<VnVisualNovelList> visualNovelListItems = new List<VnVisualNovelList>();
            bool removeItems = false;
            using (Vndb client = new Vndb(Username, Password))
            {
                bool hasMore = true;
                RequestOptions ro = new RequestOptions();
                int page = 1;
                List<UInt32> idList = new List<uint>();
                //get the list of all ids on the vnList
                int errorCounter = 0;
                //var vnList = await client.GetVisualNovelListAsync(VndbFilters.UserId.Equals(_userId), VndbFlags.FullVisualNovelList, ro);
                while (hasMore)
                {
                    ro.Page = page;
                    ro.Count = 100;
                    try
                    {
                        if (dbIdList.Length > 0)
                        {
                            var vnList = await client.GetVisualNovelListAsync(VndbFilters.UserId.Equals(_userId) & VndbFilters.VisualNovel.NotEquals(dbIdList), VndbFlags.FullVisualNovelList, ro);

                            if (vnList != null && vnList.Count > 0)
                            {
                                hasMore = vnList.HasMore;
                                idList.AddRange(vnList.Select(vn => vn.VisualNovelId));
                                visualNovelListItems.AddRange(vnList.Select(item => new VnVisualNovelList()
                                {
                                    UserId = item.UserId,
                                    VnId = item.VisualNovelId,
                                    Status = item.Status.ToString(),
                                    Notes = item.Notes,
                                    Added = item.AddedOn.ToString(CultureInfo.InvariantCulture)
                                }));
                                page++;
                            }
                            else if (vnList != null && vnList.Count == 0)
                            {
                                vnList = await client.GetVisualNovelListAsync(VndbFilters.UserId.Equals(_userId), VndbFlags.FullVisualNovelList, ro);
                                if (vnList != null)
                                {
                                    hasMore = vnList.HasMore;
                                    idList.AddRange(vnList.Select(vn => vn.VisualNovelId));
                                    visualNovelListItems.AddRange(vnList.Select(item => new VnVisualNovelList()
                                    {
                                        UserId = item.UserId,
                                        VnId = item.VisualNovelId,
                                        Status = item.Status.ToString(),
                                        Notes = item.Notes,
                                        Added = item.AddedOn.ToString(CultureInfo.InvariantCulture)
                                    }));
                                    page++;
                                    removeItems = true;
                                }
                            }
                            else
                            {
                                HandleError.HandleErrors(client.GetLastError(), errorCounter);
                                errorCounter++;
                            }
                        }
                        else
                        {
                            var vnList = await client.GetVisualNovelListAsync(VndbFilters.UserId.Equals(_userId), VndbFlags.FullVisualNovelList, ro);
                            if (vnList != null)
                            {
                                hasMore = vnList.HasMore;
                                idList.AddRange(vnList.Select(wish => wish.VisualNovelId));
                                //dbWishlistToAdd.AddRange(votelist);
                                visualNovelListItems.AddRange(vnList.Select(item => new VnVisualNovelList()
                                {
                                    UserId = item.UserId,
                                    VnId = item.VisualNovelId,
                                    Status = item.Status.ToString(),
                                    Notes = item.Notes,
                                    Added = item.AddedOn.ToString(CultureInfo.InvariantCulture)
                                }));
                                page++;
                                removeItems = true;
                            }
                            else
                            {
                                HandleError.HandleErrors(client.GetLastError(), errorCounter);
                                errorCounter++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        DebugLogging.WriteDebugLog(ex);
                        throw;
                    }

                }
                //get names from ids on vnlist, and add them to ObservableCollection
                hasMore = true;
                page = 1;
                while (hasMore)
                {
                    ro.Page = page;
                    ro.Count = 25;
                    try
                    {
                        var data = await client.GetVisualNovelAsync(VndbFilters.Id.Equals(idList.ToArray()), VndbFlags.Basic, ro);
                        if (data != null)
                        {
                            hasMore = data.HasMore;
                            foreach (var item in data)
                            {
                                _userListCollection.Add(item.Name);
                                dbItemsToAdd.Add(new Tuple<uint, string>(item.Id, item.Name));
                            }
                            page++;
                        }
                        else
                        {
                            HandleError.HandleErrors(client.GetLastError(), errorCounter);
                            errorCounter++;
                        }
                    }
                    catch (Exception ex)
                    {
                        DebugLogging.WriteDebugLog(ex);
                        throw;
                    }
                }
                client.Dispose();
            }
            AddToIdListDb(dbItemsToAdd);
            AddVnListToDb(visualNovelListItems, removeItems);

            Globals.StatusBar.ProgressText = "Done";
            Globals.StatusBar.ProgressStatus = new BitmapImage(new Uri($@"{Globals.DirectoryPath}\Data\res\icons\statusbar\ok.png"));
            Globals.StatusBar.IsDbProcessing = false;
            Globals.StatusBar.IsWorkProcessing = false;
            await Task.Delay(1500);
            Globals.StatusBar.ProgressText = string.Empty;
            Globals.StatusBar.ProgressStatus = null;
            IsUserInputEnabled = true;
        }                

        private async Task<uint> GetVnId()
        {
            using (var context = new DatabaseContext())
            {
                uint data = 0;
                data = context.VnInfo.Where(v => v.Title == SelectedItem).Select(x => x.VnId).FirstOrDefault();
                if (data != 0)
                {
                    return data;
                }
                if (data == 0)
                {
                    data = context.VnIdList.Where(v => v.Title == SelectedItem).Select(x => x.VnId)
                        .FirstOrDefault();
                    return data;
                }
                if(data == 0)
                {
                    Vndb client = new Vndb(true);
                    var response = await client.GetVisualNovelAsync(VndbFilters.Title.Equals(SelectedItem));
                    VisualNovel firstOrDefault = response?.Items.FirstOrDefault();
                    if (firstOrDefault != null)
                        data = firstOrDefault.Id;
                    client.Logout();
                    client.Dispose();
                    return data;
                }
                return 0;
            }
        }

        
    }

    

}
