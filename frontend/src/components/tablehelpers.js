export  function fixedColumns() {
    let res = [
        { text: 'Pos', value: 'identity', width: 50 },
        { text: 'Name', value: 'name', width: 250, },
        { text: 'L', value: 'score', align: "end", width: 35, hint: "Local Score" },
        { text: 'G', value: 'globalScore', align: "end", width: 35, hint: "Global Score" },
        { text: 'S', value: 'stars', align: "end", width: 35, hint: "Stars collected" },
        { text: 'T', value: 'tobiiScore', align: "end", width: 35, hint: "Tobii Score" },
        { text: 'R', value: 'raffleTickets', align: "end", width: 35, hint: "Raffle Tickets" },
    ]
    return res
}

export function fixedData(player) {
    let res = {
        position: player.LocalScoreAll.Position,
        score: player.LocalScoreAll.Score,
        globalScore: player.GlobalScore,
        stars: player.Stars,
        tobiiScore: player.TobiiScore.Score,
        tobiiScorePos: player.TobiiScore.Position,
        raffleTickets: player.RaffleTickets,
        id: player.Id,
        supporter: player.Supporter,
        accumulatedTime: player.AccumulatedTimeToComplete
    }
    res["name"] = `${player.Name}`
    res["identity"] = res.position
    return res
}
import store from '../store/index'

export function dayColumns() {
    let res = []
    for (let day = 1; day < store.getters.data.HighestDay + 1; day++) {
        res.push({ text: `<center>D${day}<br>*</center>`, value: `d_${day}_0`, align: "end", width: 15 });
        res.push( { text: `<center>D${day}<br>**</center>`, value: `d_${day}_1`, align: "end", width: 15 });
        // res.push( { text: ``, value: `x_${day}`, align: "end", width: 1 });
    }
    if (store.getters.firstDayFirst) {
        return res
    }
    return res.reverse()
}

const medals = {0: "goldMedal", 1: "silverMedal", 2: "bronzeMedal"}
export function getMedalColor(item, key) {
    const posKey = "s" + key.slice(1)
    let daynum = parseInt(key.slice(2, -2)) - 1
    let excluded = ""
    if (store.getters.data.ExcludedDays) {
        excluded = store.getters.data.ExcludedDays.includes(daynum) ? " excludedday" : ""
    }
    const medal = medals[item[posKey]] || ""
    return  medal + excluded
}

export function getSolveTime(item, key, star) {
    const posKey = "t_" + key.slice(2, -2)+"_"+star;
    const res = secondsToString(item[posKey]);
    return  res
}

export function decoratePlayerWithDayFields(player, p, day) {
    player[`s_${day}_0`] = p.PositionForStar[day-1][0]
    player[`s_${day}_1`] = p.PositionForStar[day-1][1]

    player[`t_${day}_0`] = p.TimeToComplete[day-1][0]
    player[`t_${day}_1`] = p.TimeToComplete[day-1][1]
}


const secondsInHour = 60 * 60
const secondsInDay = secondsInHour * 24

export function secondsToString(seconds) {
    if (seconds === undefined) { return ""}
    if (seconds === 0) {
        return "WINNER"}
        if (seconds === -1 || seconds === Number.MAX_SAFE_INTEGER) {
            return ""}
            if (seconds > secondsInDay) {
                const days = Math.floor(seconds / secondsInDay)
                if (days >= 365 * 2) { return `>${Math.floor(days/365)} years` }
                if (days >= 365) { return `>${Math.floor(days/365)} year` }
                if (days === 1) { return `>${days} day` }
                return `>${days} days`
            }
            const res = new Date(seconds * 1000).toISOString().substr(11, 8)

            return res
        }

export function secondsToString2(seconds) {
    if (seconds === undefined) { return ""}
    if (seconds === 0) {
        return "WINNER"
    }
    if (seconds === -1 || seconds === Number.MAX_SAFE_INTEGER) {
        return ""
    }
    const days = Math.floor(seconds / secondsInDay)
    if (days > 10) {
        if (days >= 365 * 2) { return `>${Math.floor(days/365)} years` }
        if (days >= 365) { return `>${Math.floor(days/365)} year` }
        if (days === 1) { return `>${days} day` }
        return `>${days} days`
    }

    let str = new Date(seconds * 1000).toISOString().substr(11, 8);

    if (days > 0)
        str = days + "d " + str;

    return str
}