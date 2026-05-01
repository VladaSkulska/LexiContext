import { useState, useEffect, useCallback } from "react";
import { useParams, useNavigate } from "react-router-dom";
import {
  Box,
  Container,
  Typography,
  Button,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  IconButton,
  CircularProgress,
  Breadcrumbs,
  Link,
  Chip,
  Tooltip,
  Alert,
} from "@mui/material";
import PlayArrowIcon from "@mui/icons-material/PlayArrow";
import AddIcon from "@mui/icons-material/Add";
import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/Delete";
import AutoFixHighIcon from "@mui/icons-material/AutoFixHigh";
import AutoAwesomeIcon from "@mui/icons-material/AutoAwesome";
import FormatClearIcon from "@mui/icons-material/FormatClear";
import NavigateNextIcon from "@mui/icons-material/NavigateNext";
import AutoStoriesIcon from "@mui/icons-material/AutoStories";

import { Navbar } from "../components/common/Navbar";
import axiosClient from "../api/axiosClient";
import { extractErrorMessage } from "../utils/errorHandler";
import { useTranslation } from "react-i18next";

import { GenerateStoryModal } from "../components/stories/GenerateStoryModal";
import { DeleteConfirmDialog } from "../components/decks/DeleteConfirmDialog";
import { AddCardModal } from "../components/cards/AddCardModal";
import { EditDeckModal } from "../components/decks/EditDeckModal";

