"use client";

import React, {
  useState,
  useRef,
  ChangeEvent,
  KeyboardEvent,
  useEffect,
} from "react";
import {
  Send,
  CheckCircle2,
  XCircle,
  FileText,
  ArrowRight,
} from "lucide-react";
import { API_ENDPOINTS } from "../config/api";
import { Card, CardContent, CardHeader, CardTitle } from "./ui/card";
import { Button } from "./ui/button";
import { Input } from "./ui/input";
import { Avatar, AvatarFallback } from "./ui/avatar";
import { useRouter } from "next/router";

interface Message {
  content: string;
  isUser: boolean;
}

interface InterviewResponse {
  id: number;
  question: string;
  feedback?: string;
}

interface UploadedFiles {
  resume: File | null;
  jobDescription: File | null;
}

interface ApiError {
  error?: string;
  errors?: {
    [key: string]: string[];
  };
}

export default function ChatInterface() {
  const router = useRouter();
  const [messages, setMessages] = useState<Message[]>([]);
  const [inputMessage, setInputMessage] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [currentInterviewId, setCurrentInterviewId] = useState<number | null>(
    null
  );
  const [isInterviewComplete, setIsInterviewComplete] = useState(false);
  const [uploadedFiles, setUploadedFiles] = useState<UploadedFiles>({
    resume: null,
    jobDescription: null,
  });
  const resumeInputRef = useRef<HTMLInputElement>(null);
  const jobDescriptionInputRef = useRef<HTMLInputElement>(null);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages, isLoading]);

  const formatTranscript = (transcript: string): Message[] => {
    const lines = transcript.split("\n");
    const formattedMessages: Message[] = [];

    lines.forEach((line) => {
      if (line.trim() === "") return;

      if (line.startsWith("AI:")) {
        formattedMessages.push({
          content: line.replace("AI:", "").trim(),
          isUser: false,
        });
      } else if (line.startsWith("You:")) {
        formattedMessages.push({
          content: line.replace("You:", "").trim(),
          isUser: true,
        });
      } else {
        // If no prefix, assume it's an AI message
        formattedMessages.push({
          content: line.trim(),
          isUser: false,
        });
      }
    });

    return formattedMessages;
  };

  const formatErrorMessage = (error: ApiError): string => {
    if (error.errors) {
      return Object.entries(error.errors)
        .map(([field, messages]) => {
          const fieldName = field.charAt(0).toUpperCase() + field.slice(1);
          return `${fieldName}: ${messages.join(", ")}`;
        })
        .join("\n");
    }
    return error.error || "An unexpected error occurred";
  };

  const handleFileUpload = async (
    event: ChangeEvent<HTMLInputElement>,
    fileType: "resume" | "jobDescription"
  ) => {
    const file = event.target.files?.[0];
    if (!file) return;

    if (file.size > 10 * 1024 * 1024) {
      setError("File size must be less than 10MB");
      return;
    }

    if (file.type !== "application/pdf") {
      setError("Only PDF files are allowed");
      return;
    }

    setUploadedFiles((prev) => ({
      ...prev,
      [fileType]: file,
    }));
    setError(null); // Clear any previous errors when a valid file is uploaded
  };

  const handleStartInterview = async () => {
    if (!uploadedFiles.resume || !uploadedFiles.jobDescription) {
      setError("Please upload both resume and job description");
      return;
    }

    setError(null);
    setIsLoading(true);
    setMessages([]);
    setIsInterviewComplete(false);

    try {
      const formData = new FormData();
      formData.append("resume", uploadedFiles.resume);
      formData.append("jobDescription", uploadedFiles.jobDescription);

      const response = await fetch(API_ENDPOINTS.UPLOAD, {
        method: "POST",
        body: formData,
      });

      if (!response.ok) {
        const errorData: ApiError = await response.json();
        throw new Error(formatErrorMessage(errorData));
      }

      const data: InterviewResponse = await response.json();
      setCurrentInterviewId(data.id);

      const formattedMessages = formatTranscript(data.question);
      setMessages(formattedMessages);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to upload files");
    } finally {
      setIsLoading(false);
      if (resumeInputRef.current) resumeInputRef.current.value = "";
      if (jobDescriptionInputRef.current)
        jobDescriptionInputRef.current.value = "";
      setUploadedFiles({ resume: null, jobDescription: null });
    }
  };

  const handleSendMessage = async () => {
    if (!inputMessage.trim() || !currentInterviewId) return;

    const userMessage = inputMessage;
    setInputMessage("");
    setMessages((prev) => [...prev, { content: userMessage, isUser: true }]);
    setIsLoading(true);
    setError(null);

    try {
      const response = await fetch(API_ENDPOINTS.INTERVIEW_RESPOND, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ interviewId: currentInterviewId }),
      });

      if (!response.ok) {
        const errorData: ApiError = await response.json();
        throw new Error(formatErrorMessage(errorData));
      }

      const data = await response.json();

      if (data.feedback) {
        const formattedMessages = formatTranscript(data.question);
        setMessages((prev) => [
          ...prev,
          ...formattedMessages,
          {
            content: "Here's your feedback:",
            isUser: false,
          },
          { content: data.feedback, isUser: false },
        ]);
        setCurrentInterviewId(null);
        setIsInterviewComplete(true);
      } else {
        const formattedMessages = formatTranscript(data.question);
        setMessages((prev) => [...prev, ...formattedMessages]);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to get response");
    } finally {
      setIsLoading(false);
    }
  };

  const handleEndInterview = async () => {
    if (!currentInterviewId) return;

    setIsLoading(true);
    try {
      const response = await fetch(API_ENDPOINTS.INTERVIEW_END, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ interviewId: currentInterviewId }),
      });

      if (!response.ok) {
        const errorData: ApiError = await response.json();
        throw new Error(formatErrorMessage(errorData));
      }

      setMessages((prev) => [
        ...prev,
        {
          content: "Interview ended. Thank you for your time.",
          isUser: false,
        },
      ]);
      setCurrentInterviewId(null);
      setIsInterviewComplete(true);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to end interview");
    } finally {
      setIsLoading(false);
    }
  };

  const handleViewFeedback = () => {
    if (currentInterviewId) {
      router.push(`/interviews/${currentInterviewId}`);
    }
  };

  return (
    <div className="flex flex-col h-screen bg-black text-white">
      {/* Header */}
      <header className="bg-black border-b border-gray-800 p-4">
        <div className="flex justify-between items-center">
          <h1 className="text-2xl font-bold text-white">Noor AI Interviewer</h1>
          <Button
            onClick={() => (window.location.href = "/interviews")}
            className="bg-gray-800 hover:bg-gray-700 text-white"
          >
            View All Interviews
          </Button>
        </div>
      </header>

      {/* Main Content */}
      <main className="flex-1 p-4 overflow-hidden">
        <Card className="h-full flex flex-col bg-black border-gray-800">
          <CardHeader>
            <CardTitle className="text-white">Interview Session</CardTitle>
          </CardHeader>
          <CardContent className="flex-1 overflow-y-auto space-y-4">
            {messages.length === 0 && !isLoading ? (
              <div className="flex flex-col items-center justify-center h-full space-y-6">
                <p className="text-gray-400 text-center">
                  Upload your resume and job description to start the interview
                </p>
                <div className="flex flex-col space-y-4 w-full max-w-md">
                  <div className="flex flex-col space-y-2">
                    <label className="text-gray-400 text-sm">
                      Resume (PDF)
                    </label>
                    <div className="flex items-center space-x-2">
                      <Button
                        onClick={() => resumeInputRef.current?.click()}
                        className="flex items-center space-x-2 bg-gray-800 hover:bg-gray-700 text-white w-full"
                      >
                        <FileText className="h-4 w-4" />
                        <span>
                          {uploadedFiles.resume
                            ? uploadedFiles.resume.name
                            : "Upload Resume"}
                        </span>
                      </Button>
                      <input
                        type="file"
                        ref={resumeInputRef}
                        onChange={(e) => handleFileUpload(e, "resume")}
                        className="hidden"
                        accept=".pdf"
                      />
                    </div>
                  </div>
                  <div className="flex flex-col space-y-2">
                    <label className="text-gray-400 text-sm">
                      Job Description (PDF)
                    </label>
                    <div className="flex items-center space-x-2">
                      <Button
                        onClick={() => jobDescriptionInputRef.current?.click()}
                        className="flex items-center space-x-2 bg-gray-800 hover:bg-gray-700 text-white w-full"
                      >
                        <FileText className="h-4 w-4" />
                        <span>
                          {uploadedFiles.jobDescription
                            ? uploadedFiles.jobDescription.name
                            : "Upload Job Description"}
                        </span>
                      </Button>
                      <input
                        type="file"
                        ref={jobDescriptionInputRef}
                        onChange={(e) => handleFileUpload(e, "jobDescription")}
                        className="hidden"
                        accept=".pdf"
                      />
                    </div>
                  </div>
                  <Button
                    onClick={handleStartInterview}
                    disabled={
                      !uploadedFiles.resume ||
                      !uploadedFiles.jobDescription ||
                      isLoading
                    }
                    className="flex items-center justify-center space-x-2 bg-blue-600 hover:bg-blue-700 text-white mt-4"
                  >
                    <span>Start Interview</span>
                  </Button>
                </div>
              </div>
            ) : (
              <>
                {messages.map((message, index) => (
                  <div
                    key={index}
                    className={`flex ${
                      message.isUser ? "justify-end" : "justify-start"
                    } items-start space-x-2`}
                  >
                    {!message.isUser && (
                      <Avatar className="h-8 w-8">
                        <AvatarFallback>AI</AvatarFallback>
                      </Avatar>
                    )}
                    <div
                      className={`rounded-lg p-3 max-w-[70%] ${
                        message.isUser
                          ? "bg-blue-600 text-white"
                          : "bg-gray-800 text-white"
                      }`}
                    >
                      {message.content}
                    </div>
                    {message.isUser && (
                      <Avatar className="h-8 w-8">
                        <AvatarFallback>U</AvatarFallback>
                      </Avatar>
                    )}
                  </div>
                ))}
                {isLoading && (
                  <div className="flex items-start space-x-2">
                    <Avatar className="h-8 w-8">
                      <AvatarFallback>AI</AvatarFallback>
                    </Avatar>
                    <div className="bg-gray-800 rounded-lg p-3 text-white">
                      Thinking...
                    </div>
                  </div>
                )}
                {error && (
                  <div className="flex justify-center">
                    <div className="bg-red-900 text-white rounded-lg p-3 max-w-[70%] whitespace-pre-wrap">
                      {error}
                    </div>
                  </div>
                )}
                {isInterviewComplete && (
                  <div className="flex flex-col items-center justify-center space-y-4 mt-4">
                    <div className="flex items-center space-x-2 text-green-500">
                      <CheckCircle2 className="h-6 w-6" />
                      <span className="text-lg font-semibold">
                        Interview Complete
                      </span>
                    </div>
                    <Button
                      onClick={() => {
                        setMessages([]);
                        setIsInterviewComplete(false);
                      }}
                      className="flex items-center space-x-2 bg-gray-800 hover:bg-gray-700 text-white"
                    >
                      <XCircle className="h-4 w-4" />
                      <span>Start New Interview</span>
                    </Button>
                  </div>
                )}
                <div ref={messagesEndRef} />
              </>
            )}
          </CardContent>
        </Card>
      </main>

      {/* Input Area */}
      <div className="border-t border-gray-800 bg-black p-4">
        <div className="flex items-center space-x-2 max-w-4xl mx-auto">
          <Input
            type="text"
            placeholder="Type your message..."
            value={inputMessage}
            onChange={(e: ChangeEvent<HTMLInputElement>) =>
              setInputMessage(e.target.value)
            }
            onKeyPress={(e: KeyboardEvent<HTMLInputElement>) =>
              e.key === "Enter" && handleSendMessage()
            }
            disabled={!currentInterviewId || isLoading || isInterviewComplete}
            className="flex-1 bg-gray-800 text-white border-gray-700 placeholder:text-gray-400"
          />
          <Button
            onClick={handleSendMessage}
            disabled={
              !currentInterviewId ||
              isLoading ||
              !inputMessage.trim() ||
              isInterviewComplete
            }
            className="flex items-center space-x-2 bg-gray-800 hover:bg-gray-700 text-white"
          >
            <Send className="h-4 w-4" />
            <span>Send</span>
          </Button>
          {currentInterviewId && !isInterviewComplete && (
            <Button
              onClick={handleEndInterview}
              disabled={isLoading}
              className="flex items-center space-x-2 bg-red-600 hover:bg-red-700 text-white"
            >
              <XCircle className="h-4 w-4" />
              <span>End Interview</span>
            </Button>
          )}
        </div>
      </div>

      {isInterviewComplete && currentInterviewId && (
        <div className="flex justify-center mb-4">
          <Button
            onClick={handleViewFeedback}
            className="bg-blue-600 hover:bg-blue-700 text-white flex items-center gap-2"
          >
            View Detailed Feedback
            <ArrowRight className="w-4 h-4" />
          </Button>
        </div>
      )}
    </div>
  );
}
