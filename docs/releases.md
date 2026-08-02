# Releases

GeoBingo uses Release Please to maintain a shared version and changelog.

## Contribution and release flow

- Use Conventional Commit-formatted pull-request titles. Release Please derives the next version and release notes from the squash-merge commit title: `fix` produces a patch release, `feat` a minor release, and `!` or `BREAKING CHANGE` a major release.
- Squash merge pull requests into the default branch.
- After eligible changes merge, Release Please opens or updates a release pull request. Merging that release PR publishes the release and creates the stable Docker image tag `MAJOR.MINOR.PATCH`. Builds from `master` use `sha-<short-commit>` tags. Publish `latest` only after all three exact release images succeed.

## One-time GitHub setup

Create a fine-grained personal access token restricted to `schmolldechse/geobingo` with read/write access to **Contents**, **Pull requests**, and **Issues**. Use the default 90-day expiry, renew it before it expires, and save it as the `RELEASE_PLEASE_TOKEN` repository secret. If needed, enable **Allow GitHub Actions to create and approve pull requests** under the repository's Actions settings. Do not use the default `GITHUB_TOKEN` for this workflow.

## Initial bootstrap

Squash-merge the implementation pull request as `feat: add semantic release automation`. The initial `.release-please-manifest.json` deliberately starts empty so that `initial-version: 2.0.0` creates `v2.0.0` instead of treating it as already released. Amend the first generated `v2.0.0` release notes once before publication to retain the V2 context. After `v2.0.0` is released, remove the bootstrap-only `initial-version` and `bootstrap-sha` settings from `release-please-config.json` in a `chore:` pull request.
