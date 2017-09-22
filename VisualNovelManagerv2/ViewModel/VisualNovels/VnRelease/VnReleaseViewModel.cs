using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using VisualNovelManagerv2.Converters;
using VisualNovelManagerv2.CustomClasses;
using VisualNovelManagerv2.CustomClasses.TinyClasses;
using VisualNovelManagerv2.Design.VisualNovel;
using VisualNovelManagerv2.EF.Context;
using VisualNovelManagerv2.EF.Entity.VnRelease;

// ReSharper disable ExplicitCallerInfoArgument

namespace VisualNovelManagerv2.ViewModel.VisualNovels.VnRelease
{
    public partial class VnReleaseViewModel: ViewModelBase
    {
        public VnReleaseViewModel()
        {
        }

        

        private void LoadReleaseNameList()
        {
            ReleaseNameCollection.Clear();
            try
            {
                using (var db = new DatabaseContext())
                {
                    foreach (EF.Entity.VnRelease.VnRelease release in db.Set<EF.Entity.VnRelease.VnRelease>().Where(x=>x.VnId == Globals.VnId))
                    {
                        _releaseNameCollection.Add(release.Title);
                    }
                    db.Dispose();
                }
            }

            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
        }

        private void LoadReleaseData()
        {
            int releaseId = 0;
            try
            {
                if (SelectedReleaseIndex < 0) return;
                _releaseLanguages.Clear();
                _releasePlatforms.Clear();
               
                using (var db = new DatabaseContext())
                {
                    int index = (SelectedReleaseIndex + 1);
                    int count = 1;
                    foreach (EF.Entity.VnRelease.VnRelease release in db.Set<EF.Entity.VnRelease.VnRelease>().Where(x=>x.VnId==Globals.VnId))
                    {
                        if (count != index)
                        {
                            count++;
                        }
                        else
                        {
                            releaseId = Convert.ToInt32(release.ReleaseId);

                            VnReleaseModel.Title = release.Title;
                            VnReleaseModel.OriginalTitle = release.Original;
                            VnReleaseModel.Released = release.Released;
                            VnReleaseModel.ReleaseType = release.ReleaseType;
                            VnReleaseModel.Patch = release.Patch;
                            VnReleaseModel.Freeware = release.Freeware;
                            VnReleaseModel.Doujin = release.Doujin;
                            VnReleaseModel.Website = release.Website;
                            VnReleaseModel.Notes = ConvertTextBBcode.ConvertText(release.Notes);
                            VnReleaseModel.MinAge = release.MinAge;
                            if (release.Gtin != null) VnReleaseModel.Gtin = Convert.ToUInt64(release.Gtin);
                            VnReleaseModel.Catalog = release.Catalog;
                            VnReleaseModel.Resolution = release.Resolution;
                            switch (release.Voiced)
                            {
                                case "0":
                                    VnReleaseModel.Voiced = String.Empty;
                                    return;
                                case "Not":
                                    VnReleaseModel.Voiced = $"Not Voiced";
                                    break;
                                case "Ero":
                                    VnReleaseModel.Voiced = $"Ero Scenes Only";
                                    break;
                                case "Partial":
                                    VnReleaseModel.Voiced = $"Partially Voiced";
                                    break;
                                case "Full":
                                    VnReleaseModel.Voiced = $"Fully Voiced";
                                    break;
                                default:
                                    break;
                            }
                            //TODO: see if I can make this more effiecient
                            string[] animation = release.Animation.Split(',');
                            string[] animationStrings = new string[2];
                            switch (animation[0])
                            {
                                case "0":
                                    break;
                                case "Not":
                                    animationStrings[0] = "Story: Not Animated";
                                    break;
                                case "Simple":
                                    animationStrings[0] = "Story: Simple Animations";
                                    break;
                                case "Partial":
                                    animationStrings[0] = "Story: Partial Animations";
                                    break;
                                case "Full":
                                    animationStrings[0] = "Story: Full Animations";
                                    break;
                                default:
                                    break;
                            }
                            switch (animation[1])
                            {
                                case "0":
                                    break;
                                case "Not":
                                    animationStrings[1] = "Ero Scenes: Not Animated";
                                    break;
                                case "Simple":
                                    animationStrings[1] = "Ero Scenes: Simple Animations";
                                    break;
                                case "Partial":
                                    animationStrings[1] = "Ero Scenes: Partial Animations";
                                    break;
                                case "Full":
                                    animationStrings[1] = "Ero Scenes: Full Animations";
                                    break;
                                default:
                                    break;
                            }
                            VnReleaseModel.Animation =string.Join(", ", animationStrings.Where(s => !string.IsNullOrEmpty(s)));

                            foreach (string language in GetLangauges(release.Languages))
                            {
                                if (language != null)
                                {
                                    _releaseLanguages.Add(new ReleaseLanguagesCollection
                                    {
                                        VnReleaseModel =
                                            new VnReleaseModel {Languages = new BitmapImage(new Uri(language))}
                                    });
                                }
                            }

                            foreach (string platform in GetPlatforms(release.Platforms))
                            {
                                if (platform != null)
                                {
                                    _releasePlatforms.Add(new ReleasePlatformsCollection
                                    {
                                        VnReleaseModel = new VnReleaseModel
                                        {
                                            Platforms = new BitmapImage(new Uri(platform))
                                        }
                                    });
                                }
                            }
                            break;
                        }
                    }
                    db.Dispose();
                }
            }
            catch (System.IndexOutOfRangeException ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
            
            LoadReleaseProducerData(releaseId);            
        }

        private void LoadReleaseProducerData(int releaseId)
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    foreach (VnReleaseProducers release in db.Set<VnReleaseProducers>().Where(x=>x.ReleaseId==releaseId))
                    {
                        VnReleaseProducerModel.IsDeveloper = release.Developer;
                        VnReleaseProducerModel.IsPublisher = release.Publisher;
                        VnReleaseProducerModel.Name = release.Name;
                        VnReleaseProducerModel.OriginalName = release.Original;

                        switch (release.ProducerType)
                        {
                            case "co":
                                VnReleaseProducerModel.Type = "Company";
                                break;
                            case "in":
                                VnReleaseProducerModel.Type = "Individual";
                                break;
                            case "ng":
                                VnReleaseProducerModel.Type = "Amateur group";
                                break;
                            default:
                                VnReleaseProducerModel.Type = release.ProducerType;
                                break;
                        }
                    }
                    db.Dispose();
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
            catch (Exception ex)
            {
                DebugLogging.WriteDebugLog(ex);
                throw;
            }
            
        }

        private static IEnumerable<string> GetLangauges(string csv)
        {
            string[] list = csv.Split(',');
            return list.Select(lang => File.Exists($@"{Globals.DirectoryPath}\Data\res\icons\country_flags\{lang}.png")
                    ? $@"{Globals.DirectoryPath}\Data\res\icons\country_flags\{lang}.png"
                    : $@"{Globals.DirectoryPath}\Data\res\icons\country_flags\Unknown.png")
                .ToList();
        }

        private static IEnumerable<string> GetPlatforms(string csv)
        {
            string[] list = csv.Split(',');
            return list.Select(plat => File.Exists($@"{Globals.DirectoryPath}\Data\res\icons\platforms\{plat}.png")
                    ? $@"{Globals.DirectoryPath}\Data\res\icons\platforms\{plat}.png"
                    : $@"{Globals.DirectoryPath}\Data\res\icons\platforms\Unknown.png")
                .ToList();
        }
    }

    public class ReleaseLanguagesCollection
    {
        public VnReleaseModel VnReleaseModel { get; set; }
    }

    public class ReleasePlatformsCollection
    {
        public VnReleaseModel VnReleaseModel { get; set; }
    }
}
