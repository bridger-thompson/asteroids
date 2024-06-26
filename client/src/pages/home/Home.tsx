import { Link, useNavigate } from "react-router-dom";
import {
  useCreateLobbyMutation,
  useGetLobbiesQuery,
  useJoinLobbyMutation,
} from "./homeHooks";
import { useUser } from "../../userHooks";
import { LobbyInfo, LobbyState } from "../../models/Lobby";

export const Home = () => {
  const user = useUser();
  const navigate = useNavigate();
  const lobbiesQuery = useGetLobbiesQuery();
  const createLobbyMutation = useCreateLobbyMutation();
  const joinLobbyMutation = useJoinLobbyMutation();
  const lobbies = lobbiesQuery.data ?? [];

  const createHandler = () => {
    createLobbyMutation.mutateAsync().then((id) => navigate(`/lobby/${id}`));
  };

  const joinHandler = (id: string) => {
    joinLobbyMutation.mutateAsync(id).then(() => navigate(`/lobby/${id}`));
  };

  const joinButton = (lobby: LobbyInfo) => {
    const isJoined = hasAlreadyJoined(lobby, user?.preferred_username ?? "");
    const canJoin = lobby.playerCount < lobby.maxPlayers && lobby.state === LobbyState.Joining;
    if (isJoined) return (
      <Link to={`/lobby/${lobby.id}`} className="btn btn-secondary w-100">
        Enter
      </Link>
    );
    if (canJoin) return (
      <button onClick={() => joinHandler(lobby.id)} className="btn btn-secondary w-100">
        Join
      </button>
    );
    return (
      <Link to={`/lobby/${lobby.id}`} className="btn btn-secondary w-100">
        Spectate
      </Link>
    );
  };


  return (
    <div className="container mt-2">
      <div className="row">
        <div className="col offset-2">
          <h1 className="text-center">Join Lobby</h1>
        </div>
        <div className="col-2 my-auto text-end">
          <button className="btn btn-outline-primary" onClick={createHandler}>
            Create
          </button>
        </div>
      </div>
      <div className="row">
        {lobbies.map((l, index) => (
          <div key={l.id} className="col-xl-3 col-12 col-md-6 col-lg-4">
            <div className="card shadow my-1">
              <div className="card-body text-center">
                <div className="card-title fs-4">Lobby {index + 1}</div>
                <div className="mb-2">{GetPlayerText(l.playerCount)}</div>
                <div className="mb-2">{joinButton(l)}</div>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

function GetPlayerText(count: number) {
  if (count === 1) return "1 Player";
  return `${count} Players`;
}

function hasAlreadyJoined(lobby: LobbyInfo, username: string) {
  return lobby.players[username] !== undefined || lobby.createdBy === username;
}
