// src/utils/auth.js
export const getActiveRole = () => {
  const token = localStorage.getItem("token");
  if (!token) return "Student";
  
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      window.atob(base64).split('').map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)).join('')
    );
    const decoded = JSON.parse(jsonPayload);
    return decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || decoded.role || decoded.Role || "Student";
  } catch (error) {
    console.error("Token parsing error:", error);
    return "Student";
  }
};