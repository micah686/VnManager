using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VisualNovelManagerv2.CustomClasses.TinyClasses;
using VisualNovelManagerv2.EF.Context;
using VisualNovelManagerv2.EF.Entity.VnOther;
using VisualNovelManagerv2.EF.Entity.VnUserList;

namespace VisualNovelManagerv2.ViewModel.VisualNovels.VnListViewModel
{
    public partial class VnListViewModel
    {
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
                if (wishlistItems.Count != efList.Count && wishlistItems.Count > 0)
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
                        if (onlineVoteList.Count > 0)
                        {
                            vn.Vote = onlineVoteList[counter].Vote;
                            vn.Added = onlineVoteList[counter].Added;
                            counter++;
                        }
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
