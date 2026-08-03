import { useState, useEffect, useCallback } from "react";
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

const INITIAL_FORM_STATE = {
  title: "",
  description: "",
  targetLanguage: 0,
  nativeLanguage: 1,
  proficiencyLevel: 0,
  tone: 0,
  isPublic: false,
  dailyNewCardsLimit: 20,
  dailyReviewLimit: 50,
};

export const CreateDeckModal = ({
  open,
  onClose,
  onSubmit,
  isSaving,
  limitLanguages,
  classroomId = null,
}) => {
  const { t } = useTranslation();

  const [formData, setFormData] = useState(INITIAL_FORM_STATE);
  const [validationError, setValidationError] = useState(false);
  const [serverError, setServerError] = useState("");

  const resetForm = useCallback(() => {
    setFormData(INITIAL_FORM_STATE);
    setValidationError(false);
    setServerError("");
  }, []);

  // Автоматичне очищення форми при будь-якій зміні видимості модалки (відкритті/закритті)
  useEffect(() => {
    if (open) {
      resetForm();
    }
  }, [open, resetForm]);

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    const isTextField = name === "title" || name === "description";
    const parsed = Number(value);

    setFormData((prev) => ({
      ...prev,
      [name]:
        type === "checkbox"
          ? checked
          : isTextField
          ? String(value)
          : !isNaN(parsed) && value !== ""
          ? parsed
          : value,
    }));

    if (name === "title" && String(value).trim() !== "") {
      setValidationError(false);
    }
    setServerError("");
  };

  const handleSubmit = async () => {
    const safeTitle = String(formData.title || "").trim();
    const safeDescription = String(formData.description || "").trim();

    if (safeTitle === "") {
      setValidationError(true);
      setServerError(t("modals.errors.titleRequired"));
      return;
    }
    setServerError("");

    const payload = {
      ...formData,
      title: safeTitle,
      description: safeDescription,
      targetLanguage: Number(formData.targetLanguage),
      nativeLanguage: Number(formData.nativeLanguage),
      proficiencyLevel: Number(formData.proficiencyLevel),
      tone: Number(formData.tone),
      dailyNewCardsLimit: Number(formData.dailyNewCardsLimit) || 20,
      dailyReviewLimit: Number(formData.dailyReviewLimit) || 50,
      classroomId: classroomId ? classroomId : null,
    };

    try {
      await onSubmit(payload);
      resetForm();
    } catch (err) {
      setServerError(extractErrorMessage(err));
    }
  };

  const handleClose = () => {
    resetForm();
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