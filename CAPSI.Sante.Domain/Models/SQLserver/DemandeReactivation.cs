using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Domain.Models.SQLserver
{
    public class DemandeReactivation
    {
        public Guid DemandeId { get; set; }
        public Guid PatientId { get; set; }
        public string EmailDemandeur { get; set; }
        public string MotifDemande { get; set; }
        public DateTime DateDemande { get; set; }
        public string Statut { get; set; } // EnAttente, Approuvee, Rejetee, Expiree
        public string TokenVerification { get; set; }
        public DateTime? DateExpiration { get; set; }
        public Guid? TraiteePar { get; set; }
        public DateTime? DateTraitement { get; set; }
        public string CommentaireAdmin { get; set; }

        // Navigation property
        public Patient Patient { get; set; }

        // Propriétés calculées
        public bool EstExpiree => DateExpiration.HasValue && DateExpiration.Value < DateTime.UtcNow;
        public bool EstEnAttente => Statut == "EnAttente" && !EstExpiree;
    }
}
