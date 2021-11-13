<template>
  <div>
      <div v-for="player in players" :key="player.id">
          <p>{{player.Name}} - {{player.LocalScore}}</p>
      </div>

  </div>
</template>

<script>
export default {
    props: {
        players: []
    }
}
</script>

<style>

</style>