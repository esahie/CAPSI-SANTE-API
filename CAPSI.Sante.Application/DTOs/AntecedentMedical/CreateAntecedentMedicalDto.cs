using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.DTOs.AntecedentMedical
{
    public class CreateAntecedentMedicalDto
    {
        public Guid DossierId { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public DateTime? DateDiagnostic { get; set; }
    }
}
