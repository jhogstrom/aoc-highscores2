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
        infoTitle: "ScoreDiff chart",
        infotext: "This board shows the number of points each player is behind the leader of the board.",
    }},
    computed: {
        players() {
            return this.$store.getters.filteredPlayers
        },
        data() {
            return this.$store.getters.data
        },
        // https://developers.google.com/chart/interactive/docs/gallery/linechart#configuration-options
        chartOptions() {
            return {
                height: 1200,
                chart: {
                    title: 'Day/star',
                },
                vAxis: {
                    title: 'ScoreDiff',
                    // textPosition: "none",
                    //minValue: -10,
                    direction: -1,
                    logScale: true,
                    //ticks: this.tickMarks,
                    viewWindow: {
                        // max: 1000,
                        min: 0,
                    },
                }
            }
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
                            pdata.push(p.LocalScoreAll.ScoreDiff[day][star]);
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