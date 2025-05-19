import { useEffect, useState } from "react";
import { useRouter } from "next/router";
import { API_ENDPOINTS } from "../../config/api";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "../../components/ui/card";
import { Button } from "../../components/ui/button";
import { format } from "date-fns";

interface InterviewDetails {
  id: number;
  userName: string;
  userEmail: string;
  startTime: string;
  endTime: string | null;
  status: string;
  transcript: string;
  feedback: string | null;
  questions: {
    question: string;
    answer: string;
    timestamp: string;
  }[];
}

export default function InterviewDetailsPage() {
  const [interview, setInterview] = useState<InterviewDetails | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const router = useRouter();
  const { id } = router.query;

  useEffect(() => {
    if (id) {
      fetchInterviewDetails();
    }
  }, [id]);

  const fetchInterviewDetails = async () => {
    try {
      const response = await fetch(
        API_ENDPOINTS.INTERVIEW_GET_FULL_DETAILS(Number(id))
      );
      if (!response.ok) {
        throw new Error("Failed to fetch interview details");
      }
      const data = await response.json();
      setInterview(data);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to fetch interview details"
      );
    } finally {
      setIsLoading(false);
    }
  };

  const formatTranscript = (transcript: string) => {
    return transcript.split("\n").map((line, index) => {
      if (line.trim() === "") return null;
      const isUser = line.startsWith("You:");
      const content = line.replace(/^(AI:|You:)/, "").trim();
      return (
        <div
          key={index}
          className={`flex ${isUser ? "justify-end" : "justify-start"} mb-2`}
        >
          <div
            className={`max-w-[70%] rounded-lg p-3 ${
              isUser ? "bg-blue-600 text-white" : "bg-gray-800 text-white"
            }`}
          >
            {content}
          </div>
        </div>
      );
    });
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

  if (isLoading) {
    return (
      <div className="min-h-screen bg-black text-white p-8">
        <div className="max-w-4xl mx-auto text-center">Loading...</div>
      </div>
    );
  }

  if (error || !interview) {
    return (
      <div className="min-h-screen bg-black text-white p-8">
        <div className="max-w-4xl mx-auto text-center text-red-500">
          {error || "Interview not found"}
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-black text-white p-8">
      <div className="max-w-4xl mx-auto">
        <div className="flex justify-between items-center mb-8">
          <h1 className="text-3xl font-bold">Interview Details</h1>
          <Button
            onClick={() => router.push("/interviews")}
            className="bg-gray-800 hover:bg-gray-700 text-white"
          >
            Back to Interviews
          </Button>
        </div>

        <Card className="bg-black border-gray-800 mb-8">
          <CardHeader>
            <CardTitle className="text-white">Interview Information</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-gray-400">Candidate</p>
                <p className="text-white">{interview.userName}</p>
              </div>
              <div>
                <p className="text-gray-400">Email</p>
                <p className="text-white">{interview.userEmail}</p>
              </div>
              <div>
                <p className="text-gray-400">Start Time</p>
                <p className="text-white">{formatDate(interview.startTime)}</p>
              </div>
              <div>
                <p className="text-gray-400">End Time</p>
                <p className="text-white">{formatDate(interview.endTime)}</p>
              </div>
              <div>
                <p className="text-gray-400">Status</p>
                <p className="text-white">
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
                </p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card className="bg-black border-gray-800 mb-8">
          <CardHeader>
            <CardTitle className="text-white">Transcript</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {formatTranscript(interview.transcript)}
            </div>
          </CardContent>
        </Card>

        {interview.feedback && (
          <Card className="bg-black border-gray-800">
            <CardHeader>
              <CardTitle className="text-white">Feedback</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="whitespace-pre-wrap">{interview.feedback}</div>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  );
}
