using CAPSI.Sante.Domain.Models.Firestore;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Infrastructure.Firebase.Services
{
   public class FirestoreService
    {
        protected readonly FirestoreDb _firestoreDb;

        public FirestoreService(IOptions<FirestoreSettings> settings)
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", settings.Value.CredentialsPath);
            _firestoreDb = FirestoreDb.Create(settings.Value.ProjectId);
        }
    }
}
