using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using static System.Windows.FontStyles;
using Brushes = System.Windows.Media.Brushes;


// ReSharper disable ExplicitCallerInfoArgument

namespace VisualNovelManagerv2.ViewModel.VisualNovels
{
    public class VnMainViewModel : ViewModelBase
    {
        public ObservableCollection<string> VnNameCollection { get; set; }
        public ICommand BindVnNameCollectionCommand;
        public VnMainViewModel()
        {
            VnNameCollection = new ObservableCollection<string>();
            BindVnNameCollectionCommand = new RelayCommand(LoadVnNameCollection);
            LoadVnNameCollection();
        }

        private double _maxListWidth;
        public double MaxListWidth
        {
            get { return _maxListWidth; }
            set
            {
                _maxListWidth = value;
                RaisePropertyChanged(nameof(MaxListWidth));
            }
        }

        private void LoadVnNameCollection()
        {
            using (SQLiteConnection connection = new SQLiteConnection(Globals.ConnectionString))
            {
                connection.Open();

                    using (SQLiteCommand cmd = connection.CreateCommand())
                    {

                        cmd.CommandText = "SELECT Title FROM VnInfo";
                        VnNameCollection.Clear();
                        SQLiteDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            VnNameCollection.Add((string)reader["Title"]);
                        }
                    }

                connection.Close();
            }
            SetMaxWidth();
        }

        public void SetMaxWidth()
        {
            string longestString = VnNameCollection.OrderByDescending(s => s.Length).First();
            FormattedText ft = new FormattedText
            (
                longestString,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(new System.Windows.Media.FontFamily("Segoe UI"),FontStyles.Normal,FontWeights.Bold,FontStretches.Normal),
                13,
                Brushes.Black
            );
            MaxListWidth = (ft.Width + 10);
            Thread.Sleep(0);
        }

        private void GetVnData()
        {
            using (SQLiteConnection connection = new SQLiteConnection(Globals.ConnectionString))
            {
                connection.Open();
                using (SQLiteTransaction transaction = connection.BeginTransaction())
                {
                    using (SQLiteCommand cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = transaction;

                        //TODO: put code to get all info I need in 1 transaction, so it runs fast.
                    }
                    transaction.Commit();
                }
                connection.Close();
            }
        }
    }
}
