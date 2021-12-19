<template>
<div>

  <v-card>
    <v-toolbar
      color="cyan"
      flat
    >
      <v-app-bar-nav-icon @click.stop="drawer = !drawer"></v-app-bar-nav-icon>
        <v-toolbar-title>
          <v-tooltip top>
              <template v-slot:activator="{ on }">
                <v-icon v-on="on" @click="refreshData" color="white">mdi-refresh-circle</v-icon>
              </template>
              <span>Request refresh of data</span>
          </v-tooltip>
          {{boardTitle}}
          <v-tooltip top>
              <template v-slot:activator="{ on }">
                  <v-icon v-on="on" @click="copylink">mdi-content-copy</v-icon>
              </template>
              <span>Copy link</span>
          </v-tooltip>
          <p class="fetchtime">Fetched from AoC {{updateTime}}</p>
        </v-toolbar-title>
        <template v-slot:extension>
          <menu-button class="menubutton"
            :caption="'Tables'"
            :menuItems="boardmap"></menu-button>
          <menu-button
            :caption="'Charts'"
            :menuItems="chartmap"></menu-button>
          <menu-button
            :caption="'Other'"
            :menuItems="otherPages"></menu-button>
          <year-menu></year-menu>
          <v-divider vertical></v-divider>
          <about-button></about-button>
          <div class="right-align">

          <v-btn
            class="menubutton"
            color="cyan darken-2"
            dark
            @click="$router.push('/help')">
            <v-icon>mdi-help-circle</v-icon>
            Help
          </v-btn>
          <v-icon v-if="loadingInProgress">mdi-database-refresh</v-icon>
          </div>
        </template>
      </v-toolbar>
    </v-card>
    <v-navigation-drawer
        v-model="drawer"
        absolute
        temporary
      >
      <v-checkbox
        v-model="includeZeroes"
        :label="'Include null achievers'"
        ></v-checkbox>
      <v-checkbox
        v-model="includeMedals"
        :label="'Include Columns for medals'"
        ></v-checkbox>
      <v-checkbox
        v-model="firstDayFirst"
        :label="'Start columns from first day'"
        ></v-checkbox>
      <!-- <v-checkbox
        v-model="autoRefresh"
        :label="'Reload data every 30 seconds (if page is active)'"
        ></v-checkbox> -->
    </v-navigation-drawer>
    <router-view/>
  </div>
</template>

<script>
import MenuButton from './MenuButton.vue'
import YearMenu from './YearMenu.vue'
import { boards, charts, other } from '../router'
import AboutButton from './AboutButton.vue'

export default {
    components: {
       MenuButton,
        YearMenu,
        AboutButton },
    props: ["guid", "year"],
    data() { return {
        infoTitle: "AOC FTW",
        drawer: false,
        tab: null,
        loadedOk: false,
        boardmap: boards,
        chartmap: charts,
        otherPages: other,
    }},
    computed: {
        boardTitle() {
          return `AOC -> ${this.$store.getters.boardName} - ${this.$store.getters.boardYear}`
        },
        loadingInProgress() {
          return this.$store.getters.loadingInProgress
        },
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
        },
        includeMedals: {
          get: function() {
            return this.$store.getters.includeMedals
          },
          set: function() {
            this.$store.dispatch('setIncludeMedals', !this.includeMedals)
          }
        },
        firstDayFirst: {
          get: function() {
            return this.$store.getters.firstDayFirst
          },
          set: function() {
            this.$store.dispatch('setFirstDayFirst', !this.firstDayFirst)
          }
        },
        autoRefresh: {
          get: function() {
            return this.$store.getters.autoRefresh
          },
          set: function() {
            this.$store.dispatch('setAutoRefresh', !this.autoRefresh)
          }
        }
    },
    methods: {
      copylink() {
        var url = `${window.location.origin}?guid=${this.$store.getters.guid}`
        navigator.clipboard.writeText(url)
      },


      shouldReload() {
        return this.autoRefresh && !document.hidden
      },
      refreshData() {
        this.$store.dispatch("requestRefresh")
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

.right-align {
  align-content: right;
  text-align: right;
}
</style>