// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using FluentValidation;
using Stylet;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Internal;

namespace VnManager.Utilities
{
    /// <summary>
    /// Generic class for working with the FluentValidation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FluentModelValidator<T> : IModelValidator<T>
    {
        private readonly IValidator<T> validator;
        private T subject;

        /// <summary>
        /// Default constructor for the FluentValidation
        /// </summary>
        /// <param name="validator"></param>
        public FluentModelValidator(IValidator<T> validator)
        {
            this.validator = validator;
        }

        /// <summary>
        /// Initialize the FluentValidation
        /// </summary>
        /// <param name="subject"></param>
        public void Initialize(object subject)
        {
            this.subject = (T)subject;
        }

        /// <summary>
        /// Checks to see if the Property is valid
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> ValidatePropertyAsync(string propertyName)
        {
            var result = await this.validator.ValidateAsync(subject, delegate (ValidationStrategy<T> options) { options.IncludeProperties(propertyName); }, CancellationToken.None);
            if (result != null)
            {
                var errors = result.Errors.Select(x => x.ErrorMessage);
                return errors;
            }
            else
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Checks all properties to check if any are not valid
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<string, IEnumerable<string>>> ValidateAllPropertiesAsync()
        {
            // If someone's calling us synchronously, and ValidationAsync does not complete synchronously,
            // we'll deadlock unless we continue on another thread.
            return (await this.validator.ValidateAsync(this.subject).ConfigureAwait(false))
                .Errors.GroupBy(x => x.PropertyName)
                .ToDictionary(x => x.Key, x => x.Select(failure => failure.ErrorMessage));
        }
    }
}
