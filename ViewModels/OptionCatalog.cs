using System.Collections.Generic;

namespace JsonWorkbench.ViewModels;

public static class OptionCatalog
{
    public static IEnumerable<JsonInsertOption> ItemOptions()
    {
        // From Modding:Item_Json_Properties/en (hard-coded)
        return new[]
        {
            // Core
            new JsonInsertOption("Core", "code", "/code", InsertMode.Overwrite, "\"myitem\"", "Unique identifier for the item."),
            new JsonInsertOption("Core", "enabled", "/enabled", InsertMode.SetIfMissing, "true", "Whether the item is loaded."),
            new JsonInsertOption("Core", "variantgroups", "/variantgroups", InsertMode.SetIfMissing,
                "[{ \"code\": \"type\", \"states\": [\"a\", \"b\"] }]",
                "Define variants for the item."),
            new JsonInsertOption("Core", "allowedVariants", "/allowedVariants", InsertMode.SetIfMissing,
                "[]",
                "Trim generated variants (advanced)."),
            new JsonInsertOption("Core", "skipVariants", "/skipVariants", InsertMode.SetIfMissing,
                "[]",
                "Skip specific variants (advanced)."),

            // Specific
            new JsonInsertOption("Specific", "class", "/class", InsertMode.SetIfMissing, "\"item\"", "C# class to use for extra functionality."),
            new JsonInsertOption("Specific", "durability", "/durability", InsertMode.SetIfMissing, "0", "Max uses."),
            new JsonInsertOption("Specific", "maxstacksize", "/maxstacksize", InsertMode.SetIfMissing, "64", "Max stack size."),
            new JsonInsertOption("Specific", "attackpower", "/attackpower", InsertMode.SetIfMissing, "0.5", "Melee damage."),
            new JsonInsertOption("Specific", "attackrange", "/attackrange", InsertMode.SetIfMissing, "1.5", "Melee range."),
            new JsonInsertOption("Specific", "materialdensity", "/materialdensity", InsertMode.SetIfMissing, "9999", "Float/sink behavior."),
            new JsonInsertOption("Specific", "liquidselectable", "/liquidselectable", InsertMode.SetIfMissing, "false", "Used by buckets etc."),

            // Food / transitions
            new JsonInsertOption("Food", "nutritionProps", "/nutritionProps", InsertMode.SetIfMissing,
                "{ \"foodcategory\": \"vegetable\", \"saturation\": 0, \"health\": 0 }",
                "Basic nutrition values."),
            new JsonInsertOption("Food", "transitionableProps (Perish/Cure)", "/transitionableProps", InsertMode.SetIfMissing,
                "[{ \"type\": \"Perish\", \"freshHours\": { \"avg\": 240 }, \"transitionHours\": { \"avg\": 48 }, \"transitionedStack\": { \"type\": \"item\", \"code\": \"game:rot\", \"quantity\": 1 }, \"transitionRatio\": 1 }]",
                "Spoil/cure transitions."),

            // Containers / placement
            new JsonInsertOption("Container/Placement", "shelvable", "/shelvable", InsertMode.SetIfMissing,
                "true",
                "Allow placing on shelves."),
            new JsonInsertOption("Container/Placement", "rackable", "/rackable", InsertMode.SetIfMissing,
                "true",
                "Allow placing on tool racks (use toolrackTransform too)."),
            new JsonInsertOption("Container/Placement", "inFirePitProps", "/inFirePitProps", InsertMode.SetIfMissing,
                "{ \"transform\": { \"scale\": 0.85, \"origin\": { \"x\": 0.5, \"y\": 0.0625, \"z\": 0.5 }, \"translation\": { \"x\": 0, \"y\": 0, \"z\": 0 }, \"rotation\": { \"x\": 0, \"y\": 0, \"z\": 90 } }, \"useFirepitModel\": \"Spit\" }",
                "Extra rendering/placement behavior in firepits."),
            new JsonInsertOption("Container/Placement", "waterTightContainerProps", "/waterTightContainerProps", InsertMode.SetIfMissing,
                "{ \"containable\": true, \"itemsPerLitre\": 1, \"texture\": \"block/water\", \"allowSpill\": true }",
                "Liquid-container behavior (barrels/buckets)."),

            // Rendering
            new JsonInsertOption("Rendering", "texture", "/texture", InsertMode.SetIfMissing,
                "\"item/myitem\"",
                "Item texture path."),
            new JsonInsertOption("Rendering", "textures", "/textures", InsertMode.SetIfMissing,
                "{ \"metal\": { \"base\": \"block/metal/ingot/{metal}\" } }",
                "Texture map for shaped items."),
            new JsonInsertOption("Rendering", "shape", "/shape", InsertMode.SetIfMissing,
                "{ \"base\": \"item/myshape\" }",
                "Shape definition."),
            new JsonInsertOption("Rendering", "shapeinventory", "/shapeinventory", InsertMode.SetIfMissing,
                "{ \"base\": \"item/myshape-inv\" }",
                "Different inventory shape."),
            new JsonInsertOption("Rendering", "guiTransform", "/guiTransform", InsertMode.SetIfMissing,
                "{ \"origin\": { \"x\": 0.5, \"y\": 0.5, \"z\": 0.5 }, \"translation\": { \"x\": 0, \"y\": 0, \"z\": 0 }, \"rotation\": { \"x\": 0, \"y\": 0, \"z\": 0 }, \"scale\": 1 }",
                "GUI render transform."),
            new JsonInsertOption("Rendering", "fphandtransform", "/fphandtransform", InsertMode.SetIfMissing,
                "{ \"origin\": { \"x\": 0.5, \"y\": 0.5, \"z\": 0.5 }, \"translation\": { \"x\": 0, \"y\": 0, \"z\": 0 }, \"rotation\": { \"x\": 0, \"y\": 0, \"z\": 0 }, \"scale\": 1 }",
                "First-person hand transform."),
            new JsonInsertOption("Rendering", "tphandtransform", "/tphandtransform", InsertMode.SetIfMissing,
                "{ \"origin\": { \"x\": 0.5, \"y\": 0.5, \"z\": 0.5 }, \"translation\": { \"x\": 0, \"y\": 0, \"z\": 0 }, \"rotation\": { \"x\": 0, \"y\": 0, \"z\": 0 }, \"scale\": 1 }",
                "Third-person hand transform."),
            new JsonInsertOption("Rendering", "groundtransform", "/groundtransform", InsertMode.SetIfMissing,
                "{ \"origin\": { \"x\": 0.5, \"y\": 0.5, \"z\": 0.5 }, \"translation\": { \"x\": 0, \"y\": 0, \"z\": 0 }, \"rotation\": { \"x\": 0, \"y\": 0, \"z\": 0 }, \"scale\": 1 }",
                "Dropped-on-ground transform."),
            new JsonInsertOption("Rendering", "toolrackTransform", "/toolrackTransform", InsertMode.SetIfMissing,
                "\"default\"",
                "Transform preset for tool racks."),
            new JsonInsertOption("Rendering", "glowLevel", "/glowLevel", InsertMode.SetIfMissing,
                "0",
                "Light emitted."),
            new JsonInsertOption("Rendering", "heldTpUseAnimation", "/heldTpUseAnimation", InsertMode.SetIfMissing,
                "\"hoe\"",
                "Third-person use animation."),
            new JsonInsertOption("Rendering", "heldTpHitAnimation", "/heldTpHitAnimation", InsertMode.SetIfMissing,
                "\"swordhit\"",
                "Third-person hit animation."),
            new JsonInsertOption("Rendering", "heldTpIdleAnimation", "/heldTpIdleAnimation", InsertMode.SetIfMissing,
                "\"holdbothhandslarge\"",
                "Third-person idle animation."),

            // (Handbook moved to its own tab)
        };
    }