export const DeckDetailsPage = ({ isDarkMode, toggleTheme }) => {
  const { id } = useParams();
  const navigate = useNavigate();
  const { t } = useTranslation();

  const [deck, setDeck] = useState(null);
  const [cards, setCards] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [pageError, setPageError] = useState("");

  const [openCardModal, setOpenCardModal] = useState(false);
  const [openDeckModal, setOpenDeckModal] = useState(false);
  const [openDeleteDialog, setOpenDeleteDialog] = useState(false);
  const [openStoryModal, setOpenStoryModal] = useState(false);

  const [isSavingCard, setIsSavingCard] = useState(false);
  const [cardError, setCardError] = useState("");
  const [isSavingDeck, setIsSavingDeck] = useState(false);
  const [deckError, setDeckError] = useState("");
  const [isGeneratingStory, setIsGeneratingStory] = useState(false);

  const [simplifyingId, setSimplifyingId] = useState(null);
  const [generatingId, setGeneratingId] = useState(null);
  const [clearingId, setClearingId] = useState(null);

  const loadData = useCallback(async () => {
    try {
      const [deckRes, cardsRes] = await Promise.all([
        axiosClient.get(`/Decks/${id}`),
        axiosClient.get(`/Cards/deck/${id}`),
      ]);
      setDeck(deckRes.data);
      setCards(cardsRes.data);
    } catch (error) {
      console.error("Помилка завантаження:", error);
    } finally {
      setIsLoading(false);
    }
  }, [id]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const handleSaveDeck = async (updatedData) => {
    setIsSavingDeck(true);
    setDeckError("");
    try {
      await axiosClient.put(`/Decks/${id}`, {
        ...updatedData,
        isPublic: deck.isPublic,
        proficiencyLevel: deck.proficiencyLevel,
        tone: deck.tone,
      });
      setOpenDeckModal(false);
      await loadData();
    } catch (error) {
      setDeckError(extractErrorMessage(error));
    } finally {
      setIsSavingDeck(false);
    }
  };

  const confirmDeleteDeck = async () => {
    try {
      await axiosClient.delete(`/Decks/${id}`);
      navigate("/decks");
    } catch (error) {
      setPageError(extractErrorMessage(error));
      setOpenDeleteDialog(false);
    }
  };

  const handleSaveCard = async (cardData) => {
    setIsSavingCard(true);
    setCardError("");
    try {
      await axiosClient.post("/Cards", { deckId: id, ...cardData });
      setOpenCardModal(false);
      await loadData();
    } catch (error) {
      setCardError(extractErrorMessage(error));
    } finally {
      setIsSavingCard(false);
    }
  };

  const handleDeleteCard = async (cardId) => {
    if (!window.confirm(t("common.delete") + "?")) return;
    try {
      await axiosClient.delete(`/Cards/${cardId}`);
      await loadData();
    } catch (error) {
      setPageError(extractErrorMessage(error));
    }
  };

  const handleSimplifyCard = async (cardId) => {
    setSimplifyingId(cardId);
    try {
      await axiosClient.patch(`/Cards/${cardId}/simplify`);
      await loadData();
    } catch (error) {
      setPageError(extractErrorMessage(error));
    } finally {
      setSimplifyingId(null);
    }
  };

  const handleGenerateContext = async (card) => {
    setGeneratingId(card.id);
    try {
      await axiosClient.put(`/Cards/${card.id}`, {
        front: card.front || card.Front,
        back: card.back || card.Back,
        generateAiContext: true,
      });
      await loadData();
    } catch (error) {
      setPageError(extractErrorMessage(error));
    } finally {
      setGeneratingId(null);
    }
  };

  const handleClearContext = async (card) => {
    setClearingId(card.id);
    try {
      await axiosClient.put(`/Cards/${card.id}`, {
        front: card.front || card.Front,
        back: card.back || card.Back,
        generatedContext: "",
        contextTranslation: "",
        contextReading: "",
        generateAiContext: false,
      });
      await loadData();
    } catch (error) {
      setPageError(extractErrorMessage(error));
    } finally {
      setClearingId(null);
    }
  };

  const handleGenerateStory = async (selectedGenre) => {
    setIsGeneratingStory(true);
    setPageError("");
    try {
      const response = await axiosClient.post("/Stories/generate", {
        deckId: id,
        genre: selectedGenre,
      });
      setOpenStoryModal(false);
      navigate(`/story/${response.data.id}`);
    } catch (error) {
      setOpenStoryModal(false);
      setPageError(extractErrorMessage(error));
    } finally {
      setIsGeneratingStory(false);
    }
  };

  if (isLoading)
    return (
      <Box sx={{ display: "flex", justifyContent: "center", mt: 10 }}>
        <CircularProgress />
      </Box>
    );

  return (
    <Box
      sx={{ minHeight: "100vh", bgcolor: isDarkMode ? "#121212" : "#f9f9f9" }}
    >
      <Navbar isDarkMode={isDarkMode} toggleTheme={toggleTheme} />

      <Container maxWidth="lg" sx={{ mt: 4, pb: 5 }}>
        <Breadcrumbs
          separator={<NavigateNextIcon fontSize="small" />}
          sx={{ mb: 3 }}
        >
          <Link
            underline="hover"
            color="inherit"
            onClick={() => navigate("/decks")}
            sx={{ cursor: "pointer" }}
          >
            {t("navbar.myDecks")}
          </Link>
          <Typography color="text.primary">{deck?.title}</Typography>
        </Breadcrumbs>

        {pageError && (
          <Alert
            severity="error"
            onClose={() => setPageError("")}
            sx={{ mb: 3, borderRadius: 2 }}
          >
            {pageError}
          </Alert>
        )}

        <Paper
          elevation={0}
          sx={{
            p: 4,
            borderRadius: 4,
            mb: 4,
            border: "1px solid",
            borderColor: "divider",
            display: "flex",
            flexDirection: { xs: "column", md: "row" },
            justifyContent: "space-between",
            alignItems: { xs: "flex-start", md: "center" },
            gap: 3,
          }}
        >
          <Box sx={{ flex: 1 }}>
            <Box sx={{ display: "flex", alignItems: "center", gap: 1, mb: 1 }}>
              <Typography variant="h3" fontWeight="800">
                {deck?.title}
              </Typography>
              <Tooltip title={t("deckDetails.editTooltip")}>
                <IconButton
                  size="small"
                  onClick={() => {
                    setDeckError("");
                    setOpenDeckModal(true);
                  }}
                  sx={{
                    opacity: 0.6,
                    "&:hover": { opacity: 1, color: "primary.main" },
                  }}
                >
                  <EditIcon fontSize="small" />
                </IconButton>
              </Tooltip>
              <Tooltip title={t("deckDetails.deleteTooltip")}>
                <IconButton
                  size="small"
                  onClick={() => setOpenDeleteDialog(true)}
                  sx={{
                    opacity: 0.6,
                    "&:hover": { opacity: 1, color: "error.main" },
                  }}
                >
                  <DeleteIcon fontSize="small" />
                </IconButton>
              </Tooltip>
            </Box>
            {deck?.description && (
              <Typography
                variant="body1"
                color="text.secondary"
                sx={{ mb: 3, maxWidth: "600px" }}
              >
                {deck.description}
              </Typography>
            )}
            <Box
              sx={{
                display: "flex",
                gap: 1.5,
                flexWrap: "wrap",
                mt: deck?.description ? 0 : 3,
              }}
            >
              <Chip
                label={`${t("deckDetails.queue")} ${deck?.newCards || 0}`}
                color="info"
                variant="soft"
              />
              <Chip
                label={`${t("deckDetails.learning")} ${deck?.learningCards || 0}`}
                color="error"
                variant="soft"
              />
              <Chip
                label={`${t("deckDetails.toReview")} ${deck?.toReview || 0}`}
                color="success"
                variant="soft"
              />
            </Box>
          </Box>

          <Box sx={{ display: "flex", gap: 2 }}>
            <Button
              variant="contained"
              size="large"
              startIcon={<PlayArrowIcon />}
              onClick={() => navigate(`/study/${id}`)}
              disabled={
                !deck?.newCards && !deck?.learningCards && !deck?.toReview
              }
              sx={{
                borderRadius: 3,
                px: 4,
                textTransform: "none",
                fontWeight: "bold",
              }}
            >
              {t("deckDetails.btnStudy")}
            </Button>
            <Button
              variant="contained"
              color="secondary"
              size="large"
              startIcon={<AutoStoriesIcon />}
              onClick={() => {
                setOpenStoryModal(true);
                setPageError("");
              }}
              disabled={cards.length === 0}
              sx={{
                borderRadius: 3,
                px: 3,
                textTransform: "none",
                fontWeight: "bold",
              }}
            >
              {t("deckDetails.btnStory")}
            </Button>
            <Button
              variant="outlined"
              size="large"
              color="primary"
              startIcon={<AddIcon />}
              onClick={() => {
                setOpenCardModal(true);
                setCardError("");
              }}
              sx={{
                borderRadius: 3,
                px: 3,
                textTransform: "none",
                fontWeight: "bold",
              }}
            >
              {t("deckDetails.btnAddCard")}
            </Button>
          </Box>
        </Paper>

        <Typography variant="h5" fontWeight="bold" sx={{ mb: 2 }}>
          {t("deckDetails.cardsCount", { count: cards.length })}
        </Typography>

        <TableContainer
          component={Paper}
          elevation={0}
          sx={{ borderRadius: 4, border: "1px solid", borderColor: "divider" }}
        >
          <Table>
            <TableHead
              sx={{
                bgcolor: isDarkMode
                  ? "rgba(255,255,255,0.05)"
                  : "rgba(0,0,0,0.02)",
              }}
            >
              <TableRow>
                <TableCell sx={{ fontWeight: "bold", width: "25%" }}>
                  {t("deckDetails.tableFront")}
                </TableCell>
                <TableCell sx={{ fontWeight: "bold", width: "25%" }}>
                  {t("deckDetails.tableBack")}
                </TableCell>
                <TableCell sx={{ fontWeight: "bold", width: "35%" }}>
                  {t("deckDetails.tableContext")}
                </TableCell>
                <TableCell
                  align="right"
                  sx={{ fontWeight: "bold", width: "15%" }}
                >
                  {t("deckDetails.tableActions")}
                </TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {cards.map((card) => {
                const hasContext = !!(
                  card.generatedContext || card.GeneratedContext
                );
                const isSimplified = !!(card.isSimplified || card.IsSimplified);
                return (
                  <TableRow key={card.id} hover>
                    <TableCell>
                      <Typography fontWeight="500">
                        {card.front || card.Front}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography color="text.secondary">
                        {card.back || card.Back}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      {hasContext ? (
                        <Box>
                          <Typography variant="body2" fontWeight="500">
                            {card.generatedContext || card.GeneratedContext}
                          </Typography>
                          <Typography
                            variant="caption"
                            color="text.secondary"
                            sx={{ fontStyle: "italic", display: "block" }}
                          >
                            {card.contextTranslation || card.ContextTranslation}
                          </Typography>
                        </Box>
                      ) : (
                        <Button
                          size="small"
                          startIcon={
                            generatingId === card.id ? (
                              <CircularProgress size={14} />
                            ) : (
                              <AutoAwesomeIcon />
                            )
                          }
                          onClick={() => handleGenerateContext(card)}
                          disabled={generatingId === card.id}
                          sx={{ textTransform: "none", borderRadius: 2 }}
                        >
                          {t("deckDetails.btnGenAi")}
                        </Button>
                      )}
                    </TableCell>
                    <TableCell align="right" sx={{ whiteSpace: "nowrap" }}>
                      {hasContext && !isSimplified && (
                        <Tooltip title={t("deckDetails.tooltipSimplify")}>
                          <IconButton
                            size="small"
                            color="info"
                            onClick={() => handleSimplifyCard(card.id)}
                            disabled={simplifyingId === card.id}
                          >
                            {simplifyingId === card.id ? (
                              <CircularProgress size={20} color="inherit" />
                            ) : (
                              <AutoFixHighIcon fontSize="small" />
                            )}
                          </IconButton>
                        </Tooltip>
                      )}
                      {hasContext && (
                        <Tooltip title={t("deckDetails.tooltipClear")}>
                          <IconButton
                            size="small"
                            color="warning"
                            onClick={() => handleClearContext(card)}
                            disabled={clearingId === card.id}
                          >
                            {clearingId === card.id ? (
                              <CircularProgress size={20} color="inherit" />
                            ) : (
                              <FormatClearIcon fontSize="small" />
                            )}
                          </IconButton>
                        </Tooltip>
                      )}
                      <Tooltip title={t("deckDetails.tooltipDelete")}>
                        <IconButton
                          size="small"
                          color="error"
                          onClick={() => handleDeleteCard(card.id)}
                        >
                          <DeleteIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </TableCell>
                  </TableRow>
                );
              })}
              {cards.length === 0 && (
                <TableRow>
                  <TableCell colSpan={4} align="center" sx={{ py: 10 }}>
                    {t("deckDetails.empty")}
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>
      </Container>

      <GenerateStoryModal
        key={openStoryModal ? "story-open" : "story-closed"}
        open={openStoryModal}
        onClose={() => setOpenStoryModal(false)}
        onSubmit={handleGenerateStory}
        isGenerating={isGeneratingStory}
      />
      <DeleteConfirmDialog
        key={openDeleteDialog ? "del-open" : "del-closed"}
        open={openDeleteDialog}
        onClose={() => setOpenDeleteDialog(false)}
        onConfirm={confirmDeleteDeck}
        itemName={deck?.title}
      />
      <AddCardModal
        key={openCardModal ? "add-open" : "add-closed"}
        open={openCardModal}
        onClose={() => setOpenCardModal(false)}
        onSubmit={handleSaveCard}
        isSaving={isSavingCard}
        serverError={cardError}
      />
      <EditDeckModal
        key={openDeckModal ? "edit-open" : "edit-closed"}
        open={openDeckModal}
        onClose={() => setOpenDeckModal(false)}
        onSubmit={handleSaveDeck}
        initialData={deck}
        isSaving={isSavingDeck}
        serverError={deckError}
      />
    </Box>
  );
};
