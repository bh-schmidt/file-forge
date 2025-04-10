import { PromptObject } from "../types";
import lodash from "lodash";
import { Forge } from "./Forge";
import prompts, { Falsy, PrevCaller, PromptType } from "prompts";
import chalk from "chalk";
import moment from "moment";

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

        const [finished, pending] = lodash.partition(questions, question => {
            const name = this.getValue<string, typeof question.name>(question.name, question, answers)!
            return name in answers
        })


        if (this._options.clearBetweenQuestions) {
            console.clear()
        }

        if (this._options.reprintBetweenQuestions) {
            const reprint = lodash.takeRight(finished, this._options.maxReprintCount)
            this.reprintAnswers(reprint, answers)
        }

        const newAnswers = answers as any

        while (pending.length > 0) {
            const question = pending.shift()!
            const name = this.getValue<string, typeof question.name>(question.name, question, newAnswers)!

            const recreated: PromptObject<T> = {
                name: name as any,
                type: this.getValue(question.type, question, newAnswers),
                active: this.getValue(question.active, question, newAnswers),
                choices: this.getValue(question.choices, question, newAnswers),
                float: this.getValue(question.float, question, newAnswers),
                hint: this.getValue(question.hint, question, newAnswers),
                inactive: this.getValue(question.inactive, question, newAnswers),
                increment: this.getValue(question.increment, question, newAnswers),
                initial: this.getValue(question.initial, question, newAnswers),
                limit: this.getValue(question.limit, question, newAnswers),
                mask: this.getValue(question.mask, question, newAnswers),
                max: this.getValue(question.max, question, newAnswers),
                message: this.getValue(question.message, question, newAnswers),
                min: this.getValue(question.min, question, newAnswers),
                round: this.getValue(question.round, question, newAnswers),
                separator: this.getValue(question.separator, question, newAnswers),
                style: this.getValue(question.style, question, newAnswers),
                warn: this.getValue(question.warn, question, newAnswers),
                format: (value) => {
                    if (question.format) {
                        return question.format(value, newAnswers, question)
                    }

                    return value
                },
                onState: (value) => {
                    if (question.onState) {
                        return question.onState(value, newAnswers, question)
                    }
                },
                validate: (value) => {
                    if (question.validate) {
                        return question.validate(value, newAnswers, question)
                    }

                    return true
                },
                instructions: question.instructions,
                onRender: question.onRender,
                suggest: question.suggest,
                stdin: question.stdin,
                stdout: question.stdout,
            }

            const result = await prompts<T>(recreated) as any

            if (!(name in result)) {
                process.exit()
            }

            newAnswers[name] = result[name]
            finished.push(question)
        }

        if (updateVariables) {
            this.forge.variables.setValues(newAnswers)
        }

        return newAnswers
    }

    async promptWithConfirmation<T extends string = string>(questions: PromptObject<T> | PromptObject<T>[], answers?: any, updateVariables?: boolean): Promise<prompts.Answers<T>> {
        questions = Array.isArray(questions) ? questions : [questions]
        updateVariables ??= this._options.updateVariables

        let newAnswers: any = answers

        while (true) {
            newAnswers = await this.prompt<T>(questions, newAnswers, false)
            if (this._options.clearBeforeConfirmations) {
                console.clear()
            }

            if (this._options.reprintBeforeConfirmations) {
                this.reprintAnswers(questions, newAnswers)
            }

            const editResult = await prompts([
                {
                    name: 'edit',
                    type: 'toggle',
                    message: 'Do you want to edit something?',
                    active: 'yes',
                    inactive: 'no',
                    initial: false,
                }
            ])

            if (!editResult.edit) {
                break
            }

            const fieldsResult = await prompts([
                {
                    type: 'multiselect',
                    name: 'fields',
                    message: 'Select the fields to edit:',
                    choices: questions.map(question => {
                        const message = this.getValue(question.message, question, newAnswers)
                        return {
                            title: message as string
                        }
                    })
                }
            ])

            for (const index of fieldsResult.fields) {
                const question = questions[index]
                const name = this.getValue(question.name, question, newAnswers)
                delete newAnswers[name]
            }
        }

        if (updateVariables) {
            this.forge.variables.setValues(newAnswers)
        }

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