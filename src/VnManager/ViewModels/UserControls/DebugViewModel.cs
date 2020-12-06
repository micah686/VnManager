using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using LiteDB;
using VndbSharp.Models.Common;
using VnManager.Converters;
using VnManager.MetadataProviders.Vndb;
using VnManager.Models.Db.Vndb.Main;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using AdysTech.CredentialManager;
using StyletIoC;
using VndbSharp;
using VndbSharp.Models.Dumps;
using VnManager.Helpers;
using VnManager.Models.Db;
using VnManager.Models.Db.User;
using VnManager.Models.Db.Vndb.TagTrait;
using VnManager.ViewModels.Dialogs;
using VnManager.ViewModels.Dialogs.AddGameSources;
using VnManager.ViewModels.UserControls.MainPage.Vndb;
using VnManager.ViewModels.Windows;


namespace VnManager.ViewModels.UserControls
{
    [ExcludeFromCodeCoverage]
    public class DebugViewModel: Screen
    {
        #region TestImg
        private BitmapSource _testImg;

        public BitmapSource TestImg
        {
            get
            {
                return _testImg;
            }
            set
            {
                _testImg = value;
                SetAndNotify(ref _testImg, value);
            }
        }

        #endregion

        private readonly IContainer _container;
        private readonly IWindowManager _windowManager;

        public DebugViewModel(IContainer container, IWindowManager windowManager)
        {
            _container = container;
            _windowManager = windowManager;
            _testImg = ImageHelper.CreateEmptyBitmapImage();
        }

        



        

        public void CauseException()
        {
            RaiseException(13, 0, 0, new IntPtr(1));
        }

        [DllImport("kernel32.dll")]
        internal static extern void RaiseException(uint dwExceptionCode, uint dwExceptionFlags, uint nNumberOfArguments, IntPtr lpArguments);
    }
}
