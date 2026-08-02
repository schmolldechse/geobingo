# GeoBingo

GeoBingo is a browser-based multiplayer Street View game. Players join real-time lobbies, move through Google Street View, and compete in game modes such as Capture Challenge, where submitted captures are evaluated against goals chosen by the lobby.

The application uses Google authentication, keeps active lobby state in the API process, and synchronizes gameplay over SignalR.

> GeoBingo was inspired by [s0er3n/GeoBingo.io](https://github.com/s0er3n/GeoBingo.io), the open-source multiplayer Street View bingo game by s0er3n.

## Architecture

- The [frontend](frontend/README.md) is a SvelteKit application built and served with Bun.
- The [backend](backend/README.md) is an ASP.NET Core application targeting .NET 10.
- REST endpoints cover authentication, game-mode discovery, and lobby discovery. SignalR carries lobby commands and live projections.
- PostgreSQL stores authentication data through Entity Framework Core. Active lobby and round state remains process-local.
- Production services are distributed as separate frontend, API, and migration images through GHCR.

## Development

Local development requires:

- [Bun](https://bun.sh/)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- PostgreSQL
- A Google Cloud project with billing enabled for Google Maps Platform

> Google Maps Platform requires billing to be enabled, but eligible Maps services include [monthly free usage caps](https://developers.google.com/maps/billing-and-pricing/pricing). Local development and low-volume use can therefore remain free while usage stays within the current limits. Usage above those limits is billed; [configure quota limits](https://developers.google.com/maps/billing-and-pricing/manage-costs) to avoid unexpected charges.

### Google Cloud setup

The frontend and backend use credentials from the same Google Cloud project:

1. Create or select a project in the [Google Cloud Console](https://console.cloud.google.com/).
2. [Enable the Maps JavaScript API and create an API key](https://developers.google.com/maps/documentation/javascript/get-api-key). Restrict the key to the Maps JavaScript API and the websites that serve the frontend.
3. Register the application in Google Auth Platform and [create an OAuth client](https://support.google.com/cloud/answer/15549257?hl=en) with the `Web application` type.
4. Add `http://localhost:5287/api/auth/google/callback` as an authorized redirect URI.
5. Keep the Maps API key, OAuth client ID, and OAuth client secret available for the frontend and backend setup below.

Start PostgreSQL first, then apply the database migrations and run the API. Once the API is available on `http://localhost:5287`, configure and start the frontend.

The detailed commands and configuration keys are documented in the [backend setup](backend/README.md#local-development) and [frontend setup](frontend/README.md#local-development).

## Contributions and releases

Pull request titles must follow [Conventional Commits](https://www.conventionalcommits.org/), because the squash-merge commit becomes the release input:

- `fix` produces a patch release.
- `feat` produces a minor release.
- `!` or a `BREAKING CHANGE` footer produces a major release.

Squash pull requests into `master`. After a releasable change lands, Release Please opens or updates a release pull request containing the version bump and changelog. Merging that pull request creates the `vMAJOR.MINOR.PATCH` tag and the corresponding GitHub Release.

Container tags have distinct roles:

- `sha-<short-commit>` identifies an immutable build from `master`.
- `MAJOR.MINOR.PATCH` identifies the images for an exact release.
- `latest` is promoted only after the API, migration, and frontend images for the newest GitHub Release all build successfully.

See the [changelog](CHANGELOG.md) and [published releases](https://github.com/schmolldechse/geobingo/releases) for release history.

## License

GeoBingo is available under the [Apache License 2.0](LICENSE).
