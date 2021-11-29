<template>
<div>

  <v-card>
    <v-toolbar
      color="cyan"
      dark
      flat
    >
      <v-app-bar-nav-icon @click.stop="drawer = !drawer"></v-app-bar-nav-icon>
        <v-toolbar-title>{{boardTitle}}
          <p class="fetchtime">Fetched from AoC {{updateTime}}</p>
        </v-toolbar-title>
        <template v-slot:extension>
          <menu-button class="menubutton"
            :caption="'Boards'"
            :menuItems="boardmap"></menu-button>
          <menu-button
            :caption="'Charts'"
            :menuItems="chartmap"></menu-button>
          <menu-button
            :caption="'Other'"
            :menuItems="otherPages"></menu-button>
          <year-menu></year-menu>
          <v-divider vertical></v-divider>
          <div class="right-align">
          <v-icon v-if="reloadingData">mdi-database-refresh</v-icon>
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
        v-model="firstDayFirst"
        :label="'Start columns from first day'"
        ></v-checkbox>
      <v-checkbox
        v-model="autoRefresh"
        :label="'Reload data every 30 seconds (if page is active)'"
        ></v-checkbox>
    </v-navigation-drawer>
    <router-view/>
    <footer-content class="footer"></footer-content>
  </div>
</template>

<script>
import FooterContent from './FooterContent.vue'
import MenuButton from './MenuButton.vue'
import YearMenu from './YearMenu.vue'
import { boards, charts, other } from '../router'
import {fileUrl} from '../http.js'
const SECONDS = 1000
const DELAY = 30

export default {
    components: {
      FooterContent, MenuButton,
        YearMenu },
    props: ["guid", "year"],
    data() { return {
        infoTitle: "AOC FTW",
        drawer: false,
        tab: null,
        loadedOk: false,
        boardmap: boards,
        chartmap: charts,
        otherPages: other,
        reloadingData: false
    }},
    mounted() {
      setTimeout(this.refreshData, DELAY * SECONDS)
    },
    computed: {
        boardTitle() {
          return `AOC -> ${this.$store.getters.boardName} - ${this.$store.getters.boardYear}`
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
      shouldReload() {
        return this.autoRefresh && !document.hidden
      },
      refreshData() {
        if (this.shouldReload())
        {
          this.reloadingData = true
          console.log("reloading data again")
          fetch(fileUrl(this.$store.getters.year, this.$store.getters.guid))
            .then(response => {
              console.log("status:", response.status)
              this.loadedOk = response.status === 200
              if (this.loadedOk) {
                return response.json()
              } else {
                return null
              }
            })
            .then(data => {
              if (this.loadedOk) {
                this.$store.dispatch('setData', data)
              } else {
                this.$store.dispatch('setData', {})
              }
            })
          .then(() => {
            setTimeout(this.refreshData, DELAY * SECONDS)
            this.reloadingData = false
          })
        } else {
          // No fetch, so wait another DELAY seonds and try again
          setTimeout(this.refreshData, DELAY * SECONDS)
        }
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