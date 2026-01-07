import axios from "axios";

/**
 * Normalized API error response object.
 * @typedef {Object} ApiError
 * @property {number} status - HTTP status code
 * @property {string} title - Error title/category
 * @property {string} detail - Detailed error message
 * @property {Record<string, string[]>} [errors] - Field-level validation errors
 */
export type ApiError = {
  status: number;
  title: string;
  detail: string;
  errors?: Record<string, string[]>;
};

/**
 * Axios API client instance with default configuration.
 * Automatically includes JWT bearer token from session storage.
 * Handles 401 responses by clearing authentication.
 * @type {import('axios').AxiosInstance}
 */
export const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5000",
  timeout: 10000,
});

/**
 * Request interceptor - Automatically adds JWT bearer token to all requests.
 * Reads token from session storage and adds it to Authorization header.
 */
api.interceptors.request.use((config) => {
  const token = sessionStorage.getItem("boa_token");
  if (token) {
    config.headers = config.headers ?? {};
    (config.headers as any).Authorization = `Bearer ${token}`;
  }
  return config;
});

/**
 * Response interceptor - Handles errors and normalizes error responses.
 * - Clears authentication on 401 Unauthorized
 * - Normalizes API error responses to consistent ApiError format
 * - Handles missing response data gracefully
 */
api.interceptors.response.use(
  (r) => r,
  (err) => {
    if (err?.response?.status === 401) {
      sessionStorage.removeItem("boa_token");
      sessionStorage.removeItem("boa_role");
    }
    const pd = err?.response?.data;
    const normalized: ApiError = {
      status: err?.response?.status ?? 0,
      title: pd?.title ?? "Request failed",
      detail: pd?.detail ?? "Unexpected error.",
      errors: pd?.errors ?? pd?.extensions?.errors,
    };
    return Promise.reject(normalized);
  }
);

/**
 * Converts a PascalCase string to camelCase.
 * Used for converting API response field names from C# conventions.
 * @param {string} s - The string to convert
 * @returns {string} The camelCase version of the string
 * @example
 * toCamel("UserName") // => "userName"
 * toCamel("") // => ""
 */
export const toCamel = (s: string) =>
  s.length === 0 ? s : s.charAt(0).toLowerCase() + s.slice(1);

