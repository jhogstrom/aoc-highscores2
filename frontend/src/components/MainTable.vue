<template>
<div>
  <h1>AOC Highscores</h1>
  {{ name }}
  {{ year }}
  <p v-if="loadedOk">
  {{ data }}
  </p>
  </div>
</template>

<script>
const ROOTURL="https://aochsstack-website.s3.us-east-2.amazonaws.com"
// https://aochsstack-website.s3.us-east-2.amazonaws.com/2020/7d3e8718-f15c-41ed-a561-fbba4f3fa37c.json
export default {
    props: ["guid", "year"],
    data() { return {
        name: "hello",
        data: {},
        loadedOk: false
    }},
    mounted() {
        this.name = "foobar"
        // this.fetchData(2020, "7d3e8718-f15c-41ed-a561-fbba4f3fa37c")
        this.fetchData(this.year, this.guid)

    },
    methods: {
        makeUrl(year, guid) {
          return `${ROOTURL}/${year}/${guid}.json`
        },
        fetchData(year, guid) {
          console.log("loading data for ", guid, year);

          const url = this.makeUrl(year, guid);
          return fetch(url)
            .then(response => {
              this.loadedOk = response.status == 200
              return response.json()
            })
            .then(data => this.data = data)
      }

    }


}
</script>

<style>

</style>