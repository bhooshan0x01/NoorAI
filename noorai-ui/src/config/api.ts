export const API_BASE_URL = "http://localhost:5158";

export const API_ENDPOINTS = {
  UPLOAD: `${API_BASE_URL}/api/upload`,
  INTERVIEW_RESPOND: `${API_BASE_URL}/api/interview/respond`,
  INTERVIEW_END: `${API_BASE_URL}/api/interview/end`,
  INTERVIEW_UPDATE_JD: `${API_BASE_URL}/api/interview/job-description`,
  INTERVIEW_GET_ALL: `${API_BASE_URL}/api/interview`,
  INTERVIEW_GET_DETAILS: (id: number) => `${API_BASE_URL}/api/interview/${id}`,
} as const;
