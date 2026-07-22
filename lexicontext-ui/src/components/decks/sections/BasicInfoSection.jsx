import { Grid, TextField, MenuItem } from "@mui/material";
import { useTranslation } from "react-i18next";

export const BasicInfoSection = ({
  formData,
  handleChange,
  isSaving,
  validationError,
  limitLanguages = false
}) => {
  const { t } = useTranslation();

  // Залишено лише 5 мов. Жодного сміття.
  const ALL_LANGUAGES = [
    { value: 0, label: t("constants.languages.en") },
    { value: 1, label: t("constants.languages.uk") },
    { value: 2, label: t("constants.languages.de") },
    { value: 3, label: t("constants.languages.fr") },
    { value: 4, label: t("constants.languages.es") },
  ];

  const languagesToUse = limitLanguages 
    ? ALL_LANGUAGES.filter(lang => lang.value <= 1) // Тільки EN та UK, якщо є ліміт
    : ALL_LANGUAGES;

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
          value={Number(formData.targetLanguage)}
          onChange={handleChange}
          disabled={isSaving}
          sx={{ "& .MuiOutlinedInput-root": { borderRadius: 3 } }}
        >
          {languagesToUse.map((option) => (
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
          value={Number(formData.nativeLanguage)}
          onChange={handleChange}
          disabled={isSaving}
          sx={{ "& .MuiOutlinedInput-root": { borderRadius: 3 } }}
        >
          {languagesToUse.map((option) => (
            <MenuItem key={option.value} value={option.value}>
              {option.label}
            </MenuItem>
          ))}
        </TextField>
      </Grid>
    </Grid>
  );
};