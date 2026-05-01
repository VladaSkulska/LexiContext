import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import {
  Box,
  Container,
  Typography,
  Paper,
  CircularProgress,
  Breadcrumbs,
  Link,
  Chip,
  Divider,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Button,
  IconButton,
  Snackbar,
  Alert,
  Tooltip,
} from "@mui/material";
import NavigateNextIcon from "@mui/icons-material/NavigateNext";
import DeleteIcon from "@mui/icons-material/Delete";
import AddIcon from "@mui/icons-material/Add";
import CheckIcon from "@mui/icons-material/Check";
import FolderIcon from "@mui/icons-material/Folder"; 
import { Navbar } from "../components/common/Navbar";
import { DeleteConfirmDialog } from "../components/decks/DeleteConfirmDialog.jsx";
import axiosClient from "../api/axiosClient";
import { extractErrorMessage } from "../utils/errorHandler";
import { useTranslation } from "react-i18next";

export const StoryReaderPage = ({ isDarkMode, toggleTheme }) => {
  const { id } = useParams();
  const navigate = useNavigate();
  const { t } = useTranslation();

  const [story, setStory] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [addedWords, setAddedWords] = useState(new Set());
  
  // Стейти для модалки видалення
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);

  const [snackbar, setSnackbar] = useState({
    open: false,
    message: "",
    severity: "success",
  });

  const getGenreLabel = (genreValue) => {
    if (genreValue === null || genreValue === undefined)
      return t("stories.defaultGenre", "Default");
    const keys = [
      "fairyTale",
      "everydayLife",
      "dialogue",
      "businessEmail",
      "newsReport",
      "sciFi",
    ];
    let keyToTranslate = null;

    if (typeof genreValue === "number") {
      keyToTranslate = keys[genreValue];
    } else if (!isNaN(parseInt(genreValue, 10))) {
      keyToTranslate = keys[parseInt(genreValue, 10)];
    } else if (typeof genreValue === "string") {
      const lowerStr = genreValue.toLowerCase();
      keyToTranslate = keys.find((k) => k.toLowerCase() === lowerStr);
    }

    if (keyToTranslate) {
      return t(`modals.story.genres.${keyToTranslate}`);
    }
    return t("stories.defaultGenre", "Default");
  };

  useEffect(() => {
    const fetchStory = async () => {
      try {
        const response = await axiosClient.get(`/Stories/${id}`);
        setStory(response.data);
      } catch (error) {
        console.error("Error loading a story:", error);
        setSnackbar({
          open: true,
          message: t("storyReader.loadError", "Failed to load the story."),
          severity: "error",
        });
      } finally {
        setIsLoading(false);
      }
    };
    fetchStory();
  }, [id, t]);

  // ЗМІНЕНО: тепер просто відкриваємо модалку
  const handleDeleteClick = () => {
    setDeleteDialogOpen(true);
  };

  // ДОДАНО: функція підтвердження видалення
  const handleConfirmDelete = async () => {
    setIsDeleting(true);
    try {
      await axiosClient.delete(`/Stories/${id}`);
      navigate("/stories");
    } catch (error) {
      console.error("Error deleting a story:", error);
      setSnackbar({
        open: true,
        message: t("storyReader.deleteError", "Failed to delete the story."),
        severity: "error",
      });
      setDeleteDialogOpen(false); // Закриваємо модалку при помилці
    } finally {
      setIsDeleting(false);
    }
  };

  const handleAddCard = async (phrase) => {
    try {
      const cleanFront = phrase.phrase.replace(/<\/?[^>]+(>|$)/g, "");
      await axiosClient.post("/Cards", {
        deckId: story.deckId,
        front: cleanFront,
        back: phrase.translation,
        generateAiContext: false,
      });
      setAddedWords((prev) => new Set(prev).add(phrase.id));
      setSnackbar({
        open: true,
        message: t("storyReader.addSuccess", "Added to deck successfully!"),
        severity: "success",
      });
    } catch (error) {
      console.error("Error adding a card:", error);
      setSnackbar({
        open: true,
        message: extractErrorMessage(error),
        severity: "error",
      });
    }
  };

  return (
    <Box
      sx={{
        minHeight: "100vh",
        bgcolor: isDarkMode ? "#121212" : "#f9f9f9",
        pb: 10,
      }}
    >
      <Navbar isDarkMode={isDarkMode} toggleTheme={toggleTheme} />
      {isLoading ? (
        <Box sx={{ display: "flex", justifyContent: "center", mt: 10 }}>
          <CircularProgress />
        </Box>
      ) : !story ? null : (
        <Container maxWidth="md" sx={{ mt: 4 }}>
          <Breadcrumbs
            separator={<NavigateNextIcon fontSize="small" />}
            sx={{ mb: 3 }}
          >
            <Link
              underline="hover"
              color="inherit"
              onClick={() => navigate("/stories")}
              sx={{ cursor: "pointer" }}
            >
              {t("stories.title", "Stories")}
            </Link>
            <Typography
              color="text.primary"
              sx={{ maxWidth: 200, noWrap: true }}
              noWrap
            >
              {story.title}
            </Typography>
          </Breadcrumbs>

          <Paper
            elevation={3}
            sx={{ p: { xs: 3, md: 6 }, borderRadius: 4, mb: 4 }}
          >
            <Box
              sx={{
                display: "flex",
                flexDirection: { xs: "column", sm: "row" },
                justifyContent: "space-between",
                alignItems: { xs: "flex-start", sm: "center" },
                mb: 2,
                gap: 2,
              }}
            >
              <Typography variant="h3" fontWeight="900">
                {story.title}
              </Typography>
              <Box sx={{ display: "flex", gap: 1, flexWrap: "wrap" }}>
                <Chip
                  label={getGenreLabel(story.genre)}
                  color="secondary"
                  variant="outlined"
                  sx={{ fontWeight: "bold" }}
                />
                {story.deckId && story.deckName && (
                  <Chip
                    icon={<FolderIcon fontSize="small" />}
                    label={story.deckName}
                    color="primary"
                    variant="outlined"
                    onClick={() => navigate(`/decks/${story.deckId}`)}
                    sx={{ 
                      fontWeight: "bold", 
                      cursor: "pointer",
                      "&:hover": { bgcolor: "action.hover" }
                    }}
                  />
                )}
              </Box>
            </Box>

            <Typography
              variant="caption"
              color="text.secondary"
              display="block"
              gutterBottom
            >
              {t("storyReader.created", "Created on")}{" "}
              {new Date(story.createdAt).toLocaleDateString()}
            </Typography>
            <Divider sx={{ my: 3 }} />
            <Typography
              variant="body1"
              sx={{
                whiteSpace: "pre-wrap",
                fontSize: "1.1rem",
                lineHeight: 1.8,
              }}
              dangerouslySetInnerHTML={{ __html: story.content }}
            />
            <Box sx={{ mt: 4, display: "flex", justifyContent: "flex-end" }}>
              <Button
                color="error"
                startIcon={<DeleteIcon />}
                onClick={handleDeleteClick} // ЗМІНЕНО виклик функції
              >
                {t("storyReader.deleteBtn", "Delete")}
              </Button>
            </Box>
          </Paper>

          {story.phrases && story.phrases.length > 0 && (
            <>
              <Typography variant="h5" fontWeight="bold" sx={{ mb: 2, mt: 6 }}>
                {t("storyReader.vocabTitle", "Vocabulary")}
              </Typography>
              <TableContainer
                component={Paper}
                elevation={0}
                sx={{
                  borderRadius: 4,
                  border: "1px solid",
                  borderColor: "divider",
                  tableLayout: "fixed" 
                }}
              >
                <Table>
                  <TableHead
                    sx={{
                      bgcolor: isDarkMode ? "rgba(255,255,255,0.05)" : "rgba(0,0,0,0.02)",
                    }}
                  >
                    <TableRow>
                      <TableCell sx={{ fontWeight: "bold", width: "25%" }}>
                        {t("storyReader.table.word", "Word")}
                      </TableCell>
                      <TableCell sx={{ fontWeight: "bold", width: "20%" }}>
                        {t("storyReader.table.reading", "Reading")}
                      </TableCell>
                      <TableCell sx={{ fontWeight: "bold", width: "35%" }}>
                        {t("storyReader.table.translation", "Translation")}
                      </TableCell>
                      <TableCell align="right" sx={{ fontWeight: "bold", width: "20%" }}>
                        {t("storyReader.table.add", "Action")}
                      </TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {story.phrases.map((phrase) => (
                      <TableRow key={phrase.id} hover>
                        <TableCell>
                          <Typography fontWeight="bold" color="primary">
                            {phrase.phrase.replace(/<\/?[^>]+(>|$)/g, "").replace(/\*\*/g, "")}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2" color="text.secondary">
                            {phrase.reading}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2">
                            {phrase.translation}
                          </Typography>
                        </TableCell>
                        <TableCell align="right">
                          {phrase.isAlreadyInDeck || addedWords.has(phrase.id) ? (
                            <Chip
                              icon={<CheckIcon fontSize="small" />}
                              label={t("storyReader.inDeck", "In Deck")}
                              color="success"
                              size="small"
                              variant="outlined"
                            />
                          ) : (
                            <Tooltip title={t("storyReader.addToDeck", "Add to Deck")}>
                              <IconButton
                                color="primary"
                                onClick={() => handleAddCard(phrase)}
                                size="small"
                              >
                                <AddIcon />
                              </IconButton>
                            </Tooltip>
                          )}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </>
          )}
        </Container>
      )}

      {/* ДОДАНО: Модалка підтвердження видалення */}
      <DeleteConfirmDialog
        open={deleteDialogOpen}
        onClose={() => setDeleteDialogOpen(false)}
        onConfirm={handleConfirmDelete}
        title={t("storyReader.deleteConfirmTitle", "Delete Story?")}
        content={t("storyReader.deleteConfirmMsg", "This action cannot be undone. Are you sure you want to delete this story?")}
        isDeleting={isDeleting}
      />

      <Snackbar
        open={snackbar.open}
        autoHideDuration={4000}
        onClose={() => setSnackbar({ ...snackbar, open: false })}
        anchorOrigin={{ vertical: "bottom", horizontal: "center" }}
      >
        <Alert
          onClose={() => setSnackbar({ ...snackbar, open: false })}
          severity={snackbar.severity}
          sx={{ width: "100%", borderRadius: 2 }}
        >
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
};