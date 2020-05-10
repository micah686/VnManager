using Stylet;
using System;
using System.Collections.Generic;
using System.Text;

namespace VnManager.ViewModels.UserControls
{
    public class CategoryListViewModel:Screen
    {
        public BindableCollection<string> CategoryCollection { get; private set; }

        public CategoryListViewModel()
        {
            CategoryCollection = new BindableCollection<string>();
            for (int i = 0; i < 45; i++)
            {
                CategoryCollection.Add($"test-{i}");
            }
        }
    }
}
