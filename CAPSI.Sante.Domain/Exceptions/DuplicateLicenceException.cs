using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Domain.Exceptions
{
    public class DuplicateLicenceException : Exception
    {
        public DuplicateLicenceException(string message) : base(message)
        {
        }
    }
}
