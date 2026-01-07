<script setup lang="ts">
/**
 * Props for the Modal component.
 * @property {boolean} open - Whether the modal is visible
 * @property {string} title - The modal title
 * @property {string} message - The modal message content
 */
defineProps<{ open: boolean; title: string; message: string }>();

/**
 * Emits for the Modal component.
 * @event close - Emitted when user clicks outside or closes the modal
 */
const emit = defineEmits<{ (e: "close"): void }>();
</script>

<template>
  <div v-if="open" class="overlay" @click.self="emit('close')">
    <div class="modal">
      <h3 class="h">{{ title }}</h3>
      <p class="p">{{ message }}</p>
      <div class="actions">
        <button type="button" class="btn" @click="emit('close')">OK</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.overlay {
  align-items: center;
  background: rgba(0, 0, 0, 0.35);
  display: flex;
  inset: 0;
  justify-content: center;
  padding: 16px;
  position: fixed;
}

.modal {
  background: #fff;
  border-radius: 1px;
  box-shadow: 0 10px 35px rgba(0, 0, 0, 0.2);
  max-width: 520px;
  padding: 16px;
  width: 100%;
}

.h {
  font-size: 18px;
  margin: 0 0 8px 0;
}

.p {
  color: #333;
  margin: 0;
}

.actions {
  display: flex;
  justify-content: flex-end;
  margin-top: 12px;
}

.btn {
  background: #dcdcdc;
  border: 1px solid #ddd;
  border-radius: 1px;
  cursor: pointer;
  padding: 8px 12px;
}
</style>
