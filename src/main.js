/* Required modules */
const http = require("http");
const path = require("path");
const b = require("bonescript");

/* Heat Pins */
const HEAT = "P9_12";
const TMP35 = "P9_40";

/* Networking Variables */
const HOST = "192.168.137.50";
const PORT = 1338;
let temperature;
let tempf;
b.pinMode(HEAT, b.OUTPUT);

/* Variables set by RESTful API */
var forceHeat = false; // DO NOT CHANGE
var temp = "-1"; // DO NOT CHANGE
var status = "-1"; // DO NOT CHANGE

/* Creates a web-server and facilitates API calls */
const server = http.createServer((req, res) => {
   if (req.url === path.normalize("/")) {
      /* Not authorized to view the root resource. */
      res.writeHead(403, { "Content-Type": "text/plain" });
      res.end("You do not have permission to view this resource.");
   } else if (req.url === path.normalize("/api/temp")) {
      const volt = b.analogRead(TMP35) * 1.8;
      temperature = 100 * volt - 50;
      tempf = ((temperature * 9) / 5 + 32).toPrecision(3);

      /**** Return the API call result. ****/
      console.log(
         "Displaying to client: tempID:sensor_X0,status:" +
            status +
            ",temp:" +
            tempf +
            ",units:F"
      );
      res.end(
         "tempID:sensor_5,status:" + status + ",temp:" + tempf + ",units:F"
      );
   } else if (req.url === path.normalize("/api/heat/off")) {
      /* Turn off force heat */
      forceHeat = false;
      status = "OFF";

      /**** BEGIN Write code to turn heat source OFF ****/
      b.digitalWrite(HEAT, b.LOW);
      //Write code here.

      /**** END Write code to turn heat source OFF ****/
      console.log(
         "/api/heat/off:Resource State: temp: " + tempf + ", status: " + status
      );
      res.end("FORCE HEAT OFF OK");
   } else if (req.url === path.normalize("/api/heat/on")) {
      /* Turn on force heat */
      forceHeat = true;
      status = "ON";

      /**** BEGIN Write code to turn heat source ON ****/
      b.digitalWrite(HEAT, b.HIGH);
      // Write code here.

      /**** END Write code to turn heat source ON ****/
      console.log(
         "/api/heat/on:Resource State: temp: " + tempf + ", status: " + status
      );
      res.end("FORCE HEAT ON OK");
   } else {
      /* Page not found */
      res.writeHead(404, { "Content-Type": "text/plain" });
      res.end("this page doesn't exist");
   }
});

/* Start the http server. Non-blocking. */
server.listen(PORT, HOST);
