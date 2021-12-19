<template>
<v-app>
  <div id="app">
    <main-table :guid="guid" :year="year"/>
  </div>
</v-app>
</template>

<script>
import MainTable from './components/MainTable.vue'
import { sockets } from './store/sockets.js'

// const defaultList = "fbc7a3d8-c6f4-410f-9fff-a1b42993c1c1"
const defaultList = "00000000-0000-0000-0000-000000000000"
// const defaultList = "7d3e8718-f15c-41ed-a561-fbba4f3fa37c"

export default {
  name: 'App',
  components: {
    MainTable,
    // FooterContent
  },
  data() { return {
    guid: "",
    year: 0,
    socket: sockets
  }},
  created() {
    sockets.emitter.on('Payload', this.onPayload)
    this.handleParams()
    this.fetchData()
    this.$store.dispatch("requestRefresh")
  },
  mounted() {
    },
  methods: {
    async onPayload(payload) {
      console.log("Websocket incoming", payload.event || payload)
      if (payload.event == "update") {
        console.log("WS-Request: ", JSON.stringify(payload))
        if (this.$store.getters.year == payload.year &&
          this.$store.getters.guid == payload.boardguid) {
            console.log("Reloading...")
          await this.$store.dispatch('loadData')
        } else {
          console.log("Received unexpected request for", JSON.stringify(payload))
        }
      }
    },
    handleParams() {
      const params = new URLSearchParams(window.location.search)
      const now = new Date();
      let defaultYear = now.getFullYear().toString();
      if (now.getMonth() != 11) {
        defaultYear = (now.getFullYear() - 1).toString();
      }

      this.year = params.get("year") || defaultYear
      this.guid = params.get("guid") || params.get("uuid") || this.$store.getters.guid || defaultList;
      this.$store.dispatch('setParams', {year: this.year, guid: this.guid})
    },
    async fetchData() {
      this.$store.dispatch('loadData')
    }
  }
}
</script>

<style lang="scss">
@import './assets/styles.css';

#app {
font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  /* text-align: center; */
  color: #2c3e50;
  font: 1.1rem  'Source Code Pro', Courier, monospace !important;
}

#nav {
  padding: 30px;

  a {
    font-weight: bold;
    color: #2c3e50;

    &.router-link-exact-active {
      color: #42b983;
    }
  }
body {
  width: 98%;
  padding-left: 1%;
}

.scrollable {
   overflow-y: scroll;
    height: 50rem;
 }
}
</style>
