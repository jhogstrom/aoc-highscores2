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
        data: { Players: [{ LocalScore: 0 }] },
        includeZeroes: false,
        autoRefresh: false,
    },
    getters: {
        data: state => state.data,
        updateTime: state => state.data.RetrievedFromAoC,
        includeZeroes: state => state.includeZeroes,
        autoRefresh: state => state.autoRefresh,
        isLoaded: state => (state.data) ? true : false,
        players: state => _.sortBy(state.data.Players, ["LocalScoreAll.Position"]).slice(),
        filteredPlayers: state => state.data.Players.filter(_ => state.includeZeroes || _.Stars > 0).slice()
    },
    actions: {
        async setData({ commit }, data) {
            console.log("Setting", data)
            commit(types.SET_DATA, data)
        },
        async setIncludeZeroes({ commit }, includeZeroes) {
            console.log("Setting", includeZeroes)
            commit(types.SET_INCLUDEZEROES, includeZeroes)
        },
        async setAutoRefresh({ commit }, autoRefresh) {
            console.log("Setting", autoRefresh)
            commit(types.SET_AUTOREFRESH, autoRefresh)
        },
    },
    mutations: {
        SET_DATA(state, data) {
            state.data = data
            console.log("mutating data", data)
        },
        SET_INCLUDEZEROES(state, includeZeroes) {
            state.includeZeroes = includeZeroes
            localStorage.setItem('includeZeroes', includeZeroes)
            console.log("mutating includeZeroes", includeZeroes)
        },
        SET_AUTOREFRESH(state, autoRefresh) {
            state.autoRefresh = autoRefresh
            localStorage.setItem('autoRefresh', autoRefresh)
            console.log("mutating autoRefresh", autoRefresh)
        },
        initialiseStore(state) {
            console.log("initializing store", state)
            if (localStorage.getItem('includeZeroes') == "true") {
                state.includeZeroes = localStorage.getItem('includeZeroes');
            }
            if (localStorage.getItem('autoRefresh') == "true") {
                state.autoRefresh = localStorage.getItem('autoRefresh');
            }
        }
    }
})