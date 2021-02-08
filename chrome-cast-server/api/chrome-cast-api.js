const ChromecastAPI = require('chromecast-api');
const { address, port } = require('./os-info');

function init() {

  const promise = new Promise((resolver, reject) => {

    let chromeCast = null;

    chromeCast = new ChromecastAPI();

    chromeCast.on('device', () => {
      console.log(chromeCast.devices);
      resolver(chromeCast.devices);
    });

    chromeCast.on('status', status => console.log(status));

    chromeCast.on('connected', () => console.log('connected'));

    function find(args) {

      if (!args || !args.host) {
        return "No media provided";
      }

      if (!chromeCast) {
        return 'No device connected';
      }

      chromeCast = chromeCast.devices.find(f => f.host === args.host);

      if (!chromeCast) {
        return 'No device found with this host';
      }
    }

    function playMedia() {

      if (!chromeCast) {
        return 'No device connected';
      }

      const media = {
        url: `http://${address}:${port}/videos/${encodeURI(args.mediaURL)}`,
        subtitles: [{
          url: `http://${address}:${port}/subs/${encodeURI(args.subsUrl)}`,
          language: 'en-US',
          name: 'English'
        }],
        subtitles_style: {
          backgroundColor: '#FFFFFF00', // see http://dev.w3.org/csswg/css-color/#hex-notation
          foregroundColor: '#FFFFFFFF', // see http://dev.w3.org/csswg/css-color/#hex-notation
          edgeType: 'OUTLINE', // can be: "NONE", "OUTLINE", "DROP_SHADOW", "RAISED", "DEPRESSED"
          edgeColor: '#000000FF', // see http://dev.w3.org/csswg/css-color/#hex-notation
          fontScale: 1.3, // transforms into "font-size: " + (fontScale*100) +"%"
          fontStyle: 'BOLD', // can be: "NORMAL", "BOLD", "BOLD_ITALIC", "ITALIC",
          fontFamily: 'Droid Sans',
          fontGenericFamily: 'SANS_SERIF', // can be: "SANS_SERIF", "MONOSPACED_SANS_SERIF", "SERIF", "MONOSPACED_SERIF", "CASUAL", "CURSIVE", "SMALL_CAPITALS",
          //windowColor: '#AA00FFFF', // see http://dev.w3.org/csswg/css-color/#hex-notation
          //windowRoundedCornerRadius: 10, // radius in px
          //windowType: 'ROUNDED_CORNERS' // can be: "NONE", "NORMAL", "ROUNDED_CORNERS"
        }
      };

      chromeCast.play(media, err => {
        return !!err ? err : media;
      });
    }
  });

  return promise;
}

module.exports = init;
