<template>
  <div class="page">
    <div class="page-header">
      <h2>{{ auth.isManager ? 'Все заказы' : 'Мои заказы' }}</h2>
      <div class="header-actions">
        <span class="username">👤 {{ auth.user }}</span>
        <button class="btn-logout" @click="handleLogout">Выйти</button>
      </div>
    </div>

    <nav class="tabs">
      <RouterLink to="/catalog">Каталог</RouterLink>
      <RouterLink to="/orders">{{ auth.isManager ? 'Все заказы' : 'Мои заказы' }}</RouterLink>
      <RouterLink v-if="auth.isManager" to="/manager">Управление</RouterLink>
    </nav>

    <div class="filters">
      <button
          v-for="status in statuses"
          :key="status.value"
          :class="['filter-btn', { active: selectedStatus === status.value }]"
          @click="selectedStatus = status.value"
      >
        {{ status.label }}
      </button>
    </div>

    <div v-if="ordersStore.loading" class="loading">Загрузка...</div>

    <div v-if="filteredOrders.length === 0 && !ordersStore.loading" class="empty">
      Заказов нет
    </div>

    <div class="orders-list">
      <div v-for="order in filteredOrders" :key="order.id" class="order-card">
        <div class="order-header">
          <div>
            <span class="order-number">#{{ order.orderNumber }}</span>
            <span :class="['status', statusClass(order.status)]">{{ order.status }}</span>
          </div>
          <div class="order-date">{{ formatDate(order.orderDate) }}</div>
        </div>

        <div class="order-info">
          <div v-if="auth.isManager">
            Заказчик ID: <strong>{{ order.customerId }}</strong>
          </div>
          <div v-if="order.shipmentDate">
            Дата доставки: <strong>{{ formatDate(order.shipmentDate) }}</strong>
          </div>
        </div>

        <div class="order-items">
          <div v-for="item in order.orderItems" :key="item.id" class="order-item">
            <span>{{ item.itemId }}</span>
            <span>× {{ item.itemsCount }}</span>
            <span>{{ (item.itemPrice * item.itemsCount).toFixed(2) }} руб.</span>
          </div>
        </div>

        <div v-if="auth.isManager" class="order-actions">
          <button v-if="order.status === 'Новый'" class="btn-primary" @click="openConfirm(order)">
            Подтвердить
          </button>
          <button v-if="order.status === 'Выполняется'" class="btn-success" @click="completeOrder(order.id)">
            Закрыть заказ
          </button>
        </div>

        <div v-if="auth.isCustomer && order.status === 'Новый'" class="order-actions">
          <button class="btn-danger" @click="deleteOrder(order.id)">Удалить заказ</button>
        </div>
      </div>
    </div>

    <div v-if="showConfirmModal" class="modal-overlay" @click.self="showConfirmModal = false">
      <div class="modal">
        <h3>Подтвердить заказ #{{ confirmingOrder?.orderNumber }}</h3>
        <div v-if="modalError" class="error">{{ modalError }}</div>
        <div class="field">
          <label>Дата доставки</label>
          <input v-model="shipmentDate" type="date" />
        </div>
        <div class="modal-actions">
          <button class="btn-primary" @click="confirmOrder">Подтвердить</button>
          <button class="btn-secondary" @click="showConfirmModal = false">Отмена</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useOrdersStore } from '@/stores/orders'

const router = useRouter()
const auth = useAuthStore()
const ordersStore = useOrdersStore()

const selectedStatus = ref('')
const showConfirmModal = ref(false)
const confirmingOrder = ref(null)
const shipmentDate = ref('')
const modalError = ref('')

const statuses = [
  { value: '', label: 'Все' },
  { value: 'Новый', label: 'Новый' },
  { value: 'Выполняется', label: 'Выполняется' },
  { value: 'Выполнен', label: 'Выполнен' }
]

const filteredOrders = computed(() => {
  if (!selectedStatus.value) return ordersStore.orders
  return ordersStore.orders.filter(o => o.status === selectedStatus.value)
})

onMounted(() => {
  if (auth.isManager) ordersStore.fetchAll()
  else ordersStore.fetchMy()
})

function handleLogout() {
  auth.logout()
  router.push('/login')
}

function formatDate(date) {
  if (!date) return ''
  return new Date(date).toLocaleDateString('ru-RU')
}

function statusClass(status) {
  if (status === 'Новый') return 'status-new'
  if (status === 'Выполняется') return 'status-progress'
  if (status === 'Выполнен') return 'status-done'
  return ''
}

function openConfirm(order) {
  confirmingOrder.value = order
  shipmentDate.value = ''
  modalError.value = ''
  showConfirmModal.value = true
}

async function confirmOrder() {
  if (!shipmentDate.value) {
    modalError.value = 'Выберите дату доставки'
    return
  }
  try {
    await ordersStore.confirm(confirmingOrder.value.id, shipmentDate.value)
    showConfirmModal.value = false
  } catch (e) {
    modalError.value = 'Ошибка подтверждения'
  }
}

async function completeOrder(id) {
  if (!confirm('Закрыть заказ?')) return
  await ordersStore.complete(id)
}

async function deleteOrder(id) {
  if (!confirm('Удалить заказ?')) return
  await ordersStore.remove(id)
}
</script>

<style scoped>
.page { max-width: 900px; margin: 0 auto; padding: 24px; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
.header-actions { display: flex; gap: 12px; align-items: center; }
.username { display: flex; align-items: center; font-size: 14px; color: #555; background: #f0f2f5; padding: 8px 14px; border-radius: 8px; font-weight: 500; }

.tabs { display: flex; gap: 24px; margin-bottom: 24px; border-bottom: 2px solid #eee; padding-bottom: 8px; }
.tabs a { text-decoration: none; color: #666; font-weight: 500; }
.tabs a.router-link-active { color: #4f8ef7; border-bottom: 2px solid #4f8ef7; padding-bottom: 8px; }

.filters { display: flex; gap: 8px; margin-bottom: 20px; }
.filter-btn { padding: 8px 16px; border: 1px solid #ddd; border-radius: 20px; background: white; cursor: pointer; font-size: 14px; }
.filter-btn.active { background: #4f8ef7; color: white; border-color: #4f8ef7; }

.orders-list { display: flex; flex-direction: column; gap: 16px; }
.order-card { background: white; border-radius: 12px; padding: 20px; box-shadow: 0 2px 8px rgba(0,0,0,0.08); }
.order-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px; }
.order-number { font-size: 18px; font-weight: 700; margin-right: 12px; }
.order-date { color: #888; font-size: 14px; }
.order-info { color: #555; font-size: 14px; margin-bottom: 12px; }

.status { padding: 4px 12px; border-radius: 20px; font-size: 13px; font-weight: 500; }
.status-new { background: #e3f2fd; color: #1565c0; }
.status-progress { background: #fff8e1; color: #f57f17; }
.status-done { background: #e8f5e9; color: #2e7d32; }

.order-items { border-top: 1px solid #f0f0f0; padding-top: 12px; margin-bottom: 12px; }
.order-item { display: flex; justify-content: space-between; padding: 4px 0; font-size: 14px; color: #555; }
.order-actions { display: flex; gap: 8px; }

.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.4); display: flex; align-items: center; justify-content: center; z-index: 100; }
.modal { background: white; border-radius: 16px; padding: 32px; width: 100%; max-width: 400px; }
.modal h3 { margin-bottom: 20px; }
.modal-actions { display: flex; gap: 12px; margin-top: 20px; }

.field { margin-bottom: 14px; display: flex; flex-direction: column; gap: 6px; }
label { font-size: 14px; color: #555; }
input { padding: 10px 14px; border: 1px solid #ddd; border-radius: 8px; font-size: 15px; outline: none; }

.btn-primary { padding: 10px 18px; background: #4f8ef7; color: white; border: none; border-radius: 8px; cursor: pointer; }
.btn-secondary { padding: 10px 18px; background: #f0f2f5; color: #333; border: none; border-radius: 8px; cursor: pointer; }
.btn-success { padding: 10px 18px; background: #4caf50; color: white; border: none; border-radius: 8px; cursor: pointer; }
.btn-danger { padding: 10px 18px; background: #ff5252; color: white; border: none; border-radius: 8px; cursor: pointer; }
.btn-logout { padding: 10px 18px; background: #eee; color: #333; border: none; border-radius: 8px; cursor: pointer; }
.loading { text-align: center; padding: 40px; color: #888; }
.empty { text-align: center; padding: 40px; color: #aaa; }
.error { background: #fff0f0; color: #e53935; padding: 10px; border-radius: 8px; margin-bottom: 12px; }
</style>