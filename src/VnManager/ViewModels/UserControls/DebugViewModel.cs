// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Stylet;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using StyletIoC;
using VnManager.Helpers;
using VnManager.ViewModels.Dialogs;


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
        private readonly INavigationController navigationController;

        //public DebugViewModel(IContainer container, IWindowManager windowManager)
        //{
        //    _container = container;
        //    _windowManager = windowManager;
        //    _testImg = ImageHelper.CreateEmptyBitmapImage();
        //    var vm = _container.Get<ImportViewModel>();
        //    _windowManager.ShowDialog(vm);
        //}
        public DebugViewModel(INavigationController navigationController)
        {
            this.navigationController = navigationController ?? throw new ArgumentNullException(nameof(navigationController));
        }


        public void NavTest()
        {
            //=> this.navigationController.NavigateToSettings();
        }





        public void CauseException()
        {
            RaiseException(13, 0, 0, new IntPtr(1));
        }

        [DllImport("kernel32.dll")]
        internal static extern void RaiseException(uint dwExceptionCode, uint dwExceptionFlags, uint nNumberOfArguments, IntPtr lpArguments);
    }
}
