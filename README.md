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
* session cookie
* board id
* name of board

to jonas.hogstrom at tobii.com.
# Contribute
(tbd)

# Set up your own frontend and backend
(tbd)

# Data design
(tbd)

# What remains to do

There are some issues listed on github. In addition to that, there are some major features that we've thought about, including
* Admin API + UI
* UI for creating new boards
* Add websocket support to communicate to clients that a board has been regenerated