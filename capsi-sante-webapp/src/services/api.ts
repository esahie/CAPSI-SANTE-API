import axios from 'axios';
import { Patient } from '../models/Patient';
import { ApiResponse } from '../models/ApiResponse';
import { CreatePatientDto } from '../models/CreatePatientDto';
import { UpdatePatientDto } from '../models/UpdatePatientDto';
import { RequestReactivationDto } from '../models/RequestReactivationDto';

export const API_BASE_URL = 'http://localhost:7000/api'; // Replace 7000 with your actual API port if different

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// You can add interceptors here if needed, for example, to handle errors globally
// or to add authorization tokens to requests.

// Example of an error handler interceptor:
// apiClient.interceptors.response.use(
//   response => response,
//   error => {
//     // Handle errors globally
//     console.error('API call error:', error);
//     return Promise.reject(error);
//   }
// );

export interface GetPatientsParams {
  searchTerm?: string;
  page?: number;
  pageSize?: number;
  includeInactive?: boolean;
}

export async function getPatients(params: GetPatientsParams): Promise<ApiResponse<Patient[]>> {
  try {
    const response = await apiClient.get<ApiResponse<Patient[]>>('/Patient', { params });
    return response.data;
  } catch (error) {
    console.error('Error fetching patients:', error);
    // For now, rethrow the error. Or, you could return a formatted ApiResponse:
    // return { success: false, data: [], message: 'Failed to fetch patients', errors: [error.message], requestId: '' };
    throw error;
  }
}

export async function getPatientById(id: string): Promise<ApiResponse<Patient>> {
  try {
    const response = await apiClient.get<ApiResponse<Patient>>(`/Patient/${id}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching patient with id ${id}:`, error);
    throw error;
  }
}

export async function createPatient(data: CreatePatientDto): Promise<ApiResponse<Patient>> {
  try {
    const response = await apiClient.post<ApiResponse<Patient>>('/Patient', data);
    return response.data;
  } catch (error) {
    console.error('Error creating patient:', error);
    throw error;
  }
}

export async function updatePatient(id: string, data: UpdatePatientDto): Promise<ApiResponse<Patient>> {
  try {
    const response = await apiClient.put<ApiResponse<Patient>>(`/Patient/${id}`, data);
    return response.data;
  } catch (error) {
    console.error(`Error updating patient with id ${id}:`, error);
    throw error;
  }
}

export async function deletePatient(id: string): Promise<ApiResponse<boolean>> {
  try {
    const response = await apiClient.delete<ApiResponse<boolean>>(`/Patient/${id}`);
    return response.data;
  } catch (error) {
    console.error(`Error deleting patient with id ${id}:`, error);
    throw error;
  }
}

export async function searchPatients(term: string): Promise<ApiResponse<Patient[]>> {
  try {
    const response = await apiClient.get<ApiResponse<Patient[]>>('/Patient/search', { params: { term } });
    return response.data;
  } catch (error) {
    console.error(`Error searching patients with term "${term}":`, error);
    throw error;
  }
}

export async function deactivatePatient(id: string): Promise<ApiResponse<boolean>> {
  try {
    const response = await apiClient.put<ApiResponse<boolean>>(`/Patient/${id}/deactivate`, {});
    return response.data;
  } catch (error) {
    console.error(`Error deactivating patient with id ${id}:`, error);
    throw error;
  }
}

export async function reactivatePatient(id: string): Promise<ApiResponse<boolean>> {
  try {
    const response = await apiClient.put<ApiResponse<boolean>>(`/Patient/${id}/reactivate`, {});
    return response.data;
  } catch (error) {
    console.error(`Error reactivating patient with id ${id}:`, error);
    throw error;
  }
}

export async function requestReactivation(data: RequestReactivationDto): Promise<ApiResponse<string>> {
  try {
    const response = await apiClient.post<ApiResponse<string>>('/Patient/request-reactivation', data);
    return response.data;
  } catch (error) {
    console.error('Error requesting patient reactivation:', error);
    throw error;
  }
}

export async function confirmReactivation(token: string): Promise<ApiResponse<Patient>> {
  try {
    const response = await apiClient.get<ApiResponse<Patient>>(`/Patient/confirm-reactivation/${token}`);
    return response.data;
  } catch (error) {
    console.error('Error confirming patient reactivation:', error);
    throw error;
  }
}

export default apiClient;
