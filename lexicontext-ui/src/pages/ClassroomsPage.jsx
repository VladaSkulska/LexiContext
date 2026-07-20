import { useState, useEffect } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import {
  Box,
  Typography,
  Button,
  Container,
  Card,
  CardContent,
  Divider,
  CircularProgress,
  TextField,
  Chip,
  Grid,
  Snackbar,
  Alert
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import LoginIcon from "@mui/icons-material/Login";
import { Navbar } from "../components/common/Navbar";
import axiosClient from "../api/axiosClient";
import { useTranslation } from "react-i18next";
import { getActiveRole } from "../utils/auth";
import { extractErrorMessage } from "../utils/errorHandler";

export const ClassroomsPage = ({ isDarkMode, toggleTheme }) => {
  const [classrooms, setClassrooms] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [inputValue, setInputValue] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [userRole, setUserRole] = useState(() => getActiveRole());
  
  const [snackbar, setSnackbar] = useState({ open: false, message: "", severity: "success" });

  const navigate = useNavigate();
  const location = useLocation();
  const { t } = useTranslation();

  useEffect(() => {
    const fetchClassrooms = async () => {
      setIsLoading(true);
      try {
        const endpoint = userRole === "Teacher" ? "/Classrooms/teacher" : "/Classrooms/student";
        const response = await axiosClient.get(endpoint);
        setClassrooms(response.data);
      } catch (error) {
        console.error(error);
      } finally {
        setIsLoading(false);
      }
    };

    fetchClassrooms();
  }, [userRole, location.state?.refresh]);

  const handleAction = async () => {
    if (!inputValue.trim()) return;
    setIsSubmitting(true);

    try {
      if (userRole === "Teacher") {
        await axiosClient.post("/Classrooms", { name: inputValue, description: "" });
      } else {
        await axiosClient.post("/Classrooms/join", { joinCode: inputValue });
      }
      setInputValue("");
      const endpoint = userRole === "Teacher" ? "/Classrooms/teacher" : "/Classrooms/student";
      const response = await axiosClient.get(endpoint);
      setClassrooms(response.data);
    } catch (error) {
      console.error(error);
      let rawError = extractErrorMessage(error);
      
      if (rawError.includes("was not found") || rawError.includes("Entity") || rawError.includes("classroom_not_found")) {
    rawError = t("classrooms.errors.notFound");
    } else if (rawError.includes("already enrolled") || rawError.includes("already joined")) {
        rawError = t("classrooms.errors.alreadyJoined");
    } else if (rawError.includes("teacher cannot be a student")) {
        rawError = t("classrooms.errors.teacherConflict");
    }

      setSnackbar({
        open: true,
        message: rawError,
        severity: "error"
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Box sx={{ minHeight: "100vh", bgcolor: isDarkMode ? "#121212" : "#f9f9f9" }}>
      <Navbar isDarkMode={isDarkMode} toggleTheme={toggleTheme} />
      
      <Container maxWidth="lg" sx={{ mt: 5, pb: 5 }}>
        <Box sx={{ display: "flex", flexWrap: "wrap", justifyContent: "space-between", alignItems: "center", mb: 4, gap: 2 }}>
          <Typography variant="h4" component="h1" fontWeight="bold">
            {userRole === "Teacher" ? t("classrooms.titleTeacher") : t("classrooms.titleStudent")}
          </Typography>
          
          <Box sx={{ display: "flex", gap: 2, alignItems: "center" }}>
            <TextField
              size="small"
              placeholder={userRole === "Teacher" ? t("classrooms.placeholderTeacher") : t("classrooms.placeholderStudent")}
              value={inputValue}
              onChange={(e) => setInputValue(e.target.value)}
              sx={{ minWidth: "250px", bgcolor: isDarkMode ? "#1e1e1e" : "#fff", borderRadius: 1 }}
            />
            <Button
              variant="contained"
              color={userRole === "Teacher" ? "secondary" : "primary"}
              startIcon={userRole === "Teacher" ? <AddIcon /> : <LoginIcon />}
              onClick={handleAction}
              disabled={isSubmitting || !inputValue.trim()}
              sx={{ textTransform: "none", borderRadius: 2, fontWeight: "bold", height: "40px" }}
            >
              {userRole === "Teacher" ? t("classrooms.btnCreate") : t("classrooms.btnJoin")}
            </Button>
          </Box>
        </Box>

        {isLoading ? (
          <Box sx={{ display: "flex", justifyContent: "center", mt: 10 }}>
            <CircularProgress color="secondary" />
          </Box>
        ) : classrooms.length === 0 ? (
          <Typography variant="h6" color="text.secondary" align="center" sx={{ mt: 10 }}>
            {userRole === "Teacher" ? t("classrooms.emptyTeacher") : t("classrooms.emptyStudent")}
          </Typography>
        ) : (
          <Grid container spacing={3}>
            {classrooms.map((c) => (
              <Grid item xs={12} sm={6} md={4} key={c.id}>
                <Card
                  onClick={() => navigate(`/classrooms/${c.id}`, { state: { classroom: c } })}
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
                >
                  <CardContent sx={{ flexGrow: 1 }}>
                    <Typography variant="h6" fontWeight="bold" gutterBottom>
                      {c.name}
                    </Typography>
                    
                    {userRole === "Teacher" && (
                      <Chip 
                        label={`${t("classrooms.codeLabel")} ${c.joinCode}`} 
                        size="small" 
                        color="info" 
                        variant="outlined" 
                        sx={{ mb: 2, fontFamily: "monospace", fontWeight: "bold" }} 
                      />
                    )}
                    
                    <Divider sx={{ my: 1.5 }} />
                    
                    <Box sx={{ display: "flex", justifyContent: "space-between" }}>
                      <Typography variant="body2" color="text.secondary">{t("classrooms.studentsCount")}</Typography>
                      <Typography variant="body2" fontWeight="bold" color="primary">{c.studentsCount}</Typography>
                    </Box>
                    <Box sx={{ display: "flex", justifyContent: "space-between", mt: 1 }}>
                      <Typography variant="body2" color="text.secondary">{t("classrooms.decksCount")}</Typography>
                      <Typography variant="body2" fontWeight="bold" color="secondary">{c.decksCount}</Typography>
                    </Box>
                  </CardContent>
                </Card>
              </Grid>
            ))}
          </Grid>
        )}
      </Container>

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