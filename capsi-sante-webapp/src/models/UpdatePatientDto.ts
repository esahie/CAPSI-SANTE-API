import { CreatePatientDto } from './CreatePatientDto';

export interface UpdatePatientDto extends CreatePatientDto {
  id: string; // Guid
}
