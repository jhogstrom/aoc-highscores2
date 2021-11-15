<template>
    <div>
        <div v-for="player in players" :key="player.id">
            <p>{{ player.Name }} - {{ player.LocalScoreAll.Score }}</p>
        </div>
    </div>
</template>

<script>
export default {
    computed: {
        players() {
            return this.$store.getters.filteredPlayers;
        },
    },
};
</script>

<style>
</style>