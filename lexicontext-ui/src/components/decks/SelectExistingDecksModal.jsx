import { useState, useEffect } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Checkbox,
  CircularProgress,
  Typography,
  Box
} from "@mui/material";
import axiosClient from "../../api/axiosClient";
import { useTranslation } from "react-i18next";

export const SelectExistingDecksModal = ({ open, onClose, classroomId, onSuccess }) => {
  const { t } = useTranslation();
  const [loading, setLoading] = useState(false);
  const [myDecks, setMyDecks] = useState([]);
  const [selectedDeckIds, setSelectedDeckIds] = useState([]);

  useEffect(() => {
    if (!open || !classroomId) return;

    const fetchMyDecks = async () => {
      setLoading(true);
      try {
        // Завантажуємо всі колоди користувача
        const response = await axiosClient.get(`/Decks?t=${Date.now()}`);
        // Відфільтровуємо ті, які ще НЕ прикріплені до жодного класу (або не в цьому класі)
        const availableDecks = response.data.filter(d => d.ownerClassroomId !== classroomId);
        setMyDecks(availableDecks);
        setSelectedDeckIds([]);
      } catch (error) {
        console.error("Error loading decks for modal:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchMyDecks();
  }, [open, classroomId]);

  const handleToggleDeck = (id) => {
    setSelectedDeckIds((prev) =>
      prev.includes(id) ? prev.filter((item) => item !== id) : [...prev, id]
    );
  };

  const handleSubmit = () => {
    if (onSuccess && selectedDeckIds.length > 0) {
      onSuccess(selectedDeckIds);
    }
  };

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm" PaperProps={{ sx: { borderRadius: 3, p: 1 } }}>
      <DialogTitle sx={{ fontWeight: "bold" }}>
        {t("classroomDetails.addExistingBtn") || "Вибрати з існуючих"}
      </DialogTitle>
      
      <DialogContent dividers>
        {loading ? (
          <Box sx={{ display: "flex", justifyContent: "center", p: 4 }}>
            <CircularProgress size={32} color="secondary" />
          </Box>
        ) : myDecks.length === 0 ? (
          <Typography color="text.secondary" align="center" sx={{ py: 3 }}>
            {t("classroomDetails.noAvailableDecks", "У вас немає вільних колод для додавання. Створіть нову колоду.")}
          </Typography>
        ) : (
          <List sx={{ pt: 0 }}>
            {myDecks.map((deck) => {
              const isSelected = selectedDeckIds.includes(deck.id);
              const cardsCount = (deck.newCards || 0) + (deck.learningCards || 0) + (deck.toReview || 0);
              
              return (
                <ListItem
                  key={deck.id}
                  button
                  onClick={() => handleToggleDeck(deck.id)}
                  sx={{
                    borderRadius: 2,
                    mb: 0.5,
                    bgcolor: isSelected ? "rgba(255, 64, 129, 0.08)" : "transparent",
                    border: "1px solid",
                    borderColor: isSelected ? "secondary.main" : "divider"
                  }}
                >
                  <ListItemIcon sx={{ minWidth: 40 }}>
                    <Checkbox
                      edge="start"
                      checked={isSelected}
                      tabIndex={-1}
                      disableRipple
                      color="secondary"
                    />
                  </ListItemIcon>
                  <ListItemText
                    primary={deck.title || t("dashboard.untitled")}
                    secondary={deck.description || t("deckDetails.cardsCount", { count: cardsCount }) || `Картки: ${cardsCount}`}
                    primaryTypographyProps={{ fontWeight: isSelected ? "bold" : "normal" }}
                  />
                </ListItem>
              );
            })}
          </List>
        )}
      </DialogContent>

      <DialogActions sx={{ px: 3, py: 2 }}>
        <Button onClick={onClose} color="inherit" sx={{ textTransform: "none", borderRadius: 2 }}>
          {t("common.cancel") || "Скасувати"}
        </Button>
        <Button
          onClick={handleSubmit}
          variant="contained"
          color="secondary"
          disabled={selectedDeckIds.length === 0}
          sx={{ textTransform: "none", borderRadius: 2, px: 3, fontWeight: "bold" }}
        >
          {t("classroomDetails.add") || "Додати"}
        </Button>
      </DialogActions>
    </Dialog>
  );
};