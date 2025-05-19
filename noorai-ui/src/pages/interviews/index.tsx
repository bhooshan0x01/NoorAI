import { useEffect, useState } from "react";
import { API_ENDPOINTS } from "../../config/api";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "../../components/ui/card";
import { Button } from "../../components/ui/button";
import { useRouter } from "next/router";
import { format } from "date-fns";

interface InterviewSummary {
  id: number;
  userName: string;
  userEmail: string;
  startTime: string;
  endTime: string | null;
  status: string;
  questionCount: number;
}

export default function InterviewsPage() {
  const [interviews, setInterviews] = useState<InterviewSummary[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const router = useRouter();

  useEffect(() => {
    fetchInterviews();
  }, []);

  const fetchInterviews = async () => {
    try {
      const response = await fetch(API_ENDPOINTS.INTERVIEW_GET_SUMMARIES);
      if (!response.ok) {
        throw new Error("Failed to fetch interviews");
      }
      const data = await response.json();
      setInterviews(data);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to fetch interviews"
      );
    } finally {
      setIsLoading(false);
    }
  };

  const handleViewDetails = (id: number) => {
    router.push(`/interviews/${id}`);
  };

  const formatDate = (dateString: string | null): string => {
    if (!dateString) return "-";
    try {
      const date = new Date(dateString);
      if (isNaN(date.getTime())) return "-";
      return format(date, "PPp");
    } catch (error) {
      console.error("Error formatting date:", error);
      return "-";
    }
  };

  return (
    <div className="min-h-screen bg-black text-white p-8">
      <div className="max-w-7xl mx-auto">
        <div className="flex justify-between items-center mb-8">
          <h1 className="text-3xl font-bold">Interview History</h1>
          <Button
            onClick={() => router.push("/")}
            className="bg-gray-800 hover:bg-gray-700 text-white"
          >
            Start New Interview
          </Button>
        </div>

        <Card className="bg-black border-gray-800">
          <CardHeader>
            <CardTitle className="text-white">All Interviews</CardTitle>
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <div className="text-center text-gray-400">Loading...</div>
            ) : error ? (
              <div className="text-center text-red-500">{error}</div>
            ) : interviews.length === 0 ? (
              <div className="text-center text-gray-400">
                No interviews found
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead>
                    <tr className="border-b border-gray-800">
                      <th className="text-left p-4">Candidate</th>
                      <th className="text-left p-4">Email</th>
                      <th className="text-left p-4">Start Time</th>
                      <th className="text-left p-4">End Time</th>
                      <th className="text-left p-4">Status</th>
                      <th className="text-left p-4">Questions</th>
                      <th className="text-left p-4">Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {interviews.map((interview) => (
                      <tr
                        key={interview.id}
                        className="border-b border-gray-800 hover:bg-gray-900"
                      >
                        <td className="p-4">{interview.userName}</td>
                        <td className="p-4">{interview.userEmail}</td>
                        <td className="p-4">
                          {formatDate(interview.startTime)}
                        </td>
                        <td className="p-4">{formatDate(interview.endTime)}</td>
                        <td className="p-4">
                          <span
                            className={`px-2 py-1 rounded-full text-sm ${
                              interview.status === "Completed"
                                ? "bg-green-900 text-green-300"
                                : interview.status === "InProgress"
                                ? "bg-blue-900 text-blue-300"
                                : "bg-gray-900 text-gray-300"
                            }`}
                          >
                            {interview.status}
                          </span>
                        </td>
                        <td className="p-4">{interview.questionCount}</td>
                        <td className="p-4">
                          <Button
                            onClick={() => handleViewDetails(interview.id)}
                            className="bg-gray-800 hover:bg-gray-700 text-white"
                          >
                            View Details
                          </Button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
