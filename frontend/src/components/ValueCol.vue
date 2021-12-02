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
            return getMedalColor(item, this.header)
        },
        solveTime(item, star) {
            return getSolveTime(item, this.header, star)
        },
    }
}
</script>

<style>
</style>
