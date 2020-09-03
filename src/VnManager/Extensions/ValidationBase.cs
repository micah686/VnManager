using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace VnManager.Extensions
{
    /// <summary>
    /// Abstract method for implementing validation on a model
    /// This is used to get validation working on a DataTable
    /// </summary>
    public abstract class ValidationBase: INotifyDataErrorInfo
    {
        private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

        //Set ValidatesOnNotifyDataErrors=True binding on the control.
        //Binding will call the GetErrors method of the INotifyDataErrorInfo when the property is set in the viewModel through view.
        //Subscribes to the ErrorsChanged event in the interface.
        //If the ErrorsChanged event is raised, it will re query the GetErrors method for the property for which the event is raised.

        /// <summary>
        /// Adds error to the dictionary. Accounts for multiple error messages. Raises the changed property event.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="errorMessage"></param>
        public void SetError(string propertyName, string errorMessage)
        {
            if (!_errors.ContainsKey(propertyName))
                _errors.Add(propertyName, new List<string> { errorMessage });
            RaiseErrorsChanged(propertyName);
        }

        /// <summary>
        /// Clears the error for a property. Raised the changed property event.
        /// </summary>
        /// <param name="propertyName"></param>
        protected void ClearError(string propertyName)
        {
            if (_errors.ContainsKey(propertyName))
                _errors.Remove(propertyName);

            RaiseErrorsChanged(propertyName);
        }

        /// <summary>
        /// Clears all errors set in the dictionary
        /// </summary>
        public void ClearAllErrors()
        {
            var errors = _errors.Select(error => error.Key).ToList();

            foreach (var propertyName in errors)
                ClearError(propertyName);
        }

        /// <summary>
        /// Raises the changed property event
        /// </summary>
        /// <param name="propertyName"></param>
        public void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Occurs when the validation errors have changed for a property or the entire model.
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged = delegate { return; };

        /// <summary>
        /// Gets the validation errors for a property of the entire model.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public IEnumerable GetErrors(string propertyName)
        {
            if (String.IsNullOrEmpty(propertyName) ||
               !_errors.ContainsKey(propertyName)) return Enumerable.Empty<string>();
            return _errors[propertyName];
        }

        IEnumerable INotifyDataErrorInfo.GetErrors(string propertyName)
        {
            if (String.IsNullOrEmpty(propertyName) ||
                !_errors.ContainsKey(propertyName)) return Enumerable.Empty<string>();
            return _errors[propertyName];
        }

        /// <summary>
        /// Gets a value that indicates the model has errors.
        /// </summary>
        public bool HasErrors
        {
            get { return _errors.Any(x => x.Value != null && x.Value.Count > 0); }
        }
    }
}
