<template>
<v-app>
  <div id="app">
    <main-table :guid="guid" :year="year"/>
  </div>
</v-app>
</template>

<script>
import MainTable from './components/MainTable.vue'

const tobiilist = "fbc7a3d8-c6f4-410f-9fff-a1b42993c1c1"
// const smalllist = "7d3e8718-f15c-41ed-a561-fbba4f3fa37c"

export default {
  name: 'App',
  components: {
    MainTable
  },
  data() { return {
    guid: "",
    year: 0
  }},
  created() {
    this.handleParams()
  },
  methods: {
    handleParams() {
      const params = new URLSearchParams(window.location.search)
      const now = new Date();
      let defaultYear = now.getFullYear().toString();
      if (now.getMonth() != 11) {
        defaultYear = (now.getFullYear() - 1).toString();
      }

      this.year = params.get("year") || defaultYear
      this.guid = params.get("guid") || params.get("uuid") || tobiilist;
      console.log("params:", this.year, this.guid);
    }

  }
}
</script>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 60px;
}

body {
  width: 98%;
  padding-left: 1%;
}
</style>
