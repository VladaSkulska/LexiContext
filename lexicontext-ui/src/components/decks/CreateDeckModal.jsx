import { useState } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Box,
  Divider,
  Alert,
  Typography,
} from "@mui/material";
import { extractErrorMessage } from "../../utils/errorHandler";
import { useTranslation } from "react-i18next";

import { BasicInfoSection } from "./sections/BasicInfoSection";
import { LimitsSection } from "./sections/LimitsSection";
import { AiSettingsSection } from "./sections/AiSettingsSection";

export const CreateDeckModal = ({ open, onClose, onSubmit, isSaving, limitLanguages, classroomId = null }) => {
  const { t } = useTranslation();

  const [formData, setFormData] = useState({
    title: "",
    description: "",
    targetLanguage: 0,
    nativeLanguage: 1,
    proficiencyLevel: 0,
    tone: 0,
    isPublic: false, // Залишаємо для сумісності з DTO бекенду, але в UI цього більше нема
    dailyNewCardsLimit: 20,
    dailyReviewLimit: 50,
  });

  const [validationError, setValidationError] = useState(false);
  const [serverError, setServerError] = useState("");

  const handleChange = (e) => {
  const { name, value, type, checked } = e.target;
  const parsed = Number(value);
  setFormData((prev) => ({
    ...prev,
    [name]:
      type === "checkbox"
        ? checked
        : !isNaN(parsed) && value !== ""
        ? parsed
        : value,
  }));
  if (name === "title" && value.trim() !== "") setValidationError(false);
  setServerError("");
};

  const handleSubmit = async () => {
    if (formData.title.trim() === "") {
      setValidationError(true);
      return;
    }
    setServerError("");

    try {
      await onSubmit({ ...formData, classroomId });
    } catch (err) {
      setServerError(extractErrorMessage(err));
    }
  };

  const handleClose = () => {
    setFormData({
      title: "",
      description: "",
      targetLanguage: 0,
      nativeLanguage: 1,
      proficiencyLevel: 0,
      tone: 0,
      isPublic: false,
      dailyNewCardsLimit: 20,
      dailyReviewLimit: 50,
    });
    setValidationError(false);
    setServerError("");
    onClose();
  };

  return (
    <Dialog
      open={open}
      onClose={() => !isSaving && handleClose()}
      fullWidth
      maxWidth="sm"
      PaperProps={{ sx: { borderRadius: 4, padding: 1 } }}
    >
      <DialogTitle sx={{ fontWeight: "900", fontSize: "1.5rem", pb: 1 }}>
        {t("modals.createDeck.title")}
      </DialogTitle>

      <DialogContent>
        {serverError && (
          <Alert severity="error" sx={{ mb: 3, borderRadius: 2 }}>
            {serverError}
          </Alert>
        )}

        <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
          {t("modals.createDeck.subtitle")}
        </Typography>

        <BasicInfoSection
          formData={formData}
          handleChange={handleChange}
          isSaving={isSaving}
          validationError={validationError}
          limitLanguages={limitLanguages}
        />

        <Divider sx={{ my: 4 }} />
        <LimitsSection
          formData={formData}
          handleChange={handleChange}
          isSaving={isSaving}
        />

        <Divider sx={{ my: 4 }} />
        <AiSettingsSection
          formData={formData}
          handleChange={handleChange}
          isSaving={isSaving}
        />
      </DialogContent>

      <DialogActions sx={{ p: 3, pt: 1 }}>
        <Button
          onClick={handleClose}
          color="inherit"
          disabled={isSaving}
          sx={{ fontWeight: "bold" }}
        >
          {t("common.cancel")}
        </Button>
        <Button
          onClick={handleSubmit}
          variant="contained"
          color="primary"
          disabled={isSaving}
          sx={{
            borderRadius: 3,
            px: 4,
            py: 1,
            fontWeight: "bold",
            boxShadow: 2,
          }}
        >
          {isSaving ? t("common.saving") : t("modals.createDeck.createBtn")}
        </Button>
      </DialogActions>
    </Dialog>
  );
};