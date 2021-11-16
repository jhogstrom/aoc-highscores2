<template>
<div>
  <v-card>
    <v-toolbar
      color="cyan"
      dark
      flat
    >
      <v-app-bar-nav-icon @click.stop="drawer = !drawer"></v-app-bar-nav-icon>
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
    <v-footer
      dark
      padless
    >
      <v-card
        flat
        tile
        class="indigo lighten-1 white--text text-center"
      >

        <v-card-text class="white--text pt-0">
          This page is created/maintaned by Jesper Högström (<a href="http://wwww.aditrologistics.se">AditroLogistics</a>)
          and Jonas Högström (<a href="http://www.tobii.com">Tobii</a>).
          During the month of <a href="http://www.adventofcode.com">AdventofCode</a> we try to keep it as up to date as possible
          without putting too much stress on the AoC servers.<br>
          If you find any bugs, have any questions or if you are interested in having these stats for your own
          private leaderboard, please reach out... jonas.hogstrom at tobii.com.
        </v-card-text>

        <v-divider></v-divider>

        <v-card-text class="white--text">
          {{ new Date().getFullYear() }} — <strong>JHSoftService</strong>
        </v-card-text>
      </v-card>
    </v-footer>

  <!-- <plain-highscore-list :players="plainplayers"/> -->
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
    <footer></footer>
  </div>
</template>

<script>
import PlainHighscoreList from './PlainHighscoreList.vue'
import AocHighscore from './AocHighscore.vue'

export default {
    components: { PlainHighscoreList, AocHighscore },
    props: ["guid", "year"],
    data() { return {
        drawer: false,
        includeZero: this.includeZeroes,
        tab: null,
        loadedOk: false,
        items: [
          {title: "highscore", component: "PlainHighscoreList"},
          {title: "AoC style", component: "AocHighscore"},
        ]
    }},
    computed: {
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
.v-toolbar__title {
  font-size: 1.5rem !important;
}

a:link {
  color: lightblue;

}

/* visited link */
a:visited {
  color: white;
}

/* mouse over link */
a:hover {
  color: hotpink;
}

/* selected link */
a:active {
  color: blue;
}

</style>