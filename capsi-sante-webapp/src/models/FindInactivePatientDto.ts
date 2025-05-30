export interface FindInactivePatientDto {
  email?: string;
  telephone?: string;
  numeroAssuranceMaladie?: string;
  userId?: string; // Nullable Guid
  nom?: string;
  prenom?: string;
}
