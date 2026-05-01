import { useState } from "react";
import {
  Box,
  Container,
  Typography,
  Paper,
  Alert,
  Collapse,
} from "@mui/material";
import { GoogleLogin } from "@react-oauth/google";
import { useNavigate } from "react-router-dom";
import axiosClient from "../api/axiosClient";
import { useTranslation } from "react-i18next";

export const AuthPage = () => {
  const navigate = useNavigate();
  const { t } = useTranslation();

  const [sessionExpired] = useState(() => {
    const isExpired = localStorage.getItem("session_expired");
    if (isExpired) {
      localStorage.removeItem("session_expired");
      return true;
    }
    return false;
  });

  const handleSuccess = async (credentialResponse) => {
    try {
      const googleToken = credentialResponse.credential;
      const response = await axiosClient.post("/Auth/google", {
        token: googleToken,
      });
      localStorage.setItem("token", response.data.token);
      navigate("/decks");
    } catch (error) {
      console.error("Authorization error via Google:", error);
      alert(t("auth.errorMsg"));
    }
  };

  return (
    <Box
      sx={{
        minHeight: "100vh",
        width: "100vw",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        backgroundColor: "#f5f5f5",
      }}
    >
      <Container maxWidth="xs">
        <Collapse in={sessionExpired}>
          <Alert
            severity="warning"
            sx={{ mb: 3, borderRadius: 2, boxShadow: 1 }}
          >
            {t("auth.sessionExpired")}
          </Alert>
        </Collapse>
        <Paper
          elevation={4}
          sx={{
            p: { xs: 4, md: 5 },
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
            borderRadius: 4,
          }}
        >
          <Typography
            variant="h5"
            fontWeight="900"
            sx={{ mb: 4, color: "primary.main", letterSpacing: 0.5 }}
          >
            LexiContext
          </Typography>
          <GoogleLogin
            onSuccess={handleSuccess}
            onError={() => console.log("Login Failed")}
            useOneTap={false}
            shape="rectangular"
            theme="outline"
          />
          <Typography
            variant="body2"
            sx={{
              mt: 4,
              color: "text.secondary",
              textAlign: "center",
              lineHeight: 1.6,
            }}
          >
            {t("auth.subtitle")}
          </Typography>
        </Paper>
      </Container>
    </Box>
  );
};
