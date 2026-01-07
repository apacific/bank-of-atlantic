<script setup lang="ts">
import { reactive, ref } from "vue";
import { customerSchema } from "../validation/customerSchema";
import type { CustomerFormModel } from "../validation/customerSchema";
import TextField from "../../../components/forms/TextField.vue";
import Modal from "../../../components/Modal.vue";
import { api, type ApiError, toCamel } from "../../../app/api";

function s(v: unknown) {
  return typeof v === "string" ? v : "";
}

const props = defineProps<{
  mode: "create" | "edit";
  customerId?: string;
  initial?: Partial<CustomerFormModel>;
}>();

const emit = defineEmits<{ (e: "saved"): void; (e: "cancel"): void }>();

const form = reactive<CustomerFormModel>({
  firstName: s(props.initial?.firstName) || "",
  lastName: s(props.initial?.lastName) || "",
  suffix: s(props.initial?.suffix) || "",
  title: s(props.initial?.title ?? "") || "",
  ssnTin: s(props.initial?.ssnTin) || "",
  email: s(props.initial?.email) || "",
  street: s(props.initial?.street) || "",
  city: s(props.initial?.city) || "",
  state: s(props.initial?.state) || "",
  postalCode: s(props.initial?.postalCode) || "",
  country: s(props.initial?.country) || "",
});

const errors = reactive<Record<string, string>>({});
const modal = reactive({ open: false, title: "", message: "" });
const busy = ref(false);

function clearErrors() {
  Object.keys(errors).forEach((k) => delete errors[k]);
}

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

async function submit() {
  clearErrors();
  const parsed = customerSchema.safeParse(form);
  if (!parsed.success) {
    for (const issue of parsed.error.issues) {
      const key = String(issue.path[0] ?? "");
      errors[key] = issue.message;
    }
    return;
  }

  busy.value = true;
  try {
    if (props.mode === "create") {
      await api.post("/customers", {
        FirstName: form.firstName,
        LastName: form.lastName,
        Suffix: form.suffix || null,
        Title: form.title || null,
        SsnTin: form.ssnTin,
        Email: form.email,
        Street: form.street,
        City: form.city,
        State: form.state,
        PostalCode: form.postalCode,
        Country: form.country,
      });
    } else {
      await api.put(`/customers/${props.customerId}`, {
        Id: props.customerId,
        FirstName: form.firstName,
        LastName: form.lastName,
        Suffix: form.suffix || null,
        Title: form.title || null,
        SsnTin: form.ssnTin,
        Email: form.email,
        Street: form.street,
        City: form.city,
        State: form.state,
        PostalCode: form.postalCode,
        Country: form.country,
      });
    }

    modal.open = true;
    modal.title = "Success";
    modal.message = props.mode === "create" ? "Customer created." : "Customer updated.";
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
      <TextField label="First name*" v-model="form.firstName" :error="errors.firstName" />
      <TextField label="Last name*" v-model="form.lastName" :error="errors.lastName" />
      <TextField label="Suffix" v-model="form.suffix" :error="errors.suffix" />
      <TextField label="Title" v-model="form.title" :error="errors.title" />
      <TextField label="SSN/TIN*" v-model="form.ssnTin" :error="errors.ssnTin" />
      <TextField label="Email*" v-model="form.email" :error="errors.email" type="email" />
      <TextField label="Street*" v-model="form.street" :error="errors.street" />
      <TextField label="City*" v-model="form.city" :error="errors.city" />
      <TextField label="State*" v-model="form.state" :error="errors.state" />
      <TextField label="ZIP / Postal code*" v-model="form.postalCode" :error="errors.postalCode" />
      <TextField label="Country*" v-model="form.country" :error="errors.country" />
    </div>

    <div class="row">
      <button class="btn" type="submit" :disabled="busy">{{ busy ? "Saving…" : "Submit" }}</button>
      <button class="btn ghost" type="button" @click="emit('cancel')">Cancel</button>
    </div>

    <Modal :open="modal.open" :title="modal.title" :message="modal.message" @close="modal.open = false" />
  </form>
</template>

<style scoped>
.card{background:#fff;border:1px solid #eee;padding:14px;}
.grid{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:12px;}
@media (max-width: 860px){.grid{grid-template-columns:1fr;}}
.row{display:flex;gap:10px;margin-top:12px;}
.btn{padding:10px 12px;border-radius:1px;border:1px solid #ddd;background:#DCDCDC;cursor:pointer;}
.btn:disabled{opacity:.6;cursor:not-allowed;}
.ghost{background:#fff;}
</style>
