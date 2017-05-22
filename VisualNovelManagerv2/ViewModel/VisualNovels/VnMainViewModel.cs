using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using VisualNovelManagerv2.Design.VisualNovel;
using static System.Windows.FontStyles;
using Brushes = System.Windows.Media.Brushes;


// ReSharper disable ExplicitCallerInfoArgument

namespace VisualNovelManagerv2.ViewModel.VisualNovels
{
    public class VnMainViewModel : ViewModelBase
    {
        public ObservableCollection<string> VnNameCollection { get; set; }
        public ICommand BindVnNameCollectionCommand;
        public ICommand GetVnDataCommand { get; set; }
        public VnMainViewModel()
        {
            VnNameCollection = new ObservableCollection<string>();
            BindVnNameCollectionCommand = new RelayCommand(LoadVnNameCollection);
            GetVnDataCommand = new RelayCommand(GetVnData);
            LoadVnNameCollection();
            _vnMainModel = new VnMainModel();
        }
        #region Static Properties
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

        private int _selectedListItemIndex;
        public int SelectedListItemIndex
        {
            get { return _selectedListItemIndex; }
            set
            {
                _selectedListItemIndex = value;
                RaisePropertyChanged(nameof(SelectedListItemIndex));
            }
        }

        private VnMainModel _vnMainModel;
        public VnMainModel VnMainModel
        {
            get { return _vnMainModel; }
            set
            {
                _vnMainModel = value;
                RaisePropertyChanged(nameof(VnMainModel));
            }
        }

        #endregion
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
            DataTable dataTable = new DataTable();
            using (SQLiteConnection connection = new SQLiteConnection(Globals.ConnectionString))
            {
                connection.Open();
                using (SQLiteTransaction transaction = connection.BeginTransaction())
                {
                    using (SQLiteCommand cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = transaction;

                        cmd.CommandText = "SELECT * FROM VnInfo WHERE PK_Id= @PK_Id";
                        cmd.Parameters.AddWithValue("@PK_Id", SelectedListItemIndex +1);
                        SQLiteDataReader reader = cmd.ExecuteReader();

                        dataTable.Load(reader);
                        var items = dataTable.Rows[0].ItemArray;
                        Globals.VnId = Convert.ToInt32(items[1]);
                        VnMainModel.Name = items[2].ToString();
                        VnMainModel.VnIcon = LoadIcon();
                        VnMainModel.Original = items[3].ToString();
                        Thread.Sleep(0);




                        //TODO: put code to get all info I need in 1 transaction, so it runs fast.
                    }
                    transaction.Commit();
                }
                connection.Close();
            }
        }

        private BitmapSource LoadIcon()
        {
            using (SQLiteConnection connection = new SQLiteConnection(Globals.ConnectionString))
            {
                connection.Open();
                using (SQLiteCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM VnUserData WHERE VnId=@VnId";
                    cmd.Parameters.AddWithValue("@VnId", Globals.VnId);
                    SQLiteDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(reader.GetOrdinal("IconPath")))
                        {
                            string iconpath = (string)reader["IconPath"];
                            return CreateIcon(iconpath);
                        }
                        else if(reader.IsDBNull(reader.GetOrdinal("IconPath")))
                        {
                            string exepath = (string)reader["ExePath"];
                            return CreateIcon(exepath);
                        }
                        else
                        {
                            return CreateIcon(null);
                        }
                    }
                }                
            }
            return null;
        }

        private BitmapSource CreateIcon(string path)
        {
            if (path != null)
            {
                Icon sysicon = System.Drawing.Icon.ExtractAssociatedIcon(path);
                if (sysicon == null)
                    return BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgra32, null, new byte[] {0, 0, 0, 0}, 4);
                BitmapSource bmpSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    sysicon.Handle,System.Windows.Int32Rect.Empty,System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                sysicon.Dispose();
                return bmpSrc;
            }
            else
            {
                return BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgra32, null, new byte[] { 0, 0, 0, 0 }, 4);
            }
        }
    }
}
