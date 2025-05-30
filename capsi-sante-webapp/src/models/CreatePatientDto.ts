export interface CreatePatientDto {
  numeroAssuranceMaladie: string;
  nom: string;
  prenom: string;
  dateNaissance: string; // DateTime (ISO 8601 string)
  sexe: string; // M ou F
  telephone: string;
  email: string;
  adresse?: string; // StringLength implies it can be empty, hence optional
  codePostal?: string; // StringLength implies it can be empty, hence optional
  ville?: string; // StringLength implies it can be empty, hence optional
  groupeSanguin?: string; // StringLength and specific regex, but can be absent
  photoUrl?: string; // Nullable string
  userId?: string; // Nullable Guid
}
