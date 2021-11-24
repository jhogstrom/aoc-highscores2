<template>
<div>
    <info-block :infotext="infotext" :title="infoTitle"></info-block>
    <div class="scrollable">
    <v-data-table
        :headers="allheaders"
        :items="playerList"
        :items-per-page="-1"
        class="elevation-1"
        dense
        hide-default-footer
        single-select>
        <template v-slot:item.identity="{item}">
            <position-col :item="item"/>
        </template>
        <template v-for="h in dayheaders" v-slot:[`item.${h.value}`]="{ item }">
            <span
                v-bind:class="medalColor(item, h.value)"
                v-bind:key="h.value">
                {{ getValue(item, h.value) }}
            </span>
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
import { fixedColumns, fixedData, dayColumns, getMedalColor, secondsToString2 } from './tablehelpers'

export default {
    components: { InfoBlock, PositionCol },
    data() { return {
        infoTitle: "Total time used to solve the challenges",
        infotext: "This board shows the total time used to complete the task - counted from task release to completion!"
    }},
    methods: {
        getValue(item, key) {
            const value = item[key]
            return secondsToString2(value)
        },
        medalColor(item, key) {
            return getMedalColor(item, key)
        }
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
                    const starPositions = p.PositionForStar[day-1]
                    player[`d_${day}_0`] = dataValue[0] ? dataValue[0] : ""
                    player[`d_${day}_1`] = dataValue[1] ? dataValue[1] : ""
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