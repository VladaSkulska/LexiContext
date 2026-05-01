import { Grid, TextField, MenuItem } from "@mui/material";
import { useTranslation } from "react-i18next";

export const BasicInfoSection = ({
  formData,
  handleChange,
  isSaving,
  validationError,
}) => {
  const { t } = useTranslation();

  const LANGUAGES = [
    { value: 0, label: t("constants.languages.en") },
    { value: 1, label: t("constants.languages.uk") },
    { value: 2, label: t("constants.languages.de") },
    { value: 3, label: t("constants.languages.pl") },
    { value: 4, label: t("constants.languages.es") },
    { value: 5, label: t("constants.languages.fr") },
    { value: 6, label: t("constants.languages.it") },
    { value: 7, label: t("constants.languages.zh") },
    { value: 8, label: t("constants.languages.ja") },
  ];

  return (
    <Grid container spacing={2.5}>
      <Grid size={{ xs: 12 }}>
        <TextField
          name="title"
          label={t("modals.createDeck.nameLabel")}
          placeholder={t("modals.createDeck.namePlaceholder")}
          fullWidth
          value={formData.title}
          onChange={handleChange}
          error={validationError}
          helperText={validationError ? t("modals.createDeck.nameError") : ""}
          disabled={isSaving}
          sx={{ "& .MuiOutlinedInput-root": { borderRadius: 3 } }}
        />
      </Grid>
      <Grid size={{ xs: 12 }}>
        <TextField
          name="description"
          label={t("modals.createDeck.descLabel")}
          placeholder={t("modals.createDeck.descPlaceholder")}
          fullWidth
          multiline
          rows={2}
          value={formData.description}
          onChange={handleChange}
          disabled={isSaving}
          sx={{ "& .MuiOutlinedInput-root": { borderRadius: 3 } }}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <TextField
          select
          name="targetLanguage"
          label={t("modals.createDeck.targetLang")}
          fullWidth
          value={formData.targetLanguage}
          onChange={handleChange}
          disabled={isSaving}
          sx={{ "& .MuiOutlinedInput-root": { borderRadius: 3 } }}
        >
          {LANGUAGES.map((option) => (
            <MenuItem key={option.value} value={option.value}>
              {option.label}
            </MenuItem>
          ))}
        </TextField>
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <TextField
          select
          name="nativeLanguage"
          label={t("modals.createDeck.nativeLang")}
          fullWidth
          value={formData.nativeLanguage}
          onChange={handleChange}
          disabled={isSaving}
          sx={{ "& .MuiOutlinedInput-root": { borderRadius: 3 } }}
        >
          {LANGUAGES.map((option) => (
            <MenuItem key={option.value} value={option.value}>
              {option.label}
            </MenuItem>
          ))}
        </TextField>
      </Grid>
    </Grid>
  );
};
