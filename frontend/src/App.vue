<template>
  <div id="app">
    <img alt="Vue logo" src="./assets/logo.png">
    <main-table :guid="guid" :year="year"/>
  </div>
</template>

<script>
import MainTable from './components/MainTable.vue'

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
      this.guid = params.get("uuid") || "7d3e8718-f15c-41ed-a561-fbba4f3fa37c";
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
</style>
