<template>
<div class="chartclass">
    <info-block :infotext="infotext" :title="infoTitle"></info-block>
    <GChart
        type="LineChart"
        :data="playerList"
        :options="chartOptions"    
        />
 </div>
</template>

<script>
import InfoBlock from './InfoBlock.vue'
// import { fixedColumns,  dayColumns, getMedalColor } from './tablehelpers'
import { GChart } from 'vue-google-charts'

export default {
    components: { InfoBlock, GChart },
    data() { 
        return {
        infoTitle: "Position per day and star",
        infotext: "This board shows the position per day/star.",
    }},
    computed: {
        players() {
            return this.$store.getters.filteredPlayers
        },
        data() {
            return this.$store.getters.data
        },
        chartOptions() {
            return {
                height: 800,
                chart: {
                    title: 'Day/star',
                },
                vAxis: {
                    title: 'Position',
                    // textPosition: "none",
                    // minValue: 5,
                    direction: -1,
                    ticks: this.tickMarks,
                    viewWindow: {
                        // max: 1000,
                        min: 0,
                    },
                }
            }
        },
        tickMarks() {
            const playerCount = this.players.length
            let tickInterval = 25 // for 100+ players in list
            if (playerCount < 51) {
                tickInterval = 5
            } else if (playerCount < 101) {
                tickInterval = 10
            }
            const maxTick = Math.floor(playerCount/tickInterval) * tickInterval + tickInterval
            const res = []
            for (let i = tickInterval; i <= maxTick; i += tickInterval) {
                res.push(i)
            }
            console.log(res)
            return res
        },
        playerList() {
            let headers = ["Player"]
            for (const p of this.players) {
                headers.push(p.Name);
            }
            let res = [headers]

            for (let day = 0; day < this.data.HighestDay; day++) {
                const excluded = this.$store.getters.data.ExcludedDays.includes(day)
                if (!excluded) {
                    for (let star = 0; star < 2; star++ ) {
                        let pdata = [`${day+1}-${star+1}`]
                        for (const p of this.players) {
                            pdata.push(p.LocalScoreAll.AccumulatedPosition[day][star]);

                        }   
                        res.push(pdata);          
                    }
                }
            }

            // console.log(res)
            return res
        }
    }
}
</script>

<style>

</style>