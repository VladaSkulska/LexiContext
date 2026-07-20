import { Box, Chip } from "@mui/material";
import { useTranslation } from "react-i18next";

export const DeckStatistics = ({ deck }) => {
  const { t } = useTranslation();

  return (
    <Box
      sx={{
        display: "flex",
        gap: 1.5,
        flexWrap: "wrap",
        mt: deck?.description ? 0 : 3,
      }}
    >
      <Chip
        label={`${t("deckDetails.queue")} ${deck?.newCards || 0}`}
        color="info"
        variant="soft"
      />
      <Chip
        label={`${t("deckDetails.learning")} ${deck?.learningCards || 0}`}
        color="error"
        variant="soft"
      />
      <Chip
        label={`${t("deckDetails.toReview")} ${deck?.toReview || 0}`}
        color="success"
        variant="soft"
      />
    </Box>
  );
};