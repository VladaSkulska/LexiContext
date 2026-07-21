import { Box, Typography, Tooltip, IconButton } from "@mui/material";
import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/Delete";
import { useTranslation } from "react-i18next";

export const DeckHeader = ({ deck, isEditingAllowed, onEdit, onDelete }) => {
  const { t } = useTranslation();

  const deckTitle = deck?.title || deck?.Title || t("dashboard.untitled");
  const deckDesc = deck?.description || deck?.Description;

  return (
    <Box sx={{ flex: 1, minWidth: 0 }}> {/* minWidth: 0 важливо для flex-елементів, щоб текст обрізався */}
      <Box sx={{ display: "flex", alignItems: "center", gap: 1, mb: 1, flexWrap: "nowrap" }}>
        <Typography 
          variant="h3" 
          fontWeight="800"
          sx={{
            overflow: "hidden",
            textOverflow: "ellipsis",
            whiteSpace: "nowrap"
          }}
        >
          {deckTitle}
        </Typography>
        
        {isEditingAllowed && (
          <Box sx={{ display: "flex", flexShrink: 0 }}> {/* flexShrink: 0 гарантує, що кнопки не стискатимуться */}
            <Tooltip title={t("deckDetails.editTooltip", { defaultValue: "Редагувати" })}>
              <IconButton
                size="small"
                onClick={onEdit}
                sx={{ opacity: 0.6, "&:hover": { opacity: 1, color: "primary.main" } }}
              >
                <EditIcon fontSize="small" />
              </IconButton>
            </Tooltip>
            <Tooltip title={t("deckDetails.deleteTooltip", { defaultValue: "Видалити" })}>
              <IconButton
                size="small"
                onClick={onDelete}
                sx={{ opacity: 0.6, "&:hover": { opacity: 1, color: "error.main" } }}
              >
                <DeleteIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          </Box>
        )}
      </Box>
      {deckDesc && (
        <Typography
          variant="body1"
          color="text.secondary"
          sx={{ mb: 3, maxWidth: "600px" }}
        >
          {deckDesc}
        </Typography>
      )}
    </Box>
  );
};