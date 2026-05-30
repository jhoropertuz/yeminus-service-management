export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  user: UserResponse;
}

export interface UserResponse {
  id: string;
  username: string;
  fullName: string;
  email: string;
  roles: string[];
}
