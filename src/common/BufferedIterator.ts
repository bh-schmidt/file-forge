export default class BufferedIterator<T> implements AsyncIterableIterator<T> {
    [Symbol.asyncIterator](): AsyncIterableIterator<T> {
        return this
    }

    closed = false
    emitted = false
    buffer: T[] = []

    emitterAwaiter: Promise<void> | undefined
    iteratorAwaiter: Promise<void>

    releaseEmitter: (() => void) = () => { }
    releaseIterator: (() => void) = () => { }

    constructor() {
        this.iteratorAwaiter = new Promise<void>(res => {
            this.releaseIterator = res
        })
    }

    async next(): Promise<IteratorResult<T, any>> {
        if (this.closed) {
            if (this.buffer.length > 0) {
                return { value: this.buffer.shift(), done: true }
            } else {
                return { value: undefined, done: true }
            }
        }

        await this.iteratorAwaiter
        this.iteratorAwaiter = new Promise<void>(res => {
            this.releaseIterator = res
        })

        this.releaseEmitter()

        return { value: this.buffer.shift()!, done: false }
    }

    async return?(): Promise<IteratorResult<T, any>> {
        this.closed = true
        this.releaseEmitter()
        this.releaseIterator()

        return { value: undefined, done: true }
    }

    async emit(value: T) {
        if (this.closed) {
            return
        }

        if (this.emitterAwaiter) {
            await this.emitterAwaiter
        }

        this.emitterAwaiter = new Promise<void>(res => {
            this.releaseEmitter = res
        })

        this.buffer.push(value)

        this.releaseIterator()
    }

    close() {
        this.closed = true
        this.releaseEmitter()
        this.releaseIterator()
    }
}