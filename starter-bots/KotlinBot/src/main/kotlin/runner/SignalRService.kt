package runner

import com.microsoft.signalr.HubConnection
import com.microsoft.signalr.HubConnectionBuilder
import runner.Bot.modles.BotCommand
import runner.Bot.modles.BotStateDTO
import runner.modles.InputCommand
import java.util.*
import java.util.concurrent.TimeUnit

object SignalRService {

    private lateinit var id: UUID
    private lateinit var hubConnection: HubConnection

    fun connect() {
        /**
        === Create connection with the RunnerHub ===
         */
        hubConnection = HubConnectionBuilder.create(Config.url).build()
        var shouldQuit = false

        /**
        === Receive Registered response - on successful Registration ===
         */
        hubConnection.on(
            "Registered",
            { clientId: UUID ->
                println("Registered with the runner $clientId.")
                id = clientId
            }, UUID::class.java
        )

        /**
        === Receive Disconnect command from runnerHub ===
         */
        hubConnection.on("Disconnect", {
            println("Disconnected.")
            shouldQuit = true
        }, UUID::class.java)

        /**
        === Receive the state of the bot ===
         */
        hubConnection.on("ReceiveBotState", { it: BotStateDTO ->

            println("Bot State DTO")
            println("currentLevel :     ${it.currentLevel}")
            println("connectionId :     ${it.connectionId}")
            println("collected:         ${it.collected}")
            println("elapsedTime:       ${it.elapsedTime}")
            println("hero Window:")
            it.heroWindow.forEach { println(it) }

        }, BotStateDTO::class.java)

        hubConnection.start().blockingAwait()
        println("Connection established with runner.")

        hubConnection.send("Register", Config.BOT_NICKNAME)

        do {
            val botCommand = BotCommand(id.toString(), InputCommand.RIGHT.type)

            if (botCommand != null) {
                hubConnection.send("SendPlayerCommand", botCommand)
            }
        } while (!shouldQuit)

        hubConnection.stop().blockingAwait(10, TimeUnit.SECONDS)
        println("Connection closed: ${hubConnection.connectionState}")
    }

    object Config {
        /** TODO: Change the nickname of your bot here. */
        const val BOT_NICKNAME = "KotlinBot"
        private const val ENVIRONMENT = "RUNNER_IPV4"
        private const val HOSTNAME = "localhost"
        val url = getRunnerUrl()
        private fun getRunnerUrl(): String {
            var ip = System.getenv(ENVIRONMENT)
            if (ip == null || ip.isBlank()) {
                ip = HOSTNAME
            }
            if (!ip.startsWith("http://")) {
                ip = "http://$ip:5000"
            }
            return "$ip/runnerhub"
        }
    }
}


        
        
