// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Stylet;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using StyletIoC;


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



        public DebugViewModel(IContainer container, IWindowManager windowManager)
        {

            //_testImg = ImageHelper.CreateEmptyBitmapImage();
            //var vm = container.Get<ImportViewModel>();
            //windowManager.ShowDialog(vm);
        }


        public void NavTest()
        {
            
        }





        public void CauseException()
        {
            RaiseException(13, 0, 0, new IntPtr(1));
        }

        [DllImport("kernel32.dll")]
        internal static extern void RaiseException(uint dwExceptionCode, uint dwExceptionFlags, uint nNumberOfArguments, IntPtr lpArguments);
    }

    

    

    

    

    
}
