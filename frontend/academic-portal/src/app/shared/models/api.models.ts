export interface ApiEnvelope<T> {
  success: boolean;
  data: T;
  error?: { code: string; message: string };
  validationErrors?: Record<string, string[]>;
}

export interface AuthResponse {
  accessToken: string;
  expiresAtUtc: string;
  role: 'Student' | 'Admin';
  userId: string;
  studentId?: string;
  email: string;
}

export interface MeResponse {
  userId: string;
  studentId?: string;
  email: string;
  role: 'Student' | 'Admin';
  isActive: boolean;
}
