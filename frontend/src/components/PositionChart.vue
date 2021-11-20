<template>
<div class="chartclass">
    <info-block :infotext="infotext"></info-block>
    <GChart
        type="LineChart"
        :data="playerList"
        :options="chartOptions"
        height="400px"
    />
 </div>
</template>

<script>
import InfoBlock from './InfoBlock.vue'
// import { fixedColumns,  dayColumns, getMedalColor } from './tablehelpers'
import { GChart } from 'vue-google-charts'
export default {
    components: { InfoBlock, GChart },
    data() { return {
        infotext: "This board shows the <h1>leaders</h1> and points per day.",
        chartOptions: {
            chart: {
                title: 'Day/star',
            },
            vAxis: {
                // title: 'Time',
                // textPosition: "none",
                viewWindow: {
                    // max: 1000,
                    min: 0,
                },
            },

        }
    }},
    computed: {
        players() {
            return this.$store.getters.filteredPlayers
        },
        data() {
            return this.$store.getters.data
        },
        playerList() {
            let res = [undefined]
            let headers = ["Player"]
            for (let day = 1; day < this.data.HighestDay + 1; day++) {
                headers.push(`${day}-1`)
                headers.push(`${day}-2`)
            }
            for (const p of this.players) {
                let pdata = [p.Name]
                for (let day = 1; day < this.data.HighestDay + 1; day++) {
                    const dataValue = p.LocalScoreAll.AccumulatedPosition[day-1]
                    pdata.push(dataValue[0])
                    pdata.push(dataValue[1])
                }
                res.push(pdata)
            }
            res[0] = headers
            console.log(res)
            return res
        }
    },
}
</script>

<style>

.chartclass {
    height: 800px;
}

</style>