import { Forge } from "./Forge"

export interface ForgeVariablesOptions {
    mergeProgramOptions?: boolean
    mergeConfigValues?: boolean
}

export class ForgeVariables {
    private variables: Record<string, any>

    _options: ForgeVariablesOptions = {
        mergeProgramOptions: true,
        mergeConfigValues: true
    }

    constructor(private forge: Forge) {
        this.variables = {}
    }

    private getVariables() {
        const project = this._options.mergeConfigValues ?
            this.forge.config.getValues('project') :
            undefined

        const local = this._options.mergeConfigValues ?
            this.forge.config.getValues() :
            undefined

        const options = this._options.mergeProgramOptions ?
            this.forge.program._command.opts() :
            undefined

        return {
            ...project,
            ...local,
            ...options,
            ...this.variables
        }
    }

    get<T = any>(varName: string): T | undefined {
        return structuredClone(this.getVariables()[varName])
    }

    getValues<T = any>() {
        return structuredClone(this.getVariables()) as T
    }

    getProxy<T = any>() {
        return this.variables as T
    }

    set(varName: string, value: any) {
        this.variables[varName] = structuredClone(value)
    }

    setValues(obj: Record<string, any>) {
        const entries = Object.entries(obj)

        for (const [key, value] of entries) {
            this.variables[key] = structuredClone(value)
        }
    }
}