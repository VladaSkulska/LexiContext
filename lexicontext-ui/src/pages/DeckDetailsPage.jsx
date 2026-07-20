import { useState, useEffect, useCallback } from "react";
import { useParams, useNavigate, useLocation } from "react-router-dom";
import { Box, Container, Typography, Paper, CircularProgress, Breadcrumbs, Link, Alert } from "@mui/material";
import NavigateNextIcon from "@mui/icons-material/NavigateNext";

import { Navbar } from "../components/common/Navbar";
import axiosClient from "../api/axiosClient";
import { extractErrorMessage } from "../utils/errorHandler";
import { useTranslation } from "react-i18next";

import { DeckHeader } from "../components/decks/DeckHeader";
import { DeckStatistics } from "../components/decks/DeckStatistics";
import { DeckActions } from "../components/decks/DeckActions";
import { DeckCardsTable } from "../components/decks/DeckCardsTable";

import { GenerateStoryModal } from "../components/stories/GenerateStoryModal";
import { DeleteConfirmDialog } from "../components/decks/DeleteConfirmDialog";
import { AddCardModal } from "../components/cards/AddCardModal";
import { EditDeckModal } from "../components/decks/EditDeckModal";
import { AddDeckToClassroomModal } from "../components/decks/AddDeckToClassroomModal";

