<template>
<div>
    <info-block :infotext="infotext" :title="infoTitle"></info-block>
    <div class="scrollable">
    <v-data-table
        :headers="allheaders"
        :items="playerList"
        :items-per-page="50"
        fixed-header
        height="700"
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
        <template v-slot:header.goldMedals="{ header }">
            <tooltip-header :header="header"/>
        </template>
        <template v-slot:header.silverMedals="{ header }">
            <tooltip-header :header="header"/>
        </template>
        <template v-slot:header.bronzeMedals="{ header }">
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
import { fixedColumns, fixedData, secondsToString, decoratePlayerWithDayFields } from './tablehelpers'
import TooltipHeader from './TooltipHeader.vue'
import NameCol from './NameCol.vue'
import StarCol from './StarCol.vue'

export default {
    components: { InfoBlock, PositionCol, ValueCol, TooltipHeader, NameCol, StarCol },
    data() { return {
        infoTitle: "Completion Time Star 2",
        infotext: "This board shows the time to complete second star after completing the first. The medal color is calculated by comparing the time from *1 to *2."
    }},
    methods: {
        getValue(item, key) {
            return secondsToString(item[key])
        },
    },
    computed: {
        dayheaders() {
            let res = []
            for (let day = 1; day < this.$store.getters.data.HighestDay + 1; day++) {
                res.push({ text: `D${day}<br>*->**`, value: `d_${day}_0`, align: "end", width: 15 })
            }
            if (!this.$store.getters.firstDayFirst) {
                return res.reverse()
            }
            return res
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
                    const dataValue = p.TimeToCompleteStar2[day-1]
                    const starPositions = p.PositionStar2[day-1]
                    decoratePlayerWithDayFields(player, p, day);
                    player[`d_${day}_0`] = dataValue !== -1 ? dataValue : Number.MAX_SAFE_INTEGER
                    player[`s_${day}_0`] = starPositions
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