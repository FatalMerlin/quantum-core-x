using System;
using System.Linq;
using System.Text;
using AutoBogus;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore.Core.Networking;
using QuantumCore.Extensions;
using QuantumCore.Game.Packets;
using QuantumCore.Game.Packets.General;
using QuantumCore.Game.Packets.Quest;
using QuantumCore.Game.Packets.QuickBar;
using QuantumCore.Game.Packets.Shop;
using Serilog;
using Weikio.PluginFramework.Catalogs;
using Xunit;
using Xunit.Abstractions;
using Version = QuantumCore.Game.Packets.Version;

namespace Core.Tests;

public class PacketTests
{
    private readonly IPacketManager _packetManager;

    public PacketTests(ITestOutputHelper testOutputHelper)
    {
        var services = new ServiceCollection()
            .AddCoreServices(new EmptyPluginCatalog())
            .AddLogging(x =>
            {
                x.ClearProviders();
                x.AddSerilog(new LoggerConfiguration()
                    .WriteTo.TestOutput(testOutputHelper)
                    .CreateLogger());
            })
            .BuildServiceProvider();
        _packetManager = services.GetRequiredService<IPacketManager>();
        _packetManager.RegisterNamespace("QuantumCore.Game.Packets", typeof(ItemMove).Assembly);
    }

