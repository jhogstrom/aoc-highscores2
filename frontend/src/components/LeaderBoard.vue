<template>
<div>
    <info-block :infotext="infotext" :title="infoTitle"></info-block>
    <v-data-table
        :headers="allheaders"
        :items="playerList"
        fixed-header
        height="700"
        :items-per-page="-1"
        class="elevation-1"
        dense
        hide-default-footer
        single-select>
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
</template>

<script>
import InfoBlock from './InfoBlock.vue'
import { fixedColumns, fixedData, dayColumns, getMedalColor } from './tablehelpers'
export default {
    components: { InfoBlock },
    data() { return {
        infoTitle: "Leaderboard",
        infotext: "This board shows the <h1>leaders</h1> and points per day."
    }},
    methods: {
        getValue(item, key) {
            return item[key]
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
            for (const p of this.players) {
                let player = fixedData(p)
                for (let day = 1; day < this.data.HighestDay + 1; day++) {
                    let dataValue = p.LocalScoreActive.AccumulatedScore[day-1]
                    if (this.$store.getters.includeZeroes) {
                        dataValue = p.LocalScoreAll.AccumulatedScore[day-1]
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

</style>