package runner.modles

data class GameComplete(
    val totalTicks: Int,
    val players: List<PlayerResult>,
    val worldSeeds: List<Int>,
    val winningBot: GameObject
)