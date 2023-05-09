package Models.Dtos;

public class BotStateDto {
    private int x;
    private int y;
    private String connectionId;
    private String elapsedTime;
    private int collected;
    private int[][] heroWindow;
    private int currentLevel;

    public BotStateDto(int x, int y, String connectionId, String elapsedTime, int collected, int[][] heroWindow, int currentLevel) {
        this.x = x;
        this.y = y;
        this.connectionId = connectionId;
        this.elapsedTime = elapsedTime;
        this.collected = collected;
        this.heroWindow = heroWindow;
        this.currentLevel = currentLevel;
    }

    public int getX() {
        return x;
    }

    public void setX(int x) {
        this.x = x;
    }

    public int getY() {
        return y;
    }

    public void setY(int y) {
        this.y = y;
    }

    public String getConnectionId() {
        return connectionId;
    }

    public void setConnectionId(String connectionId) {
        this.connectionId = connectionId;
    }

    public String getElapsedTime() {
        return elapsedTime;
    }

    public void setElapsedTime(String elapsedTime) {
        this.elapsedTime = elapsedTime;
    }

    public int getCollected() {
        return collected;
    }

    public void setCollected(int collected) {
        this.collected = collected;
    }

    public int[][] getHeroWindow() {
        return heroWindow;
    }

    public void setHeroWindow(int[][] heroWindow) {
        this.heroWindow = heroWindow;
    }

    public int getCurrentLevel() {
        return currentLevel;
    }

    public void setCurrentLevel(int currentLevel) {
        this.currentLevel = currentLevel;
    }

    @Override
    public String toString() {
        StringBuilder windowStr = new StringBuilder();
        if (heroWindow != null) {
            for (int y = heroWindow[0].length - 1; y >= 0; y--) {
                for (int x = 0; x < heroWindow.length; x++) {
                    windowStr.append(heroWindow[x][y]);
                }
                windowStr.append("\n");
            }
        }
        return String.format("Position: (%d, %d), Level: %d, Collected: %d\n%s", x, y, currentLevel, collected, windowStr.toString());
    }
}
