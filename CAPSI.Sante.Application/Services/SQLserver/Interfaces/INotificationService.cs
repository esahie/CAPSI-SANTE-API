using CAPSI.Sante.Domain.Enums;
using CAPSI.Sante.Domain.Models;
using CAPSI.Sante.Domain.Models.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Services.SQLserver.Interfaces
{
    public interface INotificationService
    {
        Task EnvoyerNotificationRendezVousAsync(
            RendezVous rdv,
            NotificationType type,
            string message = null);
    }
}
