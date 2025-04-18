import chalk from "chalk"
import lodash from 'lodash'
import moment from "moment"
import prompts from "prompts"
import { PromptObject } from "../types"

const defaultOptions: PromptsHelper.PromptOptions = {
    clearBeforeConfirmations: true,
    clearBetweenQuestions: true,
    maxReprintCount: 10,
    reprintBeforeConfirmations: true,
    reprintBetweenQuestions: true,
}

export namespace PromptsHelper {
    export interface PromptOptions {
        answers?: any
        clearBetweenQuestions?: boolean
        reprintBetweenQuestions?: boolean
        clearBeforeConfirmations?: boolean
        reprintBeforeConfirmations?: boolean
        maxReprintCount?: number
    }

    async function promptInternal<T extends string = string>(questions: PromptObject<T> | PromptObject<T>[], options: PromptOptions = defaultOptions): Promise<prompts.Answers<T>> {
        options = {
            ...defaultOptions,
            ...options
        }

        questions = Array.isArray(questions) ? questions : [questions]
        const answers = options.answers ?? {}

        const [finished, pending] = lodash.partition(questions, question => {
            const name = getValue<string, typeof question.name>(question.name, question, answers)!
            return name in answers
        })

        const newAnswers = answers as any
        let error: string | undefined

        while (pending.length > 0) {
            const question = pending.shift()!
            const name = getValue<string, typeof question.name>(question.name, question, newAnswers)!


            if (options.clearBetweenQuestions) {
                console.clear()
            }

            if (options.reprintBetweenQuestions) {
                const reprint = lodash.takeRight(finished, options.maxReprintCount)
                reprintAnswers(reprint, answers)
            }

            const recreated = recreate(question, newAnswers)

            if (error) {
                console.log(chalk.red(error))
            }
            const result = await prompts<T>(recreated) as any

            if (recreated.type !== false && !(name in result)) {
                process.exit()
            }

            // prompts has a bug on windows that stops reading stdin when the question.validate is async, so to 
            if (process.platform === 'win32' && question.validate) {
                const valid = await question.validate(result[name], newAnswers, question)

                if (valid === false) {
                    error = 'Invalid answer'
                    pending.unshift(question)
                    continue
                }

                if (valid !== true) {
                    error = valid
                    pending.unshift(question)
                    continue
                }

                error = undefined
            }

            newAnswers[name] = result[name]
            finished.push(question)
        }

        console.log()

        return newAnswers
    }

    export function prompt<T extends string = string>(questions: PromptObject<T> | PromptObject<T>[], options: PromptOptions = defaultOptions) {
        return promptInternal<T>(questions, options)
    }

    export async function promptWithConfirmation<T extends string = string>(questions: PromptObject<T> | PromptObject<T>[], options: PromptOptions = defaultOptions): Promise<prompts.Answers<T>> {
        questions = Array.isArray(questions) ? questions : [questions]

        let newAnswers: any = options.answers

        while (true) {
            newAnswers = await promptInternal<T>(questions, { ...options, answers: newAnswers })

            if (options.clearBeforeConfirmations) {
                console.clear()
            }

            if (options.reprintBeforeConfirmations) {
                reprintAnswers(questions, newAnswers)
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
                        const message = getValue(question.message, question, newAnswers)
                        return {
                            title: message as string
                        }
                    })
                }
            ])

            for (const index of fieldsResult.fields) {
                const question = questions[index]
                const name = getValue(question.name, question, newAnswers)
                delete newAnswers[name]
            }
        }

        console.log()

        return newAnswers
    }

    export async function waitForKey(message?: string) {
        console.log(message ?? 'Press any key to continue...');
        await waitForKeypress();

    }
    function waitForKeypress() {
        return new Promise<void>(resolve => {
            process.stdin.setRawMode(true);
            process.stdin.resume();
            process.stdin.once('data', () => {
                process.stdin.setRawMode(false);
                process.stdin.pause();
                resolve();
            });
        });
    }

    function reprintAnswers<T extends string = string>(questions: PromptObject<T>[], answers: any) {
        for (const question of questions) {
            const name = getValue(question.name, question, answers)
            const type = getValue(question.type, question, answers)
            const message = getValue(question.message, question, answers)

            const answer = answers[name]

            if (type == 'text' || type == 'number') {
                console.log(`${chalk.green('√')} ${chalk.bold(message)} ${chalk.gray('...')} ${answer}`)
                continue
            }

            if (type == 'toggle') {
                const inactive = getValue(question.inactive, question, answers)
                const active = getValue(question.active, question, answers)

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
                let mask = getValue<string, typeof question.mask>(question.mask, question, answers)
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
                const choices = getValue<prompts.Choice[], typeof question.choices>(question.choices, question, answers)!
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
                const choices = getValue<prompts.Choice[], typeof question.choices>(question.choices, question, answers)!
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

                const choices = getValue<prompts.Choice[], typeof question.choices>(question.choices, question, answers)!
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
                        return choices[a].title
                    }

                    return a
                })

                const text = temp.join(', ')
                console.log(`${chalk.green('√')} ${chalk.bold(message)} ${chalk.gray('...')} ${text}`)
                continue
            }
        }
    }

    function recreate<T extends string>(question: prompts.PromptObject<T>, values: any) {
        const recreated: PromptObject<T> = {} as any

        if (question.name) {
            recreated.name = getValue<T, typeof question.name>(question.name, question, values)!
        }

        if (question.type) {
            recreated.type = getValue(question.type, question, values)
        }

        if (question.active) {
            recreated.active = getValue(question.active, question, values)
        }

        if (question.choices) {
            recreated.choices = getValue(question.choices, question, values)
        }

        if (question.float) {
            recreated.float = getValue(question.float, question, values)
        }

        if (question.hint) {
            recreated.hint = getValue(question.hint, question, values)
        }

        if (question.inactive) {
            recreated.inactive = getValue(question.inactive, question, values)
        }

        if (question.increment) {
            recreated.increment = getValue(question.increment, question, values)
        }

        if (question.initial) {
            recreated.initial = getValue(question.initial, question, values)
        }

        if (question.limit) {
            recreated.limit = getValue(question.limit, question, values)
        }

        if (question.mask) {
            recreated.mask = getValue(question.mask, question, values)
        }

        if (question.max) {
            recreated.max = getValue(question.max, question, values)
        }

        if (question.message) {
            recreated.message = getValue(question.message, question, values)
        }

        if (question.min) {
            recreated.min = getValue(question.min, question, values)
        }

        if (question.round) {
            recreated.round = getValue(question.round, question, values)
        }

        if (question.separator) {
            recreated.separator = getValue(question.separator, question, values)
        }

        if (question.style) {
            recreated.style = getValue(question.style, question, values)
        }

        if (question.warn) {
            recreated.warn = getValue(question.warn, question, values)
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

        if (question.validate && process.platform !== 'win32') {
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

    function getValue<TMain, TOthers>(valueOrFunc: TMain | TOthers, question: PromptObject, values: any) {
        if (!valueOrFunc)
            return undefined

        if (typeof valueOrFunc == 'function') {
            const func = valueOrFunc as Function
            return func(undefined, values, question) as TMain
        }

        return valueOrFunc as TMain
    }
}