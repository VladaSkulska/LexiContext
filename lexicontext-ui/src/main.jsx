import React from "react";
import ReactDOM from "react-dom/client";
import { GoogleOAuthProvider } from "@react-oauth/google";
import { CssBaseline } from "@mui/material";
import App from "./App.jsx";
import "./i18n";

ReactDOM.createRoot(document.getElementById("root")).render(
  <React.StrictMode>
    <GoogleOAuthProvider clientId="414498357977-nval5a03ui8epiocgu6f110fkaunv0gh.apps.googleusercontent.com">
      <CssBaseline />
      <App />
    </GoogleOAuthProvider>
  </React.StrictMode>,
);
