import {
  Box,
  FormControlLabel,
  Switch,
  Typography,
  useTheme,
} from "@mui/material";
import { useTranslation } from "react-i18next";

export const AiContextToggle = ({ checked, onChange, disabled }) => {
  const theme = useTheme();
  const { t } = useTranslation();

  return (
    <Box
      sx={{
        mt: 2,
        p: 2,
        bgcolor:
          theme.palette.mode === "dark"
            ? "rgba(255,255,255,0.05)"
            : "rgba(0,0,0,0.02)",
        borderRadius: 3,
      }}
    >
      <FormControlLabel
        control={
          <Switch
            name="generateAiContext"
            checked={checked}
            onChange={onChange}
            color="primary"
            disabled={disabled}
          />
        }
        label={
          <Typography fontWeight="500">
            {t("modals.addCard.aiSwitch")}
          </Typography>
        }
      />
      <Typography
        variant="caption"
        color="text.secondary"
        sx={{ display: "block", ml: 4, mt: 0.5 }}
      >
        {checked ? t("modals.addCard.aiHintOn") : t("modals.addCard.aiHintOff")}
      </Typography>
    </Box>
  );
};
