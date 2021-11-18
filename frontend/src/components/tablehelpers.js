export  function fixedColumns() {
    let res = [
        { text: 'Pos.', value: 'identity', width: 23 },
        { text: 'Name', value: 'name', width: 250 },
        { text: 'L', value: 'score', align: "end", width: 15  },
        { text: 'G', value: 'globalScore', align: "end", width: 15 },
        { text: 'S', value: 'stars', align: "end", width: 15 },
        { text: 'T', value: 'tobiiScore', align: "end", width: 15 },
        { text: 'R', value: 'raffleTickets', align: "end", width: 15 },
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
        raffleTickets: player.RaffleTickets,
        id: player.Id
    }
    res["name"] = `${player.Name} (${player.Id})`
    res["identity"] = res.position
    return res
}
import store from '../store/index'
export function isDayExcluded(day) {
    if (store.getters.data.ExcludedDays.includes(day-1)) {
        console.log("Skipped day", day)
        return true
    }
    return false

}
export function dayColumns() {
    let res = []
    for (let day = 1; day < store.getters.data.HighestDay + 1; day++) {
        if (isDayExcluded(day)) {
            continue
        }
        res.push({ text: `day ${day} *`, value: `d_${day}_0`, align: "end", width: 15 })
        res.push({ text: `day ${day} **`, value: `d_${day}_1`, align: "end", width: 15 })
    }
    return res
}

export function getMedalColor(item, key) {
    const posKey = "s" + key.slice(1)
    const medals = {0: "goldMedal", 1: "silverMedal", 2: "bronzeMedal"}
    return medals[item[posKey]]
}