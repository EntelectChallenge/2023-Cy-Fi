package models.dtos

class BotStateDto(var x: Int, var y: Int, var connectionId: String, var elapsedTime: String, var collected: Int, var heroWindow: Array<IntArray>?, var currentLevel: Int) {

    override fun toString(): String {
        val windowStr = StringBuilder()
        if (heroWindow != null) {
            for (y in heroWindow!![0].indices.reversed()) {
                for (x in heroWindow!!.indices) {
                    windowStr.append(heroWindow!![x][y])
                }
                windowStr.append("\n")
            }
        }
        return String.format("Position: (%d, %d), Level: %d, Collected: %d\n%s", x, y, currentLevel, collected, windowStr.toString())
    }
}
