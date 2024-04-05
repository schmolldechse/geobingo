interface PlayerProps {
    name: string;
    id: string;
    picture: string;
    points: number;
    auth: any;
}

export class Player {
    name: string;
    id: string;
    picture: string;
    points: number;
    auth: any;

    constructor({ name, id, picture, points, auth }: PlayerProps) {
        this.name = name;
        this.id = id;
        this.picture = picture;
        this.points = points;
        this.auth = auth;
    }
}