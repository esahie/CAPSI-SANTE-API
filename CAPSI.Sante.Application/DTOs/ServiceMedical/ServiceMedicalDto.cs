using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.DTOs.ServiceMedical
{
    public class ServiceMedicalDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Nom { get; set; }
        public string Description { get; set; }
        public int DureeParDefaut { get; set; }
        public decimal Tarif { get; set; }
        public bool RequiertAssurance { get; set; }
    }
}
