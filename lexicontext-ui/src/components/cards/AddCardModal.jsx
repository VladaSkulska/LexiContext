import { useState } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
  Alert,
} from "@mui/material";
import LoadingButton from "@mui/lab/LoadingButton";
import SaveIcon from "@mui/icons-material/Save";
import { AiContextToggle } from "./AiContextToggle";
import { useTranslation } from "react-i18next";

export const AddCardModal = ({
  open,
  onClose,
  onSubmit,
  isSaving,
  serverError: externalServerError,
}) => {
  const { t } = useTranslation();

  const [formData, setFormData] = useState({
    front: "",
    back: "",
    generateAiContext: true,
  });

  const [localError, setLocalError] = useState("");
  const errorToDisplay = localError || externalServerError;

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    const isTextField = name === "front" || name === "back";
    
    setFormData((prev) => ({
      ...prev,
      [name]: type === "checkbox" 
        ? checked 
        : (isTextField ? String(value) : value),
    }));
    
    if (localError) setLocalError("");
  };

  const handleSubmit = () => {
    const safeFront = String(formData.front || "").trim();
    const safeBack = String(formData.back || "").trim();

    if (!safeFront) {
      setLocalError(t("modals.errors.frontRequired"));
      return;
    }

    onSubmit({
      ...formData,
      front: safeFront,
      back: safeBack,
    });
  };

  return (
    <Dialog
      open={open}
      onClose={() => !isSaving && onClose()}
      fullWidth
      maxWidth="sm"
      PaperProps={{ sx: { borderRadius: 4 } }}
    >
      <DialogTitle sx={{ fontWeight: "bold" }}>
        {t("modals.addCard.title")}
      </DialogTitle>

      <DialogContent>
        {errorToDisplay && (
          <Alert severity="error" sx={{ mb: 2, borderRadius: 2 }}>
            {errorToDisplay}
          </Alert>
        )}

        <TextField
          autoFocus
          margin="dense"
          label={t("modals.addCard.frontLabel")}
          name="front"
          fullWidth
          variant="outlined"
          value={formData.front}
          onChange={handleChange}
          disabled={isSaving}
          sx={{ mb: 2, mt: 1, "& .MuiOutlinedInput-root": { borderRadius: 2 } }}
        />

        <TextField
          margin="dense"
          label={t("modals.addCard.backLabel")}
          name="back"
          fullWidth
          variant="outlined"
          value={formData.back}
          onChange={handleChange}
          disabled={isSaving}
          sx={{ mb: 2, "& .MuiOutlinedInput-root": { borderRadius: 2 } }}
        />

        <AiContextToggle
          checked={formData.generateAiContext}
          onChange={handleChange}
          disabled={isSaving}
        />
      </DialogContent>

      <DialogActions sx={{ p: 3, pt: 1, gap: 1 }}>
        <Button
          onClick={onClose}
          color="inherit"
          disabled={isSaving}
          sx={{ borderRadius: 2 }}
        >
          {t("common.cancel")}
        </Button>

        <LoadingButton
          onClick={handleSubmit}
          loading={isSaving}
          loadingPosition="start"
          startIcon={<SaveIcon />}
          variant="contained"
          color="primary"
          sx={{ borderRadius: 2, px: 3 }}
          disabled={!formData.front}
        >
          {isSaving ? t("common.saving") : t("common.save")}
        </LoadingButton>
      </DialogActions>
    </Dialog>
  );
};