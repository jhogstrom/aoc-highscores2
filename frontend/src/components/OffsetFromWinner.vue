<template>
<div>
    <info-block :infotext="infotext" :title="infoTitle"></info-block>
    <div class="scrollable">
    <v-data-table
        :headers="allheaders"
        :items="playerList"
        :items-per-page="-1"
        fixed-header
        height="700"
        class="elevation-1"
        dense
        mobile-breakpoint="1"
        hide-default-footer
        single-select>
        <template v-for="h in allheaders" v-slot:[`header.${h.value}`]="{ header }">
            <span  v-on="on" v-html="header.text" :key="h.value"></span>
        </template>
        <template v-slot:item.identity="{item}">
            <position-col :item="item"/>
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
import { fixedColumns, fixedData, dayColumns, secondsToString } from './tablehelpers'

export default {
    components: { InfoBlock, PositionCol, ValueCol },
    data() { return {
        infoTitle: "Time offset from winner",
        infotext: "This board shows the time offset to the winner for each star."
    }},
    methods: {
        getValue(item, key) {
            const offset = secondsToString(item[key])
            if (offset === "00:00:00") {
                return "Winner"
            }
            return offset
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
                    const dataValue = p.OffsetFromWinner[day-1]
                    const starPositions = p.PositionForStar[day-1]
                    player[`d_${day}_0`] = dataValue[0] !== -1 ? dataValue[0] : Number.MAX_SAFE_INTEGER
                    player[`d_${day}_1`] = dataValue[1] !== -1 ? dataValue[1] : Number.MAX_SAFE_INTEGER
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

</style>