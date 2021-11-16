<template>
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
</template>

<script>
export default {

}
</script>

<style scoped>
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