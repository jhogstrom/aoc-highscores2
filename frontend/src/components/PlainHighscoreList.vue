<template>
  <div>
      <div v-for="player in players" :key="player.id">
          <p>{{player.Name}} - {{player.LocalScore}}</p>
          XXX
      </div>

  </div>
</template>

<script>
export default {
    props: {
        players: []
    },
    mounted() {
        console.log("XX", this.players)
    }
}
</script>

<style>

</style>