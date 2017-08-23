using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using FirstFloor.ModernUI.Presentation;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.EntityFrameworkCore;
using MvvmValidation;
using VisualNovelManagerv2.Converters;
using VisualNovelManagerv2.CustomClasses;
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

namespace VisualNovelManagerv2.ViewModel.VisualNovels
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
            AddVotelistToDb(voteListItems);

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
                                if (wishlist != null && wishlist.Count ==0)
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
                AddWishlistToDb(wishListItems);

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
                            if (vnList != null && vnList.Count == 0)
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
            AddVnListToDb(visualNovelListItems);

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
                data = context.Set<VnInfo>().Where(v => v.Title == SelectedItem).Select(x => x.VnId).FirstOrDefault();
                if (data != 0)
                {
                    return data;
                }
                if (data == 0)
                {
                    data = context.Set<VnIdList>().Where(v => v.Title == SelectedItem).Select(x => x.VnId)
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

        private void AddWishlistToDb(List<VnWishList> wishlistItems)
        {
            using (var context = new DatabaseContext())
            {
                //cheap way to make sure database is up to date
                //TODO: Fix this hack
#warning This is a hack, removing all items from the db, then adding them again. Maybe later, I can find out how to update records without creating aa new record
                context.VnWishList.RemoveRange(context.VnWishList.Where(x=>x.UserId.Equals(_userId)));

                context.AddRange(wishlistItems);
                context.SaveChanges();
            }
        }

        private void AddVotelistToDb(List<VnVoteList> votelistItems)
        {
            using (var context = new DatabaseContext())
            {
                //cheap way to make sure database is up to date
                //TODO: Fix this hack
#warning This is a hack, removing all items from the db, then adding them again. Maybe later, I can find out how to update records without creating aa new record
                context.VnVoteList.RemoveRange(context.VnVoteList.Where(x => x.UserId.Equals(_userId)));

                context.AddRange(votelistItems);
                context.SaveChanges();
            }
        }

        private void AddVnListToDb(List<VnVisualNovelList> vnlistItems)
        {
            using (var context = new DatabaseContext())
            {
                //cheap way to make sure database is up to date
                //TODO: Fix this hack
#warning This is a hack, removing all items from the db, then adding them again. Maybe later, I can find out how to update records without creating aa new record
                context.VnVisualNovelList.RemoveRange(context.VnVisualNovelList.Where(x => x.UserId.Equals(_userId)));

                context.AddRange(vnlistItems);
                context.SaveChanges();
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

    //Set on Vndb Code
    public partial class VnListViewModel
    {
        public ICommand UpdateCommand => new GalaSoft.MvvmLight.Command.RelayCommand(SetUpdateData);


        private async void SetVoteList()
        {
            try
            {
                bool didErrorOccur = false;
                if (VoteDropDownSelected == "No Change")
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
                                var test = Convert.ToByte(VotelistVote.Replace(".", String.Empty));
                                await client.SetVoteListAsync(_vnId, Convert.ToByte(VotelistVote.Replace(".", String.Empty)));
                            }
                        }

                    }
                }
            }
            catch (Exception exception)
            {
                DebugLogging.WriteDebugLog(exception);
                throw;
            }            
        }

        private async void SetVnList()
        {
            try
            {
                bool didErrorOccur = false;
                if (VnListStatus == "No Change")
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
            }
            catch (Exception exception)
            {
                DebugLogging.WriteDebugLog(exception);
                throw;
            }            
        }

        private async void SetWishlist()
        {
            try
            {
                bool didErrorOccur = false;
                if (WishlistPriority == "No Change")
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
            }
            catch (Exception exception)
            {
                DebugLogging.WriteDebugLog(exception);
                throw;
            }            
        }

        private void SetUpdateData()
        {
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

        private async Task ValidateAsync()
        {
            ValidationResult result = await Validator.ValidateAllAsync();

            UpdateValidationSummary(result);
        }
        private void OnValidationResultChanged(object sender, ValidationResultChangedEventArgs e)
        {
            if (!IsValid.GetValueOrDefault(true))
            {
                ValidationResult validationResult = Validator.GetResult();
                Debug.WriteLine(" validation updated: " + validationResult);
                UpdateValidationSummary(validationResult);
            }
        }
        private void UpdateValidationSummary(ValidationResult validationResult)
        {
            IsValid = validationResult.IsValid;
            ValidationErrorsString = validationResult.ToString();
        }

        #endregion
    }

    //Properties
    public partial class VnListViewModel
    {
        #region VnLinksModel
        private VnLinksModel _vnLinksModel;
        public VnLinksModel VnLinksModel
        {
            get { return _vnLinksModel; }
            set
            {
                _vnLinksModel = value;
                RaisePropertyChanged(nameof(VnLinksModel));
            }
        }
        #endregion

        //groups
        #region Collections
        #region UserListCollection
        private RangeEnabledObservableCollection<string> _userListCollection = new RangeEnabledObservableCollection<string>();
        public RangeEnabledObservableCollection<string> UserListCollection
        {
            get { return _userListCollection; }
            set
            {
                _userListCollection = value;
                RaisePropertyChanged(nameof(UserListCollection));
            }
        }
        #endregion

        #region VoteCollection
        private ObservableCollection<string> _voteCollection = new ObservableCollection<string>();
        public ObservableCollection<string> VoteCollection
        {
            get { return _voteCollection; }
            set
            {
                _voteCollection = value;
                RaisePropertyChanged(nameof(VoteCollection));
            }
        }
        #endregion

        #region VnListCollection
        private ObservableCollection<string> _vnListCollection = new ObservableCollection<string>();
        public ObservableCollection<string> VnListCollection
        {
            get { return _vnListCollection; }
            set
            {
                _vnListCollection = value;
                RaisePropertyChanged(nameof(VnListCollection));
            }
        }
        #endregion

        #region WishlistCollection
        private ObservableCollection<string> _wishlistCollection = new ObservableCollection<string>();
        public ObservableCollection<string> WishlistCollection
        {
            get { return _wishlistCollection; }
            set
            {
                _wishlistCollection = value;
                RaisePropertyChanged(nameof(WishlistCollection));
            }
        }
        #endregion
        #endregion Collections

        #region Information
        #region InfoStatus
        private string _infoStatus;
        public string InfoStatus
        {
            get { return _infoStatus; }
            set
            {
                _infoStatus = value;
                RaisePropertyChanged(nameof(InfoStatus));
            }
        }
        #endregion

        #region InfoAdded
        private string _infoAdded;
        public string InfoAdded
        {
            get { return _infoAdded; }
            set
            {
                _infoAdded = value;
                RaisePropertyChanged(nameof(InfoAdded));
            }
        }
        #endregion

        #region InfoVote
        private double _infoVote;
        public double InfoVote
        {
            get { return _infoVote; }
            set
            {
                _infoVote = value;
                RaisePropertyChanged(nameof(InfoVote));
            }
        }
        #endregion

        #region InfoNote
        private string _infoNote;
        public string InfoNote
        {
            get { return _infoNote; }
            set
            {
                _infoNote = value;
                RaisePropertyChanged(nameof(InfoNote));
            }
        }
        #endregion

        #region InfoPriority
        private string _infoPriority;
        public string InfoPriority
        {
            get { return _infoPriority; }
            set
            {
                _infoPriority = value;
                RaisePropertyChanged(nameof(InfoPriority));
            }
        }
        #endregion
        #endregion Information

        #region Set Command Properties
        #region VoteDropDownSelected
        private string _voteDropDownSelected;
        public string VoteDropDownSelected
        {
            get { return _voteDropDownSelected; }
            set
            {
                _voteDropDownSelected = value;
                RaisePropertyChanged(nameof(VoteDropDownSelected));
            }
        }
        #endregion

        #region VotelistVote
        private string _votelistVote;
        public string VotelistVote
        {
            get { return _votelistVote; }
            set
            {
                _votelistVote = value;
                RaisePropertyChanged(nameof(VotelistVote));
            }
        }
        #endregion

        #region VnList Status
        private string _vnlistStatus;
        public string VnListStatus
        {
            get { return _vnlistStatus; }
            set
            {
                _vnlistStatus = value;
                RaisePropertyChanged(nameof(VnListStatus));
            }
        }
        #endregion

        #region NoteEnabled
        private bool _noteEnabled;
        public bool NoteEnabled
        {
            get { return _noteEnabled; }
            set
            {
                _noteEnabled = value;
                RaisePropertyChanged(nameof(NoteEnabled));
            }
        }
        #endregion

        #region VnList Note
        private string _vnListNote;
        public string VnListNote
        {
            get { return _vnListNote; }
            set
            {
                _vnListNote = value;
                RaisePropertyChanged(nameof(VnListNote));
            }
        }
        #endregion

        #region WishlistPriority
        private string _wishlistPriority;
        public string WishlistPriority
        {
            get { return _wishlistPriority; }
            set
            {
                _wishlistPriority = value;
                RaisePropertyChanged(nameof(WishlistPriority));
            }
        }
        #endregion
        #endregion Set Command Properties

        #region Validation Properties
        #region ValidationErrorsString
        private string _validationErrorsString;
        public string ValidationErrorsString
        {
            get { return _validationErrorsString; }
            private set
            {
                _validationErrorsString = value;
                RaisePropertyChanged(nameof(ValidationErrorsString));
            }
        }
        #endregion

        #region IsValid
        private bool? _isValid;
        public bool? IsValid
        {
            get { return _isValid; }
            private set
            {
                _isValid = value;
                RaisePropertyChanged(nameof(IsValid));
            }
        }
        #endregion
        #endregion Validation Properties
        //end groups

        #region Username
        private string _username;
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                RaisePropertyChanged(nameof(Username));
            }
        }
        #endregion

        #region Password
        private SecureString _password = new SecureString();
        public SecureString Password
        {
            get { return _password; }
            set
            {
                _password = value;
                RaisePropertyChanged(nameof(Password));
            }
        }
        #endregion

        #region SelectedItem
        private string _selectedItem;
        public string SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                RaisePropertyChanged(nameof(SelectedItem));
                if (value != null)
                {
                    BindImage();
                    if (IsVoteListSelected)
                        BindVoteData();
                    else if (IsVnListSelected)
                        BindVnList();
                    else if (IsWishlistSelected)
                        BindWishList();
                }
            }
        }
        #endregion

        #region IsVoteListSelected

        private bool _isVoteListSelected = false;
        public bool IsVoteListSelected
        {
            get { return _isVoteListSelected; }
            set
            {
                _isVoteListSelected = value;
                RaisePropertyChanged(nameof(IsVoteListSelected));
            }
        }
        #endregion

        #region IsWishlistSelected
        private bool _isWishlistSelected;
        public bool IsWishlistSelected
        {
            get { return _isWishlistSelected; }
            set
            {
                _isWishlistSelected = value;
                RaisePropertyChanged(nameof(IsWishlistSelected));
            }
        }
        #endregion

        #region IsVnListSelected
        private bool _isVnListSelected;
        public bool IsVnListSelected
        {
            get { return _isVnListSelected; }
            set
            {
                _isVnListSelected = value;
                RaisePropertyChanged(nameof(IsVnListSelected));
            }
        }
        #endregion

        #region IsUserInputEnabled
        private bool _isUserInputEnabled = true;
        public bool IsUserInputEnabled
        {
            get { return _isUserInputEnabled; }
            set
            {
                _isUserInputEnabled = value;
                RaisePropertyChanged(nameof(IsUserInputEnabled));
            }
        }
        #endregion

        private uint _userId = 0;
        private uint _vnId = 0;

    }

}
