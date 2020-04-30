using FluentValidation;
using Stylet;
using System;
using System.Collections.Generic;
using System.Text;
using VnManager.Utilities;

namespace VnManager.ViewModels.Windows
{
    public class AddGameMultiViewModel: Screen
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
        }

        public void Add()
        { 
            var validator = new AddGameMultiViewModelValidator();
            this.Validate();
            bool result = validator.Validate(this).IsValid;
            if (result == true)
            {
                var exe = ExePath;
                var icon = IconPath;
                var args = ExeArguments;

                GameCollection.Add(new MultiExeGamePaths { ExePath = exe, IconPath = icon, ArgumentsString = args });
            }
        }

        public void Remove()
        {
            if(GameCollection.Count > 0)
            {
                GameCollection.RemoveAt(GameCollection.Count -1);
            }
        }
        
        public void Submit()
        {
            
            this.RequestClose(true);
        }

        public void Cancel()
        {
            this.RequestClose(false);
        }

        
    }

    public class AddGameMultiViewModelValidator : AbstractValidator<AddGameMultiViewModel>
    {
        public AddGameMultiViewModelValidator()
        {
            RuleFor(x => x.ExePath).NotEmpty().WithMessage("Exe Path cannot be empty");
            RuleFor(x => x.IconPath).NotEmpty().When(x => x.IsIconChecked == true).WithMessage("Icon Path cannot be empty");
            RuleFor(x => x.ExeArguments).NotEmpty().When(x => x.IsArgsChecked == true).WithMessage("Arguments cannot be empty");

            RuleFor(x => x.ExePath).Must(ValidateFiles.EndsWithExe).When(x => !string.IsNullOrWhiteSpace(x.ExePath) || !string.IsNullOrEmpty(x.ExePath)).WithMessage("Not a valid path to exe");
            RuleFor(x => x.ExePath).Must(ValidateFiles.ValidateExe).When(x => !string.IsNullOrWhiteSpace(x.ExePath) || !string.IsNullOrEmpty(x.ExePath)).WithMessage("Not a valid Executable");

            RuleFor(x => x.IconPath).Must(ValidateFiles.EndsWithIcoOrExe).When(x => !string.IsNullOrWhiteSpace(x.IconPath) || !string.IsNullOrEmpty(x.IconPath)).WithMessage("Not a valid path to icon");

        }
    }


    public class MultiExeGamePaths
    {
        public string ExePath { get; set; }
        public string IconPath { get; set; }
        public string ArgumentsString { get; set; }
    }
}
