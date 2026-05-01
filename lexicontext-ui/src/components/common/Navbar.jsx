import { useState } from "react";
import {
  AppBar,
  Box,
  Toolbar,
  Typography,
  Button,
  Avatar,
  IconButton,
  ToggleButtonGroup,
  ToggleButton,
  Menu,
  MenuItem,
  Divider,
} from "@mui/material";
import { DarkMode, LightMode, Menu as MenuIcon } from "@mui/icons-material";
import { useNavigate, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import axiosClient from "../../api/axiosClient";

// 1. ВИНОСИМО КОМПОНЕНТ ЗА МЕЖІ NAVBAR
const NavButton = ({ to, labelKey, t, location, navigate }) => {
  const isActive = location.pathname === to;
  return (
    <Button
      color="inherit"
      onClick={() => navigate(to)}
      sx={{
        textTransform: "none",
        fontSize: "1rem",
        px: 2,
        fontWeight: isActive ? "bold" : "500",
        bgcolor: isActive ? "rgba(255,255,255,0.15)" : "transparent",
        borderRadius: 2,
        "&:hover": {
          bgcolor: "rgba(255,255,255,0.1)",
        },
      }}
    >
      {t(labelKey)}
    </Button>
  );
};

export const Navbar = ({ isDarkMode, toggleTheme }) => {
  const navigate = useNavigate();
  const location = useLocation();
  const { t, i18n } = useTranslation();

  const [mobileMoreAnchorEl, setMobileMoreAnchorEl] = useState(null);
  const isMobileMenuOpen = Boolean(mobileMoreAnchorEl);

  const handleMobileMenuOpen = (event) => {
    setMobileMoreAnchorEl(event.currentTarget);
  };

  const handleMobileMenuClose = () => {
    setMobileMoreAnchorEl(null);
  };

  const handleLogout = () => {
    localStorage.removeItem("token");
    navigate("/login");
  };

  const handleLanguageChange = async (event, newLang) => {
    if (newLang !== null) {
      i18n.changeLanguage(newLang);
      try {
        const langCode = newLang === "uk" ? 2 : 1;
        await axiosClient.put("/UserSettings", {
          interfaceLanguage: langCode,
          theme: isDarkMode ? 1 : 0,
          fontScale: 1.0,
          enableSound: true,
        });
      } catch (error) {
        console.error("Помилка збереження мови", error);
      }
    }
  };

  const handleThemeToggle = async () => {
    toggleTheme();
    try {
      const newThemeCode = !isDarkMode ? 1 : 0;
      const currentLangCode = i18n.language?.substring(0, 2) === "uk" ? 2 : 1;
      await axiosClient.put("/UserSettings", {
        interfaceLanguage: currentLangCode,
        theme: newThemeCode,
        fontScale: 1.0,
        enableSound: true,
      });
    } catch (error) {
      console.error("Помилка збереження теми", error);
    }
  };

  const renderMobileMenu = (
    <Menu
      anchorEl={mobileMoreAnchorEl}
      anchorOrigin={{ vertical: "bottom", horizontal: "right" }}
      transformOrigin={{ vertical: "top", horizontal: "right" }}
      open={isMobileMenuOpen}
      onClose={handleMobileMenuClose}
      PaperProps={{
        sx: { width: 220, borderRadius: 3, mt: 1.5, boxShadow: 3 },
      }}
    >
      <MenuItem
        onClick={() => {
          navigate("/decks");
          handleMobileMenuClose();
        }}
        selected={location.pathname === "/decks"}
        sx={{ py: 1.5 }}
      >
        <Typography
          fontWeight={location.pathname === "/decks" ? "bold" : "normal"}
        >
          {t("navbar.myDecks")}
        </Typography>
      </MenuItem>
      <MenuItem
        onClick={() => {
          navigate("/stories");
          handleMobileMenuClose();
        }}
        selected={location.pathname === "/stories"}
        sx={{ py: 1.5 }}
      >
        <Typography
          fontWeight={location.pathname === "/stories" ? "bold" : "normal"}
        >
          {t("navbar.stories")}
        </Typography>
      </MenuItem>
      <MenuItem
        onClick={() => {
          navigate("/statistics");
          handleMobileMenuClose();
        }}
        selected={location.pathname === "/statistics"}
        sx={{ py: 1.5 }}
      >
        <Typography
          fontWeight={location.pathname === "/statistics" ? "bold" : "normal"}
        >
          {t("navbar.statistics")}
        </Typography>
      </MenuItem>
      <Divider sx={{ my: 1 }} />
      <Box sx={{ px: 2, py: 1 }}>
        <ToggleButtonGroup
          value={i18n.language?.substring(0, 2) || "uk"}
          exclusive
          onChange={handleLanguageChange}
          size="small"
          fullWidth
          color="primary"
        >
          <ToggleButton value="uk" sx={{ fontWeight: "bold" }}>
            UK
          </ToggleButton>
          <ToggleButton value="en" sx={{ fontWeight: "bold" }}>
            EN
          </ToggleButton>
        </ToggleButtonGroup>
      </Box>
      <Divider sx={{ my: 1 }} />
      <MenuItem onClick={handleLogout} sx={{ color: "error.main", py: 1.5 }}>
        <Typography fontWeight="bold">{t("navbar.logout")}</Typography>
      </MenuItem>
    </Menu>
  );

  return (
    <Box sx={{ flexGrow: 1, mb: 4 }}>
      <AppBar
        position="static"
        color="primary"
        elevation={2}
        sx={{
          borderBottom: "1px solid rgba(255,255,255,0.1)",
          background: isDarkMode
            ? "linear-gradient(90deg, #1A237E 0%, #283593 100%)"
            : "primary.main",
        }}
      >
        <Toolbar
          sx={{
            minHeight: { xs: 64, md: 70 },
            display: "flex",
            justifyContent: "space-between",
          }}
        >
          <Box sx={{ display: "flex", alignItems: "center" }}>
            <Typography
              variant="h5"
              sx={{
                fontWeight: "900",
                cursor: "pointer",
                mr: { xs: 2, md: 4 },
                letterSpacing: 1,
              }}
              onClick={() => navigate("/decks")}
            >
              LexiContext
            </Typography>

            {/* ТЕПЕР ПЕРЕДАЄМО ПАРАМЕТРИ В NAVBUTTON */}
            <Box sx={{ display: { xs: "none", md: "flex" }, gap: 1 }}>
              <NavButton
                to="/decks"
                labelKey="navbar.myDecks"
                t={t}
                location={location}
                navigate={navigate}
              />
              <NavButton
                to="/stories"
                labelKey="navbar.stories"
                t={t}
                location={location}
                navigate={navigate}
              />
              <NavButton
                to="/statistics"
                labelKey="navbar.statistics"
                t={t}
                location={location}
                navigate={navigate}
              />
            </Box>
          </Box>

          <Box
            sx={{ display: { xs: "none", md: "flex" }, alignItems: "center" }}
          >
            <ToggleButtonGroup
              value={i18n.language?.substring(0, 2) || "uk"}
              exclusive
              onChange={handleLanguageChange}
              size="small"
              sx={{ mr: 3, bgcolor: "rgba(255,255,255,0.1)", borderRadius: 2 }}
            >
              <ToggleButton
                value="uk"
                sx={{ color: "white", px: 1.5, py: 0.5, border: "none" }}
              >
                UK
              </ToggleButton>
              <ToggleButton
                value="en"
                sx={{ color: "white", px: 1.5, py: 0.5, border: "none" }}
              >
                EN
              </ToggleButton>
            </ToggleButtonGroup>

            <IconButton
              color="inherit"
              onClick={handleThemeToggle}
              sx={{ mr: 2, bgcolor: "rgba(255,255,255,0.05)" }}
            >
              {isDarkMode ? <LightMode /> : <DarkMode />}
            </IconButton>

            <Box
              sx={{
                display: "flex",
                alignItems: "center",
                mr: 3,
                bgcolor: "rgba(0,0,0,0.1)",
                py: 0.5,
                px: 1.5,
                borderRadius: 5,
              }}
            >
              <Avatar
                sx={{
                  width: 32,
                  height: 32,
                  mr: 1,
                  bgcolor: "secondary.main",
                  fontSize: "0.9rem",
                  fontWeight: "bold",
                }}
              >
                V
              </Avatar>
              <Typography variant="body2" sx={{ fontWeight: 600 }}>
                Vlada
              </Typography>
            </Box>

            <Button
              color="inherit"
              variant="outlined"
              onClick={handleLogout}
              sx={{
                textTransform: "none",
                borderRadius: 2,
                borderWidth: 2,
                borderColor: "rgba(255,255,255,0.3)",
                fontWeight: "bold",
                "&:hover": {
                  borderColor: "white",
                  bgcolor: "rgba(255,255,255,0.1)",
                  borderWidth: 2,
                },
              }}
            >
              {t("navbar.logout")}
            </Button>
          </Box>

          <Box
            sx={{
              display: { xs: "flex", md: "none" },
              alignItems: "center",
              gap: 1,
            }}
          >
            <IconButton
              color="inherit"
              onClick={handleThemeToggle}
              sx={{ bgcolor: "rgba(255,255,255,0.05)" }}
            >
              {isDarkMode ? (
                <LightMode fontSize="small" />
              ) : (
                <DarkMode fontSize="small" />
              )}
            </IconButton>
            <IconButton
              color="inherit"
              edge="end"
              onClick={handleMobileMenuOpen}
              sx={{ ml: 1 }}
            >
              <MenuIcon />
            </IconButton>
          </Box>
        </Toolbar>
      </AppBar>
      {renderMobileMenu}
    </Box>
  );
};
