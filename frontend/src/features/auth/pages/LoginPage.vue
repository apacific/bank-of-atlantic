<script setup lang="ts">
import { reactive, ref } from "vue";
import { useRouter } from "vue-router";
import TextField from "../../../components/forms/TextField.vue";
import Modal from "../../../components/Modal.vue";
import { api } from "../../../app/api";
import { useAuth } from "../authStore";

const router = useRouter();
const auth = useAuth();

/**
 * Login form reactive state.
 * @type {Object}
 * @property {string} email - User email address
 * @property {string} password - User password
 */
const form = reactive({ email: "", password: "" });

/**
 * Field-level validation errors.
 * Maps field names to error messages.
 * @type {Record<string, string>}
 */
const errors = reactive<Record<string, string>>({});

/**
 * Loading state during API request.
 * @type {import('vue').Ref<boolean>}
 */
const busy = ref(false);

/**
 * Modal error display state.
 * @type {Object}
 * @property {boolean} open - Whether modal is visible
 * @property {string} title - Modal title
 * @property {string} message - Error message text
 */
const modal = reactive({ open: false, title: "", message: "" });

/**
 * Clears all field validation errors.
 */
function clearErrors() {
  Object.keys(errors).forEach(k => delete errors[k]);
}

/**
 * Submits the login form.
 * Validates input, calls login API, and stores authentication token.
 * Redirects to customers page on success.
 * Shows error modal on failure.
 * @async
 */
async function submit() {
  clearErrors();

  if (!form.email.trim()) errors.email = "Email is required.";
  if (!form.password.trim()) errors.password = "Password is required.";
  if (Object.keys(errors).length) return;

  busy.value = true;
  try {
    const res = await api.post("/auth/login", { Email: form.email, Password: form.password });
    auth.setSession(res.data.accessToken, res.data.role);
    router.replace("/customers");
  } catch (e: any) {
    modal.open = true;
    modal.title = "Login failed";

    const status = e?.status ?? 0;
    if (!status) {
      modal.message =
        "Network error (no response). Check API is running and CORS allows this origin.";
    } else {
      modal.message = `HTTP ${status}: ${e?.detail ?? e?.title ?? "Request failed."}`;
    }
  } finally {
    busy.value = false;
  }
}
</script>

<template>
  <div class="wrap">
    <form class="card" @submit.prevent="submit">
      <h1 class="title">Bank of Atlantic</h1>
      <p class="muted">Please sign in.</p>

      <div class="grid">
        <TextField label="Email" v-model="form.email" :error="errors.email" type="email" />
        <TextField label="Password" v-model="form.password" :error="errors.password" type="password" />
      </div>

      <button class="btn" type="submit" :disabled="busy">{{ busy ? "Signing in…" : "Sign in" }}</button>
    </form>

    <Modal :open="modal.open" title="Login failed" :message="modal.message" @close="modal.open=false" />
  </div>
</template>

<style scoped>
.wrap {
  margin: 40px auto;
  max-width: 520px;
  padding: 0 14px;
}

.card {
  background: #fff;
  border: 1px solid #eee;
  border-radius: 14px;
  padding: 18px;
}

.title {
  margin: 0 0 8px 0;
}

.muted {
  color: #666;
  margin: 0 0 14px 0;
}

.grid {
  display: grid;
  gap: 12px;
  margin-bottom: 12px;
}

.btn {
  background: #f7f7f7;
  border: 1px solid #ddd;
  border-radius: 12px;
  cursor: pointer;
  padding: 10px 12px;
  width: 100%;
}

.btn:disabled {
  cursor: not-allowed;
  opacity: 0.6;
}
</style>
