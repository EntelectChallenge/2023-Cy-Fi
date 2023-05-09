package Models;

import Enums.InputCommand;

import java.util.UUID;

public class BotCommand {
    private UUID botId;
    private InputCommand action;

    public BotCommand(UUID botId, InputCommand action) {
        this.botId = botId;
        this.action = action;
    }

    public UUID getBotId() {
        return botId;
    }

    public void setBotId(UUID botId) {
        this.botId = botId;
    }

    public InputCommand getAction() {
        return action;
    }

    public void setAction(InputCommand action) {
        this.action = action;
    }

    @Override
    public String toString() {
        return "BotCommand{" +
                "botId=" + botId +
                ", action=" + action +
                '}';
    }
}
