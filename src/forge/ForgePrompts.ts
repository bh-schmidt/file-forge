import { PromptObject } from "../types";
import lodash from "lodash";
import { Forge } from "./Forge";
import prompts, { Falsy, PrevCaller, PromptType } from "prompts";
import chalk from "chalk";
import moment from "moment";
import { PromptsHelper } from "../common/PromptsHelper";

export interface ForgePromptsOptions {
    readFromVariables?: boolean
    updateVariables?: boolean
    clearBetweenQuestions?: boolean
    reprintBetweenQuestions?: boolean
    clearBeforeConfirmations?: boolean
    reprintBeforeConfirmations?: boolean
    maxReprintCount?: number
}

export class ForgePrompts {
    _options: ForgePromptsOptions = {
        clearBeforeConfirmations: true,
        clearBetweenQuestions: true,
        maxReprintCount: 10,
        readFromVariables: true,
        reprintBeforeConfirmations: true,
        reprintBetweenQuestions: true,
        updateVariables: true
    }

    constructor(private forge: Forge) { }

    async prompt<T extends string = string>(questions: PromptObject<T> | PromptObject<T>[], answers?: any, updateVariables?: boolean): Promise<prompts.Answers<T>> {
        questions = Array.isArray(questions) ? questions : [questions]
        updateVariables ??= this._options.updateVariables

        answers ??= this._options.readFromVariables ?
            this.forge.variables.getValues() :
            {}

        const newAnswers = await PromptsHelper.prompt<T>(questions, {
            ...this._options,
            answers: answers
        })

        if (updateVariables) {
            this.forge.variables.setValues(newAnswers)
        }

        return newAnswers
    }

    async promptWithConfirmation<T extends string = string>(questions: PromptObject<T> | PromptObject<T>[], answers?: any, updateVariables?: boolean): Promise<prompts.Answers<T>> {
        questions = Array.isArray(questions) ? questions : [questions]
        updateVariables ??= this._options.updateVariables

        answers ??= this._options.readFromVariables ?
            this.forge.variables.getValues() :
            {}

        let newAnswers: any = answers
        const opts = this.forge.program.options()

        if (opts.disablePromptConfirmation || this.forge.variables.get('disablePromptConfirmation')) {
            newAnswers = await PromptsHelper.prompt(questions, {
                ...this._options,
                answers: newAnswers
            })
        } else {
            newAnswers = await PromptsHelper.promptWithConfirmation(questions, {
                ...this._options,
                answers: newAnswers
            })
        }

        if (updateVariables) {
            this.forge.variables.setValues(newAnswers)
        }

        console.log()

        return newAnswers
    }

    reprintAnswers<T extends string = string>(questions: PromptObject<T>[], answers: any) {
        for (const question of questions) {
            const name = this.getValue(question.name, question, answers)
            const type = this.getValue(question.type, question, answers)
            const message = this.getValue(question.message, question, answers)

            const answer = answers[name]

            if (type == 'text' || type == 'number') {
                console.log(`${chalk.green('√')} ${chalk.bold(message)} ${chalk.gray('...')} ${answer}`)
                continue
            }

            if (type == 'toggle') {
                const inactive = this.getValue(question.inactive, question, answers)
                const active = this.getValue(question.active, question, answers)

                const off = inactive ?? 'off'
                const on = active ?? 'on'

                const first = answer == false ? chalk.cyan.underline(off) : off
                const second = answer == true ? chalk.cyan.underline(on) : on

                console.log(`${chalk.green('√')} ${chalk.bold(message)} ${chalk.gray('...')} ${first} ${chalk.gray('/')} ${second}`)
                continue
            }

            if (type == 'confirm') {
                const text = answer ? 'yes' : 'no'
                console.log(`${chalk.green('√')} ${chalk.bold(message)} ${chalk.gray('...')} ${text}`)
                continue
            }

            if (type == 'date') {
                let mask = this.getValue<string, typeof question.mask>(question.mask, question, answers)
                if (!mask) {
                    mask = 'YYYY-MM-DD HH:mm:ss'
                }

                const text = moment(answer)
                    .format(mask)
                console.log(`${chalk.green('√')} ${chalk.bold(message)} ${chalk.gray('...')} ${text}`)
                continue
            }

            if (type == 'invisible') {
                console.log(`${chalk.green('√')} ${chalk.bold(message)} ${chalk.gray('...')}`)
                continue
            }

            if (type == 'password') {
                const text = (answer as string).replaceAll(/./g, '*')
                console.log(`${chalk.green('√')} ${chalk.bold(message)} ${chalk.gray('...')} ${text}`)
                continue
            }

            if (type == 'list') {
                const text = Array.isArray(answer) ?
                    answer.join(', ') :
                    answer

                console.log(`${chalk.green('√')} ${chalk.bold(message)} ${chalk.gray('...')} ${text}`)
                continue
            }

            if (type == 'select') {
                const choices = this.getValue<prompts.Choice[], typeof question.choices>(question.choices, question, answers)!
                if (!choices) {
                    console.log(`${chalk.green('√')} ${chalk.bold(message)} ${chalk.gray('...')} ${answer}`)
                    continue
                }

                let choice = choices.find(e => e.value == answer)
                if (!choice) {
                    if (typeof answer !== 'number') {
                        console.log(`${chalk.green('√')} ${chalk.bold(message)} ${chalk.gray('...')} ${answer}`)
                        continue
                    }

                    choice = choices[answer]
                }

                console.log(`${chalk.green('√')} ${chalk.bold(message)} ${chalk.gray('...')} ${choice.title}`)
                continue
            }

            if (type == 'autocomplete') {
                const choices = this.getValue<prompts.Choice[], typeof question.choices>(question.choices, question, answers)!
                if (!choices) {
                    console.log(`${chalk.green('√')} ${chalk.bold(message)} ${chalk.gray('...')} ${answer}`)
                    continue
                }

                let choice =
                    choices.find(e => e.value == answer) ??
                    choices.find(e => e.title == answer)

                if (!choice) {
                    console.log(`${chalk.green('√')} ${chalk.bold(message)} ${chalk.gray('...')} ${answer}`)
                    continue
                }

                console.log(`${chalk.green('√')} ${chalk.bold(message)} ${chalk.gray('...')} ${choice.title}`)
                continue
            }

            if (type == 'multiselect' || type == 'autocompleteMultiselect') {
                if (!Array.isArray(answer)) {
                    console.log(`${chalk.green('√')} ${chalk.bold(message)} ${chalk.gray('...')} ${answer}`)
                    continue
                }

                const choices = this.getValue<prompts.Choice[], typeof question.choices>(question.choices, question, answers)!
                if (!choices) {
                    console.log(`${chalk.green('√')} ${chalk.bold(message)} ${chalk.gray('...')} ${answer}`)
                    continue
                }

                const temp = answer.map(a => {
                    let choice = choices.find(e => e.value == answer)
                    if (choice) {
                        return choice.title
                    }

                    if (typeof a == 'number') {
                        return choices[a]
                    }

                    return a
                })

                const text = temp.join(', ')
                console.log(`${chalk.green('√')} ${chalk.bold(message)} ${chalk.gray('...')} ${text}`)
                continue
            }
        }
    }

