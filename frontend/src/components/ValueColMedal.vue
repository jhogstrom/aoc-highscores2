<template>
        <v-tooltip bottom>
            <template v-slot:activator="{ on, attrs }">
                <v-icon v-if="isMedal(item)" v-bind="attrs" v-on="on" :color="iconColor(item)">{{getIcon(item)}}</v-icon>
                <span v-else v-bind="attrs" v-on="on" v-bind:class="medalColor(item)">{{ getValue(item, header) }}</span>
            </template>
            <span>{{item.name}}<br>*1: {{ solveTime(item, 0) }}<br>*2: {{ solveTime(item, 1) }}</span>
        </v-tooltip>
</template>

<script>
import { getMedalColor, getSolveTime } from './tablehelpers'

const icons = {
    0: {value: "gold", color: "#ffd700"},
    1: {value: "silver", color: "#C0C0C0"},
    2: {value: "bronze", color: "#DAA520"}
}
export default {
    props: ["item", "header", "getValue"],
    methods: {
        medalColor(item) {
            return getMedalColor(item, this.header)
        },
        getPosition(item) {
            const posKey = "s" + this.header.slice(1)
            return item[posKey]
        },
        iconColor(item) {
            return icons[this.getPosition(item)].color

        },
        isMedal(item) {
            const position = this.getPosition(item)
            return (position >=0 && position < 3)
        },
        getIcon(item) {
            const icon = icons[this.getPosition(item)].value
            return `mdi-podium-${icon}`
        },
        solveTime(item, star) {
            return getSolveTime(item, this.header, star)
        },
    }
}
</script>

<style>
</style>