export const API_BASE_URL = "http://localhost:5158";

export const API_ENDPOINTS = {
  UPLOAD: `${API_BASE_URL}/api/upload`,
  INTERVIEW_RESPOND: `${API_BASE_URL}/api/interview/respond`,
  INTERVIEW_END: `${API_BASE_URL}/api/interview/end`,
  INTERVIEW_GET_SUMMARIES: `${API_BASE_URL}/api/interview/summaries`,
  INTERVIEW_GET_FULL_DETAILS: (id: number) =>
    `${API_BASE_URL}/api/interview/${id}/full`,
} as const;
