<script setup lang="ts">
import { RouterView } from "vue-router";
import SiteHeader from "./components/SiteHeader.vue";
import bankLogo from "./assets/bank-logo.png";
import { useAuth } from "./features/auth/authStore";

const auth = useAuth();
</script>

<template>
  <SiteHeader v-if="auth.isAuthenticated" :logo-src="bankLogo" />

  <main class="main" :class="{ noHeader: !auth.isAuthenticated }">
    <RouterView />
  </main>
</template>

<style scoped>
.main {
  margin: 0 auto;
  max-width: 980px;
  padding: 18px 14px;
}

.noHeader {
  padding-top: 28px;
}
</style>
