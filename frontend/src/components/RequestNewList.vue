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
                v-model="email"
                label="Email"
                placeholder="Email"
                clearable
                outlined
                dense/>
            <p>
                In the future we might add more configuration options for your board.
                To ensure only you can manipulate it, add your email. The current plan is to send one time
                passwords or something along those lines once the configuration options are implemented.
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
      <v-card v-else class="narrowbody">
          <v-card-title>
              Board created
          </v-card-title>
          The board '{{boardName}}' has been created!
          <br>
          The guid for the board is <b>{{guid}}</b>. You can find the url in
          <span class="fakelink" @click="navigate('/boards')">Your Other Boards</span>,
          but the board itself will not be available to view for another few seconds.
          <v-card-actions class="text-right">
            <div >
                <v-btn
                    @click="navigate('/')">
                    OK
                </v-btn>
            </div>
          </v-card-actions>
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
        email: "",
        boardSaved: false,
        guid: ""
    }},
    computed: {
        validData() {
            return this.boardId
                && this.boardName.length > 5
                && this.sessionCookie.length > 10
                && (!this.email || this.email.includes("@"))
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
        navigate(target) {
            this.$router.push(target)
        },
        async requestList() {
            let body = {
                boardid: this.boardId,
                boardname: this.boardName,
                session_cookie: this.sessionCookie,
            }
            if (this.email) {
                body.email = this.email
            }
            console.log(body)
            const response = await HttpApi.post("/createboard", body)
            console.log(response.status)
            if (response.guid) {
                this.boardSaved = true
                this.saveBoard(this.boardName, response.guid)
                this.guid = response.guid
            } else {
                console.log(response)
                if (response.detail) {
                    let msg = ""
                    for (const detail of response.detail) {
                        msg += `${detail.msg}\n`
                    }
                    alert(msg)
                } else {
                    alert(response.message)
                }

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