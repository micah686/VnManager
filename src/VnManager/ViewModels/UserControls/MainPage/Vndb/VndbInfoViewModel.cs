// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using AdysTech.CredentialManager;
using LiteDB;
using Sentry;
using Stylet;
using VnManager.Extensions;
using VnManager.Helpers;
using VnManager.Helpers.Vndb;
using VnManager.Models.Db.User;
using VnManager.Models.Db.Vndb.Main;
using VnManager.Models.Db;

namespace VnManager.ViewModels.UserControls.MainPage.Vndb
{
    public class VndbInfoViewModel : Screen
    {
        #region Binding Properties
        public BitmapSource CoverImage { get; set; }
        public BitmapSource GameIcon { get; set; }
        public string Title { get; set; }
        public string MainTitle { get; set; }
        public string Aliases { get; set; }
        public string ReleasedDate { get; set; }
        public string VnLength { get; set; }
        public string Popularity { get; set; }
        public string Rating { get; set; }
        public BindableCollection<BitmapSource> LanguageCollection { get; } = new BindableCollection<BitmapSource>();
        public BindableCollection<TagTraitBinding> TagCollection { get; } = new BindableCollection<TagTraitBinding>();
        public Visibility SummaryHeaderVisibility { get; set; }
        public Visibility TagHeaderVisibility { get; set; }
        public Visibility RelationHeaderVisibility { get; set; }
        public Visibility RelationsDataVisibility { get; set; }
        public BindableCollection<VnRelationsBinding> VnRelations { get; } = new BindableCollection<VnRelationsBinding>();
        
        public Inline[] DescriptionInLine { get; private set; }

        public string LastPlayed { get; set; }
        public string PlayTime { get; set; }
        public Visibility IsStartButtonVisible { get; set; }

        public Tuple<string, Visibility> VndbLink { get; set; }
        public Tuple<string, Visibility> WikiLink { get; set; }
        

        #endregion


        /// <summary>
        /// Loads main data when the VnInfo view shows up
        /// </summary>
        protected override void OnViewLoaded()
        {
            VndbLink = new Tuple<string, Visibility>(string.Empty, Visibility.Visible);
            WikiLink = new Tuple<string, Visibility>(string.Empty, Visibility.Collapsed);
            LoadMainData();
            LoadUserData();
            LoadRelations();
            LoadLinks();
            
            TagCollection.Clear();
            TagCollection.AddRange(VndbTagTraitHelper.GetTags(VndbContentViewModel.VnId));
            SummaryHeaderVisibility = DescriptionInLine.Length < 1 ? Visibility.Collapsed : Visibility.Visible;
            TagHeaderVisibility = TagCollection.Count < 1 ? Visibility.Collapsed : Visibility.Visible;
            RelationHeaderVisibility = VnRelations.Count < 1 ? Visibility.Collapsed : Visibility.Visible;
            RelationsDataVisibility = VnRelations.Count < 1 ? Visibility.Collapsed : Visibility.Visible;
            IsStartButtonVisible = Visibility.Visible;
        }

        /// <summary>
        /// Get the main Vndb data
        /// </summary>
        private void LoadMainData()
        {
            try
            {
                if (VndbContentViewModel.VnId == 0)
                {
                    return;
                }
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1)
                {
                    return;
                }
                using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
                {
                    var vnInfoEntry = db.GetCollection<VnInfo>(DbVnInfo.VnInfo.ToString()).Query().Where(x => x.VnId == VndbContentViewModel.VnId).FirstOrDefault();
                    Title = vnInfoEntry.Title;
                    MainTitle = vnInfoEntry.Title;
                    Aliases = vnInfoEntry.Aliases;
                    ReleasedDate = TimeDateChanger.GetHumanDate(DateTime.Parse(vnInfoEntry.Released, CultureInfo.InvariantCulture));
                    VnLength = vnInfoEntry.Length;
                    Popularity = $"{vnInfoEntry.Popularity:F}";
                    Rating = $"{vnInfoEntry.Rating:F}";
                    LoadLanguages(ref vnInfoEntry);
                    var coverPath = $@"{App.AssetDirPath}\sources\vndb\images\cover\{vnInfoEntry.VnId}.jpg";
                    CoverImage = ImageHelper.CreateBitmapFromPath(coverPath);

                    DescriptionInLine = BBCodeHelper.Helper(vnInfoEntry.Description);

                }
            }
            catch (Exception e)
            {
                App.Logger.Warning(e, "Failed to load Main Vndb Data");
                SentrySdk.CaptureException(e);
            }
        }

