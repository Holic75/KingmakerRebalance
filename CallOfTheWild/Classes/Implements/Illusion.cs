using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild
{
    public partial class ImplementsEngine
    {
        BlueprintFeature createColorBeam()
        {
            var blinding_ray = library.Get<BlueprintAbility>("9b4d07751dd104243a94b495c571c9dd");

            var ability = library.CopyAndAdd(blinding_ray, prefix + "ColorBeam", "");

            ability.RemoveComponents<AbilityResourceLogic>();
            ability.RemoveComponents<ContextRankConfig>();
            ability.AddComponent(createClassScalingConfig(type: AbilityRankType.DamageDice));
            ability.AddComponent(resource.CreateResourceLogic());
            Common.addSpellDescriptor(ability, SpellDescriptor.MindAffecting);
            ability.SetNameDescription("Color Beam",
                                       "As a standard action, you can expend 1 point of mental focus to unleash a beam of cascading colors at any one target within 30 feet. Doing so requires a ranged touch attack. If the beam hits, the target is blinded for 1 round if it has a number of Hit Dice equal to or lower than your occultist level. A foe with a number of Hit Dice greater than your occultist level is instead dazzled for 1 round.\n"
                                       + "This is a mind-affecting illusion effect."
                                       );
 
            return Common.AbilityToFeature(ability, false);
        }


        BlueprintFeature createShadowBeast()
        {
            var wizard_spelllist = library.Get<BlueprintSpellList>("ba0401fdeb4062f40a7aa95b6f07fe89");
            var ability = Helpers.CreateAbility(prefix + "ShadowBeasts",
                                               "Shadow Beasts",
                                               "As a standard action, you can call forth one or more beasts made of shadow by expending 1 point of mental focus. This functions as shadow conjuration, but it can be used to duplicate only the effects of summon monster spells. Creatures created with this spell deal 60% of the normal damage to those that disbelieve the illusion, and their nondamaging effects have only a 60% chance of affecting disbelieving targets. This can be used to duplicate any summon monster spell up to summon monster V. For every 2 additional levels you possess beyond 9th, the maximum spell level you can duplicate with this ability increases by 1 (to a maximum of summon monster IX at 17th level). ",
                                               "",
                                               LoadIcons.Image2Sprite.Create(@"AbilityIcons/StormOfSouls.png"),
                                               AbilityType.SpellLike,
                                               UnitCommand.CommandType.Standard,
                                               AbilityRange.Unlimited,
                                               Helpers.roundsPerLevelDuration,
                                               "Will disbelief",
                                               Helpers.CreateSpellComponent(SpellSchool.Illusion),
                                               Helpers.CreateSpellDescriptor(SpellDescriptor.Summoning | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow60)
                                               );

            var spell_guids = new string[]
            {
                "8fd74eddd9b6c224693d9ab241f25e84",//summon monster 1
                "1724061e89c667045a6891179ee2e8e7",//summon elemental small
                "970c6db48ff0c6f43afc9dbb48780d03",//summon monster 2
                "5d61dde0020bbf54ba1521f7ca0229dc",//summon monster 3
                "e42b1dbff4262c6469a9ff0a6ce730e3",//summon elemental medium
                "7ed74a3ec8c458d4fb50b192fd7be6ef",//summon monster 4
                "89404dd71edc1aa42962824b44156fe5",//summon elemental large
                "630c8b85d9f07a64f917d79cb5905741",//summon monster 5
                "766ec978fa993034f86a372c8eb1fc10",//summon elemental huge
                "e740afbab0147944dab35d83faa0ae1c",//summon monster 6
                "8eb769e3b583f594faabe1cfdb0bb696",//summon elemental greater
                "ab167fd8203c1314bac6568932f1752f",//summon monster 7
                "8a7f8c1223bda1541b42fd0320cdbe2b",//summon elder elemental
                "d3ac756a229830243a72e84f3ab050d0",//summon monster 8
                "52b5df2a97df18242aec67610616ded0",//suumon monster 9
            };

            List<BlueprintAbility> abilities = new List<BlueprintAbility>();

            foreach (var id in spell_guids)
            {
                var spell = library.Get<BlueprintAbility>(id);
                var lvl = spell.GetComponents<SpellListComponent>().FirstOrDefault(f => f.SpellList == wizard_spelllist).SpellLevel;

                var spells = spell.HasVariants ? spell.Variants : new BlueprintAbility[] {spell };
                foreach (var s in spells)
                {
                    var sp = Common.convertToSpellLike(s, prefix, classes, stat, resource, archetypes: getArchetypeArray());
                    Common.addSpellDescriptor(sp, (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow60);
                    sp.RemoveComponents<SpellComponent>();
                    sp.AddComponent(Helpers.CreateSpellComponent(SpellSchool.Illusion));
                    sp.AddComponent(Helpers.Create<NewMechanics.AbilityShowIfHasClassLevels>(a => { a.character_classes = classes; a.level = lvl * 2 + 1; }));
                    sp.SetName("Shadow Beasts: " + sp.Name);
                    Common.unsetAsFullRoundAction(sp);
                    abilities.Add(sp);
                }
            }

            ability.AddComponent(Helpers.CreateAbilityVariants(ability, abilities));

            var feature = Common.AbilityToFeature(ability, false);
            addMinLevelPrerequisite(feature, 9);
            return feature;
        }


        BlueprintFeature createUnseen()
        {
            var greater_invisibility = library.Get<BlueprintAbility>("ecaa0def35b38f949bd1976a6c9539e0");

            var ability = Common.convertToSpellLikeVariants(greater_invisibility, prefix, classes, stat, resource, archetypes: getArchetypeArray(), self_only: true);
            ability.AddComponent(createClassScalingConfig(min: 10, max: 10));

            ability.SetNameDescription("Unseen",
                                       "As a standard action, you can expend 1 point of mental focus to become invisible, as greater invisibility. This effect lasts for 1 minute.\n"
                                       + "You can expend 2 points of mental focus instead of 1 to use this power on a willing adjacent creature instead of yourself.\n"
                                       + "You must be at least 7th level to select this focus power."
                                       );

            var ability2 = library.CopyAndAdd(ability, ability.Name + "Others", "");
            ability2.Range = AbilityRange.Touch;
            ability2.setMiscAbilityParametersTouchFriendly();
            ability2.ReplaceComponent<AbilityResourceLogic>(a => a.Amount = 2);
            ability2.SetName("Unseen (Others)");
            var wrapper = Common.createVariantWrapper(prefix + "UnseenBase", "", ability, ability2);
            wrapper.AddComponent(Helpers.CreateSpellComponent(SpellSchool.Illusion));

            var feature = Common.AbilityToFeature(wrapper, false);
            addMinLevelPrerequisite(feature, 7);
            return feature;
        }
    }
}
