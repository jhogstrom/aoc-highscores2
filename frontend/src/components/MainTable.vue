<template>
<div>


  <h1>AOC Highscores</h1>
  {{ name }}
  {{ year }}
  <plain-highscore-list :players="plainplayers"/>
  <p v-if="loadedOk">
    <v-expansion-panels>
      <v-expansion-panel>
        <v-expansion-panel-header>
          raw data
        </v-expansion-panel-header>
        <v-expansion-panel-content>
        {{ data }}
        </v-expansion-panel-content>
      </v-expansion-panel>
    </v-expansion-panels>

  </p>
  </div>
</template>

<script>
import PlainHighscoreList from './PlainHighscoreList.vue'
var _ = require('lodash');

const ROOTURL="https://aochsstack-website.s3.us-east-2.amazonaws.com"
// https://aochsstack-website.s3.us-east-2.amazonaws.com/2020/7d3e8718-f15c-41ed-a561-fbba4f3fa37c.json
export default {
    components: { PlainHighscoreList },
    props: ["guid", "year"],
    data() { return {
        name: "hello",
        data: {},
        loadedOk: false,
        comment: "foobar"
    }},
    mounted() {
        this.name = "foobar"
        // this.fetchData(2020, "7d3e8718-f15c-41ed-a561-fbba4f3fa37c")
        this.fetchData(this.year, this.guid)

    },
    computed: {
      plainplayers() {
        console.log(this.data.Players)
        return _.sortBy(this.data.Players, ["LocalScore"]).slice().reverse();

      }
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