import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import process from "process";

const runnerIP = process.env.RUNNER_IPV4 ?? "localhost";
const runnerURL = runnerIP.startsWith("http://")
	? `${runnerIP}:5000/runnerhub`
	: `http://${runnerIP}:5000/runnerhub`;

const botNickname = process.env.BOT_NICKNAME ?? "JSBot";
const token = process.env.REGISTRATION_TOKEN ?? createGuid();

function createGuid() {
  return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
    var r = (Math.random() * 16) | 0,
      v = c === "x" ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
}
const state = {
	connected: false,
	botId: "",
	botState: null,
};

/** @type {import('@microsoft/signalr').HubConnection} */
const connection = new HubConnectionBuilder()
	.withUrl(runnerURL)
	.configureLogging(LogLevel.Debug)
	.withAutomaticReconnect({
		keepAliveInterval: 10,
		reconnectInterval: 5,
	})
	.build();

connection.on("Disconnect", (reason) => {
	console.log("Disconnected with reason: ", reason);
});

connection.on("Registered", (botId) => {
	console.log("Bot registered with ID: ", botId);
	state.botId = botId;
});

connection.on("ReceiveBotState", (botState) => {
	console.log("Received bot state: ", botState);
	state.botState = botState;
});

connection.onclose((error) => {
	console.log("Connection closed with error: ", error);
});

(async () => {
	try {
		await connection.start();
		await connection.invoke("Register", token, botNickname);
	} catch (ex) {
		console.error("Error connecting: ", ex);
	}
})();
