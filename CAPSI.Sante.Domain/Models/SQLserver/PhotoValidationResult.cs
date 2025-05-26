using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Domain.Models.SQLserver
{
    public class PhotoValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public long FileSize { get; set; }
        public string ContentType { get; set; }
        public string Extension { get; set; }

        public static PhotoValidationResult Success()
        {
            return new PhotoValidationResult { IsValid = true };
        }

        public static PhotoValidationResult Failure(string errorMessage)
        {
            return new PhotoValidationResult { IsValid = false, ErrorMessage = errorMessage };
        }
    }
}
