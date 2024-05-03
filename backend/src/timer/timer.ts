export class Timer {
    name: string;
    remainingTime: number;
    lobbyId: string;
    reverse: boolean;
    intervalId?: NodeJS.Timeout;

    constructor(name: string, remainingTime: number, lobbyId: string, reverse?: boolean) {
        this.name = name;
        this.remainingTime = remainingTime;
        this.lobbyId = lobbyId;
        this.reverse = reverse ?? false;
    }

    start() {
        this.stop();
        this.onStart();
        
        this.intervalId = setInterval(() => {
            this.remainingTime = this.reverse ? ++this.remainingTime : --this.remainingTime;
            this.onChanging();

            if (this.remainingTime <= 0) { 
                this.stop(); 
                this.onComplete();
            }
        }, 1000);
    }

    stop() {
        if (!this.intervalId) return;

        clearInterval(this.intervalId);
        this.intervalId = undefined;
    }

    updateTime(newTime: number) {
        this.remainingTime = newTime;
    }

    onStart() {}

    onChanging() {}

    onComplete() {}
}

export class TimerManager {
    map: Timer[];
    timerIndex: number;

    constructor() {
        this.map = [];
        this.timerIndex = 0;
    }

    current() {
        return this.map[this.timerIndex];
    }

    previous() {
        if (this.timerIndex <= 0) return;

        this.current().stop();
        this.timerIndex--;
    }

    next() {
        if (this.timerIndex >= this.map.length - 1) return;

        this.current().stop();
        this.timerIndex++;
    }

    first() {
        this.current().stop();
        this.timerIndex = 0;
    }

    last() {
        this.current().stop();
        this.timerIndex = this.map.length - 1;
    }
}