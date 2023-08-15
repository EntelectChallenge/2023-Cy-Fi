import com.microsoft.signalr.HubConnectionBuilder
import com.microsoft.signalr.HubConnectionState
import models.dtos.BotStateDto
import services.BotService
import java.util.*
import java.util.logging.Logger

fun main() {
    // Try adding program arguments via Run/Debug configuration.
    // Learn more about running applications: https://www.jetbrains.com/help/idea/running-applications.html.

    val logger = Logger.getLogger("KotlinBot Logger")
    val botService = BotService()

    val environmentIp = System.getenv("RUNNER_IPV4")
    val environmentNickname = System.getenv("BOT_NICKNAME")
    var token = System.getenv("Token")
    token = token ?: System.getenv("REGISTRATION_TOKEN")

    var ip = if (environmentIp != null && environmentIp.isNotBlank()) environmentIp else "localhost"
    ip = if (ip.startsWith("http://")) ip else "http://$ip"

    val url = "$ip:5000/runnerhub"
    // create the connection
    val hubConnection = HubConnectionBuilder.create(url)
        .build()

    val nickname = environmentNickname ?: "KotlinBot"

    hubConnection.on("Disconnect", { reason: UUID? ->
        logger.info("Disconnected: $reason")
        botService.shouldQuit = true
        hubConnection.stop()
    }, UUID::class.java)

    hubConnection.on("Registered", { id ->
        println("Registered with the runner, bot ID is: $id")
        botService.botId = id
    }, UUID::class.java)

    hubConnection.on("ReceiveBotState", { botStateDto ->
        botService.receivedBotState = true
        botService.botState = botStateDto
    }, BotStateDto::class.java)

    hubConnection.start().blockingAwait()

    Thread.sleep(1000)
    println("Registering with the runner...")
    hubConnection.send("Register", token, nickname)

    hubConnection.on("ReceiveGameComplete", { state ->
        println("Game complete")
        botService.shouldQuit = true
    }, String::class.java)


    // This is a blocking call
//        hubConnection.start().subscribe(()-> {
//
//
//        });

    while (!botService.shouldQuit) {
        Thread.sleep(20)
        if (botService.receivedBotState) {
            System.out.println(botService.botState.toString())
            val botCommand = botService.computeNextPlayerAction()
            if (hubConnection.connectionState == HubConnectionState.CONNECTED && botCommand != null) {
                hubConnection.send("SendPlayerCommand", botCommand)
            }
            botService.receivedBotState = false
        }
    }

    hubConnection.stop()
}