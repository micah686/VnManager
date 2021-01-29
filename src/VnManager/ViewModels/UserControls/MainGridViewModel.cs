// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MvvmDialogs;
using Stylet;
using StyletIoC;
using VnManager.ViewModels.UserControls.MainPage;

namespace VnManager.ViewModels.UserControls
{
    public class MainGridViewModel: Conductor<Screen>
    {

        public LastPlayedViewModel LastPlayedPage { get; set; }
        public CategoryListViewModel CategoryListPage { get; set; }
        public AddGameButtonViewModel AddGamePage { get; set; }


        public static MainGridViewModel Instance { get; private set; }

        public MainGridViewModel(IContainer container, IWindowManager windowManager, IDialogService dialogService)
        {
            Instance = this;
            var _container = container;
            
            LastPlayedPage = _container.Get<LastPlayedViewModel>();
            CategoryListPage = _container.Get<CategoryListViewModel>();
            AddGamePage = _container.Get<AddGameButtonViewModel>();

            var gg = _container.Get<GameGridViewModel>();
            ActivateItem(gg);
        }

        public sealed override void ActivateItem(Screen item)
        {
            base.ActivateItem(item);
        }
    }
}
