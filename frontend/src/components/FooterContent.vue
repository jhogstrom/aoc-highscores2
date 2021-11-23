<template>
    <v-footer
      dark
      padless
    >
      <v-card
        flat
        tile
        class="footer indigo lighten-1 white--text text-center"
      >

        <v-card-text class="white--text">

          <a href="https://github.com/jhogstrom/aoc-highscores2">
            <v-icon class="iconpadding" color="lightgray">mdi-github</v-icon>
          </a>

          <v-dialog
            v-model="dialog"
            width="500">
            <template v-slot:activator="{ on, attrs }">
              <v-icon
                class="iconpadding"
                v-bind="attrs"
                v-on="on">mdi-information</v-icon>
            </template>

            <v-card
              class="mx-auto"
              color="#26c6da">
              <v-card-title class="text-h5 grey lighten-2">
                AoC Highscores
              </v-card-title>

              <v-card-text color="blue">
                This page is created/maintained by Jesper Högström (<a href="http://wwww.aditrologistics.se">AditroLogistics</a>)
                and Jonas Högström (<a href="http://www.tobii.com">Tobii</a>).
                During the month of <a href="http://www.adventofcode.com">AdventofCode</a> we try to keep it as up to date as possible
                without putting too much stress on the AoC servers.<br>
                If you find any bugs, have any questions or if you are interested in having these stats for your own
                private leaderboard, please reach out... jonas.hogstrom at tobii.com.
              </v-card-text>


              <v-card-actions>
                <v-spacer></v-spacer>
                <v-btn
                  color="primary"
                  text
                  @click="dialog = false"
                >
                  ok
                </v-btn>
              </v-card-actions>
            </v-card>
          </v-dialog>

          &copy;{{ copyrightYear }} — <strong>JHSoftService</strong>
        </v-card-text>
      </v-card>
    </v-footer>
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
.footer {
  width: 100%
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