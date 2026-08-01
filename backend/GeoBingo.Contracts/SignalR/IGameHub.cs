using System.Threading.Tasks;
using GeoBingo.Contracts.GameModes.CaptureChallenge;
using GeoBingo.Contracts.Lobbies;
using TypedSignalR.Client;

namespace GeoBingo.Contracts.SignalR;

[Hub]
public interface IGameHub
{
    Task<HubOperationResult> JoinLobby(JoinLobbyRequest request);
    Task<HubOperationResult> LeaveLobby();
    Task<HubOperationResult> CloseLobby();
    Task<HubOperationResult> RemovePlayer(RemovePlayerRequest request);
    Task<HubOperationResult> TransferHost(TransferHostRequest request);
    Task<HubOperationResult> SelectGameMode(SelectGameModeRequest request);
    Task<HubOperationResult> UpdateLobbySettings(UpdateLobbySettingsRequest request);
    Task<HubOperationResult> StartRound();
    Task<HubOperationResult> UpdateCaptureChallengeSettings(UpdateCaptureChallengeSettingsRequest request);
    Task<HubOperationResult> AddCaptureGoal(AddCaptureGoalRequest request);
    Task<HubOperationResult> UpdateCaptureGoal(UpdateCaptureGoalRequest request);
    Task<HubOperationResult> RemoveCaptureGoal(RemoveCaptureGoalRequest request);
    Task<HubOperationResult> ReorderCaptureGoals(ReorderCaptureGoalsRequest request);
    Task<HubOperationResult> SubmitCapture(SubmitCaptureRequest request);
    Task<HubOperationResult> UpdateCapture(UpdateCaptureRequest request);
    Task<HubOperationResult> RemoveCapture(RemoveCaptureRequest request);
    Task<HubOperationResult> CastVote(CastVoteRequest request);
    Task<HubOperationResult> ChangeVote(ChangeVoteRequest request);
    Task<HubOperationResult> RequestSnapshot();
    Task<HubOperationResult> RequestRoundResults(RequestRoundResultsRequest request);
}
