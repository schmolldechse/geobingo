using GeoBingo.Contracts.GameModes;
using GeoBingo.GameModes.Abstractions;
using Riok.Mapperly.Abstractions;

namespace GeoBingo.Api.Mapping;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class GameModeMapper
{
    public partial GameModeSummary Map(IGameModeModule source);
}
