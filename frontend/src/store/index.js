import Vue from 'vue'
import Vuex from 'vuex'
import * as types from './mutation-types'
Vue.use(Vuex)
const debug = process.env.NODE_ENV !== 'production'

// one store for entire application
export default new Vuex.Store({
    strict: debug,
    state: {
        data: {Players: []},
    },
    getters: {
        data: state => state.data,
        isLoaded: state => (state.data) ? true : false,
    },
    actions: {
        async setData({commit}, data) {
            console.log("Setting", data)
            commit(types.SET_DATA, data)
        },
    },
    mutations:  {
        SET_DATA(state, data) {
            state.data = data
            console.log("mutating data", data)
        },
    }
})