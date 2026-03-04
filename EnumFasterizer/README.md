# EnumFasterizer

A small .NET library focused on making common enum operations faster by avoiding repeated reflection and repeated allocations.

This project targets:
- **.NET Standard 2.0** (broad compatibility)
- **.NET 9** (latest runtime improvements)

---

## Why

The built-in `System.Enum` APIs are convenient, but some usage patterns can be expensive when called frequently (for example in hot paths, tight loops, or high-throughput services), especially when repeatedly:
- enumerating enum values
- mapping values ↔ names
- parsing strings to enums
- looking up enum metadata (attributes, underlying numeric values)

`EnumFasterizer` is intended to amortize those costs via caching and reuse.

---

## Goals / Design

High-level approach (conceptual pseudocode):

- On first use per enum type `TEnum`:
  - read and cache all values (and any needed metadata)
  - build fast lookup tables (for name→value, value→name, etc.)
- On subsequent uses:
  - serve requests from cached data structures
  - avoid reflection and minimize allocations

---

## Features (typical use cases)

Depending on what the library exposes in code, `EnumFasterizer` generally aims to help with:

- Faster “get all values” patterns (reuse a cached array/span)
- Faster value-to-name and name-to-value lookups
- Reduced allocation patterns compared to repeated `Enum.GetValues(...)` / `Enum.GetName(...)` usage
- Thread-safe, reusable caches per enum type

> Note: See the public API in the source (and IntelliSense/XML docs) for the exact entry points and supported options.

---

## Install

If this is published as a NuGet package:
