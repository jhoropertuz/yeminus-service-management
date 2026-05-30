export interface Technician {
  id: string;
  fullName: string;
  documentNumber: string;
  phone: string;
  email: string;
  specialty: string;
  createdAt: string;
}

export interface CreateTechnicianRequest {
  fullName: string;
  documentNumber: string;
  phone: string;
  email: string;
  specialty: string;
}

export interface UpdateTechnicianRequest extends CreateTechnicianRequest {}