    [Fact]
    public void SpawnCharacter()
    {
        var packetCache = _packetManager.GetOutgoingPacket(0x01);
        var obj = new AutoFaker<SpawnCharacter>()
            .RuleFor(x => x.Affects, faker => new[]
            {
                faker.Random.UInt(),
                faker.Random.UInt()
            })
            .Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x01
                }
                .Concat(BitConverter.GetBytes(obj.Vid))
                .Concat(BitConverter.GetBytes(obj.Angle))
                .Concat(BitConverter.GetBytes(obj.PositionX))
                .Concat(BitConverter.GetBytes(obj.PositionY))
                .Concat(BitConverter.GetBytes(obj.PositionZ))
                .Append(obj.CharacterType)
                .Concat(BitConverter.GetBytes(obj.Class))
                .Append(obj.MoveSpeed)
                .Append(obj.AttackSpeed)
                .Append(obj.State)
                .Concat(obj.Affects.SelectMany(BitConverter.GetBytes))
        );
    }

    [Fact]
    public void Attack()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x02);
        var obj = new AutoFaker<Attack>()
            .RuleFor(x => x.Unknown, faker => new[]
            {
                faker.Random.Byte(),
                faker.Random.Byte()
            })
            .Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x02
                }
                .Append(obj.AttackType)
                .Concat(BitConverter.GetBytes(obj.Vid))
                .Concat(obj.Unknown)
        );
    }

    [Fact]
    public void RemoveCharacter()
    {
        var packetCache = _packetManager.GetOutgoingPacket(0x02);
        var obj = new AutoFaker<RemoveCharacter>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x02
                }
                .Concat(BitConverter.GetBytes(obj.Vid))
        );
    }

    [Fact]
    public void CharacterMoveOut()
    {
        var packetCache = _packetManager.GetOutgoingPacket(0x03);
        var obj = new AutoFaker<CharacterMoveOut>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x03
                }
                .Append(obj.MovementType)
                .Append(obj.Argument)
                .Append(obj.Rotation)
                .Concat(BitConverter.GetBytes(obj.Vid))
                .Concat(BitConverter.GetBytes(obj.PositionX))
                .Concat(BitConverter.GetBytes(obj.PositionY))
                .Concat(BitConverter.GetBytes(obj.Time))
                .Concat(BitConverter.GetBytes(obj.Duration))
        );
    }

    [Fact]
    public void ChatIncoming()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x03);
        var obj = new AutoFaker<ChatIncoming>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x03
                }
                .Concat(BitConverter.GetBytes(obj.Size))
                .Append((byte)obj.MessageType)
                .Concat(Encoding.ASCII.GetBytes(obj.Message))
                .Append((byte)0) // null byte for end of message
        );
    }

    [Fact]
    public void CreateCharacter()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x04);
        var obj = new AutoFaker<CreateCharacter>()
            .RuleFor(x => x.Name, faker => faker.Lorem.Letter(25))
            .RuleFor(x => x.Unknown, faker => new[]
            {
                faker.Random.Byte(),
                faker.Random.Byte(),
                faker.Random.Byte(),
                faker.Random.Byte()
            })
            .Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x04
                }
                .Append(obj.Slot)
                .Concat(Encoding.ASCII.GetBytes(obj.Name))
                .Concat(BitConverter.GetBytes(obj.Class))
                .Append(obj.Appearance)
                .Concat(obj.Unknown)
        );
    }

    [Fact]
    public void DeleteCharacter()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x05);
        var obj = new AutoFaker<DeleteCharacter>()
            .RuleFor(x => x.Code, faker => faker.Lorem.Letter(8))
            .Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x05
                }
                .Append(obj.Slot)
                .Concat(Encoding.ASCII.GetBytes(obj.Code))
        );
    }

    [Fact]
    public void SelectCharacter()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x06);
        var obj = new AutoFaker<SelectCharacter>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x06
                }
                .Append(obj.Slot)
        );
    }

    [Fact]
    public void CharacterMove()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x07);
        var obj = new AutoFaker<CharacterMove>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x07
                }
                .Append(obj.MovementType)
                .Append(obj.Argument)
                .Append(obj.Rotation)
                .Concat(BitConverter.GetBytes(obj.PositionX))
                .Concat(BitConverter.GetBytes(obj.PositionY))
                .Concat(BitConverter.GetBytes(obj.Time))
        );
    }

    [Fact]
    public void CreateCharacterSuccess()
    {
        var packetCache = _packetManager.GetOutgoingPacket(0x08);
        var charFaker = new Faker<Character>()
            .RuleFor(x => x.Name, faker => faker.Lorem.Letter(25));
        var obj = new AutoFaker<CreateCharacterSuccess>()
            .RuleFor(x => x.Character, _ => charFaker.Generate())
            .Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x08
                }
                .Append(obj.Slot)
                .Concat(BitConverter.GetBytes(obj.Character.Id))
                .Concat(Encoding.ASCII.GetBytes(obj.Character.Name))
                .Append((byte)0) // null byte for end of string 
                .Append(obj.Character.Level)
                .Concat(BitConverter.GetBytes(obj.Character.Playtime))
                .Append(obj.Character.St)
                .Append(obj.Character.Ht)
                .Append(obj.Character.Dx)
                .Append(obj.Character.Iq)
                .Concat(BitConverter.GetBytes(obj.Character.BodyPart))
                .Append(obj.Character.NameChange)
                .Concat(BitConverter.GetBytes(obj.Character.HairPort))
                .Concat(BitConverter.GetBytes(obj.Character.Unknown))
                .Concat(BitConverter.GetBytes(obj.Character.PositionX))
                .Concat(BitConverter.GetBytes(obj.Character.PositionY))
                .Concat(BitConverter.GetBytes(obj.Character.Ip))
                .Concat(BitConverter.GetBytes(obj.Character.Port))
                .Append(obj.Character.SkillGroup)
        );
    }

    [Fact]
    public void CreateCharacterFailure()
    {
        var packetCache = _packetManager.GetOutgoingPacket(0x09);
        var obj = new AutoFaker<CreateCharacterFailure>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x09
                }
                .Append(obj.Error)
        );
    }

    [Fact]
    public void EnterGame()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x0a);
        var obj = new AutoFaker<EnterGame>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
            {
                0x0a
            }
        );
    }

    [Fact]
    public void DeleteCharacterSuccess()
    {
        var packetCache = _packetManager.GetOutgoingPacket(0x0A);
        var obj = new AutoFaker<DeleteCharacterSuccess>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x0A
                }
                .Append(obj.Slot)
        );
    }

    [Fact]
    public void DeleteCharacterFail()
    {
        var packetCache = _packetManager.GetOutgoingPacket(0x0B);
        var obj = new AutoFaker<DeleteCharacterFail>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x0B
                }
        );
    }

    [Fact]
    public void ItemUse()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x0B);
        var obj = new AutoFaker<ItemUse>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x0B
                }
                .Append(obj.Window)
                .Concat(BitConverter.GetBytes(obj.Position))
        );
    }

    [Fact]
    public void ItemMove()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x0d);
        var obj = new AutoFaker<ItemMove>()
            .Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x0d
                }
                .Append(obj.FromWindow)
                .Concat(BitConverter.GetBytes(obj.FromPosition))
                .Append(obj.ToWindow)
                .Concat(BitConverter.GetBytes(obj.ToPosition))
                .Append(obj.Count)
        );
    }

    [Fact]
    public void CharacterDead()
    {
        var packetCache = _packetManager.GetOutgoingPacket(0x0e);
        var obj = new AutoFaker<CharacterDead>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x0e
                }
                .Concat(BitConverter.GetBytes(obj.Vid)));
    }

    [Fact]
    public void ItemPickup()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x0F);
        var obj = new AutoFaker<ItemPickup>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x0F
                }
                .Concat(BitConverter.GetBytes(obj.Vid)));
    }

    [Fact]
    public void CharacterPoints()
    {
        var packetCache = _packetManager.GetOutgoingPacket(0x10);
        var obj = new AutoFaker<CharacterPoints>()
            .RuleFor(x => x.Points,
                faker => { return Enumerable.Range(0, 255).Select(_ => faker.Random.UInt()).ToArray(); })
            .Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x10
                }
                .Concat(obj.Points.SelectMany(BitConverter.GetBytes))
        );
    }

    [Fact]
    public void QuickBarAdd()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x10);
        var obj = new AutoFaker<QuickBarAdd>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x10
                }
                .Append(obj.Position)
                .Append(obj.Slot.Type)
                .Append(obj.Slot.Position)
        );
    }

    [Fact]
    public void QuickBarRemove()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x11);
        var obj = new AutoFaker<QuickBarRemove>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x11
                }
                .Append(obj.Position)
        );
    }

    [Fact]
    public void QuickBarSwap()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x12);
        var obj = new AutoFaker<QuickBarSwap>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x12
                }
                .Append(obj.Position1)
                .Append(obj.Position2)
        );
    }

    [Fact]
    public void CharacterUpdate()
    {
        var packetCache = _packetManager.GetOutgoingPacket(0x13);
        var obj = new AutoFaker<CharacterUpdate>()
            .RuleFor(x => x.Parts, faker => new[]
            {
                faker.Random.UShort(),
                faker.Random.UShort(),
                faker.Random.UShort(),
                faker.Random.UShort()
            })
            .RuleFor(x => x.Affects, faker => new[]
            {
                faker.Random.UInt(),
                faker.Random.UInt()
            })
            .Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x13
                }
                .Concat(BitConverter.GetBytes(obj.Vid))
                .Concat(obj.Parts.SelectMany(BitConverter.GetBytes))
                .Append(obj.MoveSpeed)
                .Append(obj.AttackSpeed)
                .Append(obj.State)
                .Concat(obj.Affects.SelectMany(BitConverter.GetBytes))
                .Concat(BitConverter.GetBytes(obj.GuildId))
                .Concat(BitConverter.GetBytes(obj.RankPoints))
                .Append(obj.PkMode)
                .Concat(BitConverter.GetBytes(obj.MountVnum))
                .ToArray()
        );
    }

    [Fact]
    public void ItemDrop()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x14);
        var obj = new AutoFaker<ItemDrop>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x14
                }
                .Append(obj.Window)
                .Concat(BitConverter.GetBytes(obj.Position))
                .Concat(BitConverter.GetBytes(obj.Gold))
                .Append(obj.Count)
        );
    }

    [Fact]
    public void SetItem()
    {
        var packetCache = _packetManager.GetOutgoingPacket(0x15);
        var itemBonusFaker = new AutoFaker<ItemBonus>();
        var obj = new AutoFaker<SetItem>()
            .RuleFor(x => x.Sockets, faker => new []
            {
                faker.Random.UInt(),
                faker.Random.UInt(),
                faker.Random.UInt()
            })
            .RuleFor(x => x.Bonuses, faker => new []
            {
                itemBonusFaker.Generate(),
                itemBonusFaker.Generate(),
                itemBonusFaker.Generate(),
                itemBonusFaker.Generate(),
                itemBonusFaker.Generate(),
                itemBonusFaker.Generate(),
                itemBonusFaker.Generate(),
            })
            .Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x15
                }
                .Append(obj.Window)
                .Concat(BitConverter.GetBytes(obj.Position))
                .Concat(BitConverter.GetBytes(obj.ItemId))
                .Append(obj.Count)
                .Concat(BitConverter.GetBytes(obj.Flags))
                .Concat(BitConverter.GetBytes(obj.AnitFlags))
                .Concat(BitConverter.GetBytes(obj.Highlight))
                .Concat(obj.Sockets.SelectMany(BitConverter.GetBytes))
                .Concat(obj.Bonuses.SelectMany(bonus =>
                {
                    return new[] { bonus.BonusId }.Concat(BitConverter.GetBytes(bonus.Value));
                }))
        );
    }

    [Fact]
    public void ClickNpc()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x1a);
        var obj = new AutoFaker<ClickNpc>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x1a
                }
                .Concat(BitConverter.GetBytes(obj.Vid))
        );
    }

    [Fact]
    public void GroundItemAdd()
    {
        var packetCache = _packetManager.GetOutgoingPacket(0x1A);
        var obj = new AutoFaker<GroundItemAdd>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x1A
                }
                .Concat(BitConverter.GetBytes(obj.PositionX))
                .Concat(BitConverter.GetBytes(obj.PositionY))
                .Concat(BitConverter.GetBytes(obj.PositionZ))
                .Concat(BitConverter.GetBytes(obj.Vid))
                .Concat(BitConverter.GetBytes(obj.ItemId))
        );
    }

    [Fact]
    public void GroundItemRemove()
    {
        var packetCache = _packetManager.GetOutgoingPacket(0x1B);
        var obj = new AutoFaker<GroundItemRemove>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x1B
                }
                .Concat(BitConverter.GetBytes(obj.Vid))
        );
    }

    [Fact]
    public void QuickBarAddOut()
    {
        var packetCache = _packetManager.GetOutgoingPacket(0x1C);
        var obj = new AutoFaker<QuickBarAddOut>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x1C
                }
                .Append(obj.Position)
                .Append(obj.Slot.Type)
                .Append(obj.Slot.Position)
        );
    }

    [Fact]
    public void QuestAnswer()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x1D);
        var obj = new AutoFaker<QuestAnswer>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x1D
                }
                .Append(obj.Answer)
        );
    }

    [Fact]
    public void QuickBarRemoveOut()
    {
        var packetCache = _packetManager.GetOutgoingPacket(0x1D);
        var obj = new AutoFaker<QuickBarRemoveOut>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x1D
                }
                .Append(obj.Position)
        );
    }

    [Fact]
    public void QuickBarSwapOut()
    {
        var packetCache = _packetManager.GetOutgoingPacket(0x1E);
        var obj = new AutoFaker<QuickBarSwapOut>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x1E
                }
                .Append(obj.Position1)
                .Append(obj.Position2)
        );
    }

    [Fact]
    public void Characters()
    {
        var packetCache = _packetManager.GetOutgoingPacket(0x20);
        var characterFaker = new AutoFaker<Character>()
            .RuleFor(x => x.Name, faker => faker.Lorem.Letter(25));
        var obj = new AutoFaker<Characters>()
            .RuleFor(x => x.GuildIds, faker => new[]
            {
                faker.Random.UInt(),
                faker.Random.UInt(),
                faker.Random.UInt(),
                faker.Random.UInt()
            })
            .RuleFor(x => x.GuildNames, faker => new[]
            {
                faker.Lorem.Letter(13),
                faker.Lorem.Letter(13),
                faker.Lorem.Letter(13),
                faker.Lorem.Letter(13)
            })
            .RuleFor(x => x.CharacterList, faker =>
            {
                return new[]
                {
                    characterFaker.Generate(),
                    characterFaker.Generate(),
                    characterFaker.Generate(),
                    characterFaker.Generate()
                };
            })
            .Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x20
                }
                .Concat(obj.CharacterList.SelectMany(c => Array.Empty<byte>()
                    .Concat(BitConverter.GetBytes(c.Id))
                    .Concat(Encoding.ASCII.GetBytes(c.Name))
                    .Append(c.Class)
                    .Append(c.Level)
                    .Concat(BitConverter.GetBytes(c.Playtime))
                    .Append(c.St)
                    .Append(c.Ht)
                    .Append(c.Dx)
                    .Append(c.Iq)
                    .Concat(BitConverter.GetBytes(c.BodyPart))
                    .Append(c.NameChange)
                    .Concat(BitConverter.GetBytes(c.HairPort))
                    .Concat(BitConverter.GetBytes(c.Unknown))
                    .Concat(BitConverter.GetBytes(c.PositionX))
                    .Concat(BitConverter.GetBytes(c.PositionY))
                    .Concat(BitConverter.GetBytes(c.Ip))
                    .Concat(BitConverter.GetBytes(c.Port))
                    .Append(c.SkillGroup)))
                .Concat(obj.GuildIds.SelectMany(BitConverter.GetBytes))
                .Concat(obj.GuildNames.SelectMany(Encoding.ASCII.GetBytes))
                .Concat(BitConverter.GetBytes(obj.Unknown1))
                .Concat(BitConverter.GetBytes(obj.Unknown2))
        );
    }

    [Fact]
    public void ShopOpen()
    {
        var packetCache = _packetManager.GetOutgoingPacket(0x26 << 8 | 0x00);
        var itemBonusFaker = new AutoFaker<ItemBonus>();
        var shopItemFaker = new AutoFaker<ShopItem>()
            .RuleFor(x => x.Sockets, faker => new []
            {
                faker.Random.UInt(),
                faker.Random.UInt(),
                faker.Random.UInt()
            })
            .RuleFor(x => x.Bonuses, faker => new []
            {
                itemBonusFaker.Generate(),
                itemBonusFaker.Generate(),
                itemBonusFaker.Generate(),
                itemBonusFaker.Generate(),
                itemBonusFaker.Generate(),
                itemBonusFaker.Generate(),
                itemBonusFaker.Generate()
            });
        var obj = new AutoFaker<ShopOpen>()
            .RuleFor(x => x.Items, _ => Enumerable.Range(0, 40).Select(_ => shopItemFaker.Generate()).ToArray())
            .Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x26
                }
                .Concat(BitConverter.GetBytes(obj.Size))
                .Append((byte)0x00)
                .Concat(BitConverter.GetBytes(obj.Vid))
                .Concat(obj.Items.SelectMany(item => Array.Empty<byte>()
                    .Concat(BitConverter.GetBytes(item.ItemId))
                    .Concat(BitConverter.GetBytes(item.Price))
                    .Append(item.Count)
                    .Append(item.Position)
                    .Concat(item.Sockets.SelectMany(BitConverter.GetBytes))
                    .Concat(item.Bonuses.SelectMany(bonus => Array.Empty<byte>()
                        .Append(bonus.BonusId)
                        .Concat(BitConverter.GetBytes(bonus.Value))))
                ))
        );
    }

    [Fact]
    public void ShopNotEnoughMoney()
    {
        var packetCache = _packetManager.GetOutgoingPacket(0x26 << 8 | 0x05);
        var obj = new AutoFaker<ShopNotEnoughMoney>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x26
                }
                .Concat(BitConverter.GetBytes(obj.Size))
                .Append((byte)0x05)
        );
    }

    [Fact]
    public void ShopNoSpaceLeft()
    {
        var packetCache = _packetManager.GetOutgoingPacket(0x26 << 8 | 0x07);
        var obj = new AutoFaker<ShopNoSpaceLeft>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x26
                }
                .Concat(BitConverter.GetBytes(obj.Size))
                .Append((byte)0x07)
        );
    }

    [Fact]
    public void QuestScript()
    {
        var packetCache = _packetManager.GetOutgoingPacket(0x2D);
        var obj = new AutoFaker<QuestScript>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x2D
                }
                .Concat(BitConverter.GetBytes(obj.Size))
                .Append(obj.Skin)
                .Concat(BitConverter.GetBytes(obj.SourceSize))
                .Concat(Encoding.ASCII.GetBytes(obj.Source))
                .Append((byte)0) // null byte for end of string
        );
    }

    [Fact]
    public void ShopClose()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x32 << 8 | 0x00);
        var obj = new AutoFaker<ShopClose>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x32,
                    0x00
                }
        );
    }

    [Fact]
    public void ShopBuy()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x32 << 8 | 0x01);
        var obj = new AutoFaker<ShopBuy>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x32,
                    0x01
                }
                .Append(obj.Count)
                .Append(obj.Position)
        );
    }

    [Fact]
    public void ShopSell()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x32 << 8 | 0x03);
        var obj = new AutoFaker<ShopSell>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x32,
                    0x03
                }
                .Append(obj.Position)
                .Append(obj.Count)
        );
    }

    [Fact]
    public void TargetChange()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x3d);
        var obj = new AutoFaker<TargetChange>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x3d
                }
                .Concat(BitConverter.GetBytes(obj.TargetVid))
        );
    }

    [Fact]
    public void SetTarget()
    {
        var packetCache = _packetManager.GetOutgoingPacket(0x3f);
        var obj = new AutoFaker<SetTarget>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x3f
                }
                .Concat(BitConverter.GetBytes(obj.TargetVid))
                .Append(obj.Percentage)
        );
    }

    [Fact]
    public void Warp()
    {
        var packetCache = _packetManager.GetOutgoingPacket(0x41);
        var obj = new AutoFaker<Warp>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x41
                }
                .Concat(BitConverter.GetBytes(obj.PositionX))
                .Concat(BitConverter.GetBytes(obj.PositionY))
                .Concat(BitConverter.GetBytes(obj.ServerAddress))
                .Concat(BitConverter.GetBytes(obj.ServerPort))
        );
    }

    [Fact]
    public void ItemGive()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x53);
        var obj = new AutoFaker<ItemGive>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x53
                }
                .Concat(BitConverter.GetBytes(obj.TargetVid))
                .Append(obj.Window)
                .Concat(BitConverter.GetBytes(obj.Position))
                .Append(obj.Count)
        );
    }

    [Fact]
    public void Empire()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x5a);
        var obj = new AutoFaker<Empire>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x5a
                }
                .Append(obj.EmpireId)
        );
    }

    [Fact]
    public void GameTime()
    {
        var packetCache = _packetManager.GetOutgoingPacket(0x6a);
        var obj = new AutoFaker<GameTime>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x6a
                }
                .Concat(BitConverter.GetBytes(obj.Time))
        );
    }

    [Fact]
    public void TokenLogin()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x6d);
        var obj = new AutoFaker<TokenLogin>()
            .RuleFor(x => x.Username, faker => faker.Lorem.Letter(31))
            .RuleFor(x => x.Xteakeys, faker => new []
            {
                faker.Random.UInt(),
                faker.Random.UInt(),
                faker.Random.UInt(),
                faker.Random.UInt()
            })
            .Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x6d
                }
                .Concat(Encoding.ASCII.GetBytes(obj.Username))
                .Concat(BitConverter.GetBytes(obj.Key))
                .Concat(obj.Xteakeys.SelectMany(BitConverter.GetBytes))
        );
    }

    [Fact]
    public void CharacterDetails()
    {
        var packetCache = _packetManager.GetOutgoingPacket(0x71);
        var obj = new AutoFaker<CharacterDetails>()
            .RuleFor(x => x.Name, faker => faker.Lorem.Letter(25))
            .Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x71
                }
                .Concat(BitConverter.GetBytes(obj.Vid))
                .Concat(BitConverter.GetBytes(obj.Class))
                .Concat(Encoding.ASCII.GetBytes(obj.Name))
                .Concat(BitConverter.GetBytes(obj.PositionX))
                .Concat(BitConverter.GetBytes(obj.PositionY))
                .Concat(BitConverter.GetBytes(obj.PositionZ))
                .Append(obj.Empire)
                .Append(obj.SkillGroup)
        );
    }

    [Fact]
    public void Channel()
    {
        var packetCache = _packetManager.GetOutgoingPacket(0x79);
        var obj = new AutoFaker<Channel>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x79
                }
                .Append(obj.ChannelNo)
        );
    }

    [Fact]
    public void DamageInfo()
    {
        var packetCache = _packetManager.GetOutgoingPacket(0x87);
        var obj = new AutoFaker<DamageInfo>().Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x87
                }
                .Concat(BitConverter.GetBytes(obj.Vid))
                .Append(obj.DamageFlags)
                .Concat(BitConverter.GetBytes(obj.Damage))
        );
    }

    [Fact]
    public void CharacterInfo()
    {
        var packetCache = _packetManager.GetOutgoingPacket(0x88);
        var obj = new AutoFaker<CharacterInfo>()
            .RuleFor(x => x.Name, faker => faker.Lorem.Letter(25))
            .RuleFor(x => x.Parts, faker => new[]
            {
                faker.Random.UShort(),
                faker.Random.UShort(),
                faker.Random.UShort(),
                faker.Random.UShort()
            })
            .Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0x88
                }
                .Concat(BitConverter.GetBytes(obj.Vid))
                .Concat(Encoding.ASCII.GetBytes(obj.Name))
                .Concat(obj.Parts.SelectMany(BitConverter.GetBytes))
                .Append(obj.Empire)
                .Concat(BitConverter.GetBytes(obj.GuildId))
                .Concat(BitConverter.GetBytes(obj.Level))
                .Concat(BitConverter.GetBytes(obj.RankPoints))
                .Append(obj.PkMode)
                .Concat(BitConverter.GetBytes(obj.MountVnum))
        );
    }

    [Fact]
    public void Version()
    {
        var packetCache = _packetManager.GetIncomingPacket(0xf1);
        var obj = new AutoFaker<Version>()
            .RuleFor(x => x.ExecutableName, faker => faker.Lorem.Letter(33))
            .RuleFor(x => x.Timestamp, faker => faker.Lorem.Letter(33))
            .Generate();
        var bytes = packetCache.Serialize(obj);

        bytes.Should().Equal(
            new byte[]
                {
                    0xf1
                }
                .Concat(Encoding.ASCII.GetBytes(obj.ExecutableName))
                .Concat(Encoding.ASCII.GetBytes(obj.Timestamp))
        );
    }
}