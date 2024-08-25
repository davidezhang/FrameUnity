# FrameUnity
This is a simple Unity C# script that connects an Android build to [Frame AR glasses](https://brilliant.xyz/products/frame) by Brilliant Labs via BLE. It should work with iOS but it hasn't been tested.
Frame is a pair of open-source glasses with BLE connectivity, a monocular color OLED display, a microphone, an IMU, a RGB camera, and a rechargeable battery. 
Frame currently doesn't have a Unity SDK, so this script is a starting point to connect to Frame and send/receive data.

## Features
- Scans and connects to Frame
- Subscribe to Frame's characteristic to receive data
- Write to Frame's characteristic to send data in the form of Lua strings

## Unity Setup
This script uses a paid plugin asset called [Bluetooth LE for iOS, tvOS and Android](https://assetstore.unity.com/packages/tools/network/bluetooth-le-for-ios-tvos-and-android-26661). You will need to import this asset into your project.
This asset has worked well for me in th past when connecting to BLE devices. It is simple to use and provides examples to get started. It also offers the ability to request for MTU size, which other similar plugins don't do.

## Goals
The goal of this project is to continue to add functionality to get closer to a full Unity SDK for Frame. Priorities for me include:
- [] Sending/Receiving BLE data in chunks
- [] Camera (take photo and send to Unity build)
- [] Microphone (record audio and send to Unity build)
- [] IMU (detect tap)
- [] Display (draw UI)

## References
- [Frame Lua API Reference](https://docs.brilliant.xyz/frame/building-apps-lua/)
- [Frame BLE Documentation](https://docs.brilliant.xyz/frame/building-apps-bluetooth-specs/)
- [Brillian Labs GitHub](https://github.com/brilliantlabsAR)