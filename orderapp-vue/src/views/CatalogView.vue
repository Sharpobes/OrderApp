<template>
  <div class="page">
    <div class="page-header">
      <h2>Каталог товаров</h2>
      <div class="header-actions">
        <span class="username">👤 {{ auth.user }}</span>
        <button v-if="auth.isManager" class="btn-primary" @click="openCreate">
          + Добавить товар
        </button>
        <button class="btn-logout" @click="handleLogout">Выйти</button>
      </div>
    </div>

    <nav class="tabs">
      <RouterLink to="/catalog">Каталог</RouterLink>
      <RouterLink to="/orders">{{ auth.isManager ? 'Все заказы' : 'Мои заказы' }}</RouterLink>
      <RouterLink v-if="auth.isManager" to="/manager">Управление</RouterLink>
    </nav>

    <div v-if="itemsStore.loading" class="loading">Загрузка...</div>

    <div v-if="auth.isCustomer && cart.length > 0" class="cart">
      <h3>Корзина ({{ cart.length }} позиции)</h3>
      <div v-for="item in cart" :key="item.id" class="cart-item">
        <span>{{ item.name }}</span>
        <div class="cart-controls">
          <button @click="decreaseCount(item)">−</button>
          <span>{{ item.count }}</span>
          <button @click="increaseCount(item)">+</button>
          <button class="btn-remove" @click="removeFromCart(item)">✕</button>
        </div>
        <span>{{ (item.price * item.count).toFixed(2) }} руб.</span>
      </div>
      <div class="cart-total">
        Итого: <strong>{{ cartTotal.toFixed(2) }} руб.</strong>
      </div>
      <button class="btn-primary" @click="showOrderModal = true">Оформить заказ</button>
    </div>

    <div class="items-grid">
      <div v-for="item in itemsStore.items" :key="item.id" class="item-card">
        <div class="item-category">{{ item.category }}</div>
        <div class="item-code">{{ item.code }}</div>
        <div class="item-name">{{ item.name }}</div>
        <div class="item-price">{{ item.price.toFixed(2) }} руб.</div>
        <div class="item-actions">
          <button v-if="auth.isCustomer" class="btn-primary" @click="addToCart(item)">
            В корзину
          </button>
          <template v-if="auth.isManager">
            <button class="btn-secondary" @click="openEdit(item)">Редактировать</button>
            <button class="btn-danger" @click="deleteItem(item.id)">Удалить</button>
          </template>
        </div>
      </div>
    </div>

    <div v-if="showItemModal" class="modal-overlay" @click.self="showItemModal = false">
      <div class="modal">
        <h3>{{ editingItem ? 'Редактировать товар' : 'Новый товар' }}</h3>
        <div v-if="modalError" class="error">{{ modalError }}</div>
        <div class="field">
          <label>Код (XX-XXXX-YYXX)</label>
          <input v-model="form.code" placeholder="01-0001-ABCD" />
        </div>
        <div class="field">
          <label>Наименование</label>
          <input v-model="form.name" placeholder="Название товара" />
        </div>
        <div class="field">
          <label>Цена</label>
          <input v-model.number="form.price" type="number" placeholder="0.00" />
        </div>
        <div class="field">
          <label>Категория (макс 30 символов)</label>
          <input v-model="form.category" maxlength="30" placeholder="Категория" />
        </div>
        <div class="modal-actions">
          <button class="btn-primary" @click="saveItem">Сохранить</button>
          <button class="btn-secondary" @click="showItemModal = false">Отмена</button>
        </div>
      </div>
    </div>

    <div v-if="showOrderModal" class="modal-overlay" @click.self="showOrderModal = false">
      <div class="modal">
        <h3>Оформить заказ</h3>
        <div v-if="modalError" class="error">{{ modalError }}</div>
        <div class="order-summary">
          <div v-for="item in cart" :key="item.id" class="order-row">
            <span>{{ item.name }} × {{ item.count }}</span>
            <span>{{ (item.price * item.count).toFixed(2) }} руб.</span>
          </div>
          <div class="order-total">Итого: {{ cartTotal.toFixed(2) }} руб.</div>
        </div>
        <div class="modal-actions">
          <button class="btn-primary" @click="submitOrder">Подтвердить</button>
          <button class="btn-secondary" @click="showOrderModal = false">Отмена</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useItemsStore } from '@/stores/items'
import { useOrdersStore } from '@/stores/orders'

const router = useRouter()
const auth = useAuthStore()
const itemsStore = useItemsStore()
const ordersStore = useOrdersStore()

const showItemModal = ref(false)
const showOrderModal = ref(false)
const editingItem = ref(null)
const modalError = ref('')
const form = ref({ code: '', name: '', price: 0, category: '' })
const cart = ref([])

const cartTotal = computed(() =>
    cart.value.reduce((sum, i) => sum + i.price * i.count, 0)
)

onMounted(() => itemsStore.fetchAll())

function handleLogout() {
  auth.logout()
  router.push('/login')
}

function openCreate() {
  editingItem.value = null
  form.value = { code: '', name: '', price: 0, category: '' }
  modalError.value = ''
  showItemModal.value = true
}

