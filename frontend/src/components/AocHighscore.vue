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
            {{ item.Position }})
        </template>
        <template v-slot:item.Name="{ item }">
            <!-- <span v-bind:class="coloring(item)">{{ customerString(item) }}</span> -->
            {{ item.Name }}
        </template>
        <template v-slot:item.LocalScore="{ item }">
            <!-- <span v-bind:class="coloring(item)">{{ customerString(item) }}</span> -->
            {{ item.LocalScore }}
        </template>
        <template v-slot:item.Stars="{ item }">
            {{ starString(item.UnixCompletionTime) }}
        </template>
        <template v-slot:no-data>
            No data available
        </template>
    </v-data-table>
    </div>

  
</template>

<script>
// var _ = require('lodash')

export default {
    data () { return {
        headers: [
            { text: 'Pos.', value: 'Position', width: 30 },
            { text: 'Name', value: 'Name', width: 150 },
            { text: 'Points', value: 'LocalScore', width: 20  },
            { text: 'Stars', value: 'Stars' },
        ],
    }},
    computed: {
        players() {
            return this.$store.getters.filteredPlayers
        },
    },
    methods: {
        starString(unixCompletionTime) {
            var text="";
            
            for (let i = 0; i < unixCompletionTime.length; i++) {
                if (unixCompletionTime[i][0] == -1) {
                    text += " ";
                } else if (unixCompletionTime[i][1] == -1) {
                    text += "+"; // should be a silver star
                } else {
                    text += "*"; // should be a gold star
                }
            }
            return text;

        }
    }
}
</script>

<style scoped>
>>>td.text-start {
    background-color: black;
    color: yellow;
    font-family: 'Courier New', Courier, monospace;
    /* border: 2px black; */
    font-size: 18px;

}

</style>