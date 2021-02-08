const os = require('os');
const hostName = os.hostname();
const ifaces = os.networkInterfaces();

const address = ifaces['Wi-Fi 2'].find(i => i.family === 'IPv4').address;

module.exports = {
  address,
  port: 8080
};
