using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.DTOs.DossierMedical
{
    public class DossierMedicalDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DerniereMiseAJour { get; set; }
        public string NomPatient { get; set; }
        public string PrenomPatient { get; set; }
    }
}
