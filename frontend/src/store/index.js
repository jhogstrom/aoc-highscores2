import Vue from 'vue'
import Vuex from 'vuex'
import * as types from './mutation-types'
import { HttpApi, fileUrl } from '@/http.js'
import { sockets } from './sockets.js'

Vue.use(Vuex)
var _ = require('lodash')

const debug = process.env.NODE_ENV !== 'production'

// one store for entire application
export default new Vuex.Store({
    strict: debug,
    state: {
        data: {
            Name: "",
            Year: "",
            Players: [
                {
                    Id: 0,
                    LocalScore: 0,
                    GlobalScore: 0,
                    Stars: 0,
                    RaffleTickets: 0,
                    LocalScoreAll: {
                        Position: 0,
                        Score: 0,
                    },
                    TobiiScore: {
                        Score: 0,
                    }

                }
            ] },
        includeZeroes: false,
        autoRefresh: false,
        firstDayFirst: true,
        year: null,
        guid: null,
        isLoaded: false,
        includeMedals: false,
        loadingInProgress: false
    },
    getters: {
        loadingInProgress: state => state.loadingInProgress,
        includeMedals: state => state.includeMedals,
        data: state => state.data,
        updateTime: state => state.data.RetrievedFromAoC,
        includeZeroes: state => state.includeZeroes,
        autoRefresh: state => false && state.autoRefresh,
        isLoaded: state => state.isLoaded,
        maxStars: state => state.data.HighestDay * 2,
        players: state => _.sortBy(state.data.Players, ["LocalScoreAll.Position"]).slice(),
        filteredPlayers: state => {
            console.log("getting players")
            return state.isLoaded ?
                state.data.Players.filter(_ => state.includeZeroes || _.Stars > 0)
                : []
        },
        firstDayFirst: state => state.firstDayFirst,
        boardName: state => state.isLoaded ? state.data.Name || "highscores" : "No such board",
        boardYear: state => state.isLoaded ? state.data.Year : "No data",
        year: state => state.year, // from query parameter
        guid: state => state.guid, // from query parameter
    },
    actions: {
        async setData({ commit }, data) {
            console.log("Setting", data)
            commit(types.SET_DATA, data)
        },
        async setParams({ commit, state }, payload) {
            if (!payload) {
                payload = {year: null, guid: null}
            }
            if ((state.year && state.year != payload.year) &&
                (state.guid && state.guid != payload.year)) {
                sockets.sendMessage({
                    command: "unregister",
                    guid: state.guid,
                    year: state.year})
            }
            // console.log("setParams", payload.year, payload.guid)
            commit(types.SET_YEAR, payload.year)
            commit(types.SET_GUID, payload.guid)
            sockets.sendMessage({command: "register", ...payload})
        },
        async setIncludeZeroes({ commit }, includeZeroes) {
            // console.log("Setting", includeZeroes)
            commit(types.SET_INCLUDEZEROES, includeZeroes)
        },
        async setIncludeMedals({ commit }, includeMedals) {
            // console.log("Setting", includeMedals)
            commit(types.SET_INCLUDEMEDALS, includeMedals)
        },
        async setFirstDayFirst({ commit }, firstDayFirst) {
            // console.log("Setting", firstDayFirst)
            commit(types.SET_FIRSTDAYFIRST, firstDayFirst)
        },
        async setAutoRefresh({ commit }, autoRefresh) {
            // console.log("Setting", autoRefresh)
            commit(types.SET_AUTOREFRESH, autoRefresh)
        },
        async requestRefresh() {
            console.log("requesting data for ", this.getters.year, this.getters.guid);
            HttpApi.get(`/refresh/${this.getters.year}/${this.getters.guid}`)
                .then(res => console.log(res))
        },
        async loadData({ commit, state }) {
            commit(types.SET_LOADINGINPROGRESS, true)
            const year = state.year
            const guid = state.guid
            console.log("* loading data for ", year, guid);
            fetch(fileUrl(year, guid))
                .then(response => {
                    console.log("status:", response.status)
                    const loadedOk = response.status === 200
                    if (loadedOk) {
                        return response.json()
                    } else {
                        return null
                    }
                    })
                .then(data => {
                    if (data) {
                        commit(types.SET_DATA, data)
                        let boards = JSON.parse(localStorage.getItem("knownBoards") || "[]")
                        boards = boards.filter(_ => _.guid != state.guid)
                        boards.push({
                            name: data.Name,
                            guid: guid
                        })
                        localStorage.setItem(
                            "knownBoards",
                            JSON.stringify(boards)
                            )

                    } else {
                        commit(types.SET_DATA, {})
                    }
                    })
                .then(() => commit(types.SET_LOADINGINPROGRESS, false))
        }
    },
    mutations: {
        SET_DATA(state, data) {
            state.data = data
            state.isLoaded = data ? true : false
            // console.log("mutating data", data)
        },
        SET_LOADINGINPROGRESS(state, loadingInProgress) {
            state.loadingInProgress = loadingInProgress
        },
        SET_GUID(state, guid) {
            state.guid = guid
            // console.log("mutating guid", guid)
            if (guid) {
                localStorage.setItem('aocguid', guid)
            } else {
                localStorage.removeItem('aocguid')
            }
        },
        SET_YEAR(state, year) {
            state.year = year
            // console.log("mutating year", year)
            if (year) {
                localStorage.setItem('year', year)
            } else {
                localStorage.removeItem('year')
            }
        },
        SET_INCLUDEMEDALS(state, includeMedals) {
            state.includeMedals = includeMedals
            localStorage.setItem('includeMedals', includeMedals)
            // console.log("mutating includeMedals", includeMedals)
        },
        SET_INCLUDEZEROES(state, includeZeroes) {
            state.includeZeroes = includeZeroes
            localStorage.setItem('includeZeroes', includeZeroes)
            // console.log("mutating includeZeroes", includeZeroes)
        },
        SET_FIRSTDAYFIRST(state, firstDayFirst) {
            state.firstDayFirst = firstDayFirst
            localStorage.setItem('firstDayFirst', firstDayFirst)
            // console.log("mutating firstDayFirst", firstDayFirst)
        },
        SET_AUTOREFRESH(state, autoRefresh) {
            state.autoRefresh = autoRefresh
            localStorage.setItem('autoRefresh', autoRefresh)
            // console.log("mutating autoRefresh", autoRefresh)
        },
        initialiseStore(state) {
            if (localStorage.getItem('includeZeroes')) {
                state.includeZeroes = localStorage.getItem('includeZeroes') == "true";
            }
            if (localStorage.getItem('includeMedals')) {
                state.includeMedals = localStorage.getItem('includeMedals') == "true";
            }
            if (localStorage.getItem('firstDayFirst')) {
                state.firstDayFirst = localStorage.getItem('firstDayFirst') == "true";
            }
            if (localStorage.getItem('autoRefresh')) {
                state.autoRefresh = localStorage.getItem('autoRefresh') == "true";
            }

            if (localStorage.getItem('year')) {
                state.year = localStorage.getItem('year')
            }
            const guid = localStorage.getItem('aocguid')
            if (guid) {
                state.guid = guid
                console.log("read aocgrid:", guid)
            }
            console.log("initializing store", state)
        }
    }
})