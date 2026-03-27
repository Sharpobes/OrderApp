import { defineStore } from 'pinia'
import api from '@/api/axios'

export const useItemsStore = defineStore('items', {
    state: () => ({
        items: [],
        loading: false,
        error: null
    }),

    actions: {
        async fetchAll() {
            this.loading = true
            try {
                const res = await api.get('/items')
                this.items = res.data
            } catch (e) {
                this.error = 'Ошибка загрузки товаров'
            } finally {
                this.loading = false
            }
        },

        async create(item) {
            const res = await api.post('/items', item)
            this.items.push(res.data)
        },

        async update(id, item) {
            await api.put(`/items/${id}`, item)
            const index = this.items.findIndex(i => i.id === id)
            if (index !== -1) this.items[index] = { ...this.items[index], ...item }
        },

        async remove(id) {
            await api.delete(`/items/${id}`)
            this.items = this.items.filter(i => i.id !== id)
        }
    }
})