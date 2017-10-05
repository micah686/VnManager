using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;
using VisualNovelManagerv2.CustomClasses;
using VisualNovelManagerv2.CustomClasses.TinyClasses;
using VisualNovelManagerv2.EF.Context;
using VisualNovelManagerv2.EF.Entity.VnOther;

namespace VisualNovelManagerv2.ViewModel.VisualNovels.VnMain
{
    //class for context menu
    public partial class VnMainViewModel
    {
        private void CreateContextMenu()
        {
            if (Globals.VnId > 0)
            {
                var contextMenu = new ContextMenu();
                contextMenu.Items.Add(CreateAddSubMenu("Add To Category"));
                contextMenu.Items.Add(CreateRemoveSubMenu("Remove From Category"));
                contextMenu.Items.Add(new MenuItem { Header = "Delete Vn", Command = DeleteVnCommand });
                contextMenu.IsOpen = true;
            }
            
        }

        private MenuItem CreateAddSubMenu(string header)
        {
            var item = new MenuItem { Header = header };

            using (var context = new DatabaseContext())
            {
                //get a list of all category names that are linked to the selected Vn
                var data = context.VnUserCategoryTitles.Where(x => x.VnId == Globals.VnId).Select(x => x.Title).ToList();
                foreach (var categories in context.Categories)
                {
                    //prevents adding to All or the category currently loaded, or any categories already added
                    if (categories.CategoryName != "All"  && !data.Contains(categories.CategoryName))
                    {
                        item.Items.Add(new MenuItem { Header = categories.CategoryName, Command = AddToCategoryCommand, CommandParameter = categories.CategoryName });
                    }
                }
            }
            return item;
        }

        private MenuItem CreateRemoveSubMenu(string header)
        {
            var item = new MenuItem { Header = header };
            using (var context = new DatabaseContext())
            {
                var data = context.Categories.Where(cat => context.VnUserCategoryTitles
                .Where(x => x.VnId == Globals.VnId).Select(x => x.Title)
                        .Contains(cat.CategoryName)).ToArray();
                foreach (var categories in data)
                {
                    if (categories.CategoryName != "All")
                    {
                        item.Items.Add(new MenuItem { Header = categories.CategoryName, Command = RemoveFromCategoryCommand, CommandParameter = categories.CategoryName });
                    }
                }

            }
            return item;
        }

        private void AddToCategory(string header)
        {
            try
            {
                using (var context = new DatabaseContext())
                {
                    if (!string.IsNullOrEmpty(header))
                    {
                        var categoryEntry = new VnUserCategoryTitle{Title = header, VnId = Globals.VnId};
                        context.VnUserCategoryTitles.Add(categoryEntry);
                        context.SaveChanges();

                        var mvm = ServiceLocator.Current.GetInstance<VnMainViewModel>();
                        mvm.LoadCategoriesPublic();
                    }
                }
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex);
                throw;
            }

        }


        private void RemoveFromCategory(string header)
        {
            try
            {
                using (var context = new DatabaseContext())
                {
                    if (!string.IsNullOrEmpty(header))
                    {
                        var data = context.VnUserCategoryTitles.Where(x => x.Title == header && x.VnId == Globals.VnId)
                            .ToArray();
                        if (data.Length>0)
                        {
                            context.VnUserCategoryTitles.RemoveRange(data);
                            context.SaveChanges();

                            var mvm = ServiceLocator.Current.GetInstance<VnMainViewModel>();
                            mvm.LoadCategoriesPublic();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex);
                throw;
            }
        }

        private void DeleteVn()
        {
            try
            {
                var msg = new NotificationMessageAction<MessageBoxResult>(this, "Delete Vn Confirm", (r) =>
                {
                    if (r == MessageBoxResult.Yes)
                    {
                        // do stuff
                        try
                        {

                            if (Directory.Exists($@"{Globals.DirectoryPath}\Data\images\screenshots\{Globals.VnId}"))
                            {
                                Directory.Delete($@"{Globals.DirectoryPath}\Data\images\screenshots\{Globals.VnId}", true);
                            }
                            if (Directory.Exists($@"{Globals.DirectoryPath}\Data\images\characters\{Globals.VnId}"))
                            {
                                Directory.Delete($@"{Globals.DirectoryPath}\Data\images\characters\{Globals.VnId}", true);
                            }
                            if (File.Exists($@"{Globals.DirectoryPath}\Data\images\cover\{Globals.VnId}.jpg"))
                            {
                                File.Delete($@"{Globals.DirectoryPath}\Data\images\cover\{Globals.VnId}.jpg");
                            }
                            else if (File.Exists($@"{Globals.DirectoryPath}\Data\images\cover\{Globals.VnId}"))
                            {
                                File.Delete($@"{Globals.DirectoryPath}\Data\images\cover\{Globals.VnId}");
                            }
                            using (var context = new DatabaseContext())
                            {
                                context.VnCharacter.RemoveRange(context.VnCharacter.Where(x => x.VnId == Convert.ToUInt32(Globals.VnId)));
                                context.VnCharacterVns.RemoveRange(context.VnCharacterVns.Where(x => x.VnId == Globals.VnId));
                                context.VnInfo.RemoveRange(context.VnInfo.Where(x => x.VnId == Globals.VnId));
                                context.VnInfoAnime.RemoveRange(context.VnInfoAnime.Where(x => x.VnId == Globals.VnId));
                                context.VnInfoLinks.RemoveRange(context.VnInfoLinks.Where(x => x.VnId == Globals.VnId));
                                context.VnInfoRelations.RemoveRange(context.VnInfoRelations.Where(x => x.VnId == Globals.VnId));
                                context.VnInfoScreens.RemoveRange(context.VnInfoScreens.Where(x => x.VnId == Globals.VnId));
                                context.VnInfoStaff.RemoveRange(context.VnInfoStaff.Where(x => x.VnId == Globals.VnId));
                                context.VnInfoTags.RemoveRange(context.VnInfoTags.Where(x => x.VnId == Globals.VnId));
                                context.VnRelease.RemoveRange(context.VnRelease.Where(x => x.VnId == Globals.VnId));

                                context.VnReleaseVn.RemoveRange(context.VnReleaseVn.Where(x => x.VnId == Globals.VnId));
                                context.VnUserData.RemoveRange(context.VnUserData.Where(x => x.VnId == Globals.VnId));


                                context.SaveChanges();
                            }
                            Globals.VnId = 0;
                            _selectedVn = string.Empty;
                            ClearCollectionsCommand.Execute(null);
                            LoadBindVnDataCommand.Execute(null);
                        }
                        catch (Exception exception)
                        {
                            //TODO: Figure out why in the same session that you add a game and it downloads screenshots, it prevents you from deleting the files
                            Globals.Logger.Error(exception);
                            throw;
                        }
                    }
                });

                Messenger.Default.Send(msg);
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex);
                throw;
            }                     
        }

    }
}
