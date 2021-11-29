<template>
    <div class="scrollbox">
        <info-block :infotext="infotext" :title="infoTitle"></info-block>

        <div id="aocdiv">
            <div id="greentext">
            <span v-for="i in 18" :key="key(i, 17)">&nbsp;</span>
            1111111111222222<br>
            <span v-for="i in 9" :key="key(i, 3)">&nbsp;</span>
            1234567890123456789012345
            </div>
            <div v-for="p in players" v-bind:key="p.Name">
                {{p.LocalScoreAll.Position|rightAdjust(3)}})
                {{p.LocalScoreAll.Score|rightAdjust(4)}}
                <star-line :days="p.UnixCompletionTime"></star-line>
                &nbsp;
                <span v-if="p.PublicProfile"><a :href="p.PublicProfile">{{p.Name}}</a></span>
                <span v-else>{{p.Name}}</span>
                <span class="aocyellow"> {{isSupporter(p)}}</span>
            </div>
        </div>
    </div>
</template>

<script>
import StarLine from './StarLine.vue'
import InfoBlock from './InfoBlock.vue'

export default {
    components: { StarLine,InfoBlock },
    data () { return {
        infoTitle: "Classic Aoc style board",
        infotext: "",
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
    },
    methods: {
        key(i, k) { return i * k},
        isSupporter(player) {
            return player.Supporter ? "(AoC++)" : ""
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

#aocdiv {
    font-size: 1.1rem !important;
    background-color: rgb(5, 7, 39);
    color: lightgray;
}

#greentext {
    color: rgb(00, 99, 00);;
}

.aocyellow {
    color: yellow;
}

a:link {
  color: rgb(00, 99, 00);
  text-decoration: none;
}

/* visited link */
a:visited {
  color: green;
  text-decoration: none;
}

/* mouse over link */
a:hover {
  color: rgb(99, 236, 81);
  text-decoration: none;
}

/* selected link */
a:active {
  color: blue;
  text-decoration: none;
}

</style>
