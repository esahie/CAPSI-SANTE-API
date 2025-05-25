using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Domain.Models.SQLserver
{
    public class Medecin
    {
        public Guid Id { get; set; }
        public string NumeroLicence { get; set; }

        public Guid? UserId { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Specialite { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string AdresseCabinet { get; set; }
        public string CodePostal { get; set; }
        public string Ville { get; set; }

        // Nouvelles propriétés pour la photo
        public string PhotoUrl { get; set; }
        public string PhotoNom { get; set; }
        public string PhotoType { get; set; }
        public long? PhotoTaille { get; set; }

        public List<ServiceMedical> ServicesOfferts { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
