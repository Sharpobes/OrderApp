<template>
  <div class="page">
    <div class="page-header">
      <h2>Панель управления</h2>
      <div class="header-actions">
        <span class="username">👤 {{ auth.user }}</span>
        <button class="btn-logout" @click="handleLogout">Выйти</button>
      </div>
    </div>

    <nav class="tabs">
      <RouterLink to="/catalog">Каталог</RouterLink>
      <RouterLink to="/orders">Все заказы</RouterLink>
      <RouterLink to="/manager">Управление</RouterLink>
    </nav>

    <div class="sub-tabs">
      <button :class="['sub-tab', { active: activeTab === 'users' }]" @click="activeTab = 'users'">
        Пользователи
      </button>
      <button :class="['sub-tab', { active: activeTab === 'customers' }]" @click="activeTab = 'customers'">
        Заказчики
      </button>
    </div>

    <!-- Пользователи -->
    <div v-if="activeTab === 'users'">
      <div v-if="usersStore.loading" class="loading">Загрузка...</div>
      <table v-else class="table">
        <thead>
        <tr>
          <th>Логин</th>
          <th>Роль</th>
          <th>Действия</th>
        </tr>
        </thead>
        <tbody>
        <tr v-for="user in usersStore.users" :key="user.id">
          <td>{{ user.userName }}</td>
          <td>
            <select :value="user.roles?.[0]" @change="changeRole(user.id, $event.target.value)">
              <option value="Customer">Customer</option>
              <option value="Manager">Manager</option>
            </select>
          </td>
          <td>
            <button class="btn-danger btn-sm" @click="deleteUser(user.id)">Удалить</button>
          </td>
        </tr>
        </tbody>
      </table>
    </div>

    <!-- Заказчики -->
    <div v-if="activeTab === 'customers'">
      <button class="btn-primary" style="margin-bottom: 16px" @click="openCreateCustomer">
        + Добавить заказчика
      </button>

      <div v-if="customersStore.loading" class="loading">Загрузка...</div>
      <table v-else class="table">
        <thead>
        <tr>
          <th>Код</th>
          <th>Наименование</th>
          <th>Адрес</th>
          <th>Скидка %</th>
          <th>Действия</th>
        </tr>
        </thead>
        <tbody>
        <tr v-for="customer in customersStore.customers" :key="customer.id">
          <td>{{ customer.code }}</td>
          <td>{{ customer.name }}</td>
          <td>{{ customer.address }}</td>
          <td>{{ customer.discount ?? '—' }}</td>
          <td>
            <div class="row-actions">
              <button class="btn-secondary btn-sm" @click="openEditCustomer(customer)">
                Редактировать
              </button>
              <button class="btn-danger btn-sm" @click="deleteCustomer(customer.id)">
                Удалить
              </button>
            </div>
          </td>
        </tr>
        </tbody>
      </table>
    </div>

    <div v-if="showCustomerModal" class="modal-overlay" @click.self="showCustomerModal = false">
      <div class="modal">
        <h3>{{ editingCustomer ? 'Редактировать заказчика' : 'Новый заказчик' }}</h3>
        <div v-if="modalError" class="error">{{ modalError }}</div>
        <div class="field">
          <label>Код (XXXX-ГГГГ)</label>
          <input v-model="customerForm.code" placeholder="1234-2024" />
        </div>
        <div class="field">
          <label>Наименование</label>
          <input v-model="customerForm.name" placeholder="ООО Ромашка" />
        </div>
        <div class="field">
          <label>Адрес</label>
          <input v-model="customerForm.address" placeholder="г. Москва, ул. Ленина, 1" />
        </div>
        <div class="field">
          <label>Скидка % (0 или пусто = нет скидки)</label>
          <input v-model.number="customerForm.discount" type="number" placeholder="0" />
        </div>
        <div class="modal-actions">
          <button class="btn-primary" @click="saveCustomer">Сохранить</button>
          <button class="btn-secondary" @click="showCustomerModal = false">Отмена</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useUsersStore } from '@/stores/users'
import { useCustomersStore } from '@/stores/customers'

const router = useRouter()
const auth = useAuthStore()
const usersStore = useUsersStore()
const customersStore = useCustomersStore()

const activeTab = ref('users')
const showCustomerModal = ref(false)
const editingCustomer = ref(null)
const modalError = ref('')
const customerForm = ref({ code: '', name: '', address: '', discount: null })

