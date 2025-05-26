using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.DTOs.Patient
{
    public class FindInactivePatientDto
    {
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string NumeroAssuranceMaladie { get; set; }
        public Guid? UserId { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
    }
}
