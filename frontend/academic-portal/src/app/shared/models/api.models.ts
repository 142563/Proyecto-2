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
  fullName?: string;
  carnet?: string;
  programName?: string;
  campusName?: string;
  shiftName?: string;
  currentCycle?: number;
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

export interface MockCheckoutRequest {
  cardHolderName: string;
  cardNumber: string;
  expiryMonth: number;
  expiryYear: number;
  cvv: string;
}

export interface MockCheckoutCertificateResponse {
  certificateId: string;
  status: string;
  verificationCode: string;
  pdfAvailable: boolean;
}

export interface MockCheckoutResponse {
  payment: PaymentOrderResponse;
  certificate?: MockCheckoutCertificateResponse | null;
}

export interface TransferResponse {
  transferId: string;
  studentCode: string;
  studentName: string;
  fromCampus: string;
  toCampus: string;
  shift: string;
  modality: 'Presencial' | 'Virtual';
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

export type ShiftName = 'Saturday' | 'Sunday';

export interface EnrollmentCourseSelectionRequest {
  courseId: number;
  shift: ShiftName;
}

export interface CreateEnrollmentRequest {
  courseSelections: EnrollmentCourseSelectionRequest[];
}

export interface CourseDto {
  id: number;
  code: string;
  name: string;
  credits: number;
  cycle: number;
  hoursPerWeek: number;
  hoursTotal: number;
  isLab: boolean;
  isApproved: boolean;
  isOverdue: boolean;
  hasPrerequisites: boolean;
  prerequisiteSummary: string;
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
  paymentStatus: 'Pending' | 'Paid' | 'Cancelled';
  pdfAvailable: boolean;
  verificationCode: string;
  paymentOrderId: string;
  amount: number;
  currency: string;
  paymentExpiresAt: string;
  createdAt: string;
  generatedAt?: string | null;
  sentAt?: string | null;
}

export interface CertificateTypeResponse {
  code: string;
  name: string;
  description: string;
  requiresFullPensum: boolean;
}
