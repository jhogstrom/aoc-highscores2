<template>
<div>
    <span v-if="allStarsDone">
        <b>
        {{item.stars}}
        </b>
    </span>
    <span v-else>
        {{item.stars}}
    </span>
</div>
</template>

<script>
export default {
    props: ["item"],
    computed: {
        allStarsDone() {
            return this.item.stars == this.$store.getters.maxStars
        }
    }
}
</script>

<style>

</style>