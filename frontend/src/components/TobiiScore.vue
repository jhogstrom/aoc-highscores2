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
import { fixedColumns, fixedData, dayColumns, getMedalColor } from './tablehelpers'

export default {
    components: { InfoBlock },
    props: ["infoTitle"],
    data() { return {
        infotext: `TobiiScore leaderboard is ordered by
        <ul>
            <li>Completed stars
            <li>TobiiScore
            <li>Time for last star
        </ul>
        <p>TobiiScore calculated as the number of players on the list with a better result for each star.
        That means that a gold medal gives 0 points, silver gives 1 point etc.</p>
        <p>The advantage of this is that the score does not change if new players enters the list,
        and the ordering method encourages participants to solve all puzzles.</p>`
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
                    const dataValue = p.TobiiScore.AccumulatedScore[day-1]
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