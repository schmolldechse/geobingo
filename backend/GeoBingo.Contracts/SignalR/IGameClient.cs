using System.Threading.Tasks;
using GeoBingo.Contracts.Lobbies;
using TypedSignalR.Client;

namespace GeoBingo.Contracts.SignalR;

[Receiver]
public interface IGameClient
{
    Task ReceiveLobbySnapshot(LobbySnapshot snapshot);
    Task ReceivePersonalProjection(PersonalProjection projection);
    Task ReceiveGameModeEvent(GameModeEvent message);
    Task ReceiveRoundResults(LobbyRoundResultsView results);
    Task ReceiveLobbyEnded(LobbyEnded message);
    Task ReceiveError(SignalRError error);
}
