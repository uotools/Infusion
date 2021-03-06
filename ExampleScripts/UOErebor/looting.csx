#load "common.csx"
#load "colors.csx"
#load "Specs.csx"
#load "ignore.csx"
#load "equip.csx"
#load "warehouse.csx"

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Infusion.Commands;

public static class Looting
{
    // Create a "private" instance of journal for cooking. If you delete this journal it
    // doesn't affect either UO.Journal or other instances of SpeechJournal so
    // you can try looting and healing in parallel
    // and you can still use journal.Delete method in both scripts at the same time.
    // It means, that you don't need tricks like UO.SetJournalLine(number,text) in
    // Injection.
    private static SpeechJournal journal = UO.CreateSpeechJournal();

    public static Warehouse Warehouse { get; set; } = new Warehouse();

    public static ItemSpec UselessLoot { get; } = new[]
    {
        Specs.Torsos, Specs.Rocks, Specs.Corpse,
        // ignoring some "invisible" items
        Specs.Hairs
    };
    
    public static ItemSpec InterestingLoot { get; } = new[]
    {
        Specs.Gold, Specs.Regs, Specs.Gem, Specs.Ammunition,
        Specs.MagickyPytlik, Specs.MagickyVacek, Specs.TajemnaMapa,
        Specs.Food
    };
    
    public static MobileSpec NotRippableCorpses { get; set; } = new[] { Specs.Mounts };

    public static ScriptTrace Trace { get; } = UO.Trace.Create();

    public static ItemSpec IgnoredLoot { get; set; } = UselessLoot;
    public static ItemSpec AllowedLoot { get; set; } = null;
    public static ItemSpec OnGroundLoot { get; set; } = InterestingLoot;
    public static ItemSpec HumanCorpseLoot { get; set; } = OnGroundLoot;

    public static ObjectId? LootContainerId { get; set; }
    public static ItemSpec LootContainerSpec { get; set; }
    public static ItemSpec KnivesSpec { get; set; } = Specs.Knives;
    
    public static IgnoredItems ignoredItems = new IgnoredItems();

    public static IEnumerable<Corpse> GetLootableCorpses()
    {
        var corpses = UO.Corpses
            .MaxDistance(20)
            .Where(x => !ignoredItems.IsIgnored(x))
            .OrderByDistance().ToArray();

        return corpses;
    }
    
    private static Equipment? previousEquipment;
    
    public static void RipAndLoot(Corpse corpse)
    {
        var handEquipment = Equip.GetHand();
        var handEquipmentItem = UO.Items[handEquipment.Id];
        if (handEquipmentItem != null)
        {
            if (!KnivesSpec.Matches(handEquipmentItem))
            {
                if (!previousEquipment.HasValue || handEquipment.Id != previousEquipment.Value.Id)
                {
                    Trace.Log($"Previous equipment: {previousEquipment?.ToString() ?? "null"}");
                    Trace.Log($"Current equipment: {handEquipment}");            
                    previousEquipment = handEquipment;
                }
            }
            else
                UO.Log("Holding knife, will not reequip");
        }
        else
            UO.Log("Cannot find equipment item.");


        bool humanCorpseLooting = Specs.Player.Matches(corpse.CorpseType);

        try
        {
            if (!humanCorpseLooting)
            {
                Rip(corpse);
                Loot(corpse);
            }
            else
            {
                Loot(corpse);
                Rip(corpse);
            }
        }
        catch (Exception ex)
        {
            UO.Log($"Cannot loot corpse: {ex.Message}");
        }

        if (previousEquipment.HasValue)
        {
            Equip.Set(previousEquipment.Value);
        }

        LootGround();
    }
    
