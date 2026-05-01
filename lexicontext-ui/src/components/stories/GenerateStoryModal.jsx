import { useState, useEffect, useMemo } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  MenuItem,
  Typography,
  Box,
  TextField,
  Fade,
} from "@mui/material";
import AutoStoriesIcon from "@mui/icons-material/AutoStories";
import AutoFixHighIcon from "@mui/icons-material/AutoFixHigh";
import { useTranslation } from "react-i18next";

export const GenerateStoryModal = ({ open, onClose, onSubmit, isGenerating }) => {
  const { t } = useTranslation();
  const [genre, setGenre] = useState(1);
  const [loadingIndex, setLoadingIndex] = useState(0);

  const loadingMessages = useMemo(() => t("stories.loadingMessages", { returnObjects: true }), [t]);

  useEffect(() => {
    let interval;
    if (isGenerating && Array.isArray(loadingMessages) && loadingMessages.length > 0) {
      // Відразу ставимо випадковий старт
      setLoadingIndex(Math.floor(Math.random() * loadingMessages.length));

      interval = setInterval(() => {
        setLoadingIndex((prev) => {
          let next;
          do {
            next = Math.floor(Math.random() * loadingMessages.length);
          } while (next === prev && loadingMessages.length > 1);
          return next;
        });
      }, 3500);
    }
    return () => clearInterval(interval);
  }, [isGenerating, loadingMessages]);

  return (
    <Dialog open={open} onClose={() => !isGenerating && onClose()} fullWidth maxWidth="xs" PaperProps={{ sx: { borderRadius: 4 } }}>
      <DialogTitle component="div" sx={{ textAlign: "center", pt: 3 }}>
        <AutoStoriesIcon color="primary" sx={{ fontSize: 40, mb: 1 }} />
        <Typography variant="h5" fontWeight="bold">{t("modals.story.title")}</Typography>
      </DialogTitle>

      <DialogContent sx={{ minHeight: 180, display: "flex", flexDirection: "column" }}>
        <Typography variant="body2" color="text.secondary" align="center" sx={{ mb: 3 }}>
          {t("modals.story.description")}
        </Typography>

        <Box sx={{ mt: "auto" }}>
          <TextField
            select
            label={t("modals.story.chooseGenre")}
            fullWidth
            value={genre}
            onChange={(e) => setGenre(e.target.value)}
            disabled={isGenerating}
            sx={{ "& .MuiOutlinedInput-root": { borderRadius: 3 } }}
          >
            {[0, 1, 2, 3, 4, 5].map((val) => (
              <MenuItem key={val} value={val}>
                {t(`modals.story.genres.${["fairyTale", "everydayLife", "dialogue", "businessEmail", "newsReport", "sciFi"][val]}`)}
              </MenuItem>
            ))}
          </TextField>
        </Box>

        <Box sx={{ height: 40, mt: 2, display: "flex", alignItems: "center", justifyContent: "center" }}>
          {isGenerating && (
            <Fade in={isGenerating} key={loadingIndex} timeout={1000}>
              <Typography variant="body2" color="primary" align="center" sx={{ fontStyle: "italic" }}>
                {loadingMessages[loadingIndex]}
              </Typography>
            </Fade>
          )}
        </Box>
      </DialogContent>

      <DialogActions sx={{ pb: 3, px: 3 }}>
        <Box sx={{ display: "flex", width: "100%", gap: 2 }}>
          <Button onClick={onClose} color="inherit" disabled={isGenerating} sx={{ borderRadius: 2, flex: 1 }}>
            {t("common.cancel")}
          </Button>
          <Button onClick={() => onSubmit(genre)} loading={isGenerating} variant="contained" color="secondary" sx={{ borderRadius: 2, flex: 1, fontWeight: "bold" }}>
            {t("common.generate")}
          </Button>
        </Box>
      </DialogActions>
    </Dialog>
  );
};