import { createRouter, createWebHistory } from "vue-router";
import { useAuth } from "./features/auth/authStore";
import CustomersPage from "./features/customers/pages/CustomersPage.vue";
import CustomerDetailsPage from "./features/customers/pages/CustomerDetailsPage.vue";
import AccountDetailsPage from "./features/accounts/pages/AccountDetailsPage.vue";
import LoginPage from "./features/auth/pages/LoginPage.vue";

/**
 * Router configuration for the application.
 * Defines all routes and their associated components.
 * 
 * Routes:
 * - /login (public) - Login page
 * - /customers - Customer list page
 * - /customers/:id - Customer details page
 * - /customers/:customerId/accounts/:accountId - Account details page
 * - /* - Fallback redirect to customers
 */
const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: "/login", component: LoginPage, meta: { public: true } },
    { path: "/customers", component: CustomersPage },
    { path: "/customers/:id", component: CustomerDetailsPage, props: true },
    {
      path: "/customers/:customerId/accounts/:accountId",
      component: AccountDetailsPage,
      props: true,
    },
    { path: "/:pathMatch(.*)*", redirect: "/customers" },
  ],
});

/**
 * Global navigation guard that enforces authentication.
 * Redirects unauthenticated users to login page.
 * Redirects authenticated users away from public pages (e.g., login).
 * 
 * @param {RouteLocationNormalizedLoaded} to - The destination route
 * @returns {boolean|string} True to allow navigation, route path to redirect
 */
router.beforeEach((to) => {
  const auth = useAuth();
  const isPublic = !!to.meta.public;
  if (isPublic && auth.isAuthenticated.value) return "/customers";
  if (!isPublic && !auth.isAuthenticated.value) return "/login";
  return true;
});

export default router;