    public static IEnumerable<JsonInsertOption> BlockOptions()
    {
        // From Modding:Block_Json_Properties (hard-coded subset)
        return new[]
        {
            new JsonInsertOption("Core", "code", "/code", InsertMode.Overwrite, "\"myblock\"", "Unique identifier for the block."),
            new JsonInsertOption("Core", "enabled", "/enabled", InsertMode.SetIfMissing, "true", "Whether the block is loaded."),
            new JsonInsertOption("Core", "variantgroups", "/variantgroups", InsertMode.SetIfMissing,
                "[{ \"code\": \"type\", \"states\": [\"a\", \"b\"] }]",
                "Define block variants."),
            new JsonInsertOption("Core", "allowedVariants", "/allowedVariants", InsertMode.SetIfMissing, "[]", "Trim generated variants."),
            new JsonInsertOption("Core", "skipVariants", "/skipVariants", InsertMode.SetIfMissing, "[]", "Skip specific variants."),

            new JsonInsertOption("Specific", "class", "/class", InsertMode.SetIfMissing, "\"Block\"", "C# block class for special behavior."),
            new JsonInsertOption("Specific", "entityclass", "/entityclass", InsertMode.SetIfMissing, "\"BlockEntityGeneric\"", "Block entity class."),
            new JsonInsertOption("Specific", "behaviors (append)", "/behaviors", InsertMode.AppendToArray,
                "{ \"name\": \"Falling\" }",
                "Append a BlockBehavior entry."),
            new JsonInsertOption("Specific", "entityBehaviors (append)", "/entityBehaviors", InsertMode.AppendToArray,
                "{ \"name\": \"MechanicalPower\" }",
                "Append a BlockEntityBehavior entry."),

            new JsonInsertOption("Physics/Mining", "blockmaterial", "/blockmaterial", InsertMode.SetIfMissing,
                "\"Stone\"",
                "Mining material category."),
            new JsonInsertOption("Physics/Mining", "matterstate", "/matterstate", InsertMode.SetIfMissing,
                "\"solid\"",
                "solid/liquid/etc (used for special cases)."),
            new JsonInsertOption("Physics/Mining", "resistance", "/resistance", InsertMode.SetIfMissing,
                "6",
                "Break time baseline."),
            new JsonInsertOption("Physics/Mining", "requiredminingtier", "/requiredminingtier", InsertMode.SetIfMissing,
                "0",
                "Minimum tier to get drops."),

            new JsonInsertOption("Interaction", "climbable", "/climbable", InsertMode.SetIfMissing,
                "false",
                "Walk into it to climb (ladder)."),
            new JsonInsertOption("Interaction", "rainpermeable", "/rainpermeable", InsertMode.SetIfMissing,
                "false",
                "Whether rain falls through."),

            new JsonInsertOption("Rendering", "textures", "/textures", InsertMode.SetIfMissing,
                "{ \"all\": { \"base\": \"block/myblock\" } }",
                "World textures map."),
            new JsonInsertOption("Rendering", "texturesinventory", "/texturesinventory", InsertMode.SetIfMissing,
                "{ \"all\": { \"base\": \"block/myblock\" } }",
                "Inventory textures override."),
            new JsonInsertOption("Rendering", "shape", "/shape", InsertMode.SetIfMissing,
                "{ \"base\": \"block/myshape\" }",
                "World shape."),
            new JsonInsertOption("Rendering", "shapeinventory", "/shapeinventory", InsertMode.SetIfMissing,
                "{ \"base\": \"block/myshape-inv\" }",
                "Inventory shape."),

            new JsonInsertOption("Liquids", "waterTightContainerProps", "/waterTightContainerProps", InsertMode.SetIfMissing,
                "{ \"containable\": true, \"itemsPerLitre\": 1, \"texture\": \"block/water\" }",
                "Liquid container props (rare for blocks)."),
        };
    }

