<template>
    <div>
        <div v-for="b, index in sortedBoards" :key="b.guid"
                @mouseover="show = index"
                @mouseleave="show = null">
            <div class="boardentry" @click="navigate(b)">
                <v-container>
                    <v-row>
                        <v-col></v-col> <!-- empty col for padding -->
                        <v-col>
                            <span class="fakelink">{{ b.name }}</span>
                        </v-col>
                        <v-col>
                            <span v-if="show == index">
                                <v-tooltip top>
                                    <template v-slot:activator="{ on }">
                                        <v-icon v-on="on" class="extrapadding" @click.stop="navigate(b)">mdi-open-in-app</v-icon>
                                    </template>
                                    <span>Open board</span>
                                </v-tooltip>
                                <v-tooltip top>
                                    <template v-slot:activator="{ on }">
                                        <v-icon v-on="on" class="extrapadding" @click.stop="copylink(b)">mdi-content-copy</v-icon>
                                    </template>
                                    <span>Copy link</span>
                                </v-tooltip>
                                <v-tooltip top>
                                    <template v-slot:activator="{ on }">
                                        <v-icon v-on="on" class="extrapadding" @click.stop="remove(b)">mdi-delete-alert</v-icon>
                                    </template>
                                    <span>Remove board from your boards list</span>
                                </v-tooltip>
                            </span>
                        </v-col>
                    </v-row>
                </v-container>
            </div>
        </div>
        <v-btn @click="refresh">Refresh</v-btn>
    </div>
</template>

<script>
var _ = require('lodash')
export default {
    data() { return {
        show: null,
        boards: [
            {name: "tobii", guid: "jlkjllk"},
            {name: "leica", guid: "foobar"},
            ]
    }},
    computed: {
        sortedBoards() {
            return _.sortBy(this.boards, b => b.name.toUpperCase())
        }
    },
    methods: {
        async navigate(board) {
            let year = new Date().getFullYear()
            if (new Date().getMonth < 11) {
                year--
            }
            await this.$store.dispatch('setParams', {year: year, guid: board.guid})
            await this.$store.dispatch('loadData')
            this.$store.dispatch('requestRefresh', {year: year, guid: board.guid})
            this.$router.push("/")
        },
        remove(board) {
            this.boards = this.boards.filter(_ => _.guid != board.guid)
            localStorage.setItem("knownBoards", JSON.stringify(this.boards))
            console.log("remove", board.name)
            this.refresh()
        },
        copylink(board) {
            // console.log(window.location)
            var url = `${window.location.origin}?guid=${board.guid}`
            navigator.clipboard.writeText(url)
                .then(() => console.log("copied", url))
        },
        refresh() {
            const globalGuid = "00000000-0000-0000-0000-000000000000"
            const globalBoard = {name: "Global Board", guid: globalGuid}
            const boards = localStorage.getItem("knownBoards") || JSON.stringify([globalBoard])
            this.boards = JSON.parse(boards)
            if (this.boards.filter(_ => _.guid == globalGuid).length == 0) {
                this.boards.push(globalBoard)
            }
        }
    },
    async mounted() {
        this.refresh()
    }


}
</script>

<style>
.centertext {
    text-align: center;
}
.halign {
    text-align: right;
}
.extrapadding {
    padding-left: 10px;
}
.boardentry {
    border: 2px solid black;
    background-color: blanchedalmond;
    /* color: blanchedalmond; */
    border-radius: 10px;
    padding: 10px;
    margin: 10px;
}

</style>
[{"name":"Global scores","guid":"00000000-0000-0000-0000-000000000000"},{"name":"AditroLogistics","guid":"fbc7a3d8-c6f4-410f-9fff-a1b42993c1c1"},{"name":"Tobii","guid":"5af63394-86b8-4efa-b5c4-3bf6c20685f9"}]