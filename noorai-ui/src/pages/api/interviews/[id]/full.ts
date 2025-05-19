import type { NextApiRequest, NextApiResponse } from "next";

export default async function handler(
  req: NextApiRequest,
  res: NextApiResponse
) {
  if (req.method !== "GET") {
    return res.status(405).json({ error: "Method not allowed" });
  }

  const { id } = req.query;

  try {
    const response = await fetch(
      `${process.env.NEXT_PUBLIC_API_URL}/api/interview/${id}/full`
    );
    if (!response.ok) {
      if (response.status === 404) {
        return res.status(404).json({ error: "Interview not found" });
      }
      throw new Error("Failed to fetch interview details");
    }
    const data = await response.json();
    res.status(200).json(data);
  } catch (error) {
    console.error("Error fetching interview details:", error);
    res.status(500).json({ error: "Failed to fetch interview details" });
  }
}