    public static IEnumerable<JsonInsertOption> EntityOptions()
    {
        // From Modding:Entity_Json_Properties/en (hard-coded subset)
        return new[]
        {
            new JsonInsertOption("Core", "code", "/code", InsertMode.Overwrite, "\"myentity\"", "Unique identifier for the entity."),
            new JsonInsertOption("Core", "enabled", "/enabled", InsertMode.SetIfMissing, "true", "Whether the entity is loaded."),
            new JsonInsertOption("Core", "variantgroups", "/variantgroups", InsertMode.SetIfMissing,
                "[{ \"code\": \"type\", \"states\": [\"a\", \"b\"] }]",
                "Define entity variants."),

            new JsonInsertOption("Common", "class", "/class", InsertMode.SetIfMissing, "\"entity\"", "C# entity class."),
            new JsonInsertOption("Common", "habitat", "/habitat", InsertMode.SetIfMissing, "\"land\"", "land/sea/air."),
            new JsonInsertOption("Common", "hitboxsize", "/hitboxsize", InsertMode.SetIfMissing,
                "{ \"x\": 0.125, \"y\": 0.125 }",
                "Size of hitbox."),
            new JsonInsertOption("Common", "eyeheight", "/eyeheight", InsertMode.SetIfMissing, "0.1", "Eye height from bottom."),
            new JsonInsertOption("Movement", "canclimb", "/canclimb", InsertMode.SetIfMissing, "false", "Can climb ladders."),
            new JsonInsertOption("Movement", "canclimbanywhere", "/canclimbanywhere", InsertMode.SetIfMissing, "false", "Can climb any block."),
            new JsonInsertOption("Movement", "falldamage", "/falldamage", InsertMode.SetIfMissing, "true", "Takes fall damage."),
            new JsonInsertOption("Movement", "climbtouchdistance", "/climbtouchdistance", InsertMode.SetIfMissing, "0.5", "Climb touch distance."),
            new JsonInsertOption("Movement", "rotatemodelonclimb", "/rotatemodelonclimb", InsertMode.SetIfMissing, "false", "Rotate model while climbing."),
            new JsonInsertOption("Combat", "knockbackresistance", "/knockbackresistance", InsertMode.SetIfMissing, "0.0", "Higher = less knockback."),

            new JsonInsertOption("Data", "attributes", "/attributes", InsertMode.SetIfMissing,
                "{ \"attackPower\": 10 }",
                "Custom attributes (final)."),
            new JsonInsertOption("Audio", "sounds", "/sounds", InsertMode.SetIfMissing,
                "{ \"idle\": \"game:sounds/creature/idle\" }",
                "Entity sounds mapping."),
            new JsonInsertOption("Audio", "idlesoundchance", "/idlesoundchance", InsertMode.SetIfMissing, "0.3", "Idle sound chance."),
            new JsonInsertOption("Audio", "idlesoundrange", "/idlesoundrange", InsertMode.SetIfMissing, "24", "Idle sound range."),

            new JsonInsertOption("Drops", "drops (empty)", "/drops", InsertMode.SetIfMissing, "[]", "No drops."),
            new JsonInsertOption("Drops", "drops (append example)", "/drops", InsertMode.AppendToArray,
                "{ \"type\": \"item\", \"code\": \"game:stick\", \"quantity\": { \"avg\": 0.2, \"var\": 0 } }",
                "Append a drop entry."),

            new JsonInsertOption("Client", "client", "/client", InsertMode.SetIfMissing, "{ }", "Client-only section."),
            new JsonInsertOption("Client", "client.renderer", "/client/renderer", InsertMode.SetIfMissing, "\"Shape\"", "Renderer system name."),
            new JsonInsertOption("Client", "client.texture", "/client/texture", InsertMode.SetIfMissing, "{ \"base\": \"entity/myentity\" }", "Override base texture."),

            new JsonInsertOption("Animation", "animations (empty)", "/animations", InsertMode.SetIfMissing, "[]", "Animation list."),
            new JsonInsertOption("Animation", "animations (append example)", "/animations", InsertMode.AppendToArray,
                "{ \"code\": \"walk\", \"animation\": \"walk\", \"weight\": 1.0, \"animationspeed\": 1.0, \"mulwithwalkspeed\": true }",
                "Append an animation entry."),

            new JsonInsertOption("Server", "server", "/server", InsertMode.SetIfMissing, "{ }", "Server-only section."),
            new JsonInsertOption("Server", "server.attributes", "/server/attributes", InsertMode.SetIfMissing, "{ }", "Server-only attributes."),
        };
    }

