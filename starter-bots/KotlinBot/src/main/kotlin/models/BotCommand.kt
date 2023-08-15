package models

import enums.InputCommand
import java.util.*

class BotCommand(var botId: UUID, var action: InputCommand) {

    override fun toString(): String {
        return "BotCommand{" +
                "botId=" + botId +
                ", action=" + action +
                '}'
    }
}
