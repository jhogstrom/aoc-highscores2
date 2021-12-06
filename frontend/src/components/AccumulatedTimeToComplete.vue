<template>
<div>
    <info-block :infotext="infotext" :title="infoTitle"></info-block>
    <div class="scrollable">
    <v-data-table
        :headers="allheaders"
        :items="playerList"
        :items-per-page="50"
        class="elevation-1"
        dense
        mobile-breakpoint="1"
        single-select>
        <template v-slot:header.score="{ header }">
            <tooltip-header :header="header"/>
        </template>
        <template v-slot:header.globalScore="{ header }">
            <tooltip-header :header="header"/>
        </template>
        <template v-slot:header.stars="{ header }">
            <tooltip-header :header="header"/>
        </template>
        <template v-slot:header.tobiiScore="{ header }">
            <tooltip-header :header="header"/>
        </template>
        <template v-slot:header.raffleTickets="{ header }">
            <tooltip-header :header="header"/>
        </template>
        <template v-for="h in dayheaders" v-slot:[`header.${h.value}`]="{ header }">
            <span v-html="header.text" :key="h.value"></span>
        </template>

        <template v-slot:item.identity="{item}">
            <position-col :item="item"/>
        </template>
        <template v-slot:item.name="{item}">
            <name-col :item="item"/>
        </template>
        <template v-slot:item.stars="{item}">
            <star-col :item="item"/>
        </template>
        <template v-for="h in dayheaders" v-slot:[`item.${h.value}`]="{ item }">
            <value-col
                :item="item"
                :header="h.value"
                :getValue="getValue"
                v-bind:key="h.value"></value-col>
        </template>

        <template v-slot:no-data>
            No data available
        </template>
    </v-data-table>
    </div>
</div>
</template>

<script>
import InfoBlock from './InfoBlock.vue'
import PositionCol from './PositionCol.vue'
import ValueCol from './ValueCol.vue'
import { fixedColumns, fixedData, dayColumns, secondsToString2, decoratePlayerWithDayFields } from './tablehelpers'
import TooltipHeader from './TooltipHeader.vue'
import NameCol from './NameCol.vue'
import StarCol from './StarCol.vue'
var _ = require('lodash')

export default {
    components: { InfoBlock, PositionCol, ValueCol, TooltipHeader, NameCol, StarCol },
    data() { return {
        infoTitle: "Total time used to solve the challenges",
        infotext: "This board shows the total time used to complete the task - counted from task release to completion!"
    }},
    methods: {
        getValue(item, key) {
            const value = item[key]
            return secondsToString2(value)
        },
    },
    computed: {
        dayheaders() {
            return dayColumns()
        },
        allheaders() {
            return [...fixedColumns(), ...this.dayheaders]
        },
        players() {
            return this.$store.getters.filteredPlayers
        },
        data() {
            return this.$store.getters.data
        },
        playerList() {
            let res = []
            // TODO: Sort original list by p.LocalScoreAll.Position
            for (const p of this.players) {
                let player = fixedData(p)
                for (let day = 1; day < this.data.HighestDay + 1; day++) {
                    const dataValue = p.AccumulatedTimeToComplete[day-1]
                    decoratePlayerWithDayFields(player, p, day);
                    player[`d_${day}_0`] = dataValue[0] ? dataValue[0] : ""
                    player[`d_${day}_1`] = dataValue[1] ? dataValue[1] : ""
                }
                res.push(player)
            }
            return _.orderBy(res, ["stars", "accumulatedTime"], ["desc", "desc"])
        }
    },
}
</script>

<style>

</style>