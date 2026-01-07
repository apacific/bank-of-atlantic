<script setup lang="ts">
/**
 * Props for the TextField component.
 * @property {string} label - The field label text
 * @property {string} modelValue - The current input value (v-model)
 * @property {string} [error] - Error message to display below field
 * @property {string} [help] - Help text to display below field
 * @property {string} [type] - HTML input type (default: "text")
 */
defineProps<{
  label: string;
  modelValue: string;
  error?: string;
  help?: string;
  type?: string;
}>();

/**
 * Emits for the TextField component.
 * @event update:modelValue - Emitted when input value changes (v-model)
 */
const emit = defineEmits<{ (e: "update:modelValue", v: string): void }>();
</script>

<template>
  <div class="field">
    <label class="label">{{ label }}</label>
    <input
      class="input"
      :type="type ?? 'text'"
      :value="modelValue"
      @input="emit('update:modelValue', ($event.target as HTMLInputElement).value)"
    />
    <div v-if="help" class="help">{{ help }}</div>
    <div v-if="error" class="error">{{ error }}</div>
  </div>
</template>

<style scoped>
.field {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.label {
  color: #222;
  font-size: 1em;
}

.input {
  border: 1px solid #ddd;
  border-radius: 1px;
  padding: 10px 12px;
}

.help {
  color: #006400;
  font-size: 12px;
}

.error {
  color: #b00020;
  font-size: 12px;
}
</style>