onMounted(() => {
  usersStore.fetchAll()
  customersStore.fetchAll()
})

function handleLogout() {
  auth.logout()
  router.push('/login')
}

async function changeRole(userId, role) {
  await usersStore.setRole(userId, role)
}

async function deleteUser(id) {
  if (!confirm('Удалить пользователя?')) return
  await usersStore.remove(id)
}

function openCreateCustomer() {
  editingCustomer.value = null
  customerForm.value = { code: '', name: '', address: '', discount: null }
  modalError.value = ''
  showCustomerModal.value = true
}

function openEditCustomer(customer) {
  editingCustomer.value = customer
  customerForm.value = {
    code: customer.code,
    name: customer.name,
    address: customer.address,
    discount: customer.discount
  }
  modalError.value = ''
  showCustomerModal.value = true
}

async function saveCustomer() {
  modalError.value = ''
  try {
    if (editingCustomer.value) {
      await customersStore.update(editingCustomer.value.id, customerForm.value)
    } else {
      await customersStore.create(customerForm.value)
    }
    showCustomerModal.value = false
  } catch (e) {
    modalError.value = 'Ошибка сохранения'
  }
}

async function deleteCustomer(id) {
  if (!confirm('Удалить заказчика?')) return
  await customersStore.remove(id)
}
</script>

<style scoped>
.page { max-width: 1100px; margin: 0 auto; padding: 24px; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
.header-actions { display: flex; gap: 12px; align-items: center; }
.username { display: flex; align-items: center; font-size: 14px; color: #555; background: #f0f2f5; padding: 8px 14px; border-radius: 8px; font-weight: 500; }

.tabs { display: flex; gap: 24px; margin-bottom: 24px; border-bottom: 2px solid #eee; padding-bottom: 8px; }
.tabs a { text-decoration: none; color: #666; font-weight: 500; }
.tabs a.router-link-active { color: #4f8ef7; border-bottom: 2px solid #4f8ef7; padding-bottom: 8px; }

.sub-tabs { display: flex; gap: 8px; margin-bottom: 20px; }
.sub-tab { padding: 10px 20px; border: 1px solid #ddd; border-radius: 8px; background: white; cursor: pointer; font-size: 14px; }
.sub-tab.active { background: #4f8ef7; color: white; border-color: #4f8ef7; }

.table { width: 100%; border-collapse: collapse; background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.08); }
.table th { background: #f8f9ff; padding: 14px 16px; text-align: left; font-size: 14px; color: #555; }
.table td { padding: 12px 16px; border-top: 1px solid #f0f0f0; font-size: 14px; }

select { padding: 6px 10px; border: 1px solid #ddd; border-radius: 6px; font-size: 14px; }
.row-actions { display: flex; gap: 8px; }

.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.4); display: flex; align-items: center; justify-content: center; z-index: 100; }
.modal { background: white; border-radius: 16px; padding: 32px; width: 100%; max-width: 480px; }
.modal h3 { margin-bottom: 20px; }
.modal-actions { display: flex; gap: 12px; margin-top: 20px; }

.field { margin-bottom: 14px; display: flex; flex-direction: column; gap: 6px; }
label { font-size: 14px; color: #555; }
input { padding: 10px 14px; border: 1px solid #ddd; border-radius: 8px; font-size: 15px; outline: none; }
input:focus { border-color: #4f8ef7; }

.btn-primary { padding: 10px 18px; background: #4f8ef7; color: white; border: none; border-radius: 8px; cursor: pointer; font-size: 14px; }
.btn-secondary { padding: 10px 18px; background: #f0f2f5; color: #333; border: none; border-radius: 8px; cursor: pointer; font-size: 14px; }
.btn-danger { padding: 10px 18px; background: #ff5252; color: white; border: none; border-radius: 8px; cursor: pointer; font-size: 14px; }
.btn-logout { padding: 10px 18px; background: #eee; color: #333; border: none; border-radius: 8px; cursor: pointer; }
.btn-sm { padding: 6px 12px; font-size: 13px; }
.loading { text-align: center; padding: 40px; color: #888; }
.error { background: #fff0f0; color: #e53935; padding: 10px; border-radius: 8px; margin-bottom: 12px; }
</style>