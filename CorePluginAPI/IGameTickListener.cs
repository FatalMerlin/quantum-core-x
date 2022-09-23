﻿using System.Threading;
using System.Threading.Tasks;

namespace QuantumCore.API;

public interface IGameTickListener
{
    Task PreUpdateAsync(CancellationToken token);
    Task PostUpdateAsync(CancellationToken token);
}