import { useState, useEffect } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  CircularProgress,
  Snackbar,
  Alert,
  Box
} from "@mui/material";
import axiosClient from "../../api/axiosClient";
import { extractErrorMessage } from "../../utils/errorHandler";
import { useTranslation } from "react-i18next"; // <-- Імпортуємо хук перекладу

export const AddDeckToClassroomModal = ({ open, onClose, deckId }) => {
  const [classrooms, setClassrooms] = useState([]);
  const [selectedClassroomId, setSelectedClassroomId] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  
  const { t } = useTranslation(); // <-- Підключаємо локалізацію
  
  const [snackbar, setSnackbar] = useState({ open: false, message: "", severity: "success" });

  useEffect(() => {
    if (open) {
      const fetchClassrooms = async () => {
        setIsLoading(true);
        try {
          const response = await axiosClient.get("/Classrooms/teacher");
          setClassrooms(response.data);
          if (response.data.length > 0) {
            setSelectedClassroomId(response.data[0].id);
          }
        } catch (error) {
          console.error("Error loading classrooms", error);
        } finally {
          setIsLoading(false);
        }
      };
      fetchClassrooms();
    }
  }, [open]);

  const handleSubmit = async () => {
    if (!selectedClassroomId) return;
    setIsSaving(true);
    
    try {
      await axiosClient.post(`/Classrooms/${selectedClassroomId}/decks/${deckId}`);
      
      // Використовуємо локалізацію для успішного повідомлення
      setSnackbar({ 
        open: true, 
        message: t("classroomDetails.addDeckSuccess", "Колоду успішно додано до класу! ✅"), 
        severity: "success" 
      });
      
      setTimeout(() => {
        onClose();
      }, 1500);
    } catch (error) {
      setSnackbar({ 
        open: true, 
        message: extractErrorMessage(error), 
        severity: "error" 
      });
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <>
      <Dialog open={open} onClose={() => !isSaving && onClose()} fullWidth maxWidth="xs" PaperProps={{ sx: { borderRadius: 3 } }}>
        {/* Заголовок */}
        <DialogTitle sx={{ fontWeight: "bold" }}>
          {t("deckDetails.btnAddToClass")}
        </DialogTitle>
        
        <DialogContent sx={{ minHeight: "100px" }}>
          {isLoading ? (
            <Box sx={{ display: "flex", justifyContent: "center", mt: 3 }}>
              <CircularProgress size={30} />
            </Box>
          ) : classrooms.length === 0 ? (
            <Box sx={{ mt: 2, color: "text.secondary" }}>
              {t("classrooms.emptyTeacher")}
            </Box>
          ) : (
            <FormControl fullWidth sx={{ mt: 2 }}>
              <InputLabel>{t("classroomDetails.selectClass", "Оберіть клас")}</InputLabel>
              <Select
                value={selectedClassroomId}
                label={t("classroomDetails.selectClass", "Оберіть клас")}
                onChange={(e) => setSelectedClassroomId(e.target.value)}
                sx={{ borderRadius: 2 }}
              >
                {classrooms.map((c) => (
                  <MenuItem key={c.id} value={c.id}>
                    {c.name}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          )}
        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={onClose} color="inherit" disabled={isSaving}>
            {t("common.cancel")}
          </Button>
          <Button 
            onClick={handleSubmit} 
            variant="contained" 
            disabled={isSaving || classrooms.length === 0 || !selectedClassroomId}
            sx={{ borderRadius: 2, textTransform: "none", fontWeight: "bold" }}
          >
            {isSaving ? t("common.saving") : t("classroomDetails.add")}
          </Button>
        </DialogActions>
      </Dialog>

      <Snackbar
        open={snackbar.open}
        autoHideDuration={4000}
        onClose={() => setSnackbar({ ...snackbar, open: false })}
        anchorOrigin={{ vertical: "bottom", horizontal: "center" }}
      >
        <Alert severity={snackbar.severity} onClose={() => setSnackbar({ ...snackbar, open: false })}>
          {snackbar.message}
        </Alert>
      </Snackbar>
    </>
  );
};