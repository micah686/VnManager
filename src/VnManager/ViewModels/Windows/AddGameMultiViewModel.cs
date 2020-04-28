using FluentValidation;
using Stylet;
using System;
using System.Collections.Generic;
using System.Text;

namespace VnManager.ViewModels.Windows
{
    public class AddGameMultiViewModel: ValidatingModelBase
    {
        public BindableCollection<MultiExeGamePaths> GameCollection { get; set; } = new BindableCollection<MultiExeGamePaths>();
        public string ExePath { get; set; }
        public string IconPath { get; set; }
        public string ExeArguments { get; set; }

        private bool _isIconChecked;
        public bool IsIconChecked
        {
            get => _isIconChecked;
            set
            {
                if(value== false)
                {
                    IconPath = string.Empty;
                }
                SetAndNotify(ref _isIconChecked, value);
            }
        }

        private bool _isArgsChecked;
        public bool IsArgsChecked
        {
            get => _isArgsChecked;
            set
            {
                if(value == false)
                {
                    ExeArguments = string.Empty;
                }
                SetAndNotify(ref _isArgsChecked, value);
            }
        }


        private readonly IWindowManager _windowManager;
        public AddGameMultiViewModel(IWindowManager windowManager, IModelValidator<AddGameMultiViewModel> validator) : base(validator)
        {
            _windowManager = windowManager;
            this.Validate();
            for (int i = 0; i < 25; i++)
            {
                GameCollection.Add(new MultiExeGamePaths { ExePath = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis p", IconPath = "testico", ArgumentsString = "testarg" });
            }                       
        }

        public void Submit()
        {
                       
        }

        public void Cancel()
        {
            
        }

        
    }

    public class AddGameMultiViewModelValidator : AbstractValidator<AddGameMultiViewModel>
    {
        public AddGameMultiViewModelValidator()
        {
            RuleFor(x => x.ExePath).NotEmpty().WithMessage("Exe Path cannot be empty");
        }
    }


    public class MultiExeGamePaths
    {
        public string ExePath { get; set; }
        public string IconPath { get; set; }
        public string ArgumentsString { get; set; }
    }
}