    public static void RipAndLootNearest()
    {
        var corpses = GetLootableCorpses().ToArray();
        var corpse = corpses.MaxDistance(3).FirstOrDefault();

        if (corpse != null)
        {
            var handEquipment = Equip.GetHand();
            var handEquipmentItem = UO.Items[handEquipment.Id];
            if (handEquipmentItem != null)
            {
                if (!KnivesSpec.Matches(handEquipmentItem))
                {
                    if (!previousEquipment.HasValue || handEquipment.Id != previousEquipment.Value.Id)
                    {
                        Trace.Log($"Previous equipment: {previousEquipment?.ToString() ?? "null"}");
                        Trace.Log($"Current equipment: {handEquipment}");            
                        previousEquipment = handEquipment;
                    }
                }
                else
                    UO.Log("Holding knife, will not requip");
            }
            else
                UO.Log("Cannot find equipment item.");


            bool humanCorpseLooting = Specs.Player.Matches(corpse.CorpseType);

            try
            {
                if (humanCorpseLooting)
                {
                    Loot(corpse);
                    Rip(corpse);
                }
                else
                {
                    Rip(corpse);
                    Loot(corpse);
                }
                if (corpses.Length - 1 > 0)
                    HighlightLootableCorpses(corpses.Except(new[] { corpse }));
                else
                    UO.ClientPrint($"No more corpses to loot remaining");
            }
            catch (Exception ex)
            {
                UO.Log($"Cannot loot corpse: {ex.Message}");
            }
            finally
            {
                // It seems that re-wearing an item directly
                // after ripping a body and right before
                // looting may crash the game client.
                if (previousEquipment.HasValue)
                {
                    Equip.Set(previousEquipment.Value);
                }
            }
        }
        else
        {
            if (corpses.Length > 0)
            {
                HighlightLootableCorpses(corpses);
                UO.ClientPrint($"No corpse reacheable but {corpses.Length} corpses to loot remaining");
            }
            else
                UO.ClientPrint("no corpse found");
        }        

        LootGround();
    }
    
    private static IEnumerable<Item> GetLootableItems(Item corpse) =>
        UO.Items.InContainer(corpse)
                .Where(i => IsLootable(i));
        
    private static bool IsLootable(Item item)
        => AllowedLoot != null ? AllowedLoot.Matches(item) : !IgnoredLoot.Matches(item);

    private static bool IsLootableFromHuman(Item item)
        => AllowedLoot != null ? AllowedLoot.Matches(item) : !IgnoredLoot.Matches(item) && HumanCorpseLoot.Matches(item);

    public static void HighlightLootableCorpses(IEnumerable<Item> lootableCorpses)
    {
        UO.ClientPrint($"{lootableCorpses.Count()} corpses to loot remaining");
        foreach (var corpse in lootableCorpses)
        {
            int lootableItemsCount = GetLootableItems(corpse).Count();
            UO.ClientPrint($"--{lootableItemsCount}--", "System", corpse.Id, corpse.Type,
                SpeechType.Speech, Colors.Green, log: false);
        }
    }

    public static void LootNearest()
    {
        LootGround();

        var corpses = GetLootableCorpses().ToArray();
        var corpse = corpses.MaxDistance(3).FirstOrDefault();

        if (corpse != null)
        {
            Loot(corpse);
            if (corpses.Length - 1 > 0)
                HighlightLootableCorpses(corpses.Except(new[] { corpse }));
            else
                UO.ClientPrint($"No more corpses to loot remaining");
        }
        else
        {
            if (corpses.Length > 0)
            {
                UO.ClientPrint($"No corpse reacheable but {corpses.Length} corpses to loot remaining");
                HighlightLootableCorpses(corpses);
            }
            else
                UO.ClientPrint("no corpse found");
        }
    }

    private static Item lootContainer;
    
    public static Item LootContainer
    {
        get
        {
            if (lootContainer == null || UO.Items[lootContainer.Id] == null)
            {
                if (LootContainerId.HasValue)
                {
                    lootContainer = UO.Items[LootContainerId.Value];
                }
                
                if (lootContainer == null && LootContainerSpec != null)
                {
                    lootContainer = UO.Items
                        .InContainer(UO.Me.BackPack)
                        .Matching(LootContainerSpec)
                        .FirstOrDefault();
                }
                
                if (lootContainer == null)
                {
                    lootContainer = UO.Me.BackPack;
                }
            }
            
            return lootContainer;
        }
    }

