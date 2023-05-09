package Services;

import Enums.*;
import Models.*;
import Models.Dtos.*;

import java.util.*;

public class BotService {

    Boolean shouldQuit = false;
    Boolean receivedBotState = false;
    private BotStateDto botState;
    private UUID botId;

    public BotService() {
    }

    public Boolean getReceivedBotState() {
        return receivedBotState;
    }

    public void setReceivedBotState(Boolean receivedBotState) {
        this.receivedBotState = receivedBotState;
    }

    public Boolean getShouldQuit() {
        return shouldQuit;
    }

    public void setShouldQuit(Boolean shouldQuit) {
        this.shouldQuit = shouldQuit;
    }

    public BotStateDto getBotState() {
        return botState;
    }

    public void setBotState(BotStateDto botState) {
        this.botState = botState;
    }

    public void setBotId(UUID botId) {
        this.botId = botId;
    }
    public UUID getBotId() {
        return botId;
    }
    public BotCommand computeNextPlayerAction() {
        // TODO: Replace this with your bot's logic.
        return new BotCommand(botId, InputCommand.RIGHT);
    }

}
