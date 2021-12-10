<template>
    <v-tooltip bottom>
        <template v-slot:activator="{ on, attrs }">
            <span v-bind="attrs" v-on="on">{{item.position}}</span>
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
            return "incompleteStar"
        },
        solveTime(item, star) {
            return getSolveTime(item, this.header, star)
        },
    }
}
</script>

<style>

</style>