using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.DTOs.Prescription
{
    public class MedicamentPrescritDto
    {
        public Guid? Id { get; set; }
        public string NomMedicament { get; set; }
        public string Posologie { get; set; }
        public int? Duree { get; set; }
        public string Instructions { get; set; }
    }
}
