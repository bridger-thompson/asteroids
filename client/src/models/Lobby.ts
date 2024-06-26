export interface LobbyList {
  lobbies: LobbyInfo[];
}

export enum LobbyState {
  Joining,
  Countdown,
  Playing,
  Stopped,
  GameOver,
}

export interface LobbyInfo {
  id: string;
  createdBy: string;
  playerCount: number;
  maxPlayers: number;
  state: LobbyState;
  players: { [username: string]: PlayerShip }
  asteroids: Asteroid[];
  countdownTime: number;
}


export interface PlayerShip {
  position: Vector;
  velocity: Vector;
  direction: Vector;
  inputState?: InputState;
  health: number;
  maxHealth: number;
  color: string;
  points: number;
  bullets: Bullet[];
}

export interface Asteroid {
  position: Vector;
  velocity: Vector;
  direction: Vector;
  size: number;
  damage: number;
  health: number;
}

export interface Bullet {
  position: Vector;
  velocity: Vector;
  direction: Vector;
}

export interface Vector {
  x: number;
  y: number;
}

export interface InputState {
  thrusting: boolean;
  rotationDirection: RotationDirection;
  shootPressed: boolean;
}

export enum RotationDirection {
  None,
  Left,
  Right
}


export interface CreateLobbyCommand {
  username: string;
} 
