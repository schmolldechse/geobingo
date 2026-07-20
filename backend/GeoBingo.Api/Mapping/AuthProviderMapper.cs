using GeoBingo.Api.Authentication;
using GeoBingo.Contracts.Authentication;
using Riok.Mapperly.Abstractions;

namespace GeoBingo.Api.Mapping;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class AuthProviderMapper
{
    [MapProperty(nameof(ExternalAuthProviderDescriptor.Name), nameof(AuthProvider.Key))]
    [MapProperty(nameof(ExternalAuthProviderDescriptor.DisplayName), nameof(AuthProvider.DisplayName))]
    public partial AuthProvider Map(ExternalAuthProviderDescriptor source);
}
