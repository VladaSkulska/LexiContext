import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography,
  CircularProgress,
} from "@mui/material";
import WarningAmberIcon from "@mui/icons-material/WarningAmber";
import { useTranslation } from "react-i18next";

export const DeleteConfirmDialog = ({ 
  open, 
  onClose, 
  onConfirm, 
  title, 
  content, 
  itemName, 
  isDeleting 
}) => {
  const { t } = useTranslation();

  // Використовуємо ключі з вашого JSON
  const displayTitle = title || t("modals.deleteDeck.title");
  
  const displayContent = content || (
    itemName 
      ? `${t("modals.deleteDeck.messagePart1")} "${itemName}"${t("modals.deleteDeck.messagePart2")}`
      : `${t("modals.deleteDeck.messagePart1")}${t("modals.deleteDeck.messagePart2")}`
  );

  return (
    <Dialog
      open={open}
      onClose={() => !isDeleting && onClose()}
      PaperProps={{ sx: { borderRadius: 4, p: 1, maxWidth: 420 } }}
    >
      <DialogTitle sx={{ textAlign: "center", pt: 3 }}>
        <WarningAmberIcon color="error" sx={{ fontSize: 48, mb: 1 }} />
        <Typography variant="h6" component="div" fontWeight="bold">
          {displayTitle}
        </Typography>
      </DialogTitle>
      
      <DialogContent sx={{ textAlign: "center" }}>
        <Typography variant="body1" color="text.secondary">
          {displayContent} 
        </Typography>
      </DialogContent>
      
      <DialogActions sx={{ justifyContent: "center", pb: 3, px: 3, gap: 2 }}>
        <Button
          onClick={onClose}
          variant="outlined"
          color="inherit"
          fullWidth
          disabled={isDeleting}
          sx={{ borderRadius: 2, textTransform: "none", fontWeight: "bold" }}
        >
          {t("common.cancel")}
        </Button>
        <Button
          onClick={onConfirm}
          variant="contained"
          color="error"
          fullWidth
          disabled={isDeleting}
          sx={{ borderRadius: 2, textTransform: "none", fontWeight: "bold" }}
          startIcon={isDeleting ? <CircularProgress size={20} color="inherit" /> : null}
        >
          {isDeleting ? t("common.saving") : t("common.delete")}
        </Button>
      </DialogActions>
    </Dialog>
  );
};