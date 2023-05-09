import { InputCommand } from "./input-command";

export interface BotAction {
	botId: string;
	action: InputCommand;
}
