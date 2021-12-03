<template>
  <div class="narrowbody">
      <v-card v-if="!boardSaved">
          <v-card-title>
              Add your leader board
          </v-card-title>
          <v-card-text>
            <v-text-field class="textfield"
                v-model="boardId"
                label="Board ID (from AOC)"
                placeholder="Board ID"
                clearable
                outlined
                dense/>
            <v-text-field class="textfield"
                v-model="boardName"
                label="Name of your board"
                placeholder="Board name"
                clearable
                outlined
                dense/>
            <v-text-field class="textfield"
                v-model="sessionCookie"
                label="Session Cookie from adventofcode.com"
                placeholder="Session cookie"
                clearable
                outlined
                dense/>
            <v-text-field class="textfield"
                v-model="ownerid"
                label="Owner name"
                placeholder="User id"
                clearable
                outlined
                dense/>
            <v-text-field class="textfield"
                v-model="password"
                label="Password"
                placeholder="Password"
                clearable
                outlined
                dense/>

            <p>
                In the future we might add more configuration options for your board.
                To ensure only you can manipulate it, add a user name and password.
                A suggestion is to use your aoc id or name as user name and some throw-away password.
                The password is stored as a salted MD5 hash on the server side, but it is not handled by
                an industry strength identity manager at this point.
            </p>
            <p>
                As we do not have your email address, we can't really reset the password either...
            </p>
          </v-card-text>
          <v-card-actions class="text-right">
            <div >
                <v-btn
                    @click="requestList"
                    :disabled="!validData">
                    Submit
                </v-btn>
            </div>
          </v-card-actions>
      </v-card>
      <v-card v-else>
          <v-card-title>
              Request a new board
          </v-card-title>
          The board '{{boardName}}'' has been created!
          <br>
          The guid for the board is <b>{{guid}}</b>. You can find the url in <span class="fakelink" @click="navigate">Your Other Boards</span>,
          but it will not be available to view for another few seconds.
      </v-card>

  </div>
</template>

<script>
import {HttpApi} from '@/http'
export default {
    data() { return {
        boardId: "",
        boardName: "",
        sessionCookie: "",
        password: "",
        ownerid: "",
        boardSaved: false,
        guid: ""
    }},
    computed: {
        validData() {
            return this.boardId
                && this.boardName.length > 5
                && this.sessionCookie.length > 10
                && this.ownerid
                && /^\d+$/.test(this.boardId)
        }
    },
    methods: {
        saveBoard(name, guid) {
            let boards = JSON.parse(localStorage.getItem("knownBoards") || "[]")
            boards.push({
                name: name,
                guid: guid
            })
            localStorage.setItem(
                "knownBoards",
                JSON.stringify(boards)
            )
        },
        navigate() {
            this.$router.push("/boards")
        },
        async requestList() {
            const response = await HttpApi.post("/createboard", {
                boardid: this.boardId,
                boardname: this.boardName,
                session_cookie: this.sessionCookie,
                password: this.password,
                ownerid: this.ownerid
            })
            console.log(response)
            if (response.guid) {
                this.boardSaved = true
                this.saveBoard(this.boardName, response.guid)
                this.guid = response.guid
            } else {
                alert("Not great")
            }
        }
    }

}
</script>

<style scoped>
.narrowbody {
    width: 80%;
    margin: 20px;
    padding: 20px;
}

.textfield {
    margin: 10px;
    padding: 10px;
}

.fakelink {
    text-decoration: underline;
    cursor: pointer;
}
</style>