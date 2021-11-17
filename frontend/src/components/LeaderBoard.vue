<template>
<div>
    <info-block :infotext="infotext"></info-block>
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
</template>

<script>
import InfoBlock from './InfoBlock.vue'
export default {
    components: { InfoBlock },
    data() { return {
        infotext: "This board shows the <h1>leaders</h1> and points per day."
    }},
    methods: {
        getValue(item, key) {
            return item[key]
        },
        medalColor(item, key) {
            const posKey = "s" + key.slice(1)
            if (item[posKey] === 0) { return "goldMedal"}
            if (item[posKey] === 1) { return "silverMedal"}
            if (item[posKey] === 2) { return "bronzeMedal"}
            return undefined
        }
    },
    computed: {
        dayheaders() {
            let res = []
            for (let day = 1; day < this.data.HighestDay + 1; day++) {
                if (this.data.ExcludedDays.includes(day-1)) {
                    console.log("Skipped day", day)
                    continue
                }
                res.push({ text: `day ${day} *`, value: `d_${day}_0`, align: "end", width: 15 })
                res.push({ text: `day ${day} **`, value: `d_${day}_1`, align: "end", width: 15 })
            }
            return res

        },
        headers() {
            let res = [
                { text: 'Pos.', value: 'identity', width: 23 },
                { text: 'Name', value: 'name', width: 230 },
                { text: 'L', value: 'score', align: "end", width: 15  },
                { text: 'G', value: 'globalScore', align: "end", width: 15 },
                { text: 'S', value: 'stars', align: "end", width: 15 },
                { text: 'T', value: 'tobiiScore', align: "end", width: 15 },
                { text: 'R', value: 'raffleTickets', align: "end", width: 15 },
            ]
            return res
        },
        allheaders() {
            return [...this.headers, ...this.dayheaders]
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
                let player = {
                    position: p.LocalScoreAll.Position,
                    score: p.LocalScoreAll.Score,
                    globalScore: p.GlobalScore,
                    stars: p.Stars,
                    tobiiScore: p.TobiiScore.Score,
                    raffleTickets: p.RaffleTickets,
                    id: p.Id
                }
                for (let day = 1; day < this.data.HighestDay + 1; day++) {
                    if (this.data.ExcludedDays.includes(day-1)) {
                        console.log("Skipped day", day)
                        continue
                    }
                    const accumulatedScores = p.LocalScoreAll.AccumulatedScore[day-1]
                    const starPositions = p.PositionForStar[day-1]
                    player[`d_${day}_0`] = accumulatedScores[0] > -1 ? accumulatedScores[0] : ""
                    player[`d_${day}_1`] = accumulatedScores[1] > -1 ? accumulatedScores[1] : ""
                    player[`s_${day}_0`] = starPositions[0]
                    player[`s_${day}_1`] = starPositions[1]
                }
                player["name"] = `${p.Name} (${player.id})`
                player["identity"] = `${player.position}`
                res.push(player)
            }
            return res
        }
    },
}
</script>

<style>
.goldMedal {
    background: linear-gradient(to bottom right, #ff9988 5%, #ffd700 55%, #ffffff 100%);
    padding: 5px;
}
.silverMedal {
    background: linear-gradient(to bottom right, gray 5%, silver 55%, #ffffff 100%);
    padding: 5px;
}
.bronzeMedal {
    background: linear-gradient(to bottom right, GoldenRod 5%, DarkGoldenRod 55%, #ffffff 100%);
    padding: 5px;
}
tbody tr:nth-of-type(odd) {
   background-color: rgba(0, 0, 0, .05);
 }
</style>