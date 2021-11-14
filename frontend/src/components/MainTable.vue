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
            <component v-bind:is="item.component"></component>
            <!-- <v-card-text v-text="text"></v-card-text> -->
          </v-card>
        </v-tab-item>
      </v-tabs-items>
    </v-card>

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
  </div>
</template>

<script>
import PlainHighscoreList from './PlainHighscoreList.vue'

export default {
    components: { PlainHighscoreList },
    props: ["guid", "year"],
    data() { return {
        tab: null,
        loadedOk: false,
        items: [
          {title: "highscore", component: "PlainHighscoreList"},
          {title: "highscore2", component: "PlainHighscoreList"},
        ]
    }},
}
</script>

<style>

</style>