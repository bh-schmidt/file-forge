import { Choice } from "prompts";

export namespace AutoCompleteMatcher {
    export async function prefixMatch(input: string, choices: Choice[]) {
        const filter = input.toLowerCase()

        return choices.filter(choice =>
            choice.title?.toLowerCase().startsWith(filter)
        );
    }

    export async function suffixMatch(input: string, choices: Choice[]) {
        const filter = input.toLowerCase();

        return choices.filter(choice =>
            choice.title?.toLowerCase().endsWith(filter)
        );
    }

    export async function fuzzyMatch(input: string, choices: Choice[]) {
        const filter = input.toLowerCase();

        return choices
            .map(e => {
                return {
                    choice: e,
                    firstIndex: e.title.toLocaleLowerCase().indexOf(filter)
                } as TempChoice
            })
            .filter(e => e.firstIndex! > -1)
            .sort((a, b) => {
                return a.firstIndex! - b.firstIndex!
            })
    }

    export async function wildcardMatch(input: string, choices: Choice[]) {
        if (!input || input.length == 0) {
            return choices
        }

        const filter = input.toLocaleLowerCase()

        return choices
            .map(c => {
                const title = c.title.toLocaleLowerCase() ?? ''
                const [first, last] = getWildcardIndexes(filter, title)

                return {
                    choice: c,
                    title: title,
                    firstIndex: first,
                    lastIndex: last,
                } as TempChoice
            })
            .filter(e => e.firstIndex !== undefined && e.lastIndex !== undefined)
            .sort((a, b) => {
                const span1 = a.lastIndex! - a.firstIndex!
                const span2 = b.lastIndex! - b.firstIndex!

                return span1 - span2
            })
            .map(e => e.choice)
    }


    function getWildcardIndexes(input: string, title: string) {
        let current = 0
        let firstMatch: number | undefined
        let lastMatch: number | undefined

        for (let inputIndex = 0; inputIndex < input.length; inputIndex++) {
            const inputChar = input[inputIndex];
            let found = false

            for (; current < title.length; current++) {
                const titleChar = title[current];
                if (inputChar == titleChar) {
                    firstMatch ??= current
                    lastMatch = current
                    found = true
                    current++
                    break
                }
            }

            if (!found) {
                return [firstMatch, undefined]
            }
        }

        return [firstMatch!, lastMatch!]
    }

    interface TempChoice {
        choice: Choice
        title?: string
        firstIndex?: number
        lastIndex?: number
    }
}