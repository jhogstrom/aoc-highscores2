<template>
<span v-bind:class="completionLevel()">*</span>
</template>

<script>
export default {
    props: ['times'],
    methods: {
        completionLevel() {
            if (this.times[0] == -1 && this.times[1] == -1) { return "notcompleted" }
            if (this.times[0] > -1 && this.times[1] == -1) { return "silver" }
            return "gold"
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
</style>