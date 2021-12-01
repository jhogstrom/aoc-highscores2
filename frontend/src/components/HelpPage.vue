<template>
<div>
  <h1>Additional info</h1>
<p>
  First of all - you ain't gonna get any help solving the AoC problems here! If you were looking for that, look elsewhere!
</p>

<p>
  However, we will give you some help on how to use this highscore app.
</p>
<p>
    <h2>Columns</h2>
    <ul>
        <li @click="$router.push('/')">L - Local score. The amount of points achieved in the local list (your leaderboard).</li>
        <li @click="$router.push('/globalscoreperday')"> G - Global score. How many points acheieved on the global list.</li>
        <li @click="$router.push('/positionforstar')">S - Stars. How many stars have been collected (problems solved).</li>
        <li @click="$router.push('/tobiiscore')">T - Tobii Score. Scoring based on fisrt come gets lowest score.</li>
        <li @click="$router.push('')">R - Raffle tickets. Can be used for fair-chance a lottery between all participants in the group</li>
    </ul>
</div>
</template>

<script>
export default {

}
</script>

<style>

</style>