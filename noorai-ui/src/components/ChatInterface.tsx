"use client";

import React, { useState, useRef, ChangeEvent } from "react";
import { PaperClipIcon, PaperAirplaneIcon } from "@heroicons/react/24/outline";
import { API_ENDPOINTS } from "../config/api";

interface Message {
  content: string;
  isUser: boolean;
}

interface InterviewResponse {
  id: number;
  question: string;
  feedback?: string;
}

export default function ChatInterface() {
  const [messages, setMessages] = useState<Message[]>([]);
  const [inputMessage, setInputMessage] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [currentInterviewId, setCurrentInterviewId] = useState<number | null>(
    null
  );
  const fileInputRef = useRef<HTMLInputElement>(null);

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

  const handleFileUpload = async (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    if (file.size > 10 * 1024 * 1024) {
      setError("File size must be less than 10MB");
      return;
    }

    setError(null);
    setIsLoading(true);

    try {
      const formData = new FormData();
      formData.append("file", file);

      const response = await fetch(API_ENDPOINTS.UPLOAD, {
        method: "POST",
        body: formData,
      });

      if (!response.ok) {
        throw new Error("Failed to upload file");
      }

      const data: InterviewResponse = await response.json();
      setCurrentInterviewId(data.id);

      // Format and add all messages from the transcript
      const formattedMessages = formatTranscript(data.question);
      setMessages(formattedMessages);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to upload file");
    } finally {
      setIsLoading(false);
      if (fileInputRef.current) {
        fileInputRef.current.value = "";
      }
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
        throw new Error("Failed to get response");
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

  return (
    <div className="flex flex-col h-screen bg-white">
      {/* Messages Area */}
      <div className="flex-1 overflow-y-auto p-4 space-y-4">
        {messages.map((message, index) => (
          <div
            key={index}
            className={`flex ${
              message.isUser ? "justify-end" : "justify-start"
            }`}
          >
            <div className="flex flex-col max-w-[70%]">
              <div
                className={`text-sm font-semibold mb-1 ${
                  message.isUser ? "text-blue-600" : "text-green-600"
                }`}
              >
                {message.isUser ? "You" : "Noor AI"}
              </div>
              <div
                className={`rounded-lg p-3 ${
                  message.isUser
                    ? "bg-blue-500 text-white"
                    : "bg-gray-100 text-gray-800"
                }`}
              >
                {message.content}
              </div>
            </div>
          </div>
        ))}
        {isLoading && (
          <div className="flex justify-start">
            <div className="flex flex-col max-w-[70%]">
              <div className="text-sm font-semibold mb-1 text-green-600">
                Noor AI
              </div>
              <div className="bg-gray-100 rounded-lg p-3 text-gray-800">
                Thinking...
              </div>
            </div>
          </div>
        )}
        {error && (
          <div className="flex justify-center">
            <div className="bg-red-100 text-red-800 rounded-lg p-3">
              {error}
            </div>
          </div>
        )}
      </div>

      {/* Input Area */}
      <div className="border-t p-4 bg-white">
        <div className="flex items-center space-x-2">
          <button
            onClick={() => fileInputRef.current?.click()}
            className="p-2 text-gray-500 hover:text-gray-700"
            disabled={isLoading}
          >
            <PaperClipIcon className="h-6 w-6" />
          </button>
          <input
            type="file"
            ref={fileInputRef}
            onChange={handleFileUpload}
            className="hidden"
            accept=".pdf,.doc,.docx"
          />
          <input
            type="text"
            value={inputMessage}
            onChange={(e) => setInputMessage(e.target.value)}
            onKeyPress={(e) => e.key === "Enter" && handleSendMessage()}
            placeholder="Type your message..."
            className="flex-1 p-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            disabled={isLoading}
          />
          <button
            onClick={handleSendMessage}
            disabled={isLoading || !inputMessage.trim()}
            className="p-2 text-blue-500 hover:text-blue-700 disabled:text-gray-400"
          >
            <PaperAirplaneIcon className="h-6 w-6" />
          </button>
        </div>
      </div>
    </div>
  );
}
