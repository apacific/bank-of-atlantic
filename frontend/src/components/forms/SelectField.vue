<script setup lang="ts">
defineProps<{
  label: string;
  modelValue: string;
  options: { label: string; value: string }[];
  error?: string;
}>();
const emit = defineEmits<{ (e: "update:modelValue", v: string): void }>();
</script>

<template>
  <div class="field">
    <label class="label">{{ label }}</label>
    <select class="input" :value="modelValue" @change="emit('update:modelValue', ($event.target as HTMLSelectElement).value)">
      <option value="">Select…</option>
      <option v-for="o in options" :key="o.value" :value="o.value">{{ o.label }}</option>
    </select>
    <div v-if="error" class="error">{{ error }}</div>
  </div>
</template>

<style scoped>
.field{display:flex;flex-direction:column;gap:6px;}
.label{font-size:1em;color:#222;}
.input{padding:10px 12px;border-radius:1px;border:1px solid #ddd;background:#fff;}
.error{font-size:12px;color:#b00020;}
</style>
