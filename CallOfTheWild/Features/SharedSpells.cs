using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{


    [Harmony12.HarmonyPatch(typeof(AbilityData))]
    [Harmony12.HarmonyPatch("TargetAnchor", Harmony12.MethodType.Getter)]
    class AbilityData__TargetAnchor__Getter__Patch
    {
        static bool Prefix(AbilityData __instance, ref AbilityTargetAnchor __result)
        {
            Main.TraceLog();
            if ((UnityEngine.Object)__instance.SourceItemUsableBlueprint != (UnityEngine.Object)null && __instance.SourceItemUsableBlueprint.Type == UsableItemType.Potion && !__instance.PotionForOther || __instance.IsAlchemistSpell && !__instance.AlchemistInfusion)
                __result = AbilityTargetAnchor.Owner;
            else if (__instance.Blueprint.Range == AbilityRange.Personal)
                __result = __instance.AlchemistInfusion || SharedSpells.canShareSpell(__instance) ? AbilityTargetAnchor.Unit : AbilityTargetAnchor.Owner;
            else if (!__instance.Blueprint.CanTargetFriends && !__instance.Blueprint.CanTargetEnemies && !__instance.Blueprint.CanTargetPoint)
                __result =  __instance.AlchemistInfusion ? AbilityTargetAnchor.Unit : AbilityTargetAnchor.Owner;
            else __result =  __instance.Blueprint.CanTargetPoint ? AbilityTargetAnchor.Point : AbilityTargetAnchor.Unit;

            return false;
        }
    }


    [Harmony12.HarmonyPatch(typeof(AbilityData))]
    [Harmony12.HarmonyPatch("CanTarget", Harmony12.MethodType.Normal)]
    class AbilityData__CanTarget__Patch
    {
        static bool Prefix(AbilityData __instance, TargetWrapper target, ref bool __result)
        {
            Main.TraceLog();
            foreach (IAbilityTargetChecker targetChecker in __instance.Blueprint.TargetCheckers)
            {
                if (!targetChecker.CanTarget(__instance.Caster.Unit, target))
                {
                    __result = false;
                    return false;
                }

            }

            if ((__instance.Caster.Buffs.HasFact(SharedSpells.can_only_target_self_buff) && Common.isPersonalSpell(__instance)
                || __instance.HasMetamagic((Metamagic)MetamagicFeats.MetamagicExtender.ImprovedSpellSharing)) 
                  && target.Unit != __instance.Caster.Unit)
            {
                __result = false;
                return false;
            }


            if (!__instance.Blueprint.CanTargetSelf && target.Unit == __instance.Caster.Unit)
            {
                __result = false;
                return false;
            }

            if (__instance.TargetAnchor == AbilityTargetAnchor.Unit && __instance.Blueprint.Range == AbilityRange.Personal && !__instance.AlchemistInfusion)
            {//if spell was orginally personal - check validity of sharing it with target
                __result = SharedSpells.isValidShareSpellTarget(target.Unit, __instance.Caster);
                return false;
            }

            switch (__instance.TargetAnchor)
            {
                case AbilityTargetAnchor.Owner:
                    if (target.IsUnit)
                        __result = target.Unit == __instance.Caster.Unit;
                    break;
                case AbilityTargetAnchor.Unit:
                    __result = target.IsUnit && (__instance.Blueprint.CanTargetFriends || target.Unit.IsPlayerFaction || __instance.Caster.Unit.CanAttack(target.Unit));
                    break;
                case AbilityTargetAnchor.Point:
                    __result = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return false;
        }
    }



    public class SharedSpells
    {
        internal static LibraryScriptableObject library => Main.library;
        public static BlueprintFeature ac_share_spell;
        public static BlueprintFeature familiar_share_spell;
        public static BlueprintFeature bonded_mind_feat;
        public static BlueprintFeature share_spells_feat;
        public static BlueprintBuff can_only_target_self_buff;

        public static BlueprintFeatureSelection[] ac_selections;
        public static BlueprintFeatureSelection familiar_selection = library.Get<BlueprintFeatureSelection>("363cab72f77c47745bf3a8807074d183"); //rogue familiar
                                                                                                     
            

        internal static void preload()
        {
            createCanOnlyTargetSelfBuff();
        }

        internal static void load()
        {
           ac_selections = new BlueprintFeatureSelection[]{Hunter.hunter_animal_companion,
                                                            library.Get<BlueprintFeatureSelection>("2ecd6c64683b59944a7fe544033bb533"), //domain
                                                            library.Get<BlueprintFeatureSelection>("90406c575576aee40a34917a1b429254"), //base
                                                            library.Get<BlueprintFeatureSelection>("571f8434d98560c43935e132df65fe76"), //druid
                                                            library.Get<BlueprintFeatureSelection>("738b59d0b58187f4d846b0caaf0f80d7"), //maddog
                                                            library.Get<BlueprintFeatureSelection>("ee63330662126374e8785cc901941ac7"), //ranger
                                                            library.Get<BlueprintFeatureSelection>("2995b36659b9ad3408fd26f137ee2c67"), //sacred huntsmaster
                                                            library.Get<BlueprintFeatureSelection>("a540d7dfe1e2a174a94198aba037274c"), //sylvan sorcerer
                                                            Shaman.nature_spirit.true_spirit_ability as BlueprintFeatureSelection,
                                                            Shaman.nature_spirit.true_spirit_ability_wandering as BlueprintFeatureSelection,
                                                            Oracle.animal_companion,
                                                            Summoner.eidolon_selection,
                                                            Summoner.lesser_eidolon_selection,
                                                            Summoner.twinned_eidolon_selection,
                                                            Summoner.fey_eidolon_selection,
                                                            Summoner.infernal_eidolon_selection,
                                                            Spiritualist.emotional_focus_selection,
                                                            Shaman.drake_companion,
                                                            Archetypes.DraconicDruid.drake_companion,
                                                            Warpriest.animal_companion
                                                            };
            createClassSharedSpell();
            createSharedSpellFeat();
            fixAcSpellTargetting();
            fixSpells();
        }


        static void fixSpells()
        {
            var mirror_image = library.Get<BlueprintAbility>("3e4ab69ada402d145a5e0ad3ad4b8564");
            (mirror_image.GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionApplyBuff).ToCaster = false;


            var fire_belly = library.Get<BlueprintAbility>("b065231094a21d14dbf1c3832f776871");
            (fire_belly.GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionApplyBuff).ToCaster = false;

            //fix anchors
            //shield
            library.Get<BlueprintAbility>("ef768022b0785eb43a18969903c537c4").GetComponent<AbilitySpawnFx>().Anchor = AbilitySpawnFxAnchor.SelectedTarget;
            //bless
            library.Get<BlueprintAbility>("90e59f4a4ada87243b7b3535a06d0638").GetComponent<AbilitySpawnFx>().Anchor = AbilitySpawnFxAnchor.ClickedTarget;

            //make spells unshareable
            library.Get<BlueprintAbility>("75de4ded3e731dc4f84d978fe947dc67").AddComponent(Helpers.Create<SharedSpells.CannotBeShared>()); //acid maw
            library.Get<BlueprintAbility>("90e59f4a4ada87243b7b3535a06d0638").AddComponent(Helpers.Create<SharedSpells.CannotBeShared>()); //bless
            var spells_ids_to_allow_working_on_pets = new string[] { "c60969e7f264e6d4b84a1499fdcf9039", //enlarge
                                                                     "4e0e9aba6447d514f88eff1464cc4763" }; //reduce 

            foreach (var s in spells_ids_to_allow_working_on_pets)
            {
                var spell = library.Get<BlueprintAbility>(s);
                var check_component = spell.GetComponent<AbilityTargetHasFact>();
                spell.ReplaceComponent(check_component, Helpers.Create<CompanionMechanics.AbilityTargetHasFactUnlessPet>(a => { a.CheckedFacts = check_component.CheckedFacts; a.Inverted = check_component.Inverted; }));
            }
        }

        private static void createCanOnlyTargetSelfBuff()
        {
            can_only_target_self_buff = Helpers.CreateBuff("CanOnlyTargetSelfBuff",
                                                           "Target Self",
                                                           "",
                                                           "",
                                                           null,
                                                           null);
            can_only_target_self_buff.SetBuffFlags(BuffFlags.HiddenInUi);
        }


        private static void fixAcSpellTargetting()
        {
            //dissalow casting spells on animal compnions
            var spells = new BlueprintAbility[] {library.Get<BlueprintAbility>("75de4ded3e731dc4f84d978fe947dc67"), //acid maw
                                                 library.Get<BlueprintAbility>("3e9d1119d43d07c4c8ba9ebfd1671952"), //huricane bow
                                                 library.Get<BlueprintAbility>("779179912e6c6fe458fa4cfb90d96e10") //lead blades
                                                };
            var animal_feature = library.Get<BlueprintFeature>("a95311b3dc996964cbaa30ff9965aaf6");
            foreach (var s in spells)
            {
                s.AddComponent(Common.createAbilityTargetHasFact(true, animal_feature));
            }

        }

        private static void createClassSharedSpell()
        {
            BlueprintAbility magic_fang = library.Get<BlueprintAbility>("403cf599412299a4f9d5d925c7b9fb33");
            ac_share_spell = Helpers.CreateFeature("ShareSpellAnimalCompanion",
                                         "Share Spells (Companion)",
                                         "You may cast a spell with a range of Personal on your companion (as a touch range spell) instead of on yourself. This ability does not allow the companion to share abilities that are not spells, even if they function like spells.",
                                         "",
                                         magic_fang.Icon,
                                         FeatureGroup.None);

            familiar_share_spell = Helpers.CreateFeature("ShareSpellFamiliar",
                                         "Share Spells (Familiar)",
                                         "You may cast a spell with a range of Personal on your familiar (as a touch spell) instead of on himself.",
                                         "",
                                         magic_fang.Icon,
                                         FeatureGroup.None);


            foreach (var selection in ac_selections)
            {
                selection.AddComponent(Helpers.CreateAddFact(ac_share_spell));
            }

            foreach (var f in familiar_selection.AllFeatures)
            {
                f.AddComponent(Helpers.CreateAddFact(familiar_share_spell));
            }
        }


        private static void createSharedSpellFeat()
        {
            bonded_mind_feat = Helpers.CreateFeature("BindedMindFeat",
                                         "Bonded Mind",
                                         "You and your partner are so close that you can almost read each other’s minds. You can be target of the spells with range of Personal of any of your companions who have Share Spells feat.\n"
                                         + "Note: you can not benefit from this feat or any other feat that uses it as a prerequisite unless you permanently posses them.",
                                         "",
                                         LoadIcons.Image2Sprite.Create(@"FeatIcons/Icon_Bonded_Mind.png"),
                                         FeatureGroup.Feat);
            bonded_mind_feat.Groups = new FeatureGroup[] { FeatureGroup.Feat, FeatureGroup.TeamworkFeat };
            share_spells_feat = Helpers.CreateFeature("SharedSpellFeat",
                                                     "Share Spells",
                                                     "You can cast a spell with a range of Personal on an ally as a touch spell, as per the share spells familiar ability, so long as the ally possesses the Bonded Mind feat.",
                                                     "",
                                                     LoadIcons.Image2Sprite.Create(@"FeatIcons/Icon_Share_Spells.png"),
                                                     FeatureGroup.Feat,
                                                     Helpers.PrerequisiteFeature(bonded_mind_feat),
                                                     Helpers.PrerequisiteFeaturesFromList(new BlueprintFeature[] { ac_share_spell, familiar_share_spell }, true)
                                                     );
            share_spells_feat.Groups = new FeatureGroup[]{ FeatureGroup.Feat, FeatureGroup.TeamworkFeat };
            library.AddFeats(bonded_mind_feat, share_spells_feat);

            //add these teamwork feats to feat sharing feats
            Common.addTemworkFeats(bonded_mind_feat, false);
            Common.addTemworkFeats(share_spells_feat, false);
        }




        public static bool canShareSpell(AbilityData ability_data)
        {
            if (ability_data.Blueprint.GetComponent<CannotBeShared>() != null)
            {
                return false;
            }
            if (ability_data.Blueprint.Type != AbilityType.Spell)
            {
                return false;
            }
            return (ability_data.Caster.HasFact(ac_share_spell) || ability_data.Caster.HasFact(share_spells_feat))
                    && ability_data.Spellbook != null;
        }
    
        public static bool isValidShareSpellTarget(UnitEntityData target, UnitDescriptor caster)
        {

            return caster.Pet == target && caster.HasFact(ac_share_spell)
                   || caster.Unit == target
                   || caster.HasFact(share_spells_feat) && target.Descriptor.HasFact(bonded_mind_feat);
        }




        [AllowedOn(typeof(BlueprintAbility))]
        public class CannotBeShared : BlueprintComponent
        {

        }

    }
}
