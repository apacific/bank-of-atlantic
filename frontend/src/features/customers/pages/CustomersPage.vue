<script setup lang="ts">
import { onMounted, reactive } from "vue";
import { useRouter } from "vue-router";
import Modal from "../../../components/Modal.vue";
import CustomerForm from "../components/CustomerForm.vue";
import { api, type ApiError } from "../../../app/api";

type CustomerListItem = { id: string; firstName: string; lastName: string; customerSince: string };

const router = useRouter();
const state = reactive({
  items: [] as CustomerListItem[],
  loading: false,
  showCreate: false,
});
const modal = reactive({ open: false, title: "", message: "" });

async function load() {
  state.loading = true;
  try {
    const res = await api.get<CustomerListItem[]>("/customers");
    state.items = res.data;
  } catch (e: any) {
    const err = e as ApiError;
    modal.open = true;
    modal.title = err.title;
    modal.message = err.detail;
  } finally {
    state.loading = false;
  }
}

async function removeCustomer(id: string) {
  try {
    await api.delete(`/customers/${id}`);
    state.items = state.items.filter(x => x.id !== id);
  } catch (e: any) {
    const err = e as ApiError;
    modal.open = true;
    modal.title = err.status === 409 ? "Cannot delete customer" : err.title;
    modal.message = err.detail;
  }
}

onMounted(load);
</script>

<template>
  <div class="page">
    <header class="bar">
      <h1>Customers</h1>
      <button class="btn" @click="state.showCreate = !state.showCreate">
        {{ state.showCreate ? "Close" : "Add customer" }}
      </button>
    </header>

    <CustomerForm
      v-if="state.showCreate"
      mode="create"
      @saved="state.showCreate = false; load()"
      @cancel="state.showCreate = false"
    />

    <div v-if="state.loading" class="muted">Loading…</div>

    <div class="list" v-else>
      <div v-for="c in state.items" :key="c.id" class="item">
        <button class="link" @click="router.push(`/customers/${c.id}`)">
          <div class="name">{{ c.lastName }}, {{ c.firstName }}</div>
          <div class="meta">Customer since: {{ c.customerSince }}</div>
        </button>
        <button class="btn danger" @click="removeCustomer(c.id)">Delete</button>
      </div>
      <div v-if="state.items.length === 0" class="muted">No customers yet.</div>
    </div>

    <Modal :open="modal.open" :title="modal.title" :message="modal.message" @close="modal.open=false" />
  </div>
</template>

<style scoped>
.page{max-width:980px;margin:24px auto;padding:0 14px;}
.bar{display:flex;align-items:center;justify-content:space-between;margin-bottom:14px;}
.btn{padding:10px 12px;border-radius:1px;border:1px solid #ddd;background:#DCDCDC;cursor:pointer;}
.danger{border-color:#f2c2c2;background:#fff0f0;}
.list{display:flex;flex-direction:column;gap:10px;margin-top:14px;}
.item{display:flex;align-items:center;justify-content:space-between;gap:12px;border:1px solid #eee;padding:12px;background:#fff;}
.link{all:unset;cursor:pointer;flex:1;}
.name{font-weight:600;}
.meta{color:#006400;font-size:1em;margin-top:4px;}
.muted{color:#666;margin-top:10px;}
</style>