    public static IEnumerable<JsonInsertOption> RecipeOptions()
    {
        // From JSON Reference (apidocs): GridRecipe + CookingRecipe (hard-coded)
        return new[]
        {
            // Templates (replace root)
            new JsonInsertOption("Templates", "GridRecipe template", "/", InsertMode.ReplaceRoot,
@"{
  ""ingredientPattern"": ""GS,S_"",
  ""ingredients"": {
    ""G"": { ""type"": ""item"", ""code"": ""drygrass"" },
    ""S"": { ""type"": ""item"", ""code"": ""stick"" }
  },
  ""width"": 2,
  ""height"": 2,
  ""output"": { ""type"": ""item"", ""code"": ""firestarter"" }
}",
                "Replace JSON with a GridRecipe skeleton."),

            new JsonInsertOption("Templates", "CookingRecipe template", "/", InsertMode.ReplaceRoot,
@"{
  ""code"": ""mymeal"",
  ""perishableProps"": {
    ""freshHours"": { ""avg"": 1080 },
    ""transitionHours"": { ""avg"": 180 },
    ""transitionRatio"": 1,
    ""transitionedStack"": { ""type"": ""item"", ""code"": ""rot"" }
  },
  ""shape"": { ""base"": ""block/food/meal/mymeal"" },
  ""ingredients"": [
    { ""code"": ""fruit"", ""validStacks"": [ { ""type"": ""item"", ""code"": ""fruit-*"", ""shapeElement"": ""bowl/fruit"" } ], ""minQuantity"": 2, ""maxQuantity"": 2 }
  ]
}",
                "Replace JSON with a CookingRecipe skeleton."),

