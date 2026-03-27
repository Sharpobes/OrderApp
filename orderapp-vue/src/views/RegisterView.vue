<template>
  <div class="auth-wrapper">
    <div class="auth-card">
      <h2>Регистрация</h2>

      <div v-if="error" class="error">{{ error }}</div>
      <div v-if="success" class="success">{{ success }}</div>

      <form @submit.prevent="handleRegister">
        <div class="field">
          <label>Логин</label>
          <input v-model="login" type="text" placeholder="Придумайте логин" required />
        </div>

        <div class="field">
          <label>Пароль</label>
          <input v-model="password" type="password" placeholder="Минимум 8 символов" required />
        </div>

        <div class="field">
          <label>Повторите пароль</label>
          <input v-model="confirmPassword" type="password" placeholder="Повторите пароль" required />
        </div>

        <button type="submit" :disabled="loading">
          {{ loading ? 'Регистрируем...' : 'Зарегистрироваться' }}
        </button>
      </form>

      <p class="link">
        Уже есть аккаунт? <RouterLink to="/login">Войти</RouterLink>
      </p>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const auth = useAuthStore()

const login = ref('')
const password = ref('')
const confirmPassword = ref('')
const error = ref('')
const success = ref('')
const loading = ref(false)

async function handleRegister() {
  error.value = ''
  success.value = ''

  if (password.value !== confirmPassword.value) {
    error.value = 'Пароли не совпадают'
    return
  }

  loading.value = true
  try {
    await auth.register(login.value, password.value)
    success.value = 'Регистрация прошла успешно! Перенаправляем...'
    setTimeout(() => router.push('/login'), 1500)
  } catch (e) {
    error.value = e.response?.data?.[0]?.description || 'Ошибка регистрации'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
/* Те же стили что и в LoginView */
.auth-wrapper {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f0f2f5;
}

.auth-card {
  background: white;
  padding: 40px;
  border-radius: 12px;
  box-shadow: 0 2px 16px rgba(0,0,0,0.1);
  width: 100%;
  max-width: 400px;
}

h2 { margin-bottom: 24px; text-align: center; color: #333; }
.field { margin-bottom: 16px; display: flex; flex-direction: column; gap: 6px; }
label { font-size: 14px; color: #555; }

input {
  padding: 10px 14px;
  border: 1px solid #ddd;
  border-radius: 8px;
  font-size: 15px;
  outline: none;
  transition: border 0.2s;
}

input:focus { border-color: #4f8ef7; }

button {
  width: 100%;
  padding: 12px;
  background: #4f8ef7;
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 16px;
  cursor: pointer;
  margin-top: 8px;
  transition: background 0.2s;
}

button:hover:not(:disabled) { background: #3a7de0; }
button:disabled { background: #aaa; cursor: not-allowed; }

.error {
  background: #fff0f0;
  color: #e53935;
  padding: 10px 14px;
  border-radius: 8px;
  margin-bottom: 16px;
  font-size: 14px;
}

.success {
  background: #f0fff4;
  color: #2e7d32;
  padding: 10px 14px;
  border-radius: 8px;
  margin-bottom: 16px;
  font-size: 14px;
}

.link { text-align: center; margin-top: 16px; font-size: 14px; color: #666; }
.link a { color: #4f8ef7; text-decoration: none; }
</style>