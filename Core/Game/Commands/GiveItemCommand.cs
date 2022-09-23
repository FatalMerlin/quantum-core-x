using System.Threading.Tasks;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Commands
{
    [Command("give", "Puts the given item in the inventory")]
    public class GiveItemCommand
    {
        private readonly IItemManager _itemManager;

        public GiveItemCommand(IItemManager itemManager)
        {
            _itemManager = itemManager;
        }
        
        [CommandMethod]
        public async Task GiveMyself(IPlayerEntity player, uint itemId, byte count = 1)
        {
            await GiveAnother(player, player, itemId, count);
        }

        [CommandMethod]
        public async Task GiveAnother(IPlayerEntity player, IPlayerEntity target, uint itemId, byte count = 1)
        {
            // todo replace item with item instance and let command manager do the lookup!
            // So we can also allow to give the item to another user
            var item = _itemManager.GetItem(itemId);
            if (item == null)
            {
                await player.SendChatInfo("Item not found");
                return;
            }

            // todo migrate to plugin api style as soon as more is implemented
            if (!(target is PlayerEntity p))
            {
                return;
            }

            // Create item
            var instance = _itemManager.CreateItem(item, count);
            // Add item to players inventory
            if (!await p.Inventory.PlaceItem(instance))
            {
                // No space left in inventory, drop item with player name
                return;
            }
            // Store item in cache
            await instance.Persist();

            // Send item to client
            await p.SendItem(instance);
        }
    }
}