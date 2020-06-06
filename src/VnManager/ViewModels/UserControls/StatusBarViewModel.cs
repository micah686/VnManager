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
            StatusString = "Ready";
            IsWorking = false;
            GameCount = "0 Items";
            InfoText = "";
            IsProgressBarVisible = false;
            ProgressBarValue = 0;
            IsProgressBarInfinite = false;
            IsDatabaseProcessing = false;
            StatusIcon = PackIconMaterialKind.CheckCircleOutline;
            StatusIconColor = Brushes.LimeGreen;
            StatusIconTooltip = null;

        }


        
    }
}