    public static void LootGround()
    {
        var itemsOnGround = UO.Items.Matching(OnGroundLoot).MaxDistance(3).OrderByDistance().ToArray();
        if (itemsOnGround.Length > 0)
            UO.ClientPrint("Looting ground");

        foreach (var item in itemsOnGround)
        {
            if (!Items.TryMoveItem(item, LootContainer))
            {
                UO.ClientPrint("Cannot pickup item, cancelling ground loot");
                break;
            }
        }
    }

    public static void Loot()
    {
        var container = UO.AskForItem();
        if (container != null)
        {
            Loot(container);
        }
        else
            UO.ClientPrint("no container for loot");
    }

    public static void Loot(ObjectId containerId)
    {
        var container = UO.Items[containerId];
        if (container == null)
            UO.ClientPrint($"Cannot find {containerId} container");

        Loot(container);
    }

    public static void Loot(Item container)
    {
        var originalLocation = UO.Me.Location;

        UO.Use(container);
        if (!Common.WaitForContainer())
        {
            UO.Log("Cannot open body, maybe it is not possible to loot the body");
            return;
        }

        UO.ClientPrint($"Number of items in container: {UO.Items.InContainer(container).Count()}");

        Func<Item, bool> isLootableFunc = IsLootable;
        if (container is Corpse containerCorpse)
        {
            UO.Log(containerCorpse.CorpseType.ToString());
            if (Specs.Player.Matches(containerCorpse.CorpseType))
            {
                isLootableFunc = IsLootableFromHuman;
            }
        }

        foreach (var itemToPickup in UO.Items.InContainer(container).ToArray())
        {
            if (container.GetDistance(UO.Me.Location) > 4)
            {
                UO.ClientPrint("corpse too far away, cancelling loot", "looting", UO.Me);
                return;
            }
        
            if (isLootableFunc(itemToPickup))
            {
                Trace.Log($"Looting {Specs.TranslateToName(itemToPickup)} ({itemToPickup.Amount})");
                if (!Items.TryMoveItem(itemToPickup, LootContainer))
                {
                    UO.ClientPrint($"Cannot pickup an item {Specs.TranslateToName(itemToPickup)} ({itemToPickup.Amount})");
                    return;
                }

                if (journal.Contains("Ne tak rychle!"))
                {
                    journal.Delete();
                    UO.Wait(4000);
                }
            }
            else
            {
                Trace.Log($"Ignoring {Specs.TranslateToName(itemToPickup)} ({itemToPickup.Amount})");
            }
        }

        UO.ClientPrint("Looting finished", UO.Me, Colors.Green);
        ignoredItems.Ignore(container);
    }

    public static bool Rip(Corpse corpse)
    {
        if (NotRippableCorpses.Matches(corpse))
        {
            UO.ClientPrint($"Not ripping {Specs.TranslateToName(corpse)}, it is ignored");
            return true;
        }
    
        try
        {
            UO.WaitTargetObject(corpse);
            if (!UO.TryUse(KnivesSpec))
            {
                UO.Alert("Cannot find any knife");
                return false;
            }
          
            bool result = false;
            
            journal
                .When("Rozrezal jsi mrtvolu.", () => result = true)
                .When("You are frozen and can not move.", "you can't reach anything in your state.", () => 
                {
                    result = false;
                    UO.ClientPrint("I'm paralyzed, cannot loot.", UO.Me);
                })
                .When("Unexpected target info", () => 
                {
                    UO.Log("Warning: unexpected target info when targeting a body");
                    UO.Log(corpse.ToString());
                    result = false;
                })
                .WaitAny();
                
            return result;
        }
        finally
        {        
            UO.ClearTargetObject();
        }
    }
    
    public static void UnloadLoot()
    {
        Common.OpenContainer(LootContainer.Id);
        Warehouse.Sort(LootContainer.Id);
    }
}

UO.RegisterCommand("ripandloot", Looting.RipAndLootNearest);
UO.RegisterCommand("loot", Looting.LootNearest);
UO.RegisterCommand("loot-unload", Looting.UnloadLoot);
