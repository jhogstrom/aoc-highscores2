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
            <v-tab>
              <menu-button
                :caption="'Boards'"
                :menuItems="boards"
                @menuSelected="menuSelected"></menu-button>
            </v-tab>
            <v-tab>
              <menu-button
                :caption="'Charts'"
                :menuItems="charts"
                @menuSelected="menuSelected"></menu-button>
            </v-tab>
            <v-tab>
              <menu-button
                :caption="'Other'"
                :menuItems="otherPages"
                @menuSelected="menuSelected"></menu-button>
            </v-tab>
          </v-tabs>
        </template>
      </v-toolbar>
    </v-card>
    <component v-bind:is="displayComponent" :infoTitle="infoTitle"></component>
    <v-navigation-drawer
        v-model="drawer"
        absolute
        temporary
      >
      <v-checkbox
        v-model="includeZeroes"
        :label="'Include null achievers'"
        ></v-checkbox>
    </v-navigation-drawer>
    <footer-content class="footer"></footer-content>
  </div>
</template>

<script>
import PlainHighscoreList from './PlainHighscoreList.vue'
import AocHighscore from './AocHighscore.vue'
import LeaderBoard from './LeaderBoard.vue'
import CompletionTime from './CompletionTime.vue'
import OffsetFromWinner from './OffsetFromWinner.vue'
import AccumulatedTimeToComplete from './AccumulatedTimeToComplete.vue'
import RawData from './RawData.vue'
import FooterContent from './FooterContent.vue'
import CompletionTimeStarTwo from './CompletionTimeStarTwo.vue'
import GlobalScoreForDay from './GlobalScoreForDay.vue'
import PositionChart from './PositionChart.vue'
import TobiiScore from './TobiiScore.vue'
import MenuButton from './MenuButton.vue'

export default {
    components: {
      PlainHighscoreList, AocHighscore,
      LeaderBoard, CompletionTime, OffsetFromWinner,
      AccumulatedTimeToComplete, CompletionTimeStarTwo,
      GlobalScoreForDay, TobiiScore, PositionChart,
      RawData, FooterContent, MenuButton },
    props: ["guid", "year"],
    data() { return {
        infoTitle: "AOC FTW",
        displayComponent: null,
        drawer: false,
        tab: null,
        loadedOk: false,
        boards: [
          {title: "Leaderboard", component: "LeaderBoard"},
          {title: "CompletionTime", component: "CompletionTime"},
          {title: "Time offset", component: "OffsetFromWinner"},
          {title: "Accumulated Time", component: "AccumulatedTimeToComplete"},
          {title: "Time*2", component: "CompletionTimeStarTwo"},
          {title: "Global", component: "GlobalScoreForDay"},
          {title: "TobiiScore", component: "TobiiScore"},

        ],
        charts: [
          {title: "PositionChart", component: "PositionChart"},

        ],
        otherPages: [
          {title: "AoC style", component: "AocHighscore"},
          {title: "Raw", component: "RawData"},
        ],
    }},
    computed: {
        updateTime() {
            return this.$store.getters.updateTime
        },
        includeZeroes: {
          get: function() {
            return this.$store.getters.includeZeroes
          },
          set: function() {
            this.$store.dispatch('setIncludeZeroes', !this.includeZeroes)
          }
        }
    },
    methods: {
      menuSelected(item) {
        this.infoTitle = item.title
        this.displayComponent = item.component
      }
    },
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