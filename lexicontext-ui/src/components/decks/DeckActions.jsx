import { Box, Button } from "@mui/material";
import PlayArrowIcon from "@mui/icons-material/PlayArrow";
import AutoStoriesIcon from "@mui/icons-material/AutoStories";
import AddIcon from "@mui/icons-material/Add";
import SchoolIcon from "@mui/icons-material/School";
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
  fromClassroom
}) => {
  const { t } = useTranslation();
  const navigate = useNavigate();

  return (
    <Box sx={{ display: "flex", gap: 2, flexWrap: "wrap" }}>
      {/* Кнопка Вчити є завжди */}
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

      {/* Історію ховаємо, якщо ми зайшли з класу */}
      {!fromClassroom && (
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
      )}

      {/* Додавання карток залишаємо власнику завжди, щоб міг наповнювати колоду навіть з класу */}
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

      {/* Кнопку "Додати до класу" ховаємо, якщо ми ВЖЕ в класі */}
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