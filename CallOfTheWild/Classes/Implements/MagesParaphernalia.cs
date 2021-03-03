using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
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
        public BlueprintFeature createSpellPower(BlueprintCharacterClass character_class)
        {
            var feature = Helpers.CreateFeature(prefix + "SpellPowerFeature",
                                                  "Spell Power",
                                                  "As a free action while casting a spell, you can expend 2 points of mental focus to increase that spell’s caster level by 2. At 12th and 18th levels, whenever you use this ability, you can spend an additional point of mental focus in order to increase the spell’s caster level by an additional 1.",
                                                  "",
                                                  Shaman.font_of_spirit_magic.Icon,
                                                  FeatureGroup.None
                                                  );
            for (int i = 0; i < 3; i++)
            {
                var buff = Helpers.CreateBuff(prefix + $"SpellPowerFeature{i + 1}Buff",
                                              $"Spell Power (+{2 + i})",
                                              feature.Description,
                                              "",
                                              feature.Icon,
                                              null,
                                              Helpers.Create<NewMechanics.SpendResourceOnSpecificSpellCast>(s => { s.resource = resource; s.specific_class = character_class; s.spell_descriptor = SpellDescriptor.None; s.amount = 2 + i; }),
                                              Helpers.Create<NewMechanics.ContextIncreaseDescriptorSpellLevel>(c => { c.specific_class = character_class; c.Descriptor = SpellDescriptor.None; c.Value = i + 2; })
                                              );

                var toggle = Common.buffToToggle(buff, UnitCommand.CommandType.Free, true,
                                                 resource.CreateActivatableResourceLogic(spendType: ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                                 );
                toggle.AddComponent(Helpers.Create<ResourceMechanics.RestrictionHasEnoughResource>(r => { r.resource = resource; r.amount = 2 + i; }));
                addFocusInvestmentCheck(toggle, SpellSchool.Divination, SpellSchool.Evocation, SpellSchool.Necromancy);
                toggle.Group = ActivatableAbilityGroupExtension.SpellPower.ToActivatableAbilityGroup();

                if (i == 0)
                {
                    feature.AddComponent(Helpers.CreateAddFact(toggle));
                }
                else
                {
                    feature.AddComponent(Helpers.CreateAddFeatureOnClassLevel(Common.ActivatableAbilityToFeature(toggle), i == 0 ? 1 : 6 + i*6, new BlueprintCharacterClass[] { character_class }));
                }
            }
            return feature;
        }


        public BlueprintFeature createMetamagicMaster(BlueprintCharacterClass caster_class)
        {
            var feature = Helpers.CreateFeature(prefix + "MetamagicMasterFeature",
                                                 "Metamagic Master",
                                                 "As a free action while casting a spell, you can expend 1 or more points of mental focus to apply a metamagic feat you know to that spell without increasing the spell’s casting time or the spell level of the spell slot it occupies. The number of points of mental focus you must expend to use this power is equal to the increase in spell levels the metamagic feat would normally require (minimum 1).",
                                                 "",
                                                 LoadIcons.Image2Sprite.Create(@"AbilityIcons/ArcaneExploit.png"),
                                                 FeatureGroup.None
                                                 );
            BlueprintFeature[] metamagics = library.GetAllBlueprints().OfType<BlueprintFeature>().Where(b => b.Groups.Contains(FeatureGroup.WizardFeat) && (b.GetComponent<AddMetamagicFeat>() != null) && b.AssetGuid != "2f5d1e705c7967546b72ad8218ccf99c").ToArray();
            foreach (var m in metamagics)
            {                         
                var metamagic = m.GetComponent<Kingmaker.UnitLogic.FactLogic.AddMetamagicFeat>().Metamagic;
                var cost = MetamagicHelper.DefaultCost(metamagic);
                var buff = Helpers.CreateBuff(prefix + m.name + "MetamagicMasterBuff",
                                              "Metamagic Master: " + m.Name,
                                              feature.Description + "\n" + m.Name + ": " + m.Description,
                                              "",
                                              m.Icon,
                                              null,
                                              Helpers.Create<NewMechanics.MetamagicMechanics.MetamagicOnSpellDescriptor>(ms =>
                                              {
                                                  ms.spell_descriptor = SpellDescriptor.None;
                                                  ms.specific_class = caster_class;
                                                  ms.resource = resource;
                                                  ms.Metamagic = metamagic;
                                                  ms.amount = cost;
                                              }
                                              )
                                              );

                var toggle = Common.buffToToggle(buff, CommandType.Free, true,
                                                 resource.CreateActivatableResourceLogic(ActivatableAbilityResourceLogic.ResourceSpendType.Never),
                                                 Helpers.Create<ResourceMechanics.RestrictionHasEnoughResource>(r => { r.resource = resource; r.amount = cost; })
                                                 );

                toggle.Group = ActivatableAbilityGroupExtension.MetamagicMaster.ToActivatableAbilityGroup();
                addFocusInvestmentCheck(toggle, SpellSchool.Divination, SpellSchool.Evocation, SpellSchool.Necromancy);

                var feature1 = Common.ActivatableAbilityToFeature(toggle);

                feature.AddComponent(Helpers.Create<NewMechanics.AddFeatureIfHasFactsFromList>(a => { a.Feature = feature1; a.CheckedFacts = new Kingmaker.Blueprints.Facts.BlueprintUnitFact[] { m }; }));
                m.AddComponent(Helpers.Create<NewMechanics.AddFeatureIfHasFactsFromList>(a => { a.Feature = feature1; a.CheckedFacts = new Kingmaker.Blueprints.Facts.BlueprintUnitFact[] { feature }; }));
            }
      
            return feature;
        }


        public BlueprintFeature createMetamagicKnowledge(BlueprintAbilityResource reduced_resource)
        {
            var metamagics = library.GetAllBlueprints().OfType<BlueprintFeature>().Where(b => b.Groups.Contains(FeatureGroup.WizardFeat) && (b.GetComponent<AddMetamagicFeat>() != null)).ToArray();
            var feature = Helpers.CreateFeatureSelection(prefix + "MetamagicKnowledgeFeature",
                                                        "Metamagic Knowledge",
                                                        "You receive one metamagic feat.",
                                                        "",
                                                        null,
                                                        FeatureGroup.None
                                                        );
            feature.AllFeatures = metamagics;
            feature.AddComponent(Helpers.PrerequisiteNoFeature(feature));

            return feature;
        }


        public BlueprintBuff createScholarlyKnowledge()
        {
            var property = ImplementMechanics.InvestedImplementFocusAmountProperty.createProperty(prefix + "MagesParaphernaliaFocusProperty", "",
                                                                                                  createClassScalingConfig(ContextRankProgression.MultiplyByModifier, stepLevel: 2),
                                                                                                  false,
                                                                                                  SpellSchool.Divination, SpellSchool.Evocation, SpellSchool.Necromancy);
            var buff = Helpers.CreateBuff(prefix + "MagesParaphernaliaFocusBuff",
                                          "Scholarly Knowledge",
                                          "The panoply grants a +1 bonus on all Knowledge checks for every 4 points of total mental focus invested in all of the associated implements, to a maximum bonus equal to half the occultist’s level.",
                                          "",
                                          Helpers.GetIcon("55edf82380a1c8540af6c6037d34f322"),
                                          null,
                                          Helpers.CreateAddContextStatBonus(StatType.SkillLoreNature, ModifierDescriptor.UntypedStackable),
                                          Helpers.CreateAddContextStatBonus(StatType.SkillLoreReligion, ModifierDescriptor.UntypedStackable),
                                          Helpers.CreateAddContextStatBonus(StatType.SkillKnowledgeArcana, ModifierDescriptor.UntypedStackable),
                                          Helpers.CreateAddContextStatBonus(StatType.SkillKnowledgeWorld, ModifierDescriptor.UntypedStackable),
                                          Helpers.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, ContextRankProgression.DivStep, stepLevel: 4,
                                                                          customProperty: property)
                                          );
            return buff;
        }
    }
}
