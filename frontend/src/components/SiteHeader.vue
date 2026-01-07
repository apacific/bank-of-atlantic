<script setup lang="ts">
import { useRouter } from "vue-router";

const router = useRouter();

/**
 * Props for the SiteHeader component.
 * @property {string} logoSrc - The source URL of the logo image
 * @property {string} [alt] - Alternative text for the logo image
 */
defineProps<{
  logoSrc: string;
  alt?: string;
}>();

/**
 * Navigates to the customers page when the logo is clicked.
 */
function goHome() {
  router.push("/customers");
}
</script>

<template>
  <header class="header">
    <button class="logoBtn" type="button" @click="goHome" aria-label="Go to customers">
      <img class="logo" :src="props.logoSrc" :alt="props.alt ?? 'Bank of Atlantic logo'" />
    </button>

    <div class="title">Bank of Atlantic</div>
  </header>
</template>

<style scoped>
.header {
  border-bottom: 1px solid #eee;
  background: #fff;
  display: flex;
  align-items: center;
  gap: 16px;
  height: 100px;
  padding: 0 18px;
  position: sticky;
  top: 0;
  z-index: 20;
}

.logo {
  display: block;
  height: 88px;
  width: auto;
}

.logoBtn {
  align-items: center;
  background: transparent;
  border: 0;
  cursor: pointer;
  display: flex;
  padding: 0;
}

.title {
  font-family: var(--font-header-title);
  font-size: 3.33em;
  font-weight: 600;
  letter-spacing: 0.555px;
}
</style>
