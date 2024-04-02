interface PlayerProps {
    name: string;
    id: string;
    picture: string;
    auth: any;
}

export class Player {
    name: string;
    id: string;
    picture: string;
    auth: any;

    constructor({ name, id, picture, auth }: PlayerProps) {
        this.name = name;
        this.id = id;
        this.picture = picture;
        this.auth = auth;
    }
}