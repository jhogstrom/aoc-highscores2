<template>
    <span v-bind:class="completionLevel()"><a id="aocday" :href="urlForDay()">*</a></span>
</template>

<script>
export default {
    props: ['times', 'day'],
    methods: {
        completionLevel() {
            if (this.times[0] == -1 && this.times[1] == -1) { return "notcompleted" }
            if (this.times[0] > -1 && this.times[1] == -1) { return "silver" }
            return "gold"
        },
        urlForDay() {
            return `https://adventofcode.com/${this.$store.getters.data.Year}/day/${this.day}`
        }
    }
}
</script>

<style>
.gold {
    color: #ffff66
}
.silver {
    color: #9999CC;
}
.notcompleted {
    color: #333333;
}
a#aocday {
  color: inherit; /* blue colors for links too */
  text-decoration: inherit; /* no underline */
}
</style>