﻿namespace SkEditor.API;

/// <summary>
/// Object representing a registry key, based on an IAddon instance and a string key.
/// </summary>
/// <param name="Addon">The IAddon instance linked to the key.</param>
/// <param name="Key">The string key.</param>
public record RegistryKey(IAddon Addon, string Key);