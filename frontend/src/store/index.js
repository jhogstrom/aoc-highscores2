import Vue from 'vue'
import Vuex from 'vuex'
import * as types from './mutation-types'
Vue.use(Vuex)
var _ = require('lodash')

const debug = process.env.NODE_ENV !== 'production'

// one store for entire application
export default new Vuex.Store({
    strict: debug,
    state: {
        data: {
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
        isLoaded: false
    },
    getters: {
        data: state => state.data,
        updateTime: state => state.data.RetrievedFromAoC,
        includeZeroes: state => state.includeZeroes,
        autoRefresh: state => state.autoRefresh,
        isLoaded: state => state.isLoaded,
        players: state => _.sortBy(state.data.Players, ["LocalScoreAll.Position"]).slice(),
        filteredPlayers: state => state.isLoaded ? state.data.Players.filter(_ => state.includeZeroes || _.Stars > 0).slice() : [],
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
        async setParams({ commit }, payload) {
            if (!payload) {
                payload = {year: null, guid: null}
            }
            console.log("setParams", payload.year, payload.guid)
            commit(types.SET_YEAR, payload.year)
            commit(types.SET_GUID, payload.guid)
        },
        async setIncludeZeroes({ commit }, includeZeroes) {
            console.log("Setting", includeZeroes)
            commit(types.SET_INCLUDEZEROES, includeZeroes)
        },
        async setFirstDayFirst({ commit }, firstDayFirst) {
            console.log("Setting", firstDayFirst)
            commit(types.SET_FIRSTDAYFIRST, firstDayFirst)
        },
        async setAutoRefresh({ commit }, autoRefresh) {
            console.log("Setting", autoRefresh)
            commit(types.SET_AUTOREFRESH, autoRefresh)
        },
    },
    mutations: {
        SET_DATA(state, data) {
            state.data = data
            state.isLoaded = data ? true : false
            console.log("mutating data", data)
        },
        SET_GUID(state, guid) {
            state.guid = guid
            console.log("mutating guid", guid)
            if (guid) {
                localStorage.setItem('aocguid', guid)
            } else {
                localStorage.removeItem('aocguid')
            }
        },
        SET_YEAR(state, year) {
            state.year = year
            console.log("mutating year", year)
            if (year) {
                localStorage.setItem('year', year)
            } else {
                localStorage.removeItem('year')
            }
        },
        SET_INCLUDEZEROES(state, includeZeroes) {
            state.includeZeroes = includeZeroes
            localStorage.setItem('includeZeroes', includeZeroes)
            console.log("mutating includeZeroes", includeZeroes)
        },
        SET_FIRSTDAYFIRST(state, firstDayFirst) {
            state.firstDayFirst = firstDayFirst
            localStorage.setItem('firstDayFirst', firstDayFirst)
            console.log("mutating firstDayFirst", firstDayFirst)
        },
        SET_AUTOREFRESH(state, autoRefresh) {
            state.autoRefresh = autoRefresh
            localStorage.setItem('autoRefresh', autoRefresh)
            console.log("mutating autoRefresh", autoRefresh)
        },
        initialiseStore(state) {
            if (localStorage.getItem('includeZeroes') == "true") {
                state.includeZeroes = localStorage.getItem('includeZeroes');
            }
            if (localStorage.getItem('firstDayFirst') == "true") {
                state.firstDayFirst = localStorage.getItem('firstDayFirst');
            }
            if (localStorage.getItem('autoRefresh') == "true") {
                state.autoRefresh = localStorage.getItem('autoRefresh');
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