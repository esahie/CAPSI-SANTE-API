import axios from 'axios';

const api = axios.create({
  baseURL: 'https://localhost:7069/api'
});

export default api;
