# HNode
<img src="Assets/logo.png" width="256" alt="pixelated H as HNode logo">
Open source Artnet DMX to video grid node renderer, built in unity 6, created for maximum data standard flexibility

## Features
- Optional manual change of ArtNet Address/Port
- Spout2 output as serializer
- Spout2 input as deserializer
- Many generators
- MIDIDMX
- Nerdy statistics
- Support for VRSL, Binary Stage Flight [and others](Assets/Plugin/Serializers)

## Install
Get recent version of HNode in [releases tab](/releases)

## Usage

### HNode with OBS
1. You have to get [plugin Spout2 for OBS](https://github.com/Off-World-Live/obs-spout2-plugin)
2. Add `Spout2 Capture` source
3. Set field `SpoutSenders` to `HNode Output` or the one you set in HNode application
4. Set Composite mode to Default

### HNode with MIDIDMX
[VRC-MIDIDMX](https://github.com/micksam7/VRC-MIDIDMX)

## Troubleshooting
I don't have any options in `Spout2 Capture` properties
You most likely need to restart your PC

## LICENSES

[HNode](LICENSE)

[VRC-MIDIDMX](https://github.com/micksam7/VRC-MIDIDMX/blob/main/LICENSE)

[KlakSpout](Packages/jp.keijiro.klak.spout@3be27c34696f/LICENSE)

[DryWetMIDI](https://github.com/melanchall/drywetmidi/blob/develop/LICENSE)

[CrcSharp](https://github.com/derek-will/CrcSharp/blob/main/LICENSE.md)

[YamlDotNet](Assets/Dependencies/YamlDotNet/YamlDotNet.license.txt)

[Graphy](Assets/Dependencies/Graphy%20-%20Ultimate%20Stats%20Monitor/LICENSE)
