import axios from 'axios';

const api = axios.create({
  baseURL: '/api'
});

export interface Patient {
  id: string;
  numeroAssuranceMaladie: string;
  nom: string;
  prenom: string;
  dateNaissance: string;
  sexe: string;
  telephone?: string;
  email?: string;
  adresse?: string;
  codePostal?: string;
  ville?: string;
  groupeSanguin?: string;
  photoUrl?: string;
}

export const getPatients = () => api.get<Patient[]>('/Patient');
export const getPatient = (id: string) => api.get<Patient>(`/Patient/${id}`);
export const createPatient = (patient: Omit<Patient, 'id'>) =>
  api.post<Patient>('/Patient', patient);
export const updatePatient = (patient: Patient) =>
  api.put<Patient>(`/Patient/${patient.id}`, patient);
export const deletePatient = (id: string) => api.delete(`/Patient/${id}`);

export default api;
