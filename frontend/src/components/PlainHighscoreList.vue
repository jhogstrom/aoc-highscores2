<template>
  <div>
      <div v-for="player in players" :key="player.id">
          <p>{{player.Name}} - {{player.LocalScore}}</p>
      </div>

  </div>
</template>

<script>
var _ = require('lodash');
export default {
    computed: {
        data() {
            return this.$store.getters.data
        },
        players() {
            return _.sortBy(this.data.Players, ["LocalScore"]).slice().reverse()
        }
      }
}
</script>

<style>

</style>