package runner.Bot.modles

data class BotStateDTO(
    var currentLevel: Int,
    var connectionId: String,
    var collected: Int,
    var elapsedTime: String,
    var heroWindow: List<List<Int>>,
) 
