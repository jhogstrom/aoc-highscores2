<template>
        <v-tooltip bottom>
            <template v-slot:activator="{ on, attrs }">
                <span v-bind="attrs" v-on="on" v-bind:class="medalColor(item)">{{ getValue(item, header) }}</span>
            </template>
            <span>{{item.name}}<br>*1: {{ solveTime(item, 0) }}<br>*2: {{ solveTime(item, 1) }}</span>
        </v-tooltip>
</template>

<script>
import { getMedalColor, getSolveTime } from './tablehelpers'

export default {
    props: ["item", "header", "getValue"],
    methods: {
        medalColor(item) {
            const posKey = "s" + this.header.slice(1)
            if (item[posKey] !== -1)
                return getMedalColor(item, this.header)
            console.log("incomplete")
            return "incompleteStar"
        },
        solveTime(item, star) {
            return getSolveTime(item, this.header, star)
        },
    }
}
</script>

<style>

.incompleteStar {
    color: rgb(204, 208, 212);
    padding-left: 5px;
    padding-right: 5px;
}
</style>
