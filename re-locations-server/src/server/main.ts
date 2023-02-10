import * as _ from 'lodash';
import * as arts from './modules';
import { Config } from './configuration';

// Better TypeScript error messages
require('source-map-support').install();

/**
 * Debugging
 */

// Unlimited stacktrace depth (due to RxJS)
Error.stackTraceLimit = Infinity;

// Print console errors in GUI
// const redirectConsole = new arts.RedirectConsole();


const dataStore = new arts.DataStore();




/**
 *    Servers
 */
const webServer = new arts.WebServer(Config.WEBSERVER_PORT, Config.WEBSERVER_ROOT);
const unityServer = new arts.UnityServerProxy();
const socketioServer = new arts.SocketIOServer();
const voiceServer = new arts.VoiceServer(Config.DATA_ROOT);


/**
 *    APIs
 */
const restApi = new arts.RestAPI(Config.DATA_ROOT, webServer);

/**
 *    Plumbing
 */
const messageDistributor = new arts.MessageDistributor(unityServer, socketioServer, dataStore);
const unityLog = new arts.UnityClientLogger(unityServer);
const webLog = new arts.WebLog(socketioServer);

/**
 *    Startup
 */

async function startup() {
    for (const service of arts.Service.Current) {
        await service.init();
    }

    const httpServer = webServer.start();
    socketioServer.start(httpServer);
    unityServer.start(Config.UNITY_PORT);
    voiceServer.start(Config.VOICE_PORT);
}

startup();
