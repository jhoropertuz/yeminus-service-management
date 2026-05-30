export interface Client {
  id: string;
  fullName: string;
  documentNumber: string;
  phone: string;
  email: string;
  address: string;
  createdAt: string;
}

export interface CreateClientRequest {
  fullName: string;
  documentNumber: string;
  phone: string;
  email: string;
  address: string;
}

export interface UpdateClientRequest extends CreateClientRequest {}
