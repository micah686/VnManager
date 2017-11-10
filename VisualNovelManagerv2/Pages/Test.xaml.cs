using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Microsoft.EntityFrameworkCore;
using VisualNovelManagerv2.Converters.TraitConverter;
using VisualNovelManagerv2.Converters.TraitConverter.Models;
using VisualNovelManagerv2.Converters.TraitConverter.TraitService;
using VisualNovelManagerv2.CustomClasses.ConfigSettings;
using VisualNovelManagerv2.EF.Context;
using VisualNovelManagerv2.EF.Entity.VnInfo;
using VisualNovelManagerv2.EF.Entity.VnOther;
using VisualNovelManagerv2.Model.Settings;
using VndbSharp;
using VndbSharp.Models;
#pragma warning disable 1998


namespace VisualNovelManagerv2.Pages
{
    /// <summary>
    /// Interaction logic for Test.xaml
    /// </summary>
    public partial class Test : UserControl
    {
        private readonly ITraitService _TraitService= new TraitService();
        public ObservableCollection<Button> ButtonsList { get; private set; }

        public ObservableCollection<ClickableTraits> ModListCollection { get; set; }
        //on viewmodel, use this:
        /// <summary>
        /// private ITraitService _TraitService;
        /// public CharacterViewModel(ITraitService TraitService): base()
        /// {
        ///     _TraitService = TraitService;
        /// }
        /// </summary>

        public Test()
        {
            InitializeComponent();
            Globals.StatusBar.IsWorkProcessing = true;
            Globals.StatusBar.IsDownloading = true;
            Globals.StatusBar.IsUploading = true;
            Globals.StatusBar.ProgressPercentage = 30.5;
            Globals.StatusBar.ProgressText = "testing123";
            Globals.StatusBar.Message = "msgtest";
            Globals.StatusBar.IsShowOnlineStatusEnabled = true;
            Globals.StatusBar.SetOnlineStatusColor(true);


            ButtonsList = new ObservableCollection<Button>();
            ModListCollection = new ObservableCollection<ClickableTraits>();

            //for (int i = 0; i < 5; i++)
            //{
            //    ButtonsList.Add(new Button() { Content = $"Button {i}", Command = WriteObjectCommand, CommandParameter = i });
            //}
            LoadTraits();
        }

        public ICommand WriteObjectCommand => new RelayCommand<object>(WriteME);
        private void WriteME(object obj)
        {
            var btn = (int)obj;
            Console.WriteLine(btn);
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            UserSettings userSettings = new UserSettings
            {
                NsfwEnabled = false,
                MaxSpoilerLevel = 2,
                VnSetting = new VnSetting
                {
                    Id = 11,
                    Spoiler = 3
                }
            };
            ModifyUserSettings.SaveUserSettings(userSettings);


            var foo = ModifyUserSettings.LoadUserSettings();
            var test = foo.ToString();

            ModifyUserSettings.RemoveUserSettingsNode(11);
        }


        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                List<TraitModel> traits = new List<TraitModel>();
                var traitsWithParent = new Dictionary<TraitModel, List<string>>();
                using (var context = new DatabaseContext())
                {
                    var traitArr = context.VnCharacterTraits
                        .Where(x => x.CharacterId == 107 && x.SpoilerLevel < Globals.MaxSpoiler).Select(x => x.TraitId)
                        .ToArray();
                    traits.AddRange(traitArr.Select(trait => new TraitModel(Convert.ToInt32(trait), _TraitService)));


                    foreach (var trait in traits)
                    {
                        TraitModel parenttrait = _TraitService.GetLastParentTrait(trait);

                        if (traitsWithParent.Keys.Any(x => x.Name == parenttrait.Name))
                        {
                            traitsWithParent[traitsWithParent.Keys.First(x => x.Name == parenttrait.Name)].Add(trait.Name);
                        }
                        else
                        {
                            traitsWithParent.Add(parenttrait, new List<string>() { trait.Name });
                        }
                    }
                }
                                
                var formatted = traitsWithParent
                    .OrderBy(x => x.Key.Name)
                    .ToDictionary(x => x.Key, y => string.Join(", ", y.Value.OrderBy(z => z).ToList()));

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                Globals.Logger.Error(exception);
                throw;
            }
        }

        private void LoadTraits()
        {
            uint _characterId = 20881;
            try
            {
                using (var context = new DatabaseContext())
                {
                    List<TraitModel> traits = new List<TraitModel>();
                    var traitsWithParent = new Dictionary<TraitModel, List<string>>();
                    var traitArr = context.VnCharacterTraits
                        .Where(x => x.CharacterId == _characterId && x.SpoilerLevel < Globals.MaxSpoiler).Select(x => x.TraitId)
                        .ToArray();
                    traits.AddRange(traitArr.Select(trait => new TraitModel(Convert.ToInt32(trait), _TraitService)));


                    foreach (var trait in traits)
                    {
                        TraitModel parenttrait = _TraitService.GetLastParentTrait(trait);

                        if (traitsWithParent.Keys.Any(x => x.Name == parenttrait.Name))
                        {
                            traitsWithParent[traitsWithParent.Keys.First(x => x.Name == parenttrait.Name)].Add(trait.Name);
                        }
                        else
                        {
                            traitsWithParent.Add(parenttrait, new List<string>() { trait.Name });
                        }
                    }

                    string[] rootTraits = traitsWithParent.Select(parent => parent.Key.Name).ToArray();

                    foreach (var mainTrait in rootTraits)
                    {
                       
                        var childTraitList = traitsWithParent.Where(x => x.Key.Name == mainTrait).Select(x => x.Value)
                            .First();

                        List<Button> buttonList = childTraitList.Select(trait => new Button() {Content = trait, Command = WriteObjectCommand, CommandParameter = trait}).ToList();
                        ModListCollection.Add(new ClickableTraits() { Name = mainTrait, Traits = buttonList });
                        //TraitCollection.Add(menuItem);
                    }


                    //string[] rootTraits = traitsWithParent.Select(parent => parent.Key.Name).ToArray();

                    //foreach (var mainTrait in rootTraits)
                    //{
                    //    var menuItem = new MenuItem() { Header = mainTrait };
                    //    var childTraitList = traitsWithParent.Where(x => x.Key.Name == mainTrait).Select(x => x.Value)
                    //        .First();

                    //    foreach (var trait in childTraitList)
                    //    {
                    //        menuItem.Items.Add(new MenuItem() { Header = trait });
                    //    }
                    //    //TraitCollection.Add(menuItem);
                    //}
                }
            }
            catch (Exception exception)
            {
                Globals.Logger.Error(exception);
                throw;
            }
        }
    }

    public class ClickableTraits
    {
        public string Name { get; set; }
        public List<Button> Traits { get; set; }
    }
}
