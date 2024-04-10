interface PlayerProps {
    name: string;
    id: string;
    picture: string;
    auth: any;
    points: number;
}

export class Player {
    name: string;
    id: string;
    picture: string;
    auth: any;
    points: number;

    constructor({ name, id, picture, auth, points }: PlayerProps) {
        this.name = name;
        this.id = id;
        this.picture = picture;
        this.auth = auth;
        this.points = points;
    }
}