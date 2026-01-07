<script setup lang="ts">
import { onMounted, reactive, computed } from "vue";
import { useRouter } from "vue-router";
import Modal from "../../../components/Modal.vue";
import AccountForm from "../components/AccountForm.vue";
import { api, type ApiError } from "../../../app/api";
import { useAuth } from "../../auth/authStore";
import { isCreditAccountType, formatAccountTypeLabel } from "../accountTypes";

const props = defineProps<{ customerId: string; accountId: string }>();
const router = useRouter();
const auth = useAuth();

type AccountDetail = {
  id: string;
  customerId: string;
  accountNumber: string;
  accountType: string;
  dateOpened: string;
  availableBalance: number;
};

type CustomerDetail = {
  id: string;
  firstName: string;
  lastName: string;
  suffix?: string | null;
  title?: string | null;
};

const state = reactive({
  loading: false,
  customer: null as CustomerDetail | null,
  account: null as AccountDetail | null,
  showEdit: false,
});

const modal = reactive({ open: false, title: "", message: "" });

const canDelete = computed(() => auth.role.value === "Manager");

const customerName = computed(() => {
  const c = state.customer;
  if (!c) return "Customer";
  const parts = [c.title, c.firstName, c.lastName, c.suffix].filter(Boolean);
  return parts.join(" ");
});

const amountLabel = computed(() => {
  if (!state.account) return "Amount";
  return isCreditAccountType(state.account.accountType)
    ? "Available credit"
    : "Current balance";
});

const accountTypeLabel = computed(() => {
  if (!state.account) return "";
  return formatAccountTypeLabel(state.account.accountType);
});

const amountDisplay = computed(() => {
  if (!state.account) return "0.00";
  return state.account.availableBalance.toFixed(2);
});

const initialForm = computed(() => {
  const acc = state.account;
  if (!acc) {
    return {
      accountType: "",
      currentBalance: null as number | null,
      availableCredit: null as number | null,
    };
  }
  const isCredit = isCreditAccountType(acc.accountType);
  return {
    accountType: acc.accountType,
    currentBalance: isCredit ? null : acc.availableBalance,
    availableCredit: isCredit ? acc.availableBalance : null,
  };
});

async function deleteAccount() {
  if (!state.account) return;

  try {
    await api.delete(`/customers/${props.customerId}/accounts/${props.accountId}`);
    modal.open = true;
    modal.title = "Success";
    modal.message = "Account deleted.";
    router.push(`/customers/${props.customerId}`);
  } catch (e: any) {
    const err = e;
    modal.open = true;
    modal.title = err?.response?.status === 409 ? "Cannot delete account" : "Error";
    modal.message = err?.response?.data?.detail ?? "Unexpected error.";
  }
}

async function load() {
  state.loading = true;
  try {
    const [custRes, acctRes] = await Promise.all([
      api.get<CustomerDetail>(`/customers/${props.customerId}`),
      api.get<AccountDetail>(`/customers/${props.customerId}/accounts/${props.accountId}`),
    ]);

    state.customer = custRes.data;
    state.account = acctRes.data;
  } catch (e: any) {
    const err = e as ApiError;
    modal.open = true;
    modal.title = err.status === 404 ? "Not found" : err.title;
    modal.message = err.detail;
  } finally {
    state.loading = false;
  }
}

onMounted(load);
</script>

<template>
  <div class="page">
    <header class="bar">
      <button class="btn ghost" @click="router.push(`/customers/${props.customerId}`)">Back</button>

      <h1>{{ customerName }}</h1>

      <div class="spacer"></div>

      <button class="btn danger" v-if="canDelete" @click="deleteAccount">
        Delete account
      </button>

      <button class="btn" v-if="state.account" @click="state.showEdit = !state.showEdit">
        {{ state.showEdit ? "Close edit" : "Edit" }}
      </button>
    </header>

    <div v-if="state.loading" class="muted">Loading…</div>

    <div v-if="state.account" class="stack">
      <section class="card">
        <h2 class="h2">Account details</h2>
        <dl class="dl">
          <div class="row">
            <dt>Account number</dt>
            <dd>{{ state.account.accountNumber }}</dd>
          </div>
          <div class="row">
            <dt>Account type</dt>
            <dd>{{ accountTypeLabel }}</dd>
          </div>
          <div class="row">
            <dt>Date opened</dt>
            <dd>{{ state.account.dateOpened }}</dd>
          </div>
          <div class="row">
            <dt>{{ amountLabel }}</dt>
            <dd>$ {{ amountDisplay }}</dd>
          </div>
        </dl>
      </section>

      <AccountForm
        v-if="state.showEdit && state.account"
        mode="edit"
        :customer-id="props.customerId"
        :account-id="props.accountId"
        :initial="initialForm"
        @saved="state.showEdit = false; load()"
        @cancel="state.showEdit = false"
      />
    </div>

    <Modal
      :open="modal.open"
      :title="modal.title"
      :message="modal.message"
      @close="modal.open = false"
    />
  </div>
</template>

<style scoped>
.page{max-width:980px;margin:24px auto;padding:0 14px;}
.bar{display:flex;align-items:center;gap:12px;margin-bottom:14px;}
.danger{border-color:#f1c0c0;background:#fff0f0;}
.spacer{flex:1;}
.stack{display:flex;flex-direction:column;gap:14px;}
.card{background:#fff;border:1px solid #eee;padding:14px;}
.h2{margin:0 0 10px 0;font-size:16px;}
.dl{display:grid;grid-template-columns:1fr 1fr;gap:10px;}
@media (max-width: 860px){.dl{grid-template-columns:1fr;}}
.row{display:flex;gap:10px;}
dt{min-width:130px;color:#006400;font-size:1em;}
dd{margin:0;color:#111;word-break:break-word;}
.btn{padding:10px 12px;border-radius:1px;border:1px solid #ddd;background:#DCDCDC;cursor:pointer;}
.ghost{background:#fff;}
.muted{color:#666;}
</style>
