using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using VndbSharp.Models.Common;
using VnManager.Models.Settings;

namespace VnManager.Helpers
{
    public static class NsfwHelper
    {
        /// <summary>
        /// Returns the "Raw" rating Nsfw value, so if either of the ratings are above Safe/Tame
        /// This ignores the ratings that the user sets
        /// </summary>
        /// <param name="rating">ImageRating to be checked for NSFW</param>
        /// <returns></returns>
        public static bool RawRatingIsNsfw(ImageRating rating)
        {
            if (rating == null)
            {
                return false;
            }
            return rating.SexualAvg >= 1 || rating.ViolenceAvg >= 1;
        }

        /// <summary>
        /// Checks if the specified rating should be marked NSFW
        /// If either the Sexual or Violence ratings is above the max set by the user, it will be marked NSFW(returns true)
        /// </summary>
        /// <param name="rating">ImageRating to be checked for NSFW</param>
        /// <returns></returns>
        public static bool UserIsNsfw(ImageRating rating)
        {
            if (rating == null)
            {
                return false;
            }
            var isSexualNsfwValid = false;
            var isViolenceNsfwValid = false;
            
            if (rating.SexualAvg != null && rating.SexualAvg >= Convert.ToDouble(SexualRating.Suggestive, CultureInfo.InvariantCulture) &&
                rating.SexualAvg > Convert.ToDouble(App.UserSettings.MaxSexualRating, CultureInfo.InvariantCulture))
            {
                isSexualNsfwValid = true;
            }

            if (rating.ViolenceAvg != null && rating.ViolenceAvg >= Convert.ToDouble(ViolenceRating.Violent, CultureInfo.InvariantCulture) 
                                           && rating.ViolenceAvg > Convert.ToDouble(App.UserSettings.MaxViolenceRating, CultureInfo.InvariantCulture))
            {
                isViolenceNsfwValid = true;
            }
            //if either is true, mark item as a NSFW item
            return isSexualNsfwValid || isViolenceNsfwValid;
        }
    }

}
