<template>
<div>
  <v-card>
    <v-toolbar
      color="cyan"
      dark
      flat
    >
      <v-app-bar-nav-icon @click.stop="drawer = !drawer"></v-app-bar-nav-icon>
      <v-toolbar-title>AOC Highscores
      <p class="fetchtime">Fetched from AoC {{updateTime}}</p>
      </v-toolbar-title>
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
              <component v-bind:is="item.component"></component>
              <!-- <v-card-text v-text="text"></v-card-text> -->
            </v-card>
          </v-tab-item>
        </v-tabs-items>
    </v-card>
    <v-navigation-drawer
        v-model="drawer"
        absolute
        temporary
      >
      <v-checkbox
        v-model="includeZero"
        :label="'Include null achievers'"
        @change="includeZeroChanged"></v-checkbox>
    </v-navigation-drawer>
    <footer-content class="footer"></footer-content>
  </div>
</template>

<script>
import PlainHighscoreList from './PlainHighscoreList.vue'
import AocHighscore from './AocHighscore.vue'
import LeaderBoard from './LeaderBoard.vue'
import RawData from './RawData.vue'
import FooterContent from './FooterContent.vue'

export default {
    components: { PlainHighscoreList, AocHighscore, LeaderBoard, RawData, FooterContent },
    props: ["guid", "year"],
    data() { return {
        drawer: false,
        includeZero: this.includeZeroes,
        tab: null,
        loadedOk: false,
        items: [
          {title: "Leaderboard", component: "LeaderBoard"},
          {title: "highscore", component: "PlainHighscoreList"},
          {title: "AoC style", component: "AocHighscore"},
          {title: "Raw", component: "RawData"},
        ]
    }},
    computed: {
        updateTime() {
            return this.$store.getters.updateTime
        },
        includeZeroes() {
            return this.$store.getters.includeZeroes
        },
    },
    methods: {
      includeZeroChanged() {
        this.$store.dispatch('setIncludeZeroes', this.includeZero)
      }
    }
}
</script>

<style scoped>
.fetchtime {
  text-align: right;
  font-size: 0.6rem !important;
}
.v-toolbar__title {
  font-size: 1.5rem !important;
}

.footer {
  position: absolute;
  bottom: 10px;
  /*width: 50%;
  border: 3px solid #8AC007; */
}
</style>