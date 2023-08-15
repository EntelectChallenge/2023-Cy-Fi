package services

import enums.InputCommand
import models.BotCommand
import models.dtos.BotStateDto
import java.util.*

class BotService {
    var shouldQuit = false
    var receivedBotState = false
    var botState: BotStateDto? = null
    var botId: UUID? = null
    fun computeNextPlayerAction(): BotCommand {
        // TODO: Replace this with your bot's logic.
        return BotCommand(botId!!, InputCommand.RIGHT)
    }
}
