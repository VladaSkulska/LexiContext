import {
  TableContainer, Table, TableHead, TableRow, TableCell, TableBody,
  Typography, Box, Button, IconButton, Tooltip, CircularProgress, Paper
} from "@mui/material";
import AutoAwesomeIcon from "@mui/icons-material/AutoAwesome";
import AutoFixHighIcon from "@mui/icons-material/AutoFixHigh";
import FormatClearIcon from "@mui/icons-material/FormatClear";
import DeleteIcon from "@mui/icons-material/Delete";
import { useTranslation } from "react-i18next";

export const DeckCardsTable = ({
  cards,
  isEditingAllowed,
  isDarkMode,
  generatingId,
  simplifyingId,
  clearingId,
  onGenerateContext,
  onSimplifyCard,
  onClearContext,
  onDeleteCard
}) => {
  const { t } = useTranslation();

  return (
    <TableContainer component={Paper} elevation={0} sx={{ borderRadius: 4, border: "1px solid", borderColor: "divider" }}>
      <Table>
        <TableHead sx={{ bgcolor: isDarkMode ? "rgba(255,255,255,0.05)" : "rgba(0,0,0,0.02)" }}>
          <TableRow>
            <TableCell sx={{ fontWeight: "bold", width: "25%" }}>{t("deckDetails.tableFront")}</TableCell>
            <TableCell sx={{ fontWeight: "bold", width: "25%" }}>{t("deckDetails.tableBack")}</TableCell>
            <TableCell sx={{ fontWeight: "bold", width: "35%" }}>{t("deckDetails.tableContext")}</TableCell>
            {isEditingAllowed && (
              <TableCell align="right" sx={{ fontWeight: "bold", width: "15%" }}>{t("deckDetails.tableActions")}</TableCell>
            )}
          </TableRow>
        </TableHead>
        <TableBody>
          {cards.map((card) => {
            const hasContext = !!(card.generatedContext || card.GeneratedContext);
            const isSimplified = !!(card.isSimplified || card.IsSimplified);
            return (
              <TableRow key={card.id} hover>
                <TableCell>
                  <Typography fontWeight="500">{card.front || card.Front}</Typography>
                </TableCell>
                <TableCell>
                  <Typography color="text.secondary">{card.back || card.Back}</Typography>
                </TableCell>
                <TableCell>
                  {hasContext ? (
                    <Box>
                      <Typography variant="body2" fontWeight="500">{card.generatedContext || card.GeneratedContext}</Typography>
                      <Typography variant="caption" color="text.secondary" sx={{ fontStyle: "italic", display: "block" }}>
                        {card.contextTranslation || card.ContextTranslation}
                      </Typography>
                    </Box>
                  ) : (
                    isEditingAllowed && (
                      <Button
                        size="small"
                        startIcon={generatingId === card.id ? <CircularProgress size={14} /> : <AutoAwesomeIcon />}
                        onClick={() => onGenerateContext(card)}
                        disabled={generatingId === card.id}
                        sx={{ textTransform: "none", borderRadius: 2 }}
                      >
                        {t("deckDetails.btnGenAi")}
                      </Button>
                    )
                  )}
                </TableCell>
                {isEditingAllowed && (
                  <TableCell align="right" sx={{ whiteSpace: "nowrap" }}>
                    {hasContext && !isSimplified && (
                      <Tooltip title={t("deckDetails.tooltipSimplify")}>
                        <IconButton size="small" color="info" onClick={() => onSimplifyCard(card.id)} disabled={simplifyingId === card.id}>
                          {simplifyingId === card.id ? <CircularProgress size={20} color="inherit" /> : <AutoFixHighIcon fontSize="small" />}
                        </IconButton>
                      </Tooltip>
                    )}
                    {hasContext && (
                      <Tooltip title={t("deckDetails.tooltipClear")}>
                        <IconButton size="small" color="warning" onClick={() => onClearContext(card)} disabled={clearingId === card.id}>
                          {clearingId === card.id ? <CircularProgress size={20} color="inherit" /> : <FormatClearIcon fontSize="small" />}
                        </IconButton>
                      </Tooltip>
                    )}
                    <Tooltip title={t("deckDetails.tooltipDelete")}>
                      <IconButton size="small" color="error" onClick={() => onDeleteCard(card.id)}>
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  </TableCell>
                )}
              </TableRow>
            );
          })}
          {cards.length === 0 && (
            <TableRow>
              <TableCell colSpan={isEditingAllowed ? 4 : 3} align="center" sx={{ py: 10 }}>
                {t("deckDetails.empty")}
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>
    </TableContainer>
  );
};