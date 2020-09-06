using Stylet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Media;
using MahApps.Metro.IconPacks;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;


namespace VnManager.ViewModels.UserControls
{
    public class StatusBarViewModel: Screen
    {
        public string StatusString { get; set; }
        public bool IsWorking { get; set; }
        public string GameCount { get; set; }
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
            GameCount = $"0 {App.ResMan.GetString("Games")}";
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

        public void ResetValues()
        {
            App.StatusBar.StatusString = App.ResMan.GetString("Ready");
            App.StatusBar.IsWorking = false;
            App.StatusBar.GameCount = $"0 {App.ResMan.GetString("Games")}";
            App.StatusBar.InfoText = "";
            App.StatusBar.IsProgressBarVisible = false;
            App.StatusBar.ProgressBarValue = 0;
            App.StatusBar.IsProgressBarInfinite = false;
            App.StatusBar.IsFileDownloading = false;
            App.StatusBar.IsDatabaseProcessing = false;
            App.StatusBar.StatusIcon = PackIconMaterialKind.CheckCircleOutline;
            App.StatusBar.StatusIconColor = Brushes.LimeGreen;
            App.StatusBar.StatusIconTooltip = null;
        }
        
    }
}
