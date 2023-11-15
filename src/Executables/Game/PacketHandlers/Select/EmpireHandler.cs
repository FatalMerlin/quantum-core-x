﻿using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Caching;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Select;

public class EmpireHandler : IGamePacketHandler<Empire>
{
    private readonly ILogger<EmpireHandler> _logger;
    private readonly ICacheManager _cacheManager;

    public EmpireHandler(ILogger<EmpireHandler> logger, ICacheManager cacheManager)
    {
        _logger = logger;
        _cacheManager = cacheManager;
    }

    public async Task ExecuteAsync(GamePacketContext<Empire> ctx, CancellationToken token = default)
    {
        if (ctx.Packet.EmpireId is > 0 and < 4)
        {
            await _cacheManager.Set($"empire-aid:{ctx.Connection.AccountId}", ctx.Packet.EmpireId);
        }
        else
        {
            _logger.LogWarning("Unexpected empire choice {Empire}", ctx.Packet.EmpireId);
        }
    }
}