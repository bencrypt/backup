using System;
using System.Collections.Generic;

namespace Backup.Data
{
    public class BackupException: Exception
    {
        public IList<string> ErrorMessages { get; }
        public IList<string> ErrorDetails { get; }

        /// <summary>
        /// Creates an instance of BackupException with a single error message and an optional single
        /// error details message.
        /// </summary>
        /// <param name="errorMessage">a single error message</param>
        /// <param name="errorDetails">(optional) error details message</param>
        public BackupException(string errorMessage, string errorDetails = null)
        {
            ErrorMessages = new List<string> { errorMessage };
            ErrorDetails = null;
            
            if (errorDetails != null)
            {
                ErrorDetails = new List<string> { errorDetails };
            }
        }
        
        /// <summary>
        /// Creates an instance of BackupException with multiple error messages and optional multiple
        /// error details messages.
        /// </summary>
        /// <param name="errorMessages">a single error message</param>
        /// <param name="errorDetails">(optional) error details message</param>
        public BackupException(IList<string> errorMessages, IList<string> errorDetails = null)
        {
            ErrorMessages = errorMessages;
            ErrorDetails = errorDetails;
        }
        
        /// <summary>
        /// Creates an instance of BackupException with multiple error message and an optional single
        /// error details message.
        /// </summary>
        /// <param name="errorMessages">a single error message</param>
        /// <param name="errorDetails">(optional) error details message</param>
        public BackupException(IList<string> errorMessages, string errorDetails = null)
        {
            ErrorMessages = errorMessages;
            ErrorDetails = null;
            
            if (errorDetails != null)
            {
                ErrorDetails = new List<string> { errorDetails };
            }
        }
    }
}