import { computed, ref } from "vue";

/** Session storage key for JWT access token */
const TOKEN_KEY = "boa_token";

/** Session storage key for user role */
const ROLE_KEY = "boa_role";

/** Reactive reference to the current JWT access token */
const token = ref<string | null>(sessionStorage.getItem(TOKEN_KEY));

/** Reactive reference to the current user role */
const role = ref<string | null>(sessionStorage.getItem(ROLE_KEY));

/**
 * Authentication store composable providing reactive authentication state.
 * Manages JWT token and user role in session storage.
 * 
 * @returns {Object} Authentication state and methods
 * @returns {import('vue').Ref<string|null>} .token - The JWT access token
 * @returns {import('vue').Ref<string|null>} .role - The user's role
 * @returns {import('vue').ComputedRef<boolean>} .isAuthenticated - Computed boolean indicating if user is logged in
 * @returns {Function} .setSession - Sets authentication session (token and role)
 * @returns {Function} .clearSession - Clears authentication session
 * 
 * @example
 * const auth = useAuth();
 * if (auth.isAuthenticated.value) {
 *   console.log(auth.role.value); // "Employee" or "Manager"
 * }
 * auth.setSession(jwtToken, userRole);
 * auth.clearSession(); // Logout
 */
export const useAuth = () => {
  /**
   * Computed property indicating whether user is authenticated.
   * @type {import('vue').ComputedRef<boolean>}
   */
  const isAuthenticated = computed(() => !!token.value);

  /**
   * Sets the authentication session (token and role) and persists to session storage.
   * @param {string} accessToken - JWT access token from server
   * @param {string} userRole - User's role (e.g., "Employee", "Manager")
   */
  function setSession(accessToken: string, userRole: string) {
    token.value = accessToken;
    role.value = userRole;
    sessionStorage.setItem(TOKEN_KEY, accessToken);
    sessionStorage.setItem(ROLE_KEY, userRole);
  }

  /**
   * Clears the authentication session and removes from session storage.
   * Typically called during logout.
   */
  function clearSession() {
    token.value = null;
    role.value = null;
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(ROLE_KEY);
  }

  return { token, role, isAuthenticated, setSession, clearSession };
};
