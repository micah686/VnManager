// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Stylet;
using MahApps.Metro.IconPacks;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;


namespace VnManager.ViewModels.UserControls
{
    public class StatusBarViewModel: Screen
    {
        public string StatusString { get; set; }
        public bool IsWorking { get; set; }
        private string _gameCount;
        public string GameCount
        {
            get => _gameCount;
            set
            {
                value = $"{value} {App.ResMan.GetString("Games")}";
                _gameCount = value;
                SetAndNotify(ref _gameCount, value);
            }
        }
        public string InfoText { get; set; }
        public bool IsProgressBarVisible { get; set; }
        public double ProgressBarValue { get; set; }
        public bool IsProgressBarInfinite { get; set; }
        public bool IsFileDownloading { get; set; }
        public bool IsDatabaseProcessing { get; set; }
        public PackIconMaterialKind StatusIcon { get; set; }
        public Brush StatusIconColor { get; set; }
        public string StatusIconTooltip { get; set; }


        public StatusBarViewModel()
        {
            SetInitialValues();
        }

        private void SetInitialValues()
        {
            StatusString = App.ResMan.GetString("Ready");
            IsWorking = false;
            GameCount = "0";
            InfoText = "";
            IsProgressBarVisible = false;
            ProgressBarValue = 0;
            IsProgressBarInfinite = false;
            IsFileDownloading = false;
            IsDatabaseProcessing = false;
            StatusIcon = PackIconMaterialKind.CheckCircleOutline;
            StatusIconColor = Brushes.LimeGreen;
            StatusIconTooltip = null;

        }

        public static void ResetValues()
        {
            RootViewModel.StatusBarPage.StatusString = App.ResMan.GetString("Ready");
            RootViewModel.StatusBarPage.IsWorking = false;
            RootViewModel.StatusBarPage.GameCount = "0";
            RootViewModel.StatusBarPage.InfoText = "";
            RootViewModel.StatusBarPage.IsProgressBarVisible = false;
            RootViewModel.StatusBarPage.ProgressBarValue = 0;
            RootViewModel.StatusBarPage.IsProgressBarInfinite = false;
            RootViewModel.StatusBarPage.IsFileDownloading = false;
            RootViewModel.StatusBarPage.IsDatabaseProcessing = false;
            RootViewModel.StatusBarPage.StatusIcon = PackIconMaterialKind.CheckCircleOutline;
            RootViewModel.StatusBarPage.StatusIconColor = Brushes.LimeGreen;
            RootViewModel.StatusBarPage.StatusIconTooltip = null;
        }
        

    }
}
