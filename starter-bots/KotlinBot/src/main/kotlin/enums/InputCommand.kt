package enums

import java.util.*

enum class InputCommand(val value: Int) {
    UP(1),
    DOWN(2),
    LEFT(3),
    RIGHT(4),
    UPLEFT(5),
    UPRIGHT(6),
    DOWNLEFT(7),
    DOWNRIGHT(8),
    DIGDOWN(9),
    DIGLEFT(10),
    DIGRIGHT(11);

    override fun toString(): String {
        return "$name($value)"
    }

    companion object {
        fun valueOf(value: Int): Optional<InputCommand> {
            return Arrays.stream(values()).filter { inputCommand: InputCommand -> inputCommand.value == value }
                .findFirst()
        }
    }
}
