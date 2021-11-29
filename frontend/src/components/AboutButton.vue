<template>
    <div>
        <v-btn
            class="menubutton"
            color="cyan darken-2"
            dark
            @click="dialog = true"
            >
            <v-icon
                class="iconpadding"
                v-on="on">mdi-information</v-icon>

            About
        </v-btn>
          <v-dialog
            v-model="dialog"
            class="aoc-color"
            width="500">

              <!-- color="#26c6da" -->
            <v-card
              class="mx-auto aoc-color"
              >
              <v-card-title class="aoc-color">
                <p class="aoc-color">AoC Highscores</p>
              </v-card-title>

              <v-card-text class="aoc-color">
                  <p class="aoc-color">
                This page is created/maintained by Jesper Högström (<a href="http://wwww.aditrologistics.se">AditroLogistics</a>)
                and Jonas Högström (<a href="http://www.tobii.com">Tobii</a>).
                During the month of <a href="http://www.adventofcode.com">AdventofCode</a> we try to keep it as up to date as possible
                without putting too much stress on the AoC servers.<br>
                If you find any bugs, have any questions or if you are interested in having these stats for your own
                private leaderboard, please reach out... jonas.hogstrom at tobii.com.

                <p class="aoc-color">
                    &copy;{{ copyrightYear }} — <strong>JHSoftService</strong>
                    <a href="https://github.com/jhogstrom/aoc-highscores2">
                    <v-icon class="iconpadding" color="white">mdi-github</v-icon>
                    </a>
                </p>
                  </p>
              </v-card-text>

              <v-card-actions class="aoc-color">
                <v-spacer></v-spacer>
                <v-btn
                  @click="dialog = false"
                >
                  ok
                </v-btn>
              </v-card-actions>
            </v-card>
          </v-dialog>
    </div>
</template>

<script>
export default {
  data() { return {
    dialog: false
  }},
  computed: {
    copyrightYear() {
      const startYear = 2021
      const thisYear =  new Date().getFullYear()
      if (startYear === thisYear) { return startYear }
      return `${startYear}-${thisYear}`
    }
  }

}
</script>

<style scoped>
.menubutton {
  margin: 5px;
}

.aoc-color {
        font-size: 1.1rem !important;
    background-color: rgb(5, 7, 39);
    color: lightgray;
}

a:link {
  color: lightblue;
  text-decoration: none;

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

.iconpadding {
  padding: 5px;
}


</style>