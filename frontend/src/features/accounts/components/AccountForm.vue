<script setup lang="ts">
import { reactive, ref, computed } from "vue";
import Modal from "../../../components/Modal.vue";
import TextField from "../../../components/forms/TextField.vue";
import SelectField from "../../../components/forms/SelectField.vue";
import { api, type ApiError, toCamel } from "../../../app/api";
import { accountCreateSchema, accountUpdateSchema } from "../validation/accountSchema";
import { isCreditAccountType, isDepositAccountType } from "../accountTypes";

/**
 * Available account type options for the form dropdown.
 * @type {Array<{label: string, value: string}>}
 */
const accountTypeOptions = [
  { label: "Checking", value: "Checking" },
  { label: "Savings", value: "Savings" },
  { label: "Money Market", value: "MoneyMarket" },
  { label: "CD", value: "CD" },
  { label: "Credit Card", value: "CreditCard" },
  { label: "HELOC", value: "HELOC" },
  { label: "PLOC", value: "PLOC" },
];

/**
 * Safely converts unknown value to string.
 * @param {unknown} v - The value to convert
 * @returns {string} The string value or empty string
 */
function s(v: unknown) {
  return typeof v === "string" ? v : "";
}

/**
 * Safely converts unknown value to numeric string.
 * @param {unknown} v - The value to convert
 * @returns {string} The numeric string or empty string
 */
function nToStr(v: unknown) {
  return typeof v === "number" && Number.isFinite(v) ? String(v) : "";
}

/**
 * Props for the AccountForm component.
 * @property {("create"|"edit")} mode - Form mode (create new or edit existing)
 * @property {string} customerId - The customer ID this account belongs to
 * @property {string} [accountId] - The account ID (required when mode is "edit")
 * @property {Partial<Object>} [initial] - Initial form values
 */
const props = defineProps<{
  mode: "create" | "edit";
  customerId: string;
  accountId?: string;
  initial?: Partial<{
    accountType: string;
    currentBalance: number | null;
    availableCredit: number | null;
  }>;
}>();

/**
 * Emits for the AccountForm component.
 * @event saved - Emitted when account is successfully saved
 * @event cancel - Emitted when user cancels form
 */
const emit = defineEmits<{ (e: "saved"): void; (e: "cancel"): void }>();

/** Reactive form state */
const form = reactive({
  accountType: s(props.initial?.accountType) || "",
  initialBalance: "",
  creditLimit: "",
  availableBalance: nToStr(props.initial?.currentBalance ?? props.initial?.availableCredit ?? 0),
});

/** Show initial balance field for deposit account types on create */
const showInitialBalance = computed(() => props.mode === "create" && isDepositAccountType(form.accountType));

/** Show credit limit field for credit account types on create */
const showCreditLimit = computed(() => props.mode === "create" && isCreditAccountType(form.accountType));

/** Label for balance field depends on account type */
const editValueLabel = computed(() => (isCreditAccountType(form.accountType) ? "Available credit" : "Current balance"));

/** Field-level validation errors */
const errors = reactive<Record<string, string>>({});

/** Modal error display state */
const modal = reactive({ open: false, title: "", message: "" });

/** Loading state during API request */
const busy = ref(false);

/**
 * Clears all field-level validation errors.
 */
function clearErrors() {
  Object.keys(errors).forEach(k => delete errors[k]);
}

/**
 * Sets field errors from API error response.
 * @param {ApiError} err - The API error response
 */
function setFieldErrorsFromApi(err: ApiError) {
  clearErrors();
  if (err.errors) {
    for (const [k, msgs] of Object.entries(err.errors)) {
      errors[toCamel(k)] = msgs?.[0] ?? "Invalid value.";
    }
  } else {
    modal.open = true;
    modal.title = err.title;
    modal.message = err.detail;
  }
}

/**
 * Submits the form to create or update an account.
 * Validates input using Zod schema before submission.
 * Emits "saved" event on success.
 * @async
 */
