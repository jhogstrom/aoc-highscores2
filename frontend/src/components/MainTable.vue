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
            :menuItems="boardmap"
            @menuSelected="menuSelected"></menu-button>
          <menu-button
            :caption="'Charts'"
            :menuItems="chartmap"
            @menuSelected="menuSelected"></menu-button>
          <menu-button
            :caption="'Other'"
            :menuItems="otherPages"
            @menuSelected="menuSelected"></menu-button>
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
      <v-checkbox
        v-model="firstDayFirst"
        :label="'Start columns from first day'"
        ></v-checkbox>
      <v-checkbox
        v-model="autoRefresh"
        :label="'Reload data every 20 seconds (if page is active)'"
        ></v-checkbox>
    </v-navigation-drawer>
    <router-view/>
    <footer-content class="footer"></footer-content>
  </div>
</template>

<script>
import FooterContent from './FooterContent.vue'
import MenuButton from './MenuButton.vue'
import { boards, charts, other } from '../router'

export default {
    components: {
      FooterContent, MenuButton },
    props: ["guid", "year"],
    data() { return {
        infoTitle: "AOC FTW",
        displayComponent: null,
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