<script setup lang="ts">
import { onMounted, reactive, computed } from "vue";
import { useRouter } from "vue-router";
import Modal from "../../../components/Modal.vue";
import CustomerForm from "../components/CustomerForm.vue";
import AccountForm from "../../accounts/components/AccountForm.vue";
import { api, type ApiError } from "../../../app/api";
import { isCreditAccountType, formatAccountTypeLabel } from "../../accounts/accountTypes";

const props = defineProps<{ id: string }>();
const router = useRouter();

function balanceLabelFor(accountType: string) {
  return isCreditAccountType(accountType)
    ? "Available credit"
    : "Current balance";
}

type AccountSummary = {
  id: string;
  accountNumber: string;
  accountType: string;
  availableBalance: number;
};

type CustomerDetail = {
  id: string;
  firstName: string;
  lastName: string;
  suffix?: string | null;
  title?: string | null;
  ssnTin: string;
  email: string;
  street: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  customerSince: string;
  accounts: AccountSummary[];
};

const state = reactive({
  loading: false,
  customer: null as CustomerDetail | null,
  showEdit: false,
  showAddAccount: false,
});

const modal = reactive({ open: false, title: "", message: "" });

const fullName = computed(() => {
  const c = state.customer;
  if (!c) return "";
  const parts = [c.title, c.firstName, c.lastName, c.suffix].filter(Boolean);
  return parts.join(" ");
});

async function load() {
  state.loading = true;
  try {
    const res = await api.get<CustomerDetail>(`/customers/${props.id}`);
    state.customer = res.data;
  } catch (e: any) {
    const err = e as ApiError;
    modal.open = true;
    modal.title = err.status === 404 ? "Customer not found" : err.title;
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
      <button class="btn ghost" @click="router.push('/customers')">Back</button>
      <h1 v-if="state.customer">{{ fullName }}</h1>
      <div class="spacer"></div>
      <button class="btn" v-if="state.customer" @click="state.showEdit = !state.showEdit">
        {{ state.showEdit ? "Close edit" : "Edit customer" }}
      </button>
    </header>

    <div v-if="state.loading" class="muted">Loading…</div>

    <div v-if="state.customer" class="stack">
      <section class="card">
        <h2 class="h2">Customer details</h2>
        <dl class="dl">
          <div class="row"><dt>ID</dt><dd>{{ state.customer.id }}</dd></div>
          <div class="row"><dt>Last name</dt><dd>{{ state.customer.lastName }}</dd></div>
          <div class="row"><dt>First name</dt><dd>{{ state.customer.firstName }}</dd></div>
          <div class="row"><dt>Suffix</dt><dd>{{ state.customer.suffix || "—" }}</dd></div>
          <div class="row"><dt>Title</dt><dd>{{ state.customer.title || "—" }}</dd></div>
          <div class="row"><dt>SSN/TIN</dt><dd>{{ state.customer.ssnTin }}</dd></div>
          <div class="row"><dt>Email</dt><dd>{{ state.customer.email }}</dd></div>
          <div class="row"><dt>Street</dt><dd>{{ state.customer.street }}</dd></div>
          <div class="row"><dt>City</dt><dd>{{ state.customer.city }}</dd></div>
          <div class="row"><dt>State</dt><dd>{{ state.customer.state }}</dd></div>
          <div class="row"><dt>ZIP/Postal</dt><dd>{{ state.customer.postalCode }}</dd></div>
          <div class="row"><dt>Country</dt><dd>{{ state.customer.country }}</dd></div>
          <div class="row"><dt>Customer since</dt><dd>{{ state.customer.customerSince }}</dd></div>
        </dl>
      </section>

      <CustomerForm
        v-if="state.showEdit"
        mode="edit"
        :customer-id="state.customer.id"
        :initial="{
          firstName: state.customer.firstName,
          lastName: state.customer.lastName,
          suffix: state.customer.suffix ?? '',
          title: state.customer.title ?? '',
          ssnTin: state.customer.ssnTin,
          email: state.customer.email,
          street: state.customer.street,
          city: state.customer.city,
          state: state.customer.state,
          postalCode: state.customer.postalCode,
          country: state.customer.country
        }"
        @saved="state.showEdit=false; load()"
        @cancel="state.showEdit=false"
      />

      <section class="card">
        <header class="subbar">
          <h2 class="h2">Accounts</h2>
          <button class="btn" @click="state.showAddAccount = !state.showAddAccount">
            {{ state.showAddAccount ? 'Close' : 'Add account' }}
          </button>
        </header>

        <AccountForm
          v-if="state.showAddAccount"
          mode="create"
          :customer-id="state.customer.id"
          @saved="state.showAddAccount=false; load()"
          @cancel="state.showAddAccount=false"
        />

        <div class="accounts">
          <div v-for="a in state.customer.accounts" :key="a.id" class="account">
            <div class="aTop">
              <div class="aNum">{{ a.accountNumber }}</div>
              <div class="aType">{{ formatAccountTypeLabel(a.accountType) }}</div>
            </div>

            <div class="aBal">
              {{ balanceLabelFor(a.accountType) }}: ${{ a.availableBalance.toFixed(2) }}
            </div>

            <div class="accountFooter">
              <button
                class="btn ghost"
                @click="router.push(`/customers/${state.customer!.id}/accounts/${a.id}`)"
              >
                View details
              </button>
            </div>
          </div>

          <div v-if="state.customer.accounts.length === 0" class="muted">
            No accounts yet.
          </div>
        </div>
      </section>
    </div>

    <Modal
      :open="modal.open"
      :title="modal.title"
      :message="modal.message"
      @close="modal.open=false"
    />
  </div>
</template>

<style scoped>
.page{max-width:980px;margin:24px auto;padding:0 14px;}
.bar{display:flex;align-items:center;gap:12px;margin-bottom:14px;}
.spacer{flex:1;}
.stack{display:flex;flex-direction:column;gap:14px;}
.card{background:#fff;border:1px solid #eee;padding:14px;}
.h2{margin:0 0 10px 0;}
.dl{display:grid;grid-template-columns:1fr 1fr;gap:10px;}
@media (max-width: 860px){.dl{grid-template-columns:1fr;}}
.row{display:flex;gap:10px;}
dt{min-width:130px;color:#006400;font-size:1em;}
dd{margin:0;color:#111;word-break:break-word;}
.subbar{display:flex;align-items:center;justify-content:space-between;margin-bottom:10px;}
.btn{padding:10px 12px;border-radius:1px;border:1px solid #ddd;background:#DCDCDC;cursor:pointer;}
.ghost{background:#fff;}
.accounts{display:flex;flex-direction:column;gap:10px;}
.account{border:1px solid #eee;padding:12px;background:#fafafa;display:flex;flex-direction:column;gap:6px;}
.aTop{display:flex;justify-content:space-between;gap:12px;}
.aNum{font-weight:700;}
.aType{color:#006400;}
.aBal{margin:6px 0;color:#333;}
.accountFooter{display:flex;justify-content:flex-end;margin-top:4px;}
.muted{color:#666;}
</style>
