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
                //_userId = 7887;

                if (IsVoteListSelected)
                    GetVoteList();
                else if (IsVnListSelected)
                    GetVisualNovelList();
                else if (IsWishlistSelected)
                    GetWishlist();
            }
            else
            {
                
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
        
        private async void BindImage()
        {
            try
            {
                using (Vndb client = new Vndb())
                {
                    var data = await client.GetVisualNovelAsync(VndbFilters.Title.Equals(SelectedItem), VndbFlags.Details);
                    if (data != null)
                    {
                        var id = data.Items[0].Id;
                        if (!File.Exists($@"{Globals.DirectoryPath}\Data\images\userlist\{id}.jpg"))
                        {
                            Globals.StatusBar.IsDownloading = true;
                            Thread.Sleep(1500);
                            WebClient webclient = new WebClient();
                            webclient.DownloadFile(new Uri(data.Items[0].Image), $@"{Globals.DirectoryPath}\Data\images\userlist\{id}.jpg");
                            webclient.Dispose();
                            VnLinksModel.Image = new BitmapImage(new Uri($@"{Globals.DirectoryPath}\Data\images\userlist\{id}.jpg"));
                            Globals.StatusBar.IsDownloading = false;
                        }
                        else if (File.Exists($@"{Globals.DirectoryPath}\Data\images\userlist\{id}.jpg"))
                        {
                            VnLinksModel.Image = new BitmapImage(new Uri($@"{Globals.DirectoryPath}\Data\images\userlist\{id}.jpg"));
                        }
                    }                    
                    client.Dispose();
                }
            }
            catch (Exception e)
            {
                DebugLogging.WriteDebugLog(e);
                throw;
            }
        }

        private async void BindVoteData()
        {
            _vnId = await GetVnId();
            using (var context = new DatabaseContext())
            {
                IQueryable<VnVoteList> entry = from v in context.VnVoteList
                    where v.UserId.Equals(_userId)
                    where v.VnId.Equals(_vnId)
                    select v;
                var data = entry.FirstOrDefault();
                if (data != null)
                {
                    InfoVote = data.Vote;
                    InfoAdded = data.Added;
                }
                
            }
        }

        private async void BindVnList()
        {
            _vnId = await GetVnId();
            using (var context = new DatabaseContext())
            {
                IQueryable<VnVisualNovelList> entry = from v in context.VnVisualNovelList
                    where v.UserId.Equals(_userId)
                    where v.VnId.Equals(_vnId)
                    select v;
                var data = entry.FirstOrDefault();
                if (data != null)
                {
                    InfoStatus = data.Status;
                    InfoNote = data.Notes;
                    InfoAdded = data.Added;
                }

            }
        }

        private async void BindWishList()
        {
            _vnId = await GetVnId();
            using (var context = new DatabaseContext())
            {
                IQueryable<VnWishList> entry = from v in context.VnWishList
                    where v.UserId.Equals(_userId)
                    where v.VnId.Equals(_vnId)
                    select v;
                var data = entry.FirstOrDefault();
                if (data != null)
                {
                    InfoPriority = data.Priority;                   
                    InfoAdded = data.Added;
                }

            }
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

        private void AddWishlistToDb(List<VnWishList> wishlistItems, bool removeItems)
        {
            try
            {
                List<VnWishList> efList;
                //for removing items no longer on the user's wishlist
                using (var context = new DatabaseContext())
                {
                    IQueryable<VnWishList> rtn = from temp in context.VnWishList select temp;
                    efList = rtn.ToList();

                    efList.RemoveAll(x => x.UserId != _userId);
                    //gets a list of all ids from the wishlistItems
                    List<uint> vnIdList = wishlistItems.Select(item => item.VnId).ToList();
                    //prepares EF to remove any items where the EF does not contain an item from the wishlistItems
                    if (removeItems)
                    {
                        context.VnWishList.RemoveRange(efList.Where(item => !vnIdList.Contains(item.VnId)));
                        context.SaveChanges();
                    }
                    
                }

                //begin section for modifying data
                List<VnWishList> onlineWishList = new List<VnWishList>();
                List<VnWishList> localWishList;

                using (var context = new DatabaseContext())
                {
                    localWishList = (from first in context.VnWishList
                                     join second in wishlistItems on first.VnId equals second.VnId
                                     select first).Where(x => !wishlistItems.Any(y => y.Priority == x.Priority && y.Added == x.Added))
                        .ToList();
                }


                if (efList.Count > 0)
                {
                    //gets the modified values from online, which is the wishListItems parameter
                    onlineWishList = (from first in wishlistItems join second in efList on first.VnId equals second.VnId select first)
                        .Where(x => !efList.Any(y => y.Priority == x.Priority && y.Added == x.Added)).ToList();

                    //sets each of the entries to the new value
                    int counter = 0;
                    foreach (var wish in localWishList)
                    {
                        wish.Priority = onlineWishList[counter].Priority;
                        wish.Added = onlineWishList[counter].Added;
                        counter++;
                    }
                }


                //updates each of the entries
                using (var context = new DatabaseContext())
                {
                    foreach (var wish in localWishList)
                    {
                        context.Entry(wish).State = EntityState.Modified;
                    }
                    context.SaveChanges();
                }


                List<VnWishList> addWishlistItem = wishlistItems;
                //prevents adding duplicates
                addWishlistItem.RemoveAll(x => efList.Any(y => y.VnId == x.VnId));
                if (wishlistItems.Count != efList.Count && wishlistItems.Count >0)
                {                    
                    addWishlistItem.RemoveAll(item => onlineWishList.Contains(item) && efList.Contains(item));
                    using (var context = new DatabaseContext())
                    {
                        context.VnWishList.AddRange(addWishlistItem);
                        context.SaveChanges();
                    }
                }                

            }
            catch (Exception e)
            {
                DebugLogging.WriteDebugLog(e);
                throw;
            }

        }

        private void AddVotelistToDb(List<VnVoteList> vnVoteListItems, bool removeItems)
        {
            try
            {
                List<VnVoteList> efList;
                //for removing items no longer on the user's vnVotelist
                using (var context = new DatabaseContext())
                {
                    IQueryable<VnVoteList> rtn = from temp in context.VnVoteList select temp;
                    efList = rtn.ToList();

                    efList.RemoveAll(x => x.UserId != _userId);
                    //gets a list of all ids from the vnvoteListItems
                    List<uint> vnIdList = vnVoteListItems.Select(item => item.VnId).ToList();
                    //prepares EF to remove any items where the EF does not contain an item from the vnVoteListItems
                    if (removeItems)
                    {
                        context.VnVoteList.RemoveRange(efList.Where(item => !vnIdList.Contains(item.VnId)));
                        context.SaveChanges();
                    }

                }

                //begin section for modifying data
                List<VnVoteList> onlineVoteList = new List<VnVoteList>();
                List<VnVoteList> localVoteList;

                using (var context = new DatabaseContext())
                {
                    localVoteList = (from first in context.VnVoteList
                                   join second in vnVoteListItems on first.VnId equals second.VnId
                                   select first).Where(x => !vnVoteListItems.Any(y => y.Vote == x.Vote && y.Added == x.Added))
                        .ToList();
                }


                if (efList.Count > 0)
                {
                    //gets the modified values from online, which is the vnVoteListItems parameter
                    onlineVoteList = (from first in vnVoteListItems join second in efList on first.VnId equals second.VnId select first)
                        .Where(x => !efList.Any(y => y.Vote == x.Vote && y.Added == x.Added)).ToList();

                    //sets each of the entries to the new value
                    int counter = 0;
                    foreach (var vn in localVoteList)
                    {
                        vn.Vote = onlineVoteList[counter].Vote;
                        vn.Added = onlineVoteList[counter].Added;
                        counter++;
                    }
                }


                //updates each of the entries
                using (var context = new DatabaseContext())
                {
                    foreach (var vote in localVoteList)
                    {
                        context.Entry(vote).State = EntityState.Modified;
                    }
                    context.SaveChanges();
                }


                List<VnVoteList> addVnVoteListItem = vnVoteListItems;
                //prevents adding duplicates
                addVnVoteListItem.RemoveAll(x => efList.Any(y => y.VnId == x.VnId));
                if (vnVoteListItems.Count != efList.Count && vnVoteListItems.Count > 0)
                {
                    addVnVoteListItem.RemoveAll(item => onlineVoteList.Contains(item) && efList.Contains(item));
                    using (var context = new DatabaseContext())
                    {
                        context.VnVoteList.AddRange(addVnVoteListItem);
                        context.SaveChanges();
                    }
                }

            }
            catch (Exception e)
            {
                DebugLogging.WriteDebugLog(e);
                throw;
            }
        }

        private void AddVnListToDb(List<VnVisualNovelList> vnListItems, bool removeItems)
        {
            try
            {
                List<VnVisualNovelList> efList;
                //for removing items no longer on the user's vnlist
                using (var context = new DatabaseContext())
                {
                    IQueryable<VnVisualNovelList> rtn = from temp in context.VnVisualNovelList select temp;
                    efList = rtn.ToList();

                    efList.RemoveAll(x => x.UserId != _userId);
                    //gets a list of all ids from the vnListItems
                    List<uint> vnIdList = vnListItems.Select(item => item.VnId).ToList();
                    //prepares EF to remove any items where the EF does not contain an item from the vnListItems
                    if (removeItems)
                    {
                        context.VnVisualNovelList.RemoveRange(efList.Where(item => !vnIdList.Contains(item.VnId)));
                        context.SaveChanges();
                    }

                }

                //begin section for modifying data
                List<VnVisualNovelList> onlineVnList = new List<VnVisualNovelList>();
                List<VnVisualNovelList> localVnList;

                using (var context = new DatabaseContext())
                {
                    localVnList = (from first in context.VnVisualNovelList
                                     join second in vnListItems on first.VnId equals second.VnId
                                     select first).Where(x => !vnListItems.Any(y => y.Status == x.Status && y.Added == x.Added && y.Notes == x.Notes))
                        .ToList();
                }


                if (efList.Count > 0)
                {
                    //gets the modified values from online, which is the vnListItems parameter
                    onlineVnList = (from first in vnListItems join second in efList on first.VnId equals second.VnId select first)
                        .Where(x => !efList.Any(y => y.Status == x.Status && y.Added == x.Added && y.Notes == x.Notes)).ToList();

                    //sets each of the entries to the new value
                    int counter = 0;
                    foreach (var vn in localVnList)
                    {
                        vn.Status = onlineVnList[counter].Status;
                        vn.Added = onlineVnList[counter].Added;
                        vn.Notes = onlineVnList[counter].Notes;
                        counter++;
                    }
                }


                //updates each of the entries
                using (var context = new DatabaseContext())
                {
                    foreach (var vn in localVnList)
                    {
                        context.Entry(vn).State = EntityState.Modified;
                    }
                    context.SaveChanges();
                }


                List<VnVisualNovelList> addVnListItem = vnListItems;
                //prevents adding duplicates
                addVnListItem.RemoveAll(x => efList.Any(y => y.VnId == x.VnId));
                if (vnListItems.Count != efList.Count && vnListItems.Count > 0)
                {
                    addVnListItem.RemoveAll(item => onlineVnList.Contains(item) && efList.Contains(item));
                    using (var context = new DatabaseContext())
                    {
                        context.VnVisualNovelList.AddRange(addVnListItem);
                        context.SaveChanges();
                    }
                }

            }
            catch (Exception e)
            {
                DebugLogging.WriteDebugLog(e);
                throw;
            }
        }

        private void AddToIdListDb(List<Tuple<uint, string>> itemsToAdd)
        {
            using (var context = new DatabaseContext())
            {
                //List<VnIdList> vnData = new List<VnIdList>();
                //var idlist = context.VnIdList.Select(x => x.VnId);
                //foreach (var item in itemsToAdd)
                //{
                //    if (!idlist.Contains(item.Item1))
                //    {
                //        vnData.Add(new VnIdList()
                //        {
                //            VnId = item.Item1,
                //            Title = item.Item2
                //        });
                //    }
                //}

                //add items that aren't in the VnIdList to the table
                var idlist = context.VnIdList.Select(x => x.VnId);
                List<VnIdList> vnData = (from item in itemsToAdd
                    where !idlist.Contains(item.Item1)
                    select new VnIdList()
                    {
                        VnId = item.Item1,
                        Title = item.Item2
                    }).ToList();
                context.AddRange(vnData);
                context.SaveChanges();
            }
        }
    }

    

}
