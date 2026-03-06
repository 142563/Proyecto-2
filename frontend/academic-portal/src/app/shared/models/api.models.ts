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

export interface CampusResponse {
  id: number;
  code: string;
  name: string;
  address: string;
  isActive: boolean;
  campusType: 'Campus' | 'Centro';
  region?: string;
}

export interface PaymentOrderResponse {
  id: string;
  orderType: 'Transfer' | 'Enrollment' | 'Certificate';
  referenceId: string;
  amount: number;
  currency: string;
  status: 'Pending' | 'Paid' | 'Cancelled';
  description: string;
  createdAt: string;
  expiresAt: string;
  paidAt?: string | null;
  cancelledAt?: string | null;
}

export interface TransferResponse {
  transferId: string;
  studentCode: string;
  studentName: string;
  fromCampus: string;
  toCampus: string;
  shift: string;
  status: string;
  createdAt: string;
}

export interface TransferAvailabilityResponse {
  campusId: number;
  campusName: string;
  shiftId: number;
  shiftName: string;
  totalCapacity: number;
  occupiedCapacity: number;
  availableCapacity: number;
}

export interface EnrollmentSummaryResponse {
  enrollmentId: string;
  enrollmentType: string;
  status: string;
  totalAmount: number;
  currency: string;
  paymentOrderId: string;
  paymentExpiresAt: string;
  createdAt: string;
}

export interface CourseDto {
  id: number;
  code: string;
  name: string;
  credits: number;
  isOverdue: boolean;
  hasPrerequisites: boolean;
}

export interface OverdueCourseDto {
  id: number;
  code: string;
  name: string;
  credits: number;
}

export interface CertificateSummaryResponse {
  id: string;
  purpose: string;
  status: string;
  verificationCode: string;
  paymentOrderId: string;
  amount: number;
  currency: string;
  paymentExpiresAt: string;
  createdAt: string;
  generatedAt?: string | null;
  sentAt?: string | null;
}
