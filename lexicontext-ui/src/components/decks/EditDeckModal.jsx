import { useState } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
  Grid,
  Typography,
  Box,
  Divider,
  Alert,
  Tooltip,
} from "@mui/material";
import InfoOutlinedIcon from "@mui/icons-material/InfoOutlined";
import RestartAltIcon from "@mui/icons-material/RestartAlt";
import { useTranslation } from "react-i18next";

export const EditDeckModal = ({
  open,
  onClose,
  onSubmit,
  initialData,
  isSaving,
  serverError,
}) => {
  const { t } = useTranslation();

  const [formData, setFormData] = useState({
    title: initialData?.title || "",
    description: initialData?.description || "",
    dailyNewCardsLimit: initialData?.dailyNewCardsLimit || 20,
    dailyReviewLimit: initialData?.dailyReviewLimit || 50,
  });

  const handleChange = (e) => {
    const { name, value, type } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: type === "number" ? Number(value) : value,
    }));
  };

  const handleResetLimits = () => {
    setFormData((prev) => ({
      ...prev,
      dailyNewCardsLimit: 20,
      dailyReviewLimit: 50,
    }));
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
        {t("modals.editDeck.title", "Налаштування колоди")}
      </DialogTitle>

      <DialogContent>
        {serverError && (
          <Alert severity="error" sx={{ mb: 2, borderRadius: 2 }}>
            {serverError}
          </Alert>
        )}

        <TextField
          autoFocus
          margin="dense"
          label={t("modals.createDeck.nameLabel")}
          name="title"
          fullWidth
          variant="outlined"
          value={formData.title}
          onChange={handleChange}
          disabled={isSaving}
          sx={{ mb: 2, mt: 1, "& .MuiOutlinedInput-root": { borderRadius: 2 } }}
        />

        <TextField
          margin="dense"
          label={t("modals.createDeck.descLabel")}
          name="description"
          fullWidth
          variant="outlined"
          multiline
          rows={3}
          value={formData.description}
          onChange={handleChange}
          disabled={isSaving}
          sx={{ mb: 2, "& .MuiOutlinedInput-root": { borderRadius: 2 } }}
        />

        <Divider sx={{ my: 3 }} />

        <Box
          sx={{
            mb: 2,
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
          }}
        >
          <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
            <Typography
              variant="subtitle2"
              color="primary"
              fontWeight="bold"
              sx={{ textTransform: "uppercase", letterSpacing: 1 }}
            >
              {t("modals.createDeck.limitsTitle")}
            </Typography>
            <Tooltip
              title={t(
                "modals.editDeck.limitsTooltip",
                "Рекомендуємо 15-20 нових слів та 50-100 повторень на день.",
              )}
              arrow
              placement="top"
            >
              <InfoOutlinedIcon
                fontSize="small"
                color="action"
                sx={{ cursor: "help" }}
              />
            </Tooltip>
          </Box>
          <Button
            size="small"
            color="inherit"
            startIcon={<RestartAltIcon />}
            onClick={handleResetLimits}
            disabled={isSaving}
            sx={{
              textTransform: "none",
              fontSize: "0.8rem",
              opacity: 0.7,
              "&:hover": { opacity: 1 },
            }}
          >
            {t("modals.editDeck.resetBtn", "Скинути")}
          </Button>
        </Box>

        <Grid container spacing={2}>
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField
              label={t("modals.createDeck.newCardsLimit")}
              name="dailyNewCardsLimit"
              helperText={`${t("modals.createDeck.recommended", "Рекомендовано:")} 20`}
              type="number"
              fullWidth
              variant="outlined"
              value={formData.dailyNewCardsLimit}
              onChange={handleChange}
              disabled={isSaving}
              InputProps={{ inputProps: { min: 1, max: 100 } }}
              sx={{ "& .MuiOutlinedInput-root": { borderRadius: 2 } }}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField
              label={t("modals.createDeck.reviewLimit")}
              name="dailyReviewLimit"
              helperText={`${t("modals.createDeck.recommended", "Рекомендовано:")} 50`}
              type="number"
              fullWidth
              variant="outlined"
              value={formData.dailyReviewLimit}
              onChange={handleChange}
              disabled={isSaving}
              InputProps={{ inputProps: { min: 1, max: 500 } }}
              sx={{ "& .MuiOutlinedInput-root": { borderRadius: 2 } }}
            />
          </Grid>
        </Grid>
      </DialogContent>

      <DialogActions sx={{ p: 3, pt: 1 }}>
        <Button
          onClick={onClose}
          color="inherit"
          disabled={isSaving}
          sx={{ fontWeight: "bold" }}
        >
          {t("common.cancel")}
        </Button>
        <Button
          onClick={() => onSubmit(formData)}
          variant="contained"
          color="primary"
          sx={{ borderRadius: 2, px: 3, fontWeight: "bold" }}
          disabled={!formData.title || isSaving}
        >
          {isSaving
            ? t("common.saving")
            : t("modals.editDeck.saveBtn", "Зберегти зміни")}
        </Button>
      </DialogActions>
    </Dialog>
  );
};
