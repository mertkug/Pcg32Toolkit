# Pcg32Toolkit

A small utility library with a `PCG32` random number generator.

## Install

```bash
dotnet add package Pcg32Toolkit
```

## Usage

```csharp
using Pcg32Toolkit;

var rng = new PCG32(seed: 1234, stream: 42);
uint dice = rng.NextBounded(6) + 1;
int score = rng.NextInt(10, 100);
float unitFloat = rng.NextSingle();
double unitDouble = rng.NextDouble();
```