            // GridRecipe fields
            new JsonInsertOption("GridRecipe", "ingredientPattern", "/ingredientPattern", InsertMode.SetIfMissing,
                "\"GS,S_\"",
                "Pattern (commas for rows, _ for empty)."),
            new JsonInsertOption("GridRecipe", "ingredients", "/ingredients", InsertMode.SetIfMissing,
                "{ \"A\": { \"type\": \"item\", \"code\": \"game:stick\" } }",
                "Dictionary used by pattern symbols."),
            new JsonInsertOption("GridRecipe", "output", "/output", InsertMode.SetIfMissing,
                "{ \"type\": \"item\", \"code\": \"game:stick\", \"quantity\": 1 }",
                "Resulting stack."),
            new JsonInsertOption("GridRecipe", "width", "/width", InsertMode.SetIfMissing, "3", "Required grid width."),
            new JsonInsertOption("GridRecipe", "height", "/height", InsertMode.SetIfMissing, "3", "Required grid height."),
            new JsonInsertOption("GridRecipe", "shapeless", "/shapeless", InsertMode.SetIfMissing, "false", "Ignore order."),
            new JsonInsertOption("GridRecipe", "enabled", "/enabled", InsertMode.SetIfMissing, "true", "Load recipe?"),
            new JsonInsertOption("GridRecipe", "name", "/name", InsertMode.SetIfMissing, "\"myrecipe\"", "Recipe name for logging/handbook."),
            new JsonInsertOption("GridRecipe", "attributes", "/attributes", InsertMode.SetIfMissing, "{ }", "Optional attributes blob."),
            new JsonInsertOption("GridRecipe", "allowedVariants", "/allowedVariants", InsertMode.SetIfMissing, "{ }", "Allowed variants map for {code}."),
            new JsonInsertOption("GridRecipe", "skipVariants", "/skipVariants", InsertMode.SetIfMissing, "{ }", "Skip variants map for {code}."),

            // CookingRecipe fields
            new JsonInsertOption("CookingRecipe", "code", "/code", InsertMode.SetIfMissing, "\"mymeal\"", "Unique meal/recipe code."),
            new JsonInsertOption("CookingRecipe", "ingredients (append)", "/ingredients", InsertMode.AppendToArray,
                "{ \"code\": \"fruit\", \"validStacks\": [ { \"type\": \"item\", \"code\": \"fruit-*\", \"shapeElement\": \"bowl/fruit\" } ], \"minQuantity\": 1, \"maxQuantity\": 2 }",
                "Append a CookingRecipeIngredient."),
            new JsonInsertOption("CookingRecipe", "perishableProps", "/perishableProps", InsertMode.SetIfMissing,
                "{ \"freshHours\": { \"avg\": 1080 }, \"transitionHours\": { \"avg\": 180 }, \"transitionRatio\": 1, \"transitionedStack\": { \"type\": \"item\", \"code\": \"rot\" } }",
                "Required transitionable properties (expiry)."),
            new JsonInsertOption("CookingRecipe", "shape", "/shape", InsertMode.SetIfMissing,
                "{ \"base\": \"block/food/meal/mymeal\" }",
                "Required cooking pot shape."),
            new JsonInsertOption("CookingRecipe", "enabled", "/enabled", InsertMode.SetIfMissing,
                "true",
                "Load recipe?"),
            new JsonInsertOption("CookingRecipe", "cooksInto", "/cooksInto", InsertMode.SetIfMissing,
                "{ \"type\": \"item\", \"code\": \"game:rot\", \"quantity\": 1 }",
                "Convert ingredients into a specific stack."),
            new JsonInsertOption("CookingRecipe", "isFood", "/isFood", InsertMode.SetIfMissing,
                "false",
                "If true and cooksInto set, won’t dirty the pot."),
        };
    }
}
