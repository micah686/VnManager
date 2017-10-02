using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using VisualNovelManagerv2.CustomClasses.TinyClasses;
using VisualNovelManagerv2.EF.Context;
using VisualNovelManagerv2.EF.Entity.VnUserList;
using VndbSharp;
using VndbSharp.Models;

namespace VisualNovelManagerv2.ViewModel.VisualNovels.VnListViewModel
{
    public partial class VnListViewModel
    {
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
                            Globals.StatusBar.ProgressText = "Loading Image";
                            Thread.Sleep(500);
                            WebClient webclient = new WebClient();
                            await webclient.DownloadFileTaskAsync(new Uri(data.Items[0].Image), $@"{Globals.DirectoryPath}\Data\images\userlist\{id}.jpg");
                            webclient.Dispose();
                            VnLinksModel.Image = new BitmapImage(new Uri($@"{Globals.DirectoryPath}\Data\images\userlist\{id}.jpg"));
                            Globals.StatusBar.IsDownloading = false;
                            Globals.StatusBar.ProgressText = String.Empty;
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
    }
}
