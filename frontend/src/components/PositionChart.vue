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
        infotext: "This board shows the positions per day/star.",
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

            console.log(res)
            return res
        }
    }
}
</script>

<style>

.chartclass {
    height: 800px;
}

</style>