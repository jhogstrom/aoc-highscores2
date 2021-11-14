<template>
<div>
  <v-card>
    <v-toolbar
      color="cyan"
      dark
      flat
    >
      <v-toolbar-title>AOC Highscores</v-toolbar-title>
      <template v-slot:extension>
          <v-tabs
            v-model="tab"
            align-with-title
          >
            <v-tabs-slider color="yellow"></v-tabs-slider>

            <v-tab
              v-for="item in items"
              :key="item.title"
            >
              {{ item.title }}
            </v-tab>
          </v-tabs>
        </template>
      </v-toolbar>
      <v-tabs-items v-model="tab">
        <v-tab-item
          v-for="item in items"
          :key="item.title"
        >
          <v-card flat>
            <component v-bind:is="item.component" v-bind="{...item.props}"></component>
            <!-- <v-card-text v-text="text"></v-card-text> -->
          </v-card>
        </v-tab-item>
      </v-tabs-items>
    </v-card>

  <plain-highscore-list :players="plainplayers"/>
  <h1>AOC Highscores</h1>
  <!-- {{ name }}
  {{ year }}
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

  </p> -->
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
        tab: null,
        name: "hello",
        data: {},
        loadedOk: false,
        comment: "foobar",
        items: [
          {title: "highscore", component: "PlainHighscoreList", props: {players: this.plainplayers}},
          {title: "highscore2", component: "PlainHighscoreList", props: {players: "plainplayers"}},
        ]
    }},
    created() {
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