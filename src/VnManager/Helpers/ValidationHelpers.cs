using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace VnManager.Helpers
{
    internal static class ValidationHelpers
    {
        public static IRuleBuilderOptions<T, string> ExeValidation<T>(this IRuleBuilder<T, string> rule)
        {
            return rule
                .NotEmpty().WithMessage(App.ResMan.GetString("ValidationExePathEmpty"))
                .Must(ValidateFiles.EndsWithExe).WithMessage(App.ResMan.GetString("ValidationExePathNotValid"))
                .Must(ValidateFiles.ValidateExe).WithMessage(App.ResMan.GetString("ValidationExeNotValid"));
        }

        public static IRuleBuilderOptions<T, string> IcoValidation<T>(this IRuleBuilder<T, string> rule)
        {
            return rule
                .NotEmpty().WithMessage(App.ResMan.GetString("ValidationIconPathEmpty"))
                .Must(ValidateFiles.EndsWithIcoOrExe).WithMessage(App.ResMan.GetString("ValidationIconPathNotValid"));
        }

        public static IRuleBuilderOptions<T, string> ArgsValidation<T>(this IRuleBuilder<T, string> rule)
        {
            return rule
                .NotEmpty().WithMessage(App.ResMan.GetString("ValidationArgumentsEmpty"))
                .Must(ContainsIllegalCharacters).WithMessage(App.ResMan.GetString("ValidationArgumentsIllegalChars"));

        }






        public static bool ContainsIllegalCharacters(string format)
        {
            if (format == null) return false;
            string allowableLetters = $@"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890/\-_ !?;:'+={'"'}";

            foreach (char c in format)
            {
                if (!allowableLetters.Contains(c))
                    return false;
            }

            return true;
        }


    }
}
