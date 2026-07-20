export const getActiveRole = () => {
  // Спочатку перевіряємо, чи юзер явно вибрав роль (якщо у вас є такий перемикач)
  const savedRole = localStorage.getItem("activeRole");
  if (savedRole) return savedRole;

  // Якщо ні, беремо з токена
  const token = localStorage.getItem("token");
  if (!token) return "Student";

  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function(c) {
      return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));

    const decoded = JSON.parse(jsonPayload);
    const roles = decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || decoded.role || decoded.Role;
    
    // Якщо ролей декілька, визначаємо пріоритет або беремо першу
    if (Array.isArray(roles)) {
       return roles.includes("Teacher") ? "Teacher" : "Student"; // Або логіка за замовчуванням
    }
    return roles || "Student";
  } catch (error) {
    return "Student";
  }
};