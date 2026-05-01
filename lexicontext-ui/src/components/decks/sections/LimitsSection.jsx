import { Box, Typography, Grid, TextField } from "@mui/material";
import { useTranslation } from "react-i18next";

export const LimitsSection = ({ formData, handleChange, isSaving }) => {
  const { t } = useTranslation();

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
        {t("modals.createDeck.limitsTitle")}
      </Typography>
      <Grid container spacing={2.5}>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            name="dailyNewCardsLimit"
            label={t("modals.createDeck.newCardsLimit")}
            type="number"
            fullWidth
            value={formData.dailyNewCardsLimit}
            onChange={handleChange}
            disabled={isSaving}
            InputProps={{ inputProps: { min: 1, max: 100 } }}
            sx={{
              "& .MuiOutlinedInput-root": {
                borderRadius: 3,
                bgcolor: "background.paper",
              },
            }}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            name="dailyReviewLimit"
            label={t("modals.createDeck.reviewLimit")}
            type="number"
            fullWidth
            value={formData.dailyReviewLimit}
            onChange={handleChange}
            disabled={isSaving}
            InputProps={{ inputProps: { min: 1, max: 500 } }}
            sx={{
              "& .MuiOutlinedInput-root": {
                borderRadius: 3,
                bgcolor: "background.paper",
              },
            }}
          />
        </Grid>
      </Grid>
    </Box>
  );
};
