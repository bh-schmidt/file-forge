import { Forge } from "../forge/Forge";
import { ForgeEventAction, ForgeStages } from "../types";

export class EventEmitter {
    private listeners: Record<string, ForgeEventAction[]> = {}

    async emit(name: ForgeStages, forge: Forge) {
        if (!this.listeners[name])
            return

        for (const listener of this.listeners[name]) {
            await listener(forge)
        }
    }

    addListener(name: ForgeStages, action: ForgeEventAction) {
        this.listeners[name] ??= []
        this.listeners[name].push(action)

        return () => {
            const index = this.listeners[name].indexOf(action)
            this.listeners[name].splice(index, 1)
        }
    }
}