using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Messaging;
using VisualNovelManagerv2.CustomClasses;
using VisualNovelManagerv2.EF.Context;
using VisualNovelManagerv2.EF.Entity.VnOther;

namespace VisualNovelManagerv2.ViewModel.VisualNovels.VnMain
{
    //class for context menu
    public partial class VnMainViewModel
    {
        private void CreateContextMenu()
        {
            var contextMenu = new ContextMenu();
            contextMenu.Items.Add(CreateAddSubMenu("Add To Category"));
            contextMenu.Items.Add(CreateRemoveSubMenu("Remove Category"));
            contextMenu.Items.Add(new MenuItem { Header = "Delete Vn", Command = DeleteVnCommand });
            contextMenu.Items.Add(new MenuItem { Header = "Item, disabled", IsEnabled = false });
            contextMenu.Items.Add(new MenuItem { Header = "Item, checked", IsChecked = true });
            contextMenu.Items.Add(new MenuItem { Header = "Item, checked and disabled", IsChecked = true, IsEnabled = false });
            contextMenu.Items.Add(new Separator());
            var menu = CreateAddSubMenu("Item with Submenu, disabled");
            contextMenu.Items.Add(menu);
            menu.IsEnabled = false;
            contextMenu.IsOpen = true;
        }

        private MenuItem CreateAddSubMenu(string header)
        {
            var item = new MenuItem { Header = header };

            using (var context = new DatabaseContext())
            {
                //get a list of all category names that are linked to the selected Vn
                List<string> data = context.VnUserCategoryTitles.Where(x => x.Title == SelectedVn)
                    .SelectMany(x => x.CategoryJunctions.Select(y => y.Category.CategoryName)).ToList();
                foreach (var categories in context.Set<Category>())
                {
                    //prevents adding to All or the category currently loaded, or any categories already added
                    if (categories.CategoryName != "All" && categories.CategoryName != _selectedCategory && !data.Contains(categories.CategoryName))
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
                List<Category> data = context.VnUserCategoryTitles.Where(x => x.Title == SelectedVn)
                    .SelectMany(x => x.CategoryJunctions.Select(y => y.Category)).ToList();
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
                        //create a relationship between the two members
                        var category = context.Categories.FirstOrDefault(x => x.CategoryName == header);
                        var vnUser = context.VnUserCategoryTitles.FirstOrDefault(v => v.Title == SelectedVn);
                        if (category != null && vnUser != null)
                        {
                            var addCategoryVnEntry = new CategoryJunction { Category = category, VnUserCategoryTitle = vnUser };
                            context.CategoryJunction.Add(addCategoryVnEntry);
                            context.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
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
                        CategoryJunction data = context.CategoryJunction
                            .FirstOrDefault(x => x.Category.CategoryName == header && x.VnUserCategoryTitle.Title == SelectedVn);
                        if (data != null)
                        {
                            context.CategoryJunction.Remove(data);
                            context.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
        }

        private void DeleteVn()
        {
            var msg = new NotificationMessageAction<MessageBoxResult>(this, "Delete Vn Confirm", (r) =>
            {
                if (r == MessageBoxResult.Yes)
                {
                    // do stuff
                    try
                    {
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
                            context.SaveChanges();
                        }
                        ClearCollectionsCommand.Execute(null);
                        LoadBindVnDataCommand.Execute(null);
                    }
                    catch (Exception exception)
                    {
                        DebugLogging.WriteDebugLog(exception);
                        throw;
                    }
                }
            });

            Messenger.Default.Send(msg);            
        }

    }
}
