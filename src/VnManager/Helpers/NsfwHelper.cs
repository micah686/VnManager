using System;
using System.Collections.Generic;
using System.Text;
using VndbSharp.Models.Common;

namespace VnManager.Helpers
{
    public static class NsfwHelper
    {
        public static bool IsNsfw(ImageRating rating)
        {
            if (rating == null) return false;
            var isSexualValid = false;
            var isViolenceValid = false;
            if (rating.SexualAvg != null)
            {
                if (rating.SexualAvg > Convert.ToDouble(App.UserSettings.MaxSexualRating))
                {
                    isSexualValid = true;
                }
            }

            if (rating.ViolenceAvg != null)
            {
                if (rating.ViolenceAvg > Convert.ToDouble(App.UserSettings.MaxViolenceRating))
                {
                    isViolenceValid = true;
                }
            }
            return isSexualValid || isViolenceValid;
        }
    }

}
