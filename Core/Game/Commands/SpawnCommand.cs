using System.Security.Cryptography;
using System.Threading.Tasks;
using QuantumCore.API;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Commands
{
    [Command("spawn", "Spawn a monster or npc")]
    public class SpawnCommand
    {
        private readonly IMonsterManager _monsterManager;
        private readonly IAnimationManager _animationManager;
        private readonly IWorld _world;

        public SpawnCommand(IMonsterManager monsterManager, IAnimationManager animationManager, IWorld world)
        {
            _monsterManager = monsterManager;
            _animationManager = animationManager;
            _world = world;
        }
        
        [CommandMethod]
        public async Task SpawnMonster(IPlayerEntity player, uint monsterId, byte count = 1)
        {
            var proto = _monsterManager.GetMonster(monsterId);
            if (proto == null)
            {
                await player.SendChatInfo("No monster found with the specified id");
            }

            for (var i = 0; i < count; i++)
            {
                // Calculate random spawn position close by the player
                var x = player.PositionX + RandomNumberGenerator.GetInt32(-1500, 1501);
                var y = player.PositionY + RandomNumberGenerator.GetInt32(-1500, 1501);

                // Create entity instance
                var monster = new MonsterEntity(_monsterManager, _animationManager, _world, monsterId, x, y);
                await _world.SpawnEntity(monster);
            }
        }
    }
}