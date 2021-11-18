<template>
<v-app>
  <div id="app">
    <main-table :guid="guid" :year="year"/>
  </div>
</v-app>
</template>

<script>
import MainTable from './components/MainTable.vue'
// import FooterContent from './components/FooterContent.vue'


const tobiilist = "fbc7a3d8-c6f4-410f-9fff-a1b42993c1c1"
// const smalllist = "7d3e8718-f15c-41ed-a561-fbba4f3fa37c"
const ROOTURL="https://aochsstack-website.s3.us-east-2.amazonaws.com"

export default {
  name: 'App',
  components: {
    MainTable,
    // FooterContent
  },
  data() { return {
    guid: "",
    year: 0
  }},
  created() {
    this.handleParams()
    this.fetchData(this.year, this.guid)
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
    },
    makeUrl(year, guid) {
      return `${ROOTURL}/${year}/${guid}.json`
    },
    async fetchData(year, guid) {
      console.log("loading data for ", guid, year);

      const url = this.makeUrl(year, guid);
      return fetch(url)
        .then(response => {
          // this.loadedOk = response.status == 200
          return response.json()
        })
        .then(data => {
          this.data = data
          this.$store.dispatch('setData', data)
        })
    }

  }
}
</script>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  /* text-align: center; */
  color: #2c3e50;
  font: 1.1rem  'Source Code Pro', Courier, monospace !important;
}

body {
  width: 98%;
  padding-left: 1%;
}

.goldMedal {
    background: linear-gradient(to bottom right, #ff9988 5%, #ffd700 55%, #ffffff 100%);
    padding: 5px;
}
.silverMedal {
    background: linear-gradient(to bottom right, gray 5%, silver 55%, #ffffff 100%);
    padding: 5px;
}
.bronzeMedal {
    background: linear-gradient(to bottom right, GoldenRod 5%, DarkGoldenRod 55%, #ffffff 100%);
    padding: 5px;
}
tbody tr:nth-of-type(odd) {
   background-color: rgba(0, 0, 0, .05);
 }
 .scrollable {
    overflow-y: scroll;
    height: 50rem;
 }
</style>
