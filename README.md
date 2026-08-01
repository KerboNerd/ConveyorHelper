# Conveyor Helper

Client-only [Pulsar](https://github.com/SpaceGT/Pulsar) plugin for Space Engineers 1 by **LYNX**.

While placing a block, draws thick cyan markers on every conveyor port of the placement ghost so you can see how it will connect before you confirm. Works with vanilla and modded conveyors (including AQD armored pieces).

**Toggle:** NumPad `*` (configurable in plugin settings)

## Requirements

- Space Engineers
- [Pulsar](https://github.com/SpaceGT/Pulsar)

## Install

Enable **Conveyor Helper** from Pulsar’s plugin list (PluginHub) after it is published, or copy a build of `ConveyorHelper.dll` into Pulsar’s `Legacy\Local` folder for local testing.

## Build

See the [client plugin template](https://github.com/viktor-ferenczi/se-client-plugin-template) prerequisites. Run `setup.py`, then:

```powershell
dotnet build ClientPlugin\ClientPlugin.csproj -c Release
```

## License

See [LICENSE](LICENSE).
