// Copyright (c) micah686. All Rights Reserved.
// Licensed under the MIT License.  See the LICENSE file in the project root for license information.

namespace VnManager.Helpers
{
    public static class NormalizeWhiteSpace
    {
        /// <summary>
        /// Removes extra whitespace (eg: '  ', or trailing whitespace)
        /// </summary>
        /// <param name="input">String with extra whitespace</param>
        /// <returns></returns>
        public static string FixWhiteSpace(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            int current = 0;
            char[] output = new char[input.Length];
            bool skipped = false;

            foreach (char c in input)
            {
                if (char.IsWhiteSpace(c))
                {
                    if (skipped)
                    {
                        continue;
                    }
                    if (current > 0)
                    {
                        output[current++] = ' ';
                    }

                    skipped = true;
                }
                else
                {
                    skipped = false;
                    output[current++] = c;
                }
            }

            return new string(output, 0, current);
        }
    }
}
