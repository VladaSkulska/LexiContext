import { Box, Typography, Grid, TextField, MenuItem } from "@mui/material";
import { useTranslation } from "react-i18next";

export const AiSettingsSection = ({ formData, handleChange, isSaving }) => {
  const { t } = useTranslation();

  const PROFICIENCY_LEVELS = [
    { value: 0, label: t("constants.proficiency.beginner") },
    { value: 1, label: t("constants.proficiency.intermediate") },
    { value: 2, label: t("constants.proficiency.advanced") },
  ];

  const AI_TONES = [
    { value: 0, label: t("constants.tones.neutral") },
    { value: 1, label: t("constants.tones.business") },
    { value: 2, label: t("constants.tones.slang") },
    { value: 3, label: t("constants.tones.academic") },
  ];

  return (
    <Box
      sx={{
        bgcolor: "rgba(0, 0, 0, 0.03)",
        p: 3,
        borderRadius: 4,
        border: "1px solid",
        borderColor: "divider",
      }}
    >
      <Typography
        variant="subtitle2"
        color="primary"
        fontWeight="bold"
        sx={{ mb: 2, textTransform: "uppercase", letterSpacing: 1 }}
      >
        {t("modals.createDeck.aiSettingsTitle")}
      </Typography>
      <Grid container spacing={2.5}>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            select
            name="proficiencyLevel"
            label={t("modals.createDeck.profLevel")}
            fullWidth
            value={formData.proficiencyLevel}
            onChange={handleChange}
            disabled={isSaving}
            sx={{
              "& .MuiOutlinedInput-root": {
                borderRadius: 3,
                bgcolor: "background.paper",
              },
            }}
          >
            {PROFICIENCY_LEVELS.map((option) => (
              <MenuItem key={option.value} value={option.value}>
                {option.label}
              </MenuItem>
            ))}
          </TextField>
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            select
            name="tone"
            label={t("modals.createDeck.aiTone")}
            fullWidth
            value={formData.tone}
            onChange={handleChange}
            disabled={isSaving}
            sx={{
              "& .MuiOutlinedInput-root": {
                borderRadius: 3,
                bgcolor: "background.paper",
              },
            }}
          >
            {AI_TONES.map((option) => (
              <MenuItem key={option.value} value={option.value}>
                {option.label}
              </MenuItem>
            ))}
          </TextField>
        </Grid>
      </Grid>
    </Box>
  );
};
