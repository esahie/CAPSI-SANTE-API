import { useState } from 'react';
import { createPatient, Patient } from '../api';

const empty: Omit<Patient, 'id'> = {
  numeroAssuranceMaladie: '',
  nom: '',
  prenom: '',
  dateNaissance: '',
  sexe: '',
  telephone: '',
  email: '',
  adresse: '',
  codePostal: '',
  ville: '',
  groupeSanguin: '',
  photoUrl: ''
};

export default function PatientForm() {
  const [patient, setPatient] = useState(empty);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setPatient({ ...patient, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await createPatient(patient);
    setPatient(empty);
  };

  return (
    <form onSubmit={handleSubmit}>
      <input name="nom" value={patient.nom} onChange={handleChange} placeholder="Nom" />
      <input name="prenom" value={patient.prenom} onChange={handleChange} placeholder="Prénom" />
      <button type="submit">Ajouter</button>
    </form>
  );
}
