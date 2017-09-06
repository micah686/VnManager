using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualNovelManagerv2.CustomClasses;
using VisualNovelManagerv2.EF.Context;
using VisualNovelManagerv2.EF.Entity.VnInfo;
using VisualNovelManagerv2.EF.Entity.VnOther;
using VndbSharp.Models;
using VndbSharp.Models.Character;
using VndbSharp.Models.Release;
using VndbSharp.Models.VisualNovel;

namespace VisualNovelManagerv2.ViewModel.VisualNovels.AddVn
{
    public partial class AddVnViewModel
    {
        private async Task AddToDatabase(VndbResponse<VisualNovel> visualNovels, List<Release> releases, List<Character> characters)
        {
            try
            {
                using (var context = new DatabaseContext())
                {
                    VisualNovel vn = visualNovels.FirstOrDefault();
                    if (vn != null)
                    {
                        context.VnInfo.Add(new VnInfo
                        {
                            VnId = vn.Id,
                            Title = vn.Name,
                            Original = vn.OriginalName,
                            Released = vn.Released?.ToString() ?? null,
                            Languages = ConvertToCsv(vn.Languages),
                            OriginalLanguage = ConvertToCsv(vn.OriginalLanguages),
                            Platforms = ConvertToCsv(vn.Platforms),
                            Aliases = ConvertToCsv(vn.Aliases),
                            Length = vn.Length?.ToString(),
                            Description = vn.Description,
                            ImageLink = vn.Image,
                            ImageNsfw = vn.IsImageNsfw.ToString(),
                            Popularity = vn.Popularity,
                            Rating = (int)vn.Rating

                        });
                    }


                    #region UserData
                    context.VnUserData.Add(new VnUserData
                    {
                        VnId = _vnid,
                        ExePath = FileName,
                        IconPath = IconName,
                        LastPlayed = String.Empty,
                        PlayTime = "0,0,0,0"
                    });
                    #endregion
                }
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
        }

        string ConvertToCsv(ReadOnlyCollection<string> input)
        {
            return input != null ? string.Join(",", input) : null;
        }
    }
}
