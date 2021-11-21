import Vue from 'vue'
import App from './App.vue'
import vuetify from '@/plugins/vuetify' // path to vuetify export
import store from './store/index'

Vue.config.productionTip = false

new Vue({
  vuetify,
  render: h => h(App),
  store,
  beforeCreate() { this.$store.commit('initialiseStore');},
}).$mount('#app')