export const DeckDetailsPage = ({ isDarkMode, toggleTheme }) => {
  const { id } = useParams();
  const navigate = useNavigate();
  const location = useLocation();
  const { t } = useTranslation();

  const [deck, setDeck] = useState(null);
  const [cards, setCards] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [pageError, setPageError] = useState("");
  
  const [userRole, setUserRole] = useState("Student"); 
  const [currentUserId, setCurrentUserId] = useState(null); 

  const [openCardModal, setOpenCardModal] = useState(false);
  const [openDeckModal, setOpenDeckModal] = useState(false);
  const [openDeleteDialog, setOpenDeleteDialog] = useState(false);
  const [openStoryModal, setOpenStoryModal] = useState(false);
  const [openClassroomModal, setOpenClassroomModal] = useState(false); 

  const [isSavingCard, setIsSavingCard] = useState(false);
  const [cardError, setCardError] = useState("");
  const [isSavingDeck, setIsSavingDeck] = useState(false);
  const [deckError, setDeckError] = useState("");
  const [isGeneratingStory, setIsGeneratingStory] = useState(false);

  const [simplifyingId, setSimplifyingId] = useState(null);
  const [generatingId, setGeneratingId] = useState(null);
  const [clearingId, setClearingId] = useState(null);

  useEffect(() => {
    const token = localStorage.getItem("token");
    if (token) {
      try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));

        const decoded = JSON.parse(jsonPayload);
        const role = decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || decoded.role || decoded.Role || "Student";
        const userId = decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] || decoded.sub || decoded.nameid || decoded.id;
        
        setUserRole(role);
        setCurrentUserId(userId);
      } catch (error) {
        console.error("Token parsing error:", error);
      }
    }
  }, []);

  const loadData = useCallback(async () => {
    setIsLoading(true);
    setPageError("");
    
    try {
      const deckRes = await axiosClient.get(`/Decks/${id}`);
      setDeck(deckRes.data);
      
      try {
        const cardsRes = await axiosClient.get(`/Cards/deck/${id}`);
        setCards(cardsRes.data);
      } catch (cardError) {
        console.error("Помилка завантаження карток (500):", cardError);
        setPageError("Колоду завантажено, але бекенд не зміг віддати картки (Помилка 500).");
        setCards([]); 
      }

    } catch (error) {
      console.error("Помилка завантаження колоди:", error);
      setPageError(extractErrorMessage(error));
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

  const isEditingAllowed = Boolean(
    deck && 
    currentUserId && 
    (deck.createdId === currentUserId || deck.CreatedId === currentUserId)
  );

  // ВИРІШУЄМО, ЩО ПІДСВІЧУВАТИ:
  // Якщо студент і не власник -> це точно з класу.
  // Якщо перейшли сюди з ClassroomDetailsPage -> state.fromClassroom буде true
  const isFromClassroom = (userRole === "Student" && !isEditingAllowed) || location.state?.fromClassroom;
  
  const goBackPath = isFromClassroom ? "/classrooms" : "/decks";
  const backLabel = isFromClassroom ? t("navbar.classrooms") : t("navbar.myDecks");

  if (isLoading)
    return (
      <Box sx={{ display: "flex", justifyContent: "center", mt: 10 }}>
        <CircularProgress />
      </Box>
    );

  return (
    <Box sx={{ minHeight: "100vh", bgcolor: isDarkMode ? "#121212" : "#f9f9f9" }}>
      {/* ПЕРЕДАЄМО overrideActivePath */}
      <Navbar 
        isDarkMode={isDarkMode} 
        toggleTheme={toggleTheme} 
        overrideActivePath={isFromClassroom ? "/classrooms" : "/decks"} 
      />

      <Container maxWidth="lg" sx={{ mt: 4, pb: 5 }}>
        <Breadcrumbs separator={<NavigateNextIcon fontSize="small" />} sx={{ mb: 3 }}>
          <Link underline="hover" color="inherit" onClick={() => navigate(goBackPath)} sx={{ cursor: "pointer" }}>
            {backLabel}
          </Link>
          <Typography color="text.primary">{deck?.title || t("dashboard.untitled")}</Typography>
        </Breadcrumbs>

        {pageError && (
          <Alert severity="error" onClose={() => setPageError("")} sx={{ mb: 3, borderRadius: 2 }}>
            {pageError}
          </Alert>
        )}

        <Paper elevation={0} sx={{ p: 4, borderRadius: 4, mb: 4, border: "1px solid", borderColor: "divider", display: "flex", flexDirection: { xs: "column", md: "row" }, justifyContent: "space-between", alignItems: { xs: "flex-start", md: "center" }, gap: 3 }}>
          <Box sx={{ flex: 1 }}>
            <DeckHeader 
              deck={deck} 
              isEditingAllowed={isEditingAllowed} 
              onEdit={() => { setDeckError(""); setOpenDeckModal(true); }}
              onDelete={() => setOpenDeleteDialog(true)}
            />
            <DeckStatistics deck={deck} />
          </Box>

          <DeckActions 
            deck={deck}
            cardsCount={cards.length}
            isEditingAllowed={isEditingAllowed}
            userRole={userRole}
            fromClassroom={isFromClassroom}
            onOpenStoryModal={() => { setOpenStoryModal(true); setPageError(""); }}
            onOpenCardModal={() => { setOpenCardModal(true); setCardError(""); }}
            onOpenClassroomModal={() => setOpenClassroomModal(true)}
          />
        </Paper>

        <Typography variant="h5" fontWeight="bold" sx={{ mb: 2 }}>
          {t("deckDetails.cardsCount", { count: cards.length })}
        </Typography>

        <DeckCardsTable 
          cards={cards}
          isEditingAllowed={isEditingAllowed}
          isDarkMode={isDarkMode}
          generatingId={generatingId}
          simplifyingId={simplifyingId}
          clearingId={clearingId}
          onGenerateContext={handleGenerateContext}
          onSimplifyCard={handleSimplifyCard}
          onClearContext={handleClearContext}
          onDeleteCard={handleDeleteCard}
        />
      </Container>

      <GenerateStoryModal open={openStoryModal} onClose={() => setOpenStoryModal(false)} onSubmit={handleGenerateStory} isGenerating={isGeneratingStory} />
      <DeleteConfirmDialog open={openDeleteDialog} onClose={() => setOpenDeleteDialog(false)} onConfirm={confirmDeleteDeck} itemName={deck?.title || t("dashboard.untitled")} />
      <AddCardModal open={openCardModal} onClose={() => setOpenCardModal(false)} onSubmit={handleSaveCard} isSaving={isSavingCard} serverError={cardError} />
      <EditDeckModal open={openDeckModal} onClose={() => setOpenDeckModal(false)} onSubmit={handleSaveDeck} initialData={deck} isSaving={isSavingDeck} serverError={deckError} />
      <AddDeckToClassroomModal open={openClassroomModal} onClose={() => setOpenClassroomModal(false)} deckId={id} />
    </Box>
  );
};