    private recreate<T extends string>(question: prompts.PromptObject<T>, values: any) {
        const recreated: PromptObject<T> = {} as any

        if (question.name) {
            recreated.name = this.getValue<T, typeof question.name>(question.name, question, values)!
        }

        if (question.type) {
            recreated.type = this.getValue(question.type, question, values)
        }

        if (question.active) {
            recreated.active = this.getValue(question.active, question, values)
        }

        if (question.choices) {
            recreated.choices = this.getValue(question.choices, question, values)
        }

        if (question.float) {
            recreated.float = this.getValue(question.float, question, values)
        }

        if (question.hint) {
            recreated.hint = this.getValue(question.hint, question, values)
        }

        if (question.inactive) {
            recreated.inactive = this.getValue(question.inactive, question, values)
        }

        if (question.increment) {
            recreated.increment = this.getValue(question.increment, question, values)
        }

        if (question.initial) {
            recreated.initial = this.getValue(question.initial, question, values)
        }

        if (question.limit) {
            recreated.limit = this.getValue(question.limit, question, values)
        }

        if (question.mask) {
            recreated.mask = this.getValue(question.mask, question, values)
        }

        if (question.max) {
            recreated.max = this.getValue(question.max, question, values)
        }

        if (question.message) {
            recreated.message = this.getValue(question.message, question, values)
        }

        if (question.min) {
            recreated.min = this.getValue(question.min, question, values)
        }

        if (question.round) {
            recreated.round = this.getValue(question.round, question, values)
        }

        if (question.separator) {
            recreated.separator = this.getValue(question.separator, question, values)
        }

        if (question.style) {
            recreated.style = this.getValue(question.style, question, values)
        }

        if (question.warn) {
            recreated.warn = this.getValue(question.warn, question, values)
        }

        if (question.format) {
            recreated.format = (value) => {
                return question.format!(value, values, question)
            }
        }

        if (question.onState) {
            recreated.onState = (value) => {
                return question.onState!(value, values, question)
            }
        }

        if (question.validate) {
            recreated.validate = (value) => {
                return question.validate!(value, values, question)
            }
        }

        if (question.instructions) {
            recreated.instructions = question.instructions
        }

        if (question.onRender) {
            recreated.onRender = question.onRender
        }

        if (question.suggest) {
            recreated.suggest = question.suggest
        }

        if (question.stdin) {
            recreated.stdin = question.stdin
        }

        if (question.stdout) {
            recreated.stdout = question.stdout
        }

        return recreated
    }

    private getValue<TMain, TOthers>(valueOrFunc: TMain | TOthers, question: PromptObject, values: any) {
        if (!valueOrFunc)
            return undefined

        if (typeof valueOrFunc == 'function') {
            const func = valueOrFunc as Function
            return func(undefined, values, question) as TMain
        }

        return valueOrFunc as TMain
    }
}