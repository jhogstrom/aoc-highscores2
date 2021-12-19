import Vue from 'vue'
import VueRouter from 'vue-router'

import LeaderBoard from '../components/LeaderBoard.vue'
import CompletionTime from '../components/CompletionTime.vue'
import OffsetFromWinner from '../components/OffsetFromWinner.vue'
import AccumulatedTimeToComplete from '../components/AccumulatedTimeToComplete.vue'
import CompletionTimeStarTwo from '../components/CompletionTimeStarTwo.vue'
import GlobalScoreForDay from '../components/GlobalScoreForDay.vue'
import TobiiScore from '../components/TobiiScore.vue'
import PositionForStar from '../components/PositionForStar.vue'
// Charts
// import PositionChart from '../components/PositionChart.vue'
// import ScoreChart from '../components/ScoreChart.vue'
// Other
import RawData from '../components/RawData.vue'
import AocHighscore from '../components/AocHighscore.vue'
import BoardList from '../components/BoardList.vue'
import HelpPage from '../components/HelpPage.vue'
import RequestNewList from '../components/RequestNewList.vue'

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
    path: '/positionforstar',
    title: "Position for Star",
    component: PositionForStar
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
    component: () => import(/* webpackChunkName: "charts" */ '@/components/PositionChart.vue')
    // component: PositionChart
  },
  {
    path: '/scorechart',
    title: "ScoreChart",
    component: () => import(/* webpackChunkName: "charts" */ '@/components/ScoreChart.vue')
    // component: ScoreChart
  },
]

export const other = [
  {
    path: '/aoc',
    title: "Aoc Style",
    component: AocHighscore
  },
  {
    path: '/boards',
    title: "Your other boards",
    component: BoardList
  },
  {
    path: '/newlist',
    title: "Add a leader board",
    component: RequestNewList
  },
  {
    path: '/help',
    title: "Help",
    component: HelpPage
  },
  {
    path: '/raw',
    title: "Raw data",
    component: RawData
  },
]
const routes = [...boards, ...charts, ...other]

const router = new VueRouter({
  mode: 'history',
  base: process.env.BASE_URL,
  routes
})

export default router
