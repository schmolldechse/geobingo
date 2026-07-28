const LOBBY_CODE_PATTERN = /^[A-Z0-9]{8}$/;

const normalizeLobbyCode = (value: string): string => value.trim().toUpperCase();

const isValidLobbyCode = (value: string): boolean => LOBBY_CODE_PATTERN.test(value);

export { normalizeLobbyCode, isValidLobbyCode };
