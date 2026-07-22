import { Box, Button, CircularProgress } from "@mui/material";
import PlayArrowIcon from "@mui/icons-material/PlayArrow";
import AutoStoriesIcon from "@mui/icons-material/AutoStories";
import AddIcon from "@mui/icons-material/Add";
import SchoolIcon from "@mui/icons-material/School";
import ContentCopyIcon from "@mui/icons-material/ContentCopy";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";

export const DeckActions = ({
  deck,
  cardsCount,
  isEditingAllowed,
  userRole,
  onOpenStoryModal,
  onOpenCardModal,
  onOpenClassroomModal,
  fromClassroom,
  isForking,
  onForkDeck
}) => {
  const { t } = useTranslation();
  const navigate = useNavigate();

  return (
    <Box sx={{ display: "flex", gap: 2, flexWrap: "wrap" }}>
      {!(userRole === "Teacher" && fromClassroom) && (
        <Button
          variant="contained"
          size="large"
          startIcon={<PlayArrowIcon />}
          onClick={(e) => {
            e.stopPropagation();
            navigate(`/study/${deck.id}`, { state: { fromClassroom } });
          }}
          disabled={!deck?.newCards && !deck?.learningCards && !deck?.toReview}
          sx={{ borderRadius: 3, px: 4, textTransform: "none", fontWeight: "bold" }}
        >
          {t("deckDetails.btnStudy")}
        </Button>
      )}

      <Button
        variant="contained"
        color="secondary"
        size="large"
        startIcon={<AutoStoriesIcon />}
        onClick={onOpenStoryModal}
        disabled={cardsCount === 0}
        sx={{ borderRadius: 3, px: 3, textTransform: "none", fontWeight: "bold" }}
      >
        {t("deckDetails.btnStory")}
      </Button>

      {fromClassroom && userRole === "Student" && !isEditingAllowed && (
        <Button
          variant="outlined"
          color="info"
          size="large"
          startIcon={isForking ? <CircularProgress size={20} /> : <ContentCopyIcon />}
          onClick={onForkDeck}
          disabled={isForking}
          sx={{ borderRadius: 3, px: 3, textTransform: "none", fontWeight: "bold" }}
        >
          {t("deckDetails.btnFork", "Copy to My Decks")}
        </Button>
      )}

      {isEditingAllowed && (
        <Button
          variant="outlined"
          size="large"
          color="primary"
          startIcon={<AddIcon />}
          onClick={onOpenCardModal}
          sx={{ borderRadius: 3, px: 3, textTransform: "none", fontWeight: "bold" }}
        >
          {t("deckDetails.btnAddCard")}
        </Button>
      )}

      {userRole === "Teacher" && isEditingAllowed && !fromClassroom && (
        <Button
          variant="outlined"
          size="large"
          color="info"
          startIcon={<SchoolIcon />}
          onClick={onOpenClassroomModal}
          sx={{ borderRadius: 3, px: 3, textTransform: "none", fontWeight: "bold" }}
        >
          {t("deckDetails.btnAddToClass")}
        </Button>
      )}
    </Box>
  );
};