        /// <summary>
        /// Load user data associated with the game
        /// </summary>
        private void LoadUserData()
        {
            try
            {
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1)
                {
                    return;
                }
                using var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}");
                var dbUserData = db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString()).Query()
                    .Where(x => x.Id == VndbContentViewModel.SelectedGame.Id).FirstOrDefault();
                if (dbUserData != null)
                {
                    LastPlayed = TimeDateChanger.GetHumanDate(dbUserData.LastPlayed);
                    PlayTime = TimeDateChanger.GetHumanTime(dbUserData.PlayTime);

                    GameIcon = ImageHelper.CreateIcon(!string.IsNullOrEmpty(dbUserData.IconPath) ? dbUserData.IconPath : dbUserData.ExePath);
                }
            }
            catch (Exception e)
            {
                App.Logger.Warning(e, "Failed to load Vndb userData");
                SentrySdk.CaptureException(e);
            }
        }
        
        /// <summary>
        /// Load Relation data
        /// </summary>
        private void LoadRelations()
        {
            var relations = VndbDataHelper.LoadRelations();
            VnRelations.AddRange(relations);
            RelationsDataVisibility = VnRelations.Count < 1 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void LoadLinks()
        {
            WikiLink = VndbDataHelper.LoadLinks();
        }

        /// <summary>
        /// Click event to the Vndb page
        /// <see cref="VndbLinkClick"/>
        /// </summary>
        public void VndbLinkClick()
        {
            var link = $"https://vndb.org/v{VndbContentViewModel.VnId}";
            var ps = new ProcessStartInfo(link)
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(ps);
        }

        /// <summary>
        /// Click event to the associated Wikipedia page
        /// <see cref="WikiLinkClick"/>
        /// </summary>
        public void WikiLinkClick()
        {
            if (WikiLink.Item2 == Visibility.Visible)
            {

                var link = $@"https://wikipedia.org/wiki/{WikiLink.Item1}";
                var ps = new ProcessStartInfo(link)
                {
                    UseShellExecute = true,
                    Verb = "open"
                };
                Process.Start(ps);
            }
        }
        
        /// <summary>
        /// Load the languages, and add them to the view
        /// </summary>
        /// <param name="vnInfoEntry"></param>
        private void LoadLanguages(ref VnInfo vnInfoEntry)
        {
            var langCollection = VndbDataHelper.LoadLanguages(ref vnInfoEntry);
            LanguageCollection.Clear();
            LanguageCollection.AddRange(langCollection);
        }

        /// <summary>
        /// Referenced by the Play button on the GUI
        /// <see cref="StartVn"/>
        /// </summary>
        public void StartVn()
        {
            try
            {
                var parent = (VndbContentViewModel)Parent;
                if (parent.IsGameRunning)
                {
                    return;
                }

                if (VndbContentViewModel.SelectedGame?.ExePath != null && File.Exists(VndbContentViewModel.SelectedGame.ExePath))
                {
                    var filePath = VndbContentViewModel.SelectedGame.ExePath;
                    Directory.SetCurrentDirectory(Path.GetDirectoryName(filePath));
                    var process = new Process { StartInfo = { FileName = filePath, Arguments = VndbContentViewModel.SelectedGame.Arguments }, EnableRaisingEvents = true };
                    parent.ProcessList.Add(process);
                    process.Exited += MainOrChildProcessExited;

                    process.Start();
                    parent.IsGameRunning = true;
                    parent.GameStopwatch.Start();
                    IsStartButtonVisible = Visibility.Collapsed;
                    parent.ProcessList.AddRange(process.GetChildProcesses());
                }
            }
            catch (Exception e)
            {
                App.Logger.Warning(e, "Failed to start the game");
                SentrySdk.CaptureException(e);
            }

        }

        /// <summary>
        /// Referenced by the Stop button on the GUI
        /// <see cref="StopVn"/>
        /// </summary>
        public void StopVn()
        {
            try
            {
                var parent = (VndbContentViewModel)Parent;
                if (!parent.GameStopwatch.IsRunning)
                {
                    return;
                }
                const int maxWaitTime = 30000; //30 seconds
                foreach (var process in parent.ProcessList)
                {
                    process.CloseMainWindow();
                    process.WaitForExit(maxWaitTime);
                }

                parent.ProcessList = parent.ProcessList.Where(x => !x.HasExited).ToList();
                foreach (var process in parent.ProcessList)
                {
                    process.Kill(true);
                }

                parent.IsGameRunning = false;
                parent.GameStopwatch.Reset();
                IsStartButtonVisible = Visibility.Visible;
                parent.ProcessList.Clear();
            }
            catch (Exception e)
            {
                App.Logger.Warning(e, "Failed to Stop the game");
                SentrySdk.CaptureException(e);
            }
        }

        private void MainOrChildProcessExited(object sender, EventArgs e)
        {
            try
            {
                var parent = (VndbContentViewModel) Parent;
                var process = (Process)sender;
                if (process == null)
                {
                    return;
                }
                var children = process.GetChildProcesses().ToArray();
                if (children.Length > 0)
                {
                    foreach (var childProcess in children)
                    {
                        childProcess.EnableRaisingEvents = true;
                        childProcess.Exited += MainOrChildProcessExited;
                    }
                    parent.ProcessList.AddRange(children);

                    parent.ProcessList = parent.ProcessList.Where(x => x.HasExited == false).ToList();
                }
                else
                {
                    parent.GameStopwatch.Stop();
                    var cred = CredentialManager.GetCredentials(App.CredDb);
                    if (cred == null || cred.UserName.Length < 1)
                    {
                        return;
                    }

                    using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}{cred.Password}"))
                    {
                        var dbUserData = db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString());
                        var gameEntry = dbUserData.Query().Where(x => x.Id == VndbContentViewModel.SelectedGame.Id).FirstOrDefault();
                        gameEntry.LastPlayed = DateTime.UtcNow;
                        gameEntry.PlayTime = gameEntry.PlayTime + parent.GameStopwatch.Elapsed;
                        LastPlayed = $"{App.ResMan.GetString("LastPlayed")}: {TimeDateChanger.GetHumanDate(gameEntry.LastPlayed)}";
                        PlayTime = $"{App.ResMan.GetString("PlayTime")}: {TimeDateChanger.GetHumanTime(gameEntry.PlayTime)}";
                        dbUserData.Update(gameEntry);
                        VndbContentViewModel.SelectedGame = gameEntry;
                    }
                    parent.GameStopwatch.Reset();
                    parent.IsGameRunning = false;
                    IsStartButtonVisible = Visibility.Visible;
                }
            }
            catch (Exception exception)
            {
                App.Logger.Warning(exception, "Failed to deal with an exited Process");
                SentrySdk.CaptureException(exception);
                throw;
            }
        }
        
        
        
        
        public class VnRelationsBinding
        {
            public string RelTitle { get; set; }
            public string RelRelation { get; set; }
        }

    }
}
