<template>
<div>
    <info-block :infotext="infotext" :title="infoTitle"></info-block>
    <v-data-table
        :headers="allheaders"
        :items="playerList"
        fixed-header
        height="700"
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
</template>

<script>
import InfoBlock from './InfoBlock.vue'
import PositionCol from './PositionCol.vue'
import ValueCol from './ValueCol.vue'
import { fixedColumns, fixedData, dayColumns } from './tablehelpers'
import TooltipHeader from './TooltipHeader.vue'
import NameCol from './NameCol.vue'

export default {
    components: { InfoBlock, PositionCol, ValueCol, TooltipHeader, NameCol },
    data() { return {
        infoTitle: "Leaderboard",
        infotext: "This board shows the accummulated position of each player. <br>" +
            "The ordering is based on the official AoC 'local score' where the winner each day receives the same number of points as players on the list, but only results up to that day/star is considered.<br>"+
            "The 'medal color' shows the top three players for each day/star"
    }},
    methods: {
        getValue(item, key) {
            return item[key]
        },
    },
    computed: {
        dayheaders() {
            return dayColumns()
        },
        fixedColumns2() {
            return fixedColumns()
        },
        allheaders() {
            return [...this.fixedColumns2, ...this.dayheaders]
        },
        players() {
            return this.$store.getters.filteredPlayers
        },
        data() {
            return this.$store.getters.data
        },
        playerList() {
            let res = []
            for (const p of this.players) {
                let player = fixedData(p)
                for (let day = 1; day < this.data.HighestDay + 1; day++) {
                    let dataValue = p.LocalScoreActive.AccumulatedPosition[day-1]
                    if (this.$store.getters.includeZeroes) {
                        dataValue = p.LocalScoreAll.AccumulatedPosition[day-1]
                    }
                    const starPositions = p.PositionForStar[day-1]
                    player[`d_${day}_0`] = dataValue[0] > -1 ? dataValue[0] : ""
                    player[`d_${day}_1`] = dataValue[1] > -1 ? dataValue[1] : ""
                    player[`s_${day}_0`] = starPositions[0]
                    player[`s_${day}_1`] = starPositions[1]
                }
                res.push(player)
            }
            return res
        }
    },
}
</script>

<style>
.excludedday {
    color: red;
}
</style>