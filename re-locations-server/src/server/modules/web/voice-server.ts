import { Service } from '../core';
import * as dgram from 'dgram';
import { AddressInfo } from 'net';

class VoiceClient {
    public ip: string;
    public port: number;
    public userId: number;
    public lastHeartbeat: number;
    public frameSize: number; // How many samples are sent at one time
    public frameSizeMillis: number;
    public recordingStartDate: Date;
    public recordingData: number[];
}

export class VoiceServer extends Service {

    public get serviceName(): string { return 'VoiceServer'; }
    public get groupName(): string { return 'web'; }

    private udpSocket: dgram.Socket;
    private clients: Map<string, VoiceClient> = new Map<string, VoiceClient>();
    private disconnectTimeoutMillis = 2000;
    private samplingRate = 48000; // Default Unity sampling rate
    private channels = 1; // Microphone data is normally mono
    private recordingVoiceData = false;

    public constructor(private voiceRecordingPath: string) {
        super();
    }

    public start(voicePort: number,) {
        this.udpSocket = dgram.createSocket('udp4');
        this.udpSocket.on('listening', () => {
            const address = this.udpSocket.address() as AddressInfo;
            console.log(`Voice server listening on ${address.address}:${address.port}`);
            this.logInfo(`Voice server listening on ${address.address}:${address.port}`);
            if (this.recordingVoiceData) {
                this.logWarning('Warning: Voice recording is enabled');
            }
        });
        this.udpSocket.on('message', (message, remote) => {
            const now = new Date();
            const nowMillis = now.getTime();

            // First 2 bytes contains the user id
            const userId = message.readIntLE(0, 2); // .net (Unity) decodes default in little-endian order

            // Current voice client
            let voiceClient: VoiceClient;

            // Add to clients if new client
            if (!this.clients.has(remote.address)) {
                voiceClient = new VoiceClient();
                voiceClient.ip = remote.address;
                voiceClient.port = remote.port;
                voiceClient.userId = userId;
                voiceClient.lastHeartbeat = nowMillis;
                voiceClient.frameSize = (message.length - 2) / 4; // First 2 bytes userId, every float needs 4 bytes
                voiceClient.frameSizeMillis = voiceClient.frameSize / this.samplingRate * 1000;
                voiceClient.recordingStartDate = now;
                voiceClient.recordingData = [];
                this.clients.set(remote.address, voiceClient);
                this.logDebug(`New voice client connected from ${remote.address}:${remote.port} ID: ${userId}`);
            } else {
                voiceClient = this.clients.get(remote.address);
            }

            // Update heartbeat time
            this.clients.get(remote.address).lastHeartbeat = nowMillis;

            // Send message to all other clients
            this.clients.forEach((value, key, map) => {
                if (key !== remote.address) {
                    this.udpSocket.send(message, 0, message.length, value.port, value.ip, (err, bytes) => {
                        if (err) {
                            throw err;
                        }
                    });
                }
            });

            if (this.recordingVoiceData) {
                // Fill lost recording data with silent sound
                const recordingTime = nowMillis - voiceClient.recordingStartDate.getTime();
                const recordedDataTime = voiceClient.recordingData.length / (this.samplingRate / 1000);
                const recordedDateTimeDifference = recordingTime - recordedDataTime;
                if (recordedDateTimeDifference > voiceClient.frameSizeMillis / 2) {
                    const countSkippedFrames: number = Math.floor((recordedDateTimeDifference + (voiceClient.frameSizeMillis / 2)) / voiceClient.frameSizeMillis);
                    const silentSoundSamples: number[] = Array(voiceClient.frameSize * countSkippedFrames).fill(0.0);
                    Array.prototype.push.apply(voiceClient.recordingData, silentSoundSamples); // around 945x faster than .concat
                }
                for (let i = 2; i <= message.length - 4; i += 4) {
                    voiceClient.recordingData.push(message.readFloatLE(i)); // .net (Unity) decodes default in little-endian order
                }
            }
        });
        this.udpSocket.on('error', (exception) => {
            console.error(exception.message);
        });
        this.udpSocket.bind(voicePort);

        // Check if clients disconnected every second
        setInterval(this.checkClientsDisconnected, 1000, this);
    }

    private checkClientsDisconnected(context: VoiceServer) {
        const now = Date.now();
        context.clients.forEach((value, key, map) => {
            // Remove inactive clients
            if (now - value.lastHeartbeat > context.disconnectTimeoutMillis) {

                // Check if recording data is available
                if (value.recordingData.length > 0) {

                    // Create wave file from recording data
                    const WaveFile = require('wavefile');
                    const wav = new WaveFile();
                    wav.fromScratch(context.channels, context.samplingRate, '32f', value.recordingData); // 32f means 32 bit floating data ranging from -1 to 1

                    // Save wave file
                    const fs = require('fs');
                    const dateString = context.filenameTimestamp(value.recordingStartDate);
                    fs.writeFileSync(`${this.voiceRecordingPath} rec_${dateString}_ID_${value.userId}.wav`, wav.toBuffer());
                }

                // Delete client
                context.clients.delete(key);
                context.logDebug(`Voice client ${value.ip} disconnected ID: ${value.userId}`);
            }
        });
    }


    private filenameTimestamp(date: Date): string {
        // Create an array with the date parts
        const dateArray = [date.getFullYear() + '', date.getMonth() + 1, date.getDate(), date.getHours(), date.getMinutes(), date.getSeconds()];

        // Add leading zeros
        for (let i = 1; i < 6; i++) {
            if (dateArray[i] < 10) {
                dateArray[i] = '0' + date[i];
            }
        }

        return dateArray.join('_');
    }

}
