export interface RequestReactivationDto {
  email: string;
  motifDemande?: string; // StringLength implies it can be absent
}
