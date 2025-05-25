using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.DTOs.Medecin
{
    public class MedecinListItemDto
    {
        public Guid Id { get; set; }
        public string NumeroLicence { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Specialite { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
    }
}
