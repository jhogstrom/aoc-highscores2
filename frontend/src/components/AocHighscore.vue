<template>
    <div class="scrollbox">
    <v-data-table
        :headers="headers"
        :items="players"
        :items-per-page="-1"
        class="elevation-1"
        dense
        hide-default-footer
        single-select>
        <template v-slot:item.Position="{ item }">
            {{ item.LocalScoreAll.Position }})
        </template>
        <template v-slot:item.Filler>

        </template>
        <template v-slot:item.Score="{ item }">
            {{ item.LocalScoreAll.Score }}
        </template>
        <template v-slot:item.Stars="{ item }">
            <star-line :times="item.UnixCompletionTime"></star-line>
        </template>
    </v-data-table>

    <!-- <div v-for="p in players" v-bind:key="p.Name">
        {{p.LocalScoreAll.Position|rightAdjust(3)}}) {{p.LocalScoreAll.Score|rightAdjust(4)}}
    </div> -->
    </div>

  
</template>

<script>
import StarLine from './StarLine.vue'


export default {
    components: { StarLine },
    data () { return {
        headers: [
            { text: 'Pos.', value: 'Position', width: 20 },
            { text: 'Points', value: 'Score', width: 20  },
            { text: 'Stars', value: 'Stars', width: 100 },
            { text: 'Name', value: 'Name', width: 250 },
            { text: '', value: 'Filler', width: 200},
        ],
    }},
    computed: {
        players() {
            return this.$store.getters.filteredPlayers
        },
    },
    filters: {
        rightAdjust(value, c) {
            let res = String(value).padStart(c, "*")
            res = res.replaceAll("*", '\xa0')
            return res
        }
    }
}
</script>

<style scoped>
>>> .v-data-table__mobile-row{
    font-size: 1.1rem !important;
    background-color: rgb(5, 7, 39);
    color: lightgray;
}
>>> td.text-start {
    font-size: 1.1rem !important;
    background-color: rgb(5, 7, 39);
    color: lightgray;
}

</style>
