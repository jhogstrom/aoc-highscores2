import Vue from 'vue'
import App from './App.vue'
import vuetify from '@/plugins/vuetify' // path to vuetify export
import store from './store/index'
import router from './router'
import VueGtag from "vue-gtag"

Vue.config.productionTip = false

Vue.use(VueGtag, {
  config: { id: "G-ZWGPG7GN8G" }
})

new Vue({
  vuetify,
  render: h => h(App),
  store,
  router,
  beforeCreate() { this.$store.commit('initialiseStore');}
}).$mount('#app')
