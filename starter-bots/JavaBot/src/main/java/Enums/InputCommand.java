package Enums;

import java.util.Arrays;
import java.util.Optional;

public enum InputCommand {
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

    private final int value;

    private InputCommand(int value) {
        this.value = value;
    }

    public static Optional<InputCommand> valueOf(int value) {
        return Arrays.stream(InputCommand.values()).filter(inputCommand -> inputCommand.value == value).findFirst();
    }

    public int getValue() {
        return this.value;
    }

    @Override
    public String toString() {
        return this.name() + "(" + value + ")";
    }
}
