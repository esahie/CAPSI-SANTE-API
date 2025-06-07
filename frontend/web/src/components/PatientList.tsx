import { useEffect, useState } from 'react';
import { getPatients, deletePatient, Patient } from '../api';

export default function PatientList() {
  const [patients, setPatients] = useState<Patient[]>([]);

  const load = async () => {
    const { data } = await getPatients();
    setPatients((data as any).data || data);
  };

  useEffect(() => {
    load();
  }, []);

  const handleDelete = async (id: string) => {
    await deletePatient(id);
    load();
  };

  return (
    <div>
      <h2>Patients</h2>
      <ul>
        {patients.map(p => (
          <li key={p.id}>
            {p.nom} {p.prenom}
            <button onClick={() => handleDelete(p.id)}>Delete</button>
          </li>
        ))}
      </ul>
    </div>
  );
}
