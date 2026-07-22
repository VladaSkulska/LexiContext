import { useState, useEffect, useCallback } from "react";
import { useParams, useNavigate, useLocation } from "react-router-dom";
import {
  Box,
  Typography,
  Button,
  Container,
  Card,
  CardContent,
  CardActions,
  Chip,
  Divider,
  CircularProgress,
  Paper,
  LinearProgress,
  TextField,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Checkbox,
  IconButton,
  Tooltip,
  Snackbar,
  Alert
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import PlayArrowIcon from "@mui/icons-material/PlayArrow";
import AutoStoriesIcon from "@mui/icons-material/AutoStories";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import PeopleIcon from "@mui/icons-material/People";
import StyleIcon from "@mui/icons-material/Style";
import DeleteIcon from "@mui/icons-material/Delete";
import AssignmentIcon from "@mui/icons-material/Assignment";
import RemoveCircleOutlineIcon from "@mui/icons-material/RemoveCircleOutline";
import ExitToAppIcon from "@mui/icons-material/ExitToApp";
import ContentCopyIcon from "@mui/icons-material/ContentCopy";
import CheckIcon from "@mui/icons-material/Check";
import { Navbar } from "../components/common/Navbar";
import { GenerateStoryModal } from "../components/stories/GenerateStoryModal";
import { CreateDeckModal } from "../components/decks/CreateDeckModal";
import { SelectExistingDecksModal } from "../components/decks/SelectExistingDecksModal";
import { DeleteConfirmDialog } from "../components/decks/DeleteConfirmDialog";
import axiosClient from "../api/axiosClient";
import { useTranslation } from "react-i18next";
import { getActiveRole } from "../utils/auth";

export const ClassroomDetailsPage = ({ isDarkMode, toggleTheme }) => {
  const { id: classroomId } = useParams();
  const navigate = useNavigate();
  const location = useLocation();
  const { t, i18n } = useTranslation();

  const [classroom, setClassroom] = useState(
    location.state?.classroom || { name: t("classroomDetails.loading"), studentsCount: 0, decksCount: 0, joinCode: "" }
  );

  const [decks, setDecks] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [averageProgress, setAverageProgress] = useState(0);

  const [isStoryModalOpen, setIsStoryModalOpen] = useState(false);
  const [selectedDeckId, setSelectedDeckId] = useState(null);
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [isCreatingDeck, setIsCreatingDeck] = useState(false);
  const [userRole, setUserRole] = useState(() => getActiveRole());
  const [isAddExistingModalOpen, setIsAddExistingModalOpen] = useState(false);

  const [tasks, setTasks] = useState([]);
  const [isLoadingTasks, setIsLoadingTasks] = useState(true);
  const [newTaskText, setNewTaskText] = useState("");

  const [codeCopied, setCodeCopied] = useState(false);
  const [snackbar, setSnackbar] = useState({ open: false, message: "", severity: "success" });

  const [confirmDialog, setConfirmDialog] = useState({ open: false, title: "", content: "", action: null });
  const [isProcessingAction, setIsProcessingAction] = useState(false);

  const fetchHomework = useCallback(async () => {
    setIsLoadingTasks(true);
    try {
      const endpoint =
        userRole === "Teacher"
          ? `/Classrooms/${classroomId}/homework/teacher`
          : `/Classrooms/${classroomId}/homework/student`;
      const response = await axiosClient.get(endpoint);
      setTasks(response.data || []);
    } catch (error) {
      console.error("Error loading homework:", error);
      setTasks([]);
    } finally {
      setIsLoadingTasks(false);
    }
  }, [classroomId, userRole]);

  useEffect(() => {
    if (userRole) fetchHomework();
  }, [fetchHomework, userRole]);

  const handleAddExistingDecks = async (selectedDeckIds) => {
    try {
      await Promise.all(
        selectedDeckIds.map((deckId) =>
          axiosClient.post(`/Classrooms/${classroomId}/decks/${deckId}`)
        )
      );

      const response = await axiosClient.get(`/Classrooms/${classroomId}/decks?t=${Date.now()}`);
      setDecks(response.data);
      setClassroom((prev) => ({ ...prev, decksCount: response.data.length }));

      setIsAddExistingModalOpen(false);
      setSnackbar({ 
        open: true, 
        message: t("classroomDetails.addExistingSuccess"), 
        severity: "success" 
      });
    } catch (error) {
      console.error("Error adding existing decks:", error);
      setSnackbar({ 
        open: true, 
        message: error.response?.data?.message || t("classroomDetails.addExistingError"), 
        severity: "error" 
      });
    }
  };

  useEffect(() => {
    const fetchClassroomDetails = async () => {
      try {
        const endpoint = userRole === "Teacher" 
          ? `/Classrooms/teacher?t=${Date.now()}` 
          : `/Classrooms/student?t=${Date.now()}`;
          
        const response = await axiosClient.get(endpoint);
        const currentClass = response.data.find((c) => c.id === classroomId);

        if (currentClass) {
          setClassroom(currentClass);
        } else {
          setClassroom({ name: t("classroomDetails.notFound"), studentsCount: 0, decksCount: 0, joinCode: "" });
        }
      } catch (error) {
        console.error("Error loading classroom info:", error);
      }
    };
    if (userRole) fetchClassroomDetails();
  }, [classroomId, userRole, t]);

  useEffect(() => {
    if (userRole === "Teacher") {
      const fetchProgress = async () => {
        try {
          const response = await axiosClient.get(`/Statistics/classroom/${classroomId}/progress?t=${Date.now()}`);
          setAverageProgress(response.data);
        } catch (error) {
          console.error("Error loading class progress:", error);
        }
      };
      fetchProgress();
    }
  }, [userRole, classroomId]);

  useEffect(() => {
    const fetchClassroomDecks = async () => {
      setIsLoading(true);
      try {
        const response = await axiosClient.get(`/Classrooms/${classroomId}/decks?t=${Date.now()}`);
        setDecks(response.data);
      } catch (error) {
        console.error("Error loading classroom decks:", error);
      } finally {
        setIsLoading(false);
      }
    };
    fetchClassroomDecks();
  }, [classroomId]);

  const handleCopyCode = () => {
    if (!classroom?.joinCode) return;
    navigator.clipboard.writeText(classroom.joinCode).then(() => {
      setCodeCopied(true);
      setTimeout(() => setCodeCopied(false), 2000);
    });
  };

  const handleCreateDeck = async (formData) => {
    setIsCreatingDeck(true);
    try {
      const payload = { ...formData, classroomId };
      const response = await axiosClient.post("/Decks", payload);
      setDecks((prev) => [response.data, ...prev]);
      setClassroom((prev) => ({ ...prev, decksCount: (prev.decksCount || 0) + 1 }));
      setIsCreateModalOpen(false);
      setSnackbar({ open: true, message: t("common.save") + " ✅", severity: "success" });
    } catch (error) {
      console.error("Error creating deck:", error);
      setSnackbar({ open: true, message: error.response?.data?.message || t("classroomDetails.errorDelete"), severity: "error" });
    } finally {
      setIsCreatingDeck(false);
    }
  };

  const executeDialogAction = async () => {
    if (!confirmDialog.action) return;
    setIsProcessingAction(true);
    await confirmDialog.action();
    setIsProcessingAction(false);
    setConfirmDialog({ ...confirmDialog, open: false });
  };

  const handleRemoveDeckFromClass = (deckId) => {
    setConfirmDialog({
      open: true,
      title: t("classroomDetails.removeFromClass"),
      content: t("classroomDetails.confirmRemoveDeck"),
      action: async () => {
        try {
          await axiosClient.delete(`/Classrooms/${classroomId}/decks/${deckId}`);
          setDecks((prev) => prev.filter((d) => d.id !== deckId));
          setClassroom((prev) => ({ ...prev, decksCount: Math.max(0, (prev.decksCount || 0) - 1) }));
          setSnackbar({ open: true, message: t("common.delete") + " ✅", severity: "success" });
        } catch (error) {
          setSnackbar({ open: true, message: error.response?.data?.message || t("classroomDetails.errorDelete"), severity: "error" });
        }
      }
    });
  };

  const handleDeleteClassroom = () => {
    setConfirmDialog({
      open: true,
      title: t("classroomDetails.deleteClass"),
      content: t("classroomDetails.confirmDeleteClass"),
      action: async () => {
        try {
          await axiosClient.delete(`/Classrooms/${classroomId}`);
          navigate("/classrooms", { state: { refresh: Date.now() } });
        } catch (error) {
          setSnackbar({ open: true, message: t("classroomDetails.errorDelete"), severity: "error" });
        }
      }
    });
  };

  const handleLeaveClassroom = () => {
    setConfirmDialog({
      open: true,
      title: t("classroomDetails.leaveClass"),
      content: t("classroomDetails.confirmLeaveClass"),
      action: async () => {
        try {
          await axiosClient.post(`/Classrooms/leave`, { classroomId });
          navigate("/classrooms", { state: { refresh: Date.now() } });
        } catch (error) {
          setSnackbar({ open: true, message: t("classroomDetails.errorLeave"), severity: "error" });
        }
      }
    });
  };

  const handleAddTask = async () => {
    if (!newTaskText.trim()) return;
    try {
      await axiosClient.post(`/Classrooms/${classroomId}/homework`, { text: newTaskText.trim() });
      setNewTaskText("");
      setSnackbar({ open: true, message: t("classroomDetails.taskAdded") || "Завдання додано!", severity: "success" });
      await fetchHomework(); 
    } catch (error) {
      console.error("Error adding homework:", error);
      setSnackbar({ open: true, message: error.response?.data?.message || t("classroomDetails.taskError") || "Помилка", severity: "error" });
    }
  };

  const handleDeleteTask = (task) => {
    setConfirmDialog({
      open: true,
      title: t("common.delete"),
      content: t("classroomDetails.confirmDeleteTask", "Ви дійсно хочете видалити це завдання?"),
      action: async () => {
        try {
          const targetId = task.groupTaskId || task.id;
          await axiosClient.delete(`/Classrooms/homework/${targetId}`);
          setTasks((prev) => prev.filter((t) => t.groupTaskId !== targetId && t.id !== targetId));
          setSnackbar({ open: true, message: t("common.delete") + " ✅", severity: "success" });
        } catch (error) {
          setSnackbar({ open: true, message: error.response?.data?.message || t("classroomDetails.errorDelete"), severity: "error" });
        }
      }
    });
  };

  const handleToggleTask = async (task) => {
    if (userRole === "Teacher") return;
    try {
      const targetId = task.groupTaskId || task.id;
      if (!targetId) throw new Error("Task ID is missing");

      await axiosClient.patch(`/Classrooms/homework/${targetId}/toggle`);
      setTasks((prev) =>
        prev.map((t) => (t.groupTaskId === targetId || t.id === targetId ? { ...t, isCompleted: !t.isCompleted } : t))
      );
    } catch (error) {
      console.error("Error toggling task:", error);
      setSnackbar({ open: true, message: error.response?.data?.message || "Не вдалося змінити статус", severity: "error" });
    }
  };

  const classroomName = classroom?.name || t("classroomDetails.loading");

  return (
    <Box sx={{ minHeight: "100vh", bgcolor: isDarkMode ? "#121212" : "#f4f5f7" }}>
      <Navbar isDarkMode={isDarkMode} toggleTheme={toggleTheme} />

      <Container maxWidth="lg" sx={{ mt: 4, pb: 6 }}>
        {/* ── Header ── */}
        <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 4, flexWrap: "wrap", gap: 2 }}>
          <Box sx={{ display: "flex", alignItems: "center", gap: 1.5, flexWrap: "wrap" }}>
            <Button
              startIcon={<ArrowBackIcon />}
              onClick={() => navigate("/classrooms")}
              color="inherit"
              sx={{ textTransform: "none", borderRadius: 2 }}
            >
              {t("classroomDetails.back")}
            </Button>
            <Typography variant="h4" component="h1" fontWeight="bold">
              {classroomName}
            </Typography>

            {classroom?.joinCode && (
              <Tooltip title={codeCopied ? (t("common.copied") || "Скопійовано!") : (t("common.clickToCopy") || "Натисни, щоб скопіювати")}>
                <Chip
                  icon={codeCopied ? <CheckIcon sx={{ fontSize: 16 }} /> : <ContentCopyIcon sx={{ fontSize: 16 }} />}
                  label={`${t("classrooms.codeLabel")} ${classroom.joinCode}`}
                  color={codeCopied ? "success" : "info"}
                  variant="outlined"
                  size="small"
                  onClick={handleCopyCode}
                  sx={{
                    ml: 1,
                    fontWeight: "bold",
                    fontFamily: "monospace",
                    cursor: "pointer",
                    transition: "all 0.2s",
                    "&:hover": { opacity: 0.8 },
                  }}
                />
              </Tooltip>
            )}
          </Box>

          <Box sx={{ display: "flex", gap: 2 }}>
            {userRole === "Teacher" ? (
              <Button
                variant="outlined"
                color="error"
                startIcon={<DeleteIcon />}
                onClick={handleDeleteClassroom}
                sx={{ textTransform: "none", borderRadius: 2, fontWeight: "bold" }}
              >
                {t("classroomDetails.deleteClass")}
              </Button>
            ) : (
              <Button
                variant="outlined"
                color="error"
                startIcon={<ExitToAppIcon />}
                onClick={handleLeaveClassroom}
                sx={{ textTransform: "none", borderRadius: 2, fontWeight: "bold" }}
              >
                {t("classroomDetails.leaveClass")}
              </Button>
            )}
          </Box>
        </Box>

        <Box
          sx={{
            display: "grid",
            gridTemplateColumns: { xs: "1fr", md: userRole === "Teacher" ? "7fr 5fr" : "1fr" },
            gap: 3,
            mb: 5,
          }}
        >
          {userRole === "Teacher" && (
            <Paper
              elevation={isDarkMode ? 2 : 1}
              sx={{
                p: 3,
                borderRadius: 4,
                bgcolor: isDarkMode ? "#1e1e1e" : "#fff",
                border: "1px solid",
                borderColor: isDarkMode ? "rgba(255,255,255,0.05)" : "rgba(0,0,0,0.05)",
                display: "flex",
                flexDirection: "column",
                justifyContent: "space-between",
                height: 320,
              }}
            >
              <Typography variant="h6" fontWeight="bold">
                {t("classroomDetails.overview")}
              </Typography>

              <Box sx={{ display: "flex", gap: 4, my: "auto" }}>
                <Box sx={{ display: "flex", alignItems: "center", gap: 1.5 }}>
                  <Box sx={{ p: 1.5, borderRadius: 2, bgcolor: "rgba(33,150,243,0.1)", color: "#2196f3" }}>
                    <PeopleIcon />
                  </Box>
                  <Box>
                    <Typography variant="body2" color="text.secondary">
                      {t("classroomDetails.students")}
                    </Typography>
                    <Typography variant="h5" fontWeight="bold">
                      {classroom?.studentsCount ?? 0}
                    </Typography>
                  </Box>
                </Box>

                <Box sx={{ display: "flex", alignItems: "center", gap: 1.5 }}>
                  <Box sx={{ p: 1.5, borderRadius: 2, bgcolor: "rgba(255,64,129,0.1)", color: "#ff4081" }}>
                    <StyleIcon />
                  </Box>
                  <Box>
                    <Typography variant="body2" color="text.secondary">
                      {t("classroomDetails.decks")}
                    </Typography>
                    <Typography variant="h5" fontWeight="bold">
                      {decks ? decks.length : 0}
                    </Typography>
                  </Box>
                </Box>
              </Box>

              <Box>
                <Box sx={{ display: "flex", justifyContent: "space-between", mb: 1 }}>
                  <Typography variant="body2" color="text.secondary">
                    {t("classroomDetails.progress")}
                  </Typography>
                  <Typography variant="body2" fontWeight="bold" color="success.main">
                    {averageProgress}%
                  </Typography>
                </Box>
                <LinearProgress variant="determinate" value={averageProgress} color="success" sx={{ height: 8, borderRadius: 4 }} />
              </Box>
            </Paper>
          )}

          <Paper
            elevation={isDarkMode ? 2 : 1}
            sx={{
              p: 3,
              borderRadius: 4,
              bgcolor: isDarkMode ? "#1e1e1e" : "#fff",
              border: "1px solid",
              borderColor: isDarkMode ? "rgba(255,255,255,0.05)" : "rgba(0,0,0,0.05)",
              display: "flex",
              flexDirection: "column",
              height: 320,
            }}
          >
            <Box sx={{ display: "flex", alignItems: "center", gap: 1, mb: 2 }}>
              <AssignmentIcon color="primary" />
              <Typography variant="h6" fontWeight="bold">
                {t("classroomDetails.homework")}
              </Typography>
            </Box>

            {userRole === "Teacher" && (
              <Box sx={{ display: "flex", gap: 1, mb: 2 }}>
                <TextField
                  size="small"
                  fullWidth
                  placeholder={t("classroomDetails.hwPlaceholder")}
                  value={newTaskText}
                  onChange={(e) => setNewTaskText(e.target.value)}
                  onKeyDown={(e) => e.key === "Enter" && handleAddTask()}
                  sx={{ "& .MuiOutlinedInput-root": { borderRadius: 2 } }}
                />
                <Button
                  variant="contained"
                  onClick={handleAddTask}
                  disabled={!newTaskText.trim() || isLoadingTasks}
                  sx={{ borderRadius: 2, textTransform: "none", px: 2, whiteSpace: "nowrap" }}
                >
                  {t("classroomDetails.add")}
                </Button>
              </Box>
            )}

            <Box sx={{ flexGrow: 1, overflowY: "auto", pr: 0.5 }}>
            {isLoadingTasks ? (
              <Box sx={{ display: "flex", justifyContent: "center", mt: 4 }}>
                <CircularProgress size={24} color="primary" />
              </Box>
            ) : tasks.length === 0 ? (
              <Typography variant="body2" color="text.secondary" align="center" sx={{ mt: 4 }}>
                {t("classroomDetails.noTasks")}
              </Typography>
            ) : (
              <List disablePadding>
                {tasks.map((task) => {
                  const taskId = task.id || task.Id || task.groupTaskId || task.GroupTaskId;
                  const text = task.taskText || task.TaskText || task.text;
                  const isCompleted = task.isCompleted || task.IsCompleted || false;
                  const date = task.createdAt || task.CreatedAt;
                  
                  const completedCount = task.completedCount || task.CompletedCount || 0;
                  const totalStudents = task.totalStudents || task.TotalStudents || 0;

                  return (
                    <ListItem
                      key={taskId || Math.random()}
                      secondaryAction={
                        userRole === "Teacher" ? (
                          <IconButton edge="end" size="small" onClick={() => handleDeleteTask(task)}>
                            <DeleteIcon color="error" fontSize="small" />
                          </IconButton>
                        ) : (
                          <Checkbox
                            edge="end"
                            checked={isCompleted}
                            onChange={() => handleToggleTask(task)}
                            color="success"
                            size="small"
                          />
                        )
                      }
                      sx={{
                        bgcolor: isDarkMode ? "rgba(255,255,255,0.03)" : "rgba(0,0,0,0.02)",
                        mb: 0.75,
                        borderRadius: 2,
                        "&:hover": {
                          bgcolor: isDarkMode ? "rgba(255,255,255,0.06)" : "rgba(0,0,0,0.04)",
                        },
                      }}
                    >
                      <ListItemIcon sx={{ minWidth: 40 }}>
                        <AssignmentIcon color={isCompleted && userRole !== "Teacher" ? "success" : "primary"} />
                      </ListItemIcon>
                      
                      <ListItemText
                        primary={text || t("classroomDetails.emptyTask")}
                        secondary={
                          <Box sx={{ display: 'flex', gap: 2, mt: 0.5, alignItems: 'center', flexWrap: 'wrap' }}>
                            {date && (
                              <Typography component="span" variant="caption" color="text.secondary">
                                {`${t("classroomDetails.created")} ${new Date(date).toLocaleDateString()}`}
                              </Typography>
                            )}
                            
                            {userRole === "Teacher" && totalStudents > 0 && (
                              <Typography component="span" variant="caption" sx={{ color: "#2196f3", fontWeight: "bold" }}>
                                {t("classroomDetails.completedStats", { 
                                  count: completedCount, 
                                  total: totalStudents 
                                })}
                              </Typography>
                            )}
                          </Box>
                        }
                        sx={{
                          "& .MuiListItemText-primary": {
                            textDecoration: isCompleted && userRole !== "Teacher" ? "line-through" : "none",
                            opacity: isCompleted && userRole !== "Teacher" ? 0.5 : 1,
                            fontWeight: 500,
                            fontSize: "0.9rem",
                          }
                        }}
                      />
                    </ListItem>
                  );
                })}
              </List>
            )}
          </Box>
          </Paper>
        </Box>

        <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 3, flexWrap: "wrap", gap: 2 }}>
          <Typography variant="h5" fontWeight="bold">
            {t("classroomDetails.materials")}
          </Typography>

          {userRole === "Teacher" && (
            <Box sx={{ display: "flex", gap: 2 }}>
              <Button
                variant="outlined"
                color="secondary"
                startIcon={<StyleIcon />}
                onClick={() => setIsAddExistingModalOpen(true)}
                sx={{ textTransform: "none", borderRadius: 2, fontWeight: "bold" }}
              >
                {t("classroomDetails.addExistingBtn") || "Вибрати з існуючих"}
              </Button>
              <Button
                variant="contained"
                color="secondary"
                startIcon={<AddIcon />}
                onClick={() => setIsCreateModalOpen(true)}
                sx={{ textTransform: "none", borderRadius: 2, fontWeight: "bold" }}
              >
                {t("classroomDetails.createDeck")}
              </Button>
            </Box>
          )}
        </Box>

        {isLoading ? (
          <Box sx={{ display: "flex", justifyContent: "center", mt: 6 }}>
            <CircularProgress color="secondary" />
          </Box>
        ) : !decks || decks.length === 0 ? (
          <Typography variant="h6" color="text.secondary" sx={{ mt: 3, textAlign: "center" }}>
            {t("classroomDetails.noDecks")}
          </Typography>
        ) : (
          <Box sx={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(280px, 1fr))", gap: 3 }}>
            {decks.map((deck) => (
              <Card
                key={deck.id}
                sx={{
                  display: "flex",
                  flexDirection: "column",
                  height: "100%",
                  borderRadius: 3,
                  boxShadow: 3,
                  cursor: "pointer",
                  transition: "transform 0.2s, box-shadow 0.2s",
                  "&:hover": { transform: "translateY(-4px)", boxShadow: 6 },
                }}
                onClick={() => navigate(`/decks/${deck.id}`, { state: { fromClassroom: true } })}
              >
                <CardContent sx={{ flexGrow: 1, display: "flex", flexDirection: "column" }}>
                  <Typography
                    variant="h6"
                    component="h2"
                    sx={{
                      mb: 2,
                      fontWeight: "bold",
                      display: "-webkit-box",
                      WebkitLineClamp: 2,
                      WebkitBoxOrient: "vertical",
                      overflow: "hidden",
                      wordBreak: "break-word",
                    }}
                  >
                    {deck.title || t("dashboard.untitled")}
                  </Typography>
                  <Divider sx={{ mb: 2 }} />

                  <Box sx={{ mt: "auto" }}>
                    <Box sx={{ display: "flex", justifyContent: "space-between", mb: 1 }}>
                      <Typography variant="body2" color="text.secondary">{t("dashboard.stats.new")}</Typography>
                      <Chip label={deck.newCards ?? 0} size="small" color="info" variant={deck.newCards > 0 ? "filled" : "outlined"} />
                    </Box>
                    <Box sx={{ display: "flex", justifyContent: "space-between", mb: 1 }}>
                      <Typography variant="body2" color="text.secondary">{t("dashboard.stats.learning")}</Typography>
                      <Chip label={deck.learningCards ?? 0} size="small" color="error" variant={deck.learningCards > 0 ? "filled" : "outlined"} />
                    </Box>
                    <Box sx={{ display: "flex", justifyContent: "space-between" }}>
                      <Typography variant="body2" color="text.secondary">{t("dashboard.stats.review")}</Typography>
                      <Chip label={deck.toReview ?? 0} size="small" color="success" variant={deck.toReview > 0 ? "filled" : "outlined"} />
                    </Box>
                  </Box>
                </CardContent>

                <CardActions sx={{ p: 2, pt: 0, display: "flex", flexDirection: "column", gap: 1 }}>
                  <Button
                  fullWidth
                  variant="contained"
                  color="primary"
                  startIcon={<PlayArrowIcon />}
                  sx={{ textTransform: "none", borderRadius: 2 }}
                  disabled={!deck.newCards && !deck.learningCards && !deck.toReview}
                  onClick={(e) => { 
                    e.stopPropagation(); 
                    navigate(`/study/${deck.id}`, { 
                      state: { 
                        fromClassroom: true, 
                        classroomId: classroomId, 
                        backUrl: `/classrooms/${classroomId}` 
                      } 
                    }); 
                  }}
                >
                  {t("dashboard.studyBtn")}
                </Button>
                  <Button
                    fullWidth
                    variant="outlined"
                    color="secondary"
                    startIcon={<AutoStoriesIcon />}
                    sx={{ textTransform: "none", borderRadius: 2 }}
                    onClick={(e) => { e.stopPropagation(); setSelectedDeckId(deck.id); setIsStoryModalOpen(true); }}
                  >
                    {t("stories.generateBtn")}
                  </Button>

                  {userRole === "Teacher" && (
                    <Button
                      fullWidth
                      variant="text"
                      color="error"
                      startIcon={<RemoveCircleOutlineIcon />}
                      sx={{ textTransform: "none", fontWeight: "bold" }}
                      onClick={(e) => { e.stopPropagation(); handleRemoveDeckFromClass(deck.id); }}
                    >
                      {t("classroomDetails.removeFromClass")}
                    </Button>
                  )}
                </CardActions>
              </Card>
            ))}
          </Box>
        )}
      </Container>

      <DeleteConfirmDialog
        open={confirmDialog.open}
        onClose={() => setConfirmDialog({ ...confirmDialog, open: false })}
        onConfirm={executeDialogAction}
        title={confirmDialog.title}
        content={confirmDialog.content}
        isDeleting={isProcessingAction}
      />

      <SelectExistingDecksModal
      open={isAddExistingModalOpen}
      onClose={() => setIsAddExistingModalOpen(false)}
      classroomId={classroomId}
      onSuccess={handleAddExistingDecks}
      />

      {selectedDeckId && (
        <GenerateStoryModal
          open={isStoryModalOpen}
          onClose={() => { setIsStoryModalOpen(false); setSelectedDeckId(null); }}
          deckId={selectedDeckId}
        />
      )}

      <CreateDeckModal
        open={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
        onSubmit={handleCreateDeck}
        isSaving={isCreatingDeck}
        limitLanguages={true}
      />

      <Snackbar
        open={snackbar.open}
        autoHideDuration={4000}
        onClose={() => setSnackbar((s) => ({ ...s, open: false }))}
        anchorOrigin={{ vertical: "bottom", horizontal: "center" }}
      >
        <Alert severity={snackbar.severity} onClose={() => setSnackbar((s) => ({ ...s, open: false }))}>
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
};