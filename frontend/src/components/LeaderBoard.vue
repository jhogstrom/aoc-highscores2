<template>
<div>
    <info-block :infotext="infotext"></info-block>
    <v-data-table
        :headers="headers"
        :items="playerList"
        :items-per-page="-1"
        class="elevation-1"
        dense
        hide-default-footer
        single-select>
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
    computed: {
        headers() {
            let res = [
                { text: 'Pos.', value: 'Identity', width: 230 },
                { text: 'L', value: 'score', align: "end", width: 15  },
                { text: 'G', value: 'globalScore', align: "end", width: 15 },
                { text: 'S', value: 'stars', align: "end", width: 15 },
                { text: 'T', value: 'tobiiScore', align: "end", width: 15 },
                { text: 'R', value: 'raffleTickets', align: "end", width: 15 },
            ]
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
                    name: p.Name,
                    position: p.LocalScoreAll.Position,
                    score: p.LocalScoreAll.Score,
                    globalScore: p.GlobalScore,
                    stars: p.Stars,
                    tobiiScore: p.TobiiScore.Score,
                    raffleTickets: p.RaffleTickets,
                    id: p.Id
                }
                let i = 0
                for (const day of p.LocalScoreAll.AccumulatedScore) {
                    i++
                    if (this.data.ExcludedDays.includes(i-1)) {
                        console.log("Skipped day", i)
                        continue
                    }
                    player[`d_${i}_0`] = day[0] > -1 ? day[0] : ""
                    player[`d_${i}_1`] = day[1] > -1 ? day[1] : ""
                }
                player["Identity"] = `${player.position}. ${player.name} (${player.id})`
                res.push(player)
            }
            return res
        }
    },
}
</script>

<style>

</style>