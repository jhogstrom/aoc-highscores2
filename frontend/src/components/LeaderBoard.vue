<template>
<div>
    <v-data-table
        :headers="headers"
        :items="playerList"
        :items-per-page="-1"
        class="elevation-1"
        dense
        hide-default-footer
        single-select>
        <template v-slot:item.Position="{ item }">
            {{ item.position }}. {{ item.name }} ({{item.id}})
        </template>
        <template v-slot:no-data>
            No data available
        </template>
    </v-data-table>
</div>
</template>

<script>
export default {
    data () { return {
        headers: [
            { text: 'Pos.', value: 'Position', width: 230 },
            { text: 'L', value: 'score', align: "end", width: 15  },
            { text: 'G', value: 'globalScore', align: "end", width: 15 },
            { text: 'S', value: 'stars', align: "end", width: 15 },
            { text: 'T', value: 'tobiiScore', align: "end", width: 15 },
            { text: 'R', value: 'raffleTickets', align: "end", width: 15 },
        ],
    }},
    computed: {
        players() {
            return this.$store.getters.filteredPlayers
        },
        playerList() {
            let res = []
            for (const p of this.players) {
                console.log(p.Name)
                res.push(
                    {
                        name: p.Name,
                        position: p.LocalScoreAll.Position,
                        score: p.LocalScoreAll.Score,
                        globalScore: p.GlobalScore,
                        stars: p.Stars,
                        tobiiScore: p.TobiiScore.Score,
                        raffleTickets: p.RaffleTickets,
                        id: p.Id
                    })
            }
            return res
        }
    },
}
</script>

<style>

</style>