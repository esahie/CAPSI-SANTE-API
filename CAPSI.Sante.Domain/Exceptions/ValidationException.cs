using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Domain.Exceptions
{
    public class ValidationException : Exception
    {
        //    public ValidationException() : base() { }
        //    public ValidationException(string message) : base(message) { }
        //    public ValidationException(string message, Exception innerException)
        //        : base(message, innerException) { }

        //    public IDictionary<string, string[]> Errors { get; }

        //    public ValidationException(IDictionary<string, string[]> errors)
        //        : base("Une ou plusieurs erreurs de validation se sont produites.")
        //    {
        //        Errors = errors;
        //    }
        public IEnumerable<string> Errors { get; }

        public ValidationException(string message, IEnumerable<string> errors = null)
            : base(message)
        {
            Errors = errors ?? new List<string>();
        }
    }
}
