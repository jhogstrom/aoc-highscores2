<template>
    <div class="scrollbox">
    <v-data-table
        :headers="headers"
        :items="players"
        :items-per-page="-1"
        class="elevation-1 aocstyle"
        dense
        hide-default-footer
        single-select>
        <!-- @click:row="rowClick"
        @dblclick:row="editItem"> -->
        <template v-slot:item.Position="{ item }">
            <!-- <span v-bind:class="coloring(item)">{{ customerString(item) }}</span> -->
            {{ item.LocalScoreAll.Position }})
        </template>
        <template v-slot:item.Name="{ item }">
            <!-- <span v-bind:class="coloring(item)">{{ customerString(item) }}</span> -->
            {{ item.Name }}
        </template>
        <template v-slot:item.Score="{ item }">
            <!-- <span v-bind:class="coloring(item)">{{ customerString(item) }}</span> -->
            {{ item.LocalScoreAll.Score }}
        </template>
        <template v-slot:item.Stars="{ item }">
            <star-line :times="item.UnixCompletionTime"></star-line>
        </template>
        <template v-slot:no-data>
            No data available
        </template>
    </v-data-table>
    </div>

  
</template>

<script>
import StarLine from './StarLine.vue'


export default {
    components: { StarLine },
    data () { return {
        headers: [
            { text: 'Pos.', value: 'Position', width: 30 },
            { text: 'Name', value: 'Name', width: 150 },
            { text: 'Points', value: 'Score', width: 20  },
            { text: 'Stars', value: 'Stars' },
        ],
    }},
    computed: {
        players() {
            return this.$store.getters.filteredPlayers
        },
    },
}
</script>

<style scoped>
>>>td.text-start {
    background-color: rgb(5, 7, 39);
    color: lightgray;
    /* font-family: 'Courier New', Courier, monospace; */
    font: 16px 'Courier New', Courier, monospace;
    /* border: 2px black; */
    /* font-size: 32px; */

}

</style>