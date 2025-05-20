import React from "react";
import {
  Card,
  CardContent,
  Typography,
  Divider,
  List,
  ListItem,
  ListItemText,
  Paper,
} from "@mui/material";

interface FeedbackSection {
  title: string;
  content: string[];
}

interface InterviewFeedbackProps {
  feedback: string;
}

const InterviewFeedback: React.FC<InterviewFeedbackProps> = ({ feedback }) => {
  console.log("InterviewFeedback component received feedback:", feedback);
  // Parse the feedback string into sections based on the actual structure
  const parseFeedback = (feedbackText: string): FeedbackSection[] => {
    const sections: FeedbackSection[] = [];
    const lines = feedbackText.split("\n").filter((line) => line.trim() !== ""); // Filter out empty lines
    let currentSection: FeedbackSection | null = null;

    lines.forEach((line) => {
      // Check for lines that look like section titles (numbered or bolded)
      if (
        /^\s*\d+\.\s+\*\*.+\*\*\:?/.test(line) ||
        /^\s*\*\*.+\*\*\:?/.test(line)
      ) {
        if (currentSection) {
          sections.push(currentSection);
        }
        // Extract title, removing markdown bold and numbering
        const title = line
          .replace(/^\s*\d+\.\s*/, "")
          .replace(/\*\*/g, "")
          .replace(/:$/, "")
          .trim();
        currentSection = {
          title: title,
          content: [],
        };
      } else if (currentSection) {
        // Add line to current section content, removing leading list markers if present
        const contentLine = line.replace(/^\s*[-*+]\s*/, "").trim();
        if (contentLine) {
          currentSection.content.push(contentLine);
        }
      }
    });

    if (currentSection) {
      sections.push(currentSection);
    }

    return sections;
  };

  const feedbackSections = parseFeedback(feedback);
  console.log("Parsed feedback sections:", feedbackSections);

  return (
    <Paper elevation={3} sx={{ p: 3, maxWidth: "100%", overflow: "auto" }}>
      <Typography
        variant="h4"
        gutterBottom
        sx={{ color: "primary.main", fontWeight: "bold" }}
      >
        Interview Feedback
      </Typography>

      {feedbackSections.map((section, index) => (
        <Card key={index} sx={{ mb: 3, backgroundColor: "background.paper" }}>
          <CardContent>
            <Typography
              variant="h6"
              gutterBottom
              sx={{ color: "primary.main" }}
            >
              {section.title}
            </Typography>
            <Divider sx={{ mb: 2 }} />

            {/* Render content as a list if there are multiple items, otherwise as a paragraph */}
            {section.content.length > 0 ? (
              <List dense={true}>
                {section.content.map((item, itemIndex) => (
                  <ListItem
                    key={itemIndex}
                    sx={{ display: "list-item", pl: 0, pt: 0, pb: 0 }}
                  >
                    <ListItemText
                      primary={item}
                      sx={{
                        "& .MuiListItemText-primary": {
                          color: "text.primary",
                          fontSize: "1rem",
                          lineHeight: 1.5,
                        },
                      }}
                    />
                  </ListItem>
                ))}
              </List>
            ) : (
              <Typography
                variant="body1"
                sx={{ whiteSpace: "pre-line", color: "text.primary" }}
              >
                {/* Fallback or empty state if no content */}
              </Typography>
            )}
          </CardContent>
        </Card>
      ))}
      {feedbackSections.length === 0 && feedback && (
        <Typography
          variant="body1"
          sx={{ whiteSpace: "pre-line", color: "text.primary" }}
        >
          {feedback} {/* Display raw feedback if parsing fails */}
        </Typography>
      )}
    </Paper>
  );
};

export default InterviewFeedback;
