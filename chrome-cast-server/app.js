const express = require('express');
const cors = require('cors');
const srt2vtt = require('srt-to-vtt');
const fs = require('fs');
const { address, port } = require('./api/os-info');
const start = require('./api/chrome-cast-api');

const app = express();

app.use(cors());

app.get('/subs/:fileName', (req, res, next) => {
  fs.createReadStream(req.params.fileName)
    .pipe(srt2vtt())
    .pipe(res);
});

app.get('/videos/:fileName', (req, res, next) => {

  fs.createReadStream(req.params.fileName).pipe(res);
});

app.get('/ping', (req, res, next) => {

  start().then(args => {
    res.send(args);
    res.end();
  });
});

app.listen(port, address, () => console.log(`listening on port: ${port} - address: ${address}`));
