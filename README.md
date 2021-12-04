# aoc-highscores2

This is the third (or fourth? fifth) incarnation of the AoC high score list.

Jonas started it, pulling data from adventofcode.com and running a html-generator locally, scripting upload to his server.

Jesper figured it would be possible to write it as a web application (duh), and wrote a python port of the generator.
That was a mess, and generated a lot of code and looked ugly on a good day. Next move was to write a table generator component in javascript.
Worth noting that neither Jesper nor Jonas are front end developers.

Jesper's frontend team lead explained carefully that this is not how it's done nowadays and pointed towards ag-grid. A new world! Much less code and
it was the nicest looking UI Jesper had ever written. The backend powered by lambda functions written in Python.

Fast forward almost a year and Jonas wondered if we should replace his generated version with the aggrid version.

Since then Jesper had taken to learning Vue. Looking at the old code it was not production worthy... A complete rewrite again, now with a Vue frontend, backend in Python and C#, communication using SQS and a couple of Dynamo tables.

Both Jonas and Jesper learned a lot writing this application and had a lot of fun. It took us evenings and some weekend work over two weeks. The code is available on github.

# Request your own board

This will be automated soon (probably after 2021 AoC). For now, send your
* aoc session cookie (for instructions, see https://github.com/wimglenn/advent-of-code-wim/issues/1)
* board id
* name of board

to jonas.hogstrom at tobii.com.
# Contribute
Feel free to fork the repo, and submit pull requests. Common sense and best practises applies.

# Set up your own frontend and backend
It should be possible to simply run  `cdk deploy` to spin up the backend. You may want to tweak the CloudFront
creation, as it relies on a certificate that will not do what you want (it allows your CloudFront distribution
to handle requests for aoc.lillfiluren.se).

For local testing you can spin up `uvicorn` to test the backend API. It ought to be possible to run some local aws
emulator to avoid hitting your backend. That may or may not require some tweaks or parametrization. Please share how you did that.

The frontend can be run locally by `npm run serve`. The current deployment is manual; drag the contents of the `dist` directory to your bucket root. If you have CloudFront helping you out, then invalidate the cache. Yes, scripts should be written for this! (https://github.com/jhogstrom/aoc-highscores2/issues/32)

# Data design

Data resides in DynamoDB tables.

## boardsconfig
* Partition key: id - string
* Sort key: sk - string

Partition key is typically a board guid.
### Board definition
`BOARDINFO|<aoc_id>`

* Column `name` contains the name of the board

### Session cookie
`SESSION|<cookie>`

* Column `added` cotains the date the cookie was added in the format YYYY-MM-DD

### Year
`YEAR|<year>`

* No additional columns

### Excluded player
`EXCLUDEPLAYER|<year>|<aocid>`

### Name remapping
`NAMEMAP|<playerid>`

* Column `to_name` contains the replacement string


# What remains to do

There are some issues listed on github. In addition to that, there are some major features that we've thought about, including
* Admin API + UI
* UI for creating new boards
* Add websocket support to communicate to clients that a board has been regenerated
