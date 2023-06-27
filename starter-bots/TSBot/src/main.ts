import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import process from "process";

const runnerIP = process.env.RUNNER_IPV4 ?? "localhost";
const runnerURL = runnerIP.startsWith("http://")
	? `${runnerIP}:5000/runnerhub`
	: `http://${runnerIP}:5000/runnerhub`;

const botNickname = process.env.BOT_NICKNAME ?? "TSBot";

const token = process.env.Token ?? process.env.REGISTRATION_TOKEN;

const state = {
	connected: false,
	botId: "",
	botState: null,
};

const connection = new HubConnectionBuilder()
	.withUrl(runnerURL)
	.configureLogging(LogLevel.Debug)
	.withAutomaticReconnect()
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
