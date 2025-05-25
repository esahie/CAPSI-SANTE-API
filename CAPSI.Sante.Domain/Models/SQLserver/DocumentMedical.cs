using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Domain.Models.SQLserver
{
    public class DocumentMedical
    {
        public Guid Id { get; set; }
        public Guid DossierId { get; set; }
        public string Type { get; set; }  // "Ordonnance", "Analyse", "RadioX", etc.
        public string Titre { get; set; }
        public string CheminFichier { get; set; }
        public long TailleFichier { get; set; }
        public string ContentType { get; set; }
        public DateTime DateUpload { get; set; }
        public DateTime? DateModification { get; set; }
        public string UploadParUtilisateur { get; set; }
    }
}
