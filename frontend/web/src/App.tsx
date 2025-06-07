import PatientForm from './components/PatientForm';
import PatientList from './components/PatientList';

export default function App() {
  return (
    <div>
      <h1>CAPSI Santé</h1>
      <PatientForm />
      <PatientList />
    </div>
  );
}
