export interface Patient {
  id: string; // Guid
  numeroAssuranceMaladie: string;
  nom: string;
  prenom: string;
  dateNaissance: string; // DateTime (ISO 8601 string)
  sexe: string; // M ou F
  telephone: string;
  email: string;
  adresse: string;
  codePostal: string;
  ville: string;
  groupeSanguin: string; // (A|B|AB|O)[+-]
  photoUrl?: string; // Nullable
  createdAt: string; // DateTime (ISO 8601 string)
  updatedAt?: string; // Nullable DateTime (ISO 8601 string)
  userId?: string; // Nullable Guid
  estActif: boolean;
}