async function submit() {
  clearErrors();

  if (props.mode === "create") {
    const parsed = accountCreateSchema.safeParse({
      accountType: form.accountType,
      initialBalance: form.initialBalance === "" ? undefined : form.initialBalance,
      creditLimit: form.creditLimit === "" ? undefined : form.creditLimit,
    });

    if (!parsed.success) {
      for (const issue of parsed.error.issues) {
        const key = String(issue.path[0] ?? "");
        errors[key] = issue.message;
      }
      return;
    }

    busy.value = true;
    try {
      const payload: any = { AccountType: form.accountType };

      if (isDepositAccountType(form.accountType)) {
        payload.InitialBalance = form.initialBalance === "" ? 0 : Number(form.initialBalance);
      } else if (isCreditAccountType(form.accountType)) {
        payload.CreditLimit = Number(form.creditLimit);
      }

      await api.post(`/customers/${props.customerId}/accounts`, payload);

      modal.open = true;
      modal.title = "Success";
      modal.message = "Account created.";
      emit("saved");
    } catch (e: any) {
      setFieldErrorsFromApi(e);
    } finally {
      busy.value = false;
    }

    return;
  }

  const parsed = accountUpdateSchema.safeParse({
    accountType: form.accountType,
    availableBalance: form.availableBalance,
  });

  if (!parsed.success) {
    for (const issue of parsed.error.issues) {
      const key = String(issue.path[0] ?? "");
      errors[key] = issue.message;
    }
    return;
  }

  busy.value = true;
  try {
    await api.put(`/customers/${props.customerId}/accounts/${props.accountId}`, {
      CustomerId: props.customerId,
      AccountId: props.accountId,
      AccountType: form.accountType,
      AvailableBalance: Number(form.availableBalance),
    });

    modal.open = true;
    modal.title = "Success";
    modal.message = "Account updated.";
    emit("saved");
  } catch (e: any) {
    setFieldErrorsFromApi(e);
  } finally {
    busy.value = false;
  }
}
</script>

<template>
  <form class="card" @submit.prevent="submit">
    <div class="grid">
      <SelectField
        label="Account type*"
        v-model="form.accountType"
        :options="accountTypeOptions"
        :error="errors.accountType"
      />

      <TextField
        v-if="showInitialBalance"
        label="Initial balance"
        v-model="form.initialBalance"
        :error="errors.initialBalance"
      />

      <TextField
        v-if="showCreditLimit"
        label="Credit limit*"
        v-model="form.creditLimit"
        :error="errors.creditLimit"
      />

      <TextField
        v-if="mode==='edit'"
        :label="`${editValueLabel}`"
        v-model="form.availableBalance"
        :error="errors.availableBalance"
      />
    </div>

    <div class="row">
      <button class="btn" type="submit" :disabled="busy">{{ busy ? "Saving…" : "Submit" }}</button>
      <button class="btn ghost" type="button" @click="emit('cancel')">Cancel</button>
    </div>

    <Modal :open="modal.open" :title="modal.title" :message="modal.message" @close="modal.open=false" />
  </form>
</template>

<style scoped>
.card {
  background: #fff;
  border: 1px solid #eee;
  padding: 14px;
}

.grid {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(2, minmax(0, 1fr));
}

@media (max-width: 860px) {
  .grid {
    grid-template-columns: 1fr;
  }
}

.row {
  display: flex;
  gap: 10px;
  margin-top: 12px;
}

.btn {
  background: #dcdcdc;
  border: 1px solid #ddd;
  border-radius: 1px;
  cursor: pointer;
  padding: 10px 12px;
}

.btn:disabled {
  cursor: not-allowed;
  opacity: 0.6;
}

.ghost {
  background: #fff;
}

.label {
  color: #222;
  display: block;
  font-size: 1em;
  margin-bottom: 6px;
}

.input {
  border: 1px solid #ddd;
  border-radius: 1px;
  padding: 10px 12px;
  width: 100%;
}

.error {
  color: #b00020;
  font-size: 12px;
  margin-top: 6px;
}
</style>
