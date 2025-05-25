using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Domain.Models.SQLserver
{
    public class ServiceMedical
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Nom { get; set; }
        public string Description { get; set; }
        public int DureeParDefaut { get; set; }
        public decimal Tarif { get; set; }
        public bool RequiertAssurance { get; set; }
        public bool EstActif { get; set; } = true; // Par défaut, un service est actif
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
