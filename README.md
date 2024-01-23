# Lobby Compatibility

This mod aims to provide better vanilla/modded lobby compatibility and browsing.

## For Players

### Lobby Browser

## For Developers

To use this, you need to add a package reference to `Max.LobbyCompatibility` in your `.csproj` file. You can use the following code:

```xml
<ItemGroup>
    <PackageReference Include="Max.LobbyCompatibility" PrivateAssets="all" />
</ItemGroup>
```

You can also use your IDE's interface to add the reference. For Visual Studio 2022, you do so by clicking on the `Project` dropdown, and clicking `Manage NuGet Packages`. You then can search for `Max.LobbyCompatibility` and add it from there.

### Usage

Simply add `[LobbyCompatibility(CompatibilityLevel, VersionStrictness)]` above your `Plugin` class like so:

```csharp
[LobbyCompatibility(CompatibilityLevel = CompatibilityLevel.Everyone, VersionStrictness = VersionStrictness.Minor)]
class MyPlugin : BaseUnityPlugin
```

The following enums are usable:

#### `CompatibilityLevel`

If the Compatibility Level is not specified, it will result in no checking.

- `ClientOnly`
  - Mod only impacts the client.
  - Mod is not checked at all, VersionStrictness does not apply.
- `ServerOnly`
  - Mod only impacts the server, and might implicitly impact the client without the client needing to have it installed for it to work.
  - Mod is only required by the server. VersionStrictness only applies if the mod is installed on the client.
- `Everyone`
  - Mod impacts both the client and the server, and adds functionality that requires the mod to be installed on both.
  - Mod must be loaded on server and client. Version checking depends on the VersionStrictness.
- `ClientOptional`
  - Not every client needs to have the mod installed, but if it is installed, the server also needs to have it. Generally used for mods that add extra (optional) functionality to the client if the server has it installed.
  - Mod must be loaded on server. Version checking depends on the VersionStrictness.


#### `VersionStrictness`

If the Version Strictness is not specified, it will default to `None`.

- `None`
    - No version check is done
- `Major`
    - Mod must have the same Major version (1.x.x)
- `Minor`
    - Mods must have the same Minor version (x.1.x)
- `Patch`
    - Mods must have the same Patch version (x.x.1)