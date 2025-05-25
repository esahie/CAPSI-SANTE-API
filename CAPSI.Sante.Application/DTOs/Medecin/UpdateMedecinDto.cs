using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.DTOs.Medecin
{
    public class UpdateMedecinDto : MedecinBaseDto
    {
        [Required]
        public Guid Id { get; set; }

        public List<Guid> ServicesOfferts { get; set; } = new List<Guid>();
    }
}
