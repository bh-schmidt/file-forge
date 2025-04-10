import { Command, OptionValues } from "commander";
import { execa, Options, ResultPromise } from "execa";
import { ConfigureCommand, ValidateCommands, ValidateOptions, Writable } from "../types";
import { Forge } from "./Forge";

export class ForgeProgram {
    _commander: Command = null!
    _configureCommands: ConfigureCommand = () => { }
    _validateCommands: ValidateCommands = () => true
    _validateOptions: ValidateOptions = () => true

    constructor(private forge: Forge) { }

    options<T extends OptionValues = OptionValues>() {
        return this._commander.opts<T>()
    }

    async run<TOptions extends Options = Options>(command: string): Promise<ResultPromise<{} & TOptions>>
    async run<TOptions extends Options = Options>(command: string, options?: TOptions): Promise<ResultPromise<{} & TOptions>>
    async run<TOptions extends Options = Options>(command: string, parameters?: string[], options?: TOptions): Promise<ResultPromise<{} & TOptions>>

    async run<TOptions extends Writable<Options> = Options>(command: string, paramsOrOptions?: string[] | TOptions, options?: TOptions): Promise<ResultPromise<{} & TOptions>> {
        if (Array.isArray(paramsOrOptions)) {
            options ??= {} as TOptions
            options!.cwd ??= this.forge.paths.targetPath()

            return await execa<TOptions>(command, paramsOrOptions, options);
        } else {
            paramsOrOptions ??= {} as TOptions
            paramsOrOptions!.cwd ??= this.forge.paths.targetPath()

            return await execa<TOptions>(command, paramsOrOptions);
        }
    }
}