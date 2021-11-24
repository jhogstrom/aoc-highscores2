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
import { fixedColumns, fixedData, getMedalColor, secondsToString } from './tablehelpers'
export default {
    components: { InfoBlock },
    data() { return {
        infoTitle: "Completion Time Star 2",
        infotext: "This board shows the time to complete second star after completing the first."
    }},
    methods: {
        getValue(item, key) {
            return secondsToString(item[key])
        },
        medalColor(item, key) {
            return getMedalColor(item, key)
        }
    },
    computed: {
        dayheaders() {
            let res = []
            for (let day = 1; day < this.$store.getters.data.HighestDay + 1; day++) {
                res.push({ text: `day ${day} *->**`, value: `d_${day}_0`, align: "end", width: 15 })
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