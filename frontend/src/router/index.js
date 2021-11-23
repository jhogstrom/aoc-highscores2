import Vue from 'vue'
import VueRouter from 'vue-router'

import LeaderBoard from '../components/LeaderBoard.vue'
import CompletionTime from '../components/CompletionTime.vue'
import OffsetFromWinner from '../components/OffsetFromWinner.vue'
import AccumulatedTimeToComplete from '../components/AccumulatedTimeToComplete.vue'
import CompletionTimeStarTwo from '../components/CompletionTimeStarTwo.vue'
import GlobalScoreForDay from '../components/GlobalScoreForDay.vue'
import TobiiScore from '../components/TobiiScore.vue'
// Charts
import PositionChart from '../components/PositionChart.vue'
// Other
import RawData from '../components/RawData.vue'
import AocHighscore from '../components/AocHighscore.vue'

Vue.use(VueRouter)

export const boards = [
  {
    path: '/',
    title: "Leaderboard",
    component: LeaderBoard
  },
  {
    path: '/timeoffset',
    title: "Time offset",
    component: OffsetFromWinner
  },
  {
    path: '/accumulatedtime',
    title: "Accumulated Time",
    component: AccumulatedTimeToComplete
  },
  {
    path: '/completiontimestar2',
    title: "Time*2",
    component: CompletionTimeStarTwo
  },
  {
    path: '/globalscoreperday',
    title: "Global",
    component: GlobalScoreForDay
  },
  {
    path: '/tobiiscore',
    title: "TobiiScore",
    component: TobiiScore
  },
  {
    path: '/completiontime',
    name: 'completiontime',
    title: "Completion time",
    // route level code-splitting
    // this generates a separate chunk (about.[hash].js) for this route
    // which is lazy-loaded when the route is visited.
    // component: () => import(/* webpackChunkName: "about" */ '../views/About.vue')
    component: CompletionTime
  }
]

export const charts = [
  {
    path: '/positionchart',
    title: "PositionChart",
    component: PositionChart
  },
]

export const other = [
  {
    path: '/raw',
    title: "Raw data",
    component: RawData
  },
  {
    path: '/aoc',
    title: "Aoc Style",
    component: AocHighscore
  },
]
const routes = [...boards, ...charts, ...other]

const router = new VueRouter({
  mode: 'history',
  base: process.env.BASE_URL,
  routes
})

export default router