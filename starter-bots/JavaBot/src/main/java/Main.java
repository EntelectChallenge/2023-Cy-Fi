
import Models.*;
import Models.Dtos.*;
import Services.*;
import com.microsoft.signalr.*;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;


import java.util.*;

public class Main {

    public static void main(String[] args) throws Exception {
        Logger logger = LoggerFactory.getLogger(Main.class);
        BotService botService = new BotService();

        String environmentIp = System.getenv("RUNNER_IPV4");
        String environmentNickname = System.getenv("BOT_NICKNAME");
        String token = System.getenv("Token");
        token = (token != null) ? token : System.getenv("REGISTRATION_TOKEN");

        String ip = (environmentIp != null && !environmentIp.isBlank()) ? environmentIp : "localhost";
        ip = ip.startsWith("http://") ? ip : "http://" + ip;

        String url = ip + ":" + "5000" + "/runnerhub";
        // create the connection
        HubConnection hubConnection = HubConnectionBuilder.create(url)
                .build();

        String nickname = environmentNickname != null ? environmentNickname : "JavaBot";

        hubConnection.on("Disconnect", (reason) -> {
            logger.info("Disconnected: {}",
                    reason);
            botService.setShouldQuit(true);
            hubConnection.stop();
        }, UUID.class);

        hubConnection.on("Registered", (id) -> {
            System.out.println("Registered with the runner, bot ID is: " + id);
            botService.setBotId(id);
        }, UUID.class);

        hubConnection.on("ReceiveBotState", (botStateDto) -> {
            botService.setReceivedBotState(true);
            botService.setBotState(botStateDto);
        }, BotStateDto.class);


        hubConnection.start().blockingAwait();

        Thread.sleep(1000);
        System.out.println("Registering with the runner...");
        hubConnection.send("Register", token, nickname);

        hubConnection.on("ReceiveGameComplete", (state) -> {
            System.out.println("Game complete");
            botService.setShouldQuit(true);
        }, String.class);


        // This is a blocking call
//        hubConnection.start().subscribe(()-> {
//
//
//        });

        while (!botService.getShouldQuit()) {
            Thread.sleep(20);

            if (botService.getReceivedBotState()) {
                System.out.println(botService.getBotState().toString());
                BotCommand botCommand = botService.computeNextPlayerAction();

                if (hubConnection.getConnectionState() == HubConnectionState.CONNECTED && botCommand != null) {
                    hubConnection.send("SendPlayerCommand", botCommand);
                }
                botService.setReceivedBotState(false);
            }
        }

        hubConnection.stop();

    }
}