function openEdit(item) {
  editingItem.value = item
  form.value = { code: item.code, name: item.name, price: item.price, category: item.category }
  modalError.value = ''
  showItemModal.value = true
}

async function saveItem() {
  modalError.value = ''
  try {
    if (editingItem.value) {
      await itemsStore.update(editingItem.value.id, form.value)
    } else {
      await itemsStore.create(form.value)
    }
    showItemModal.value = false
  } catch (e) {
    modalError.value = 'Ошибка сохранения'
  }
}

async function deleteItem(id) {
  if (!confirm('Удалить товар?')) return
  await itemsStore.remove(id)
}

function addToCart(item) {
  const existing = cart.value.find(i => i.id === item.id)
  if (existing) existing.count++
  else cart.value.push({ ...item, count: 1 })
}

function increaseCount(item) { item.count++ }
function decreaseCount(item) {
  if (item.count > 1) item.count--
  else removeFromCart(item)
}
function removeFromCart(item) {
  cart.value = cart.value.filter(i => i.id !== item.id)
}

async function submitOrder() {
  modalError.value = ''
  try {
    const items = cart.value.map(i => ({ itemId: i.id, count: i.count }))
    await ordersStore.create(items)
    cart.value = []
    showOrderModal.value = false
    router.push('/orders')
  } catch (e) {
    modalError.value = 'Ошибка оформления заказа'
  }
}
</script>

<style scoped>
.page { max-width: 1200px; margin: 0 auto; padding: 24px; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
.header-actions { display: flex; gap: 12px; align-items: center; }
.username { display: flex; align-items: center; font-size: 14px; color: #555; background: #f0f2f5; padding: 8px 14px; border-radius: 8px; font-weight: 500; }

.tabs { display: flex; gap: 24px; margin-bottom: 24px; border-bottom: 2px solid #eee; padding-bottom: 8px; }
.tabs a { text-decoration: none; color: #666; font-weight: 500; }
.tabs a.router-link-active { color: #4f8ef7; border-bottom: 2px solid #4f8ef7; padding-bottom: 8px; }

.items-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(250px, 1fr)); gap: 16px; }
.item-card { background: white; border-radius: 12px; padding: 20px; box-shadow: 0 2px 8px rgba(0,0,0,0.08); display: flex; flex-direction: column; gap: 8px; }
.item-category { font-size: 12px; color: #888; text-transform: uppercase; }
.item-code { font-size: 13px; color: #aaa; }
.item-name { font-size: 16px; font-weight: 600; color: #333; }
.item-price { font-size: 18px; font-weight: 700; color: #4f8ef7; }
.item-actions { display: flex; gap: 8px; margin-top: 8px; flex-wrap: wrap; }

.cart { background: #f8f9ff; border: 1px solid #d0dcff; border-radius: 12px; padding: 20px; margin-bottom: 24px; }
.cart h3 { margin-bottom: 12px; }
.cart-item { display: flex; align-items: center; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #eee; }
.cart-controls { display: flex; align-items: center; gap: 8px; }
.cart-controls button { width: 28px; height: 28px; border: 1px solid #ddd; border-radius: 6px; background: white; cursor: pointer; }
.cart-total { margin-top: 12px; font-size: 16px; text-align: right; }

.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.4); display: flex; align-items: center; justify-content: center; z-index: 100; }
.modal { background: white; border-radius: 16px; padding: 32px; width: 100%; max-width: 480px; }
.modal h3 { margin-bottom: 20px; }
.modal-actions { display: flex; gap: 12px; margin-top: 20px; }

.field { margin-bottom: 14px; display: flex; flex-direction: column; gap: 6px; }
label { font-size: 14px; color: #555; }
input { padding: 10px 14px; border: 1px solid #ddd; border-radius: 8px; font-size: 15px; outline: none; }
input:focus { border-color: #4f8ef7; }

.order-summary { background: #f8f9ff; border-radius: 8px; padding: 16px; margin: 16px 0; }
.order-row { display: flex; justify-content: space-between; padding: 4px 0; }
.order-total { margin-top: 8px; font-weight: 700; text-align: right; }

.btn-primary { padding: 10px 18px; background: #4f8ef7; color: white; border: none; border-radius: 8px; cursor: pointer; font-size: 14px; }
.btn-primary:hover { background: #3a7de0; }
.btn-secondary { padding: 10px 18px; background: #f0f2f5; color: #333; border: none; border-radius: 8px; cursor: pointer; font-size: 14px; }
.btn-danger { padding: 10px 18px; background: #ff5252; color: white; border: none; border-radius: 8px; cursor: pointer; font-size: 14px; }
.btn-danger:hover { background: #e53935; }
.btn-remove { background: none; border: none; cursor: pointer; color: #ff5252; font-size: 16px; }
.btn-logout { padding: 10px 18px; background: #eee; color: #333; border: none; border-radius: 8px; cursor: pointer; }
.loading { text-align: center; padding: 40px; color: #888; }
.error { background: #fff0f0; color: #e53935; padding: 10px; border-radius: 8px; margin-bottom: 12px; }
</style>