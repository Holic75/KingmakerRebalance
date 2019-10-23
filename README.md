# Call of the Wild

This is Pathfinder: Kingmaker mod.

It introduces new classes:  
- Hunter  with following archetypes: Divine Hunter, Forester and Feykiller,  
- Witch with with following archetypes: Ley Line Guardian, Hedge Witch and Hex Channeler,   
- Bloodrager with following archetypes: Metamagic Rager, Spelleater and Steelblood,
- Skald with following archetypes: Urban Skald, Herald of the Horn and War Drummer,
- Warpriest with following archetypes: Sacred Fist, Cult Leader and Champion of the Faith,
- Shaman.
- Hinterlander and Holy Vindicator prestige classes,
- Vindictive Bastard Paladin Archetype (implemented as a separate class).

Regarding skald mechanics: it is different from pnp in the fact that it allows other classes to stack their rages (with bonuses and rage powers)
with skald Inspiring Rage. Also an ability was added to reject rage either from skald Inspired Rage, Inspire Ferocity or Rage Spell; everyone should get it at lvl 1.

New feats:
- Extra Hex,
- Accursed Hex,
- Amplified Hex,
- Split Hex,
- Rage Casting,
- Raging Brutality,
- Blooded Arcane Strike,
- Riving Strike,
- Feral Combat Training,
- Furious Focus,
- Felling Smash,
- Strike True,
- Devastating Strike,
- Weapon of The Chosen, Improved Weapon of the Chosen, Greater Weapon of the Chosen,
- Planar Wild Shape,
- Powerful Shape,
- Coordinated Shot,
- Skald's Vigor and Greater Skald's Vigor,
- Discordant Voice,
- Stalwart and Improved Stalwart,
- Quick Channel,
- Channel Smite,
- Channeling Scourge,
- Versatile Channeler,
- Deadeye's Blessing,
- Guided Hand,
- Planar Focus,
- Bonded Mind and Share Spell.

New rage powers:
- Terrifying Howl
- Taunting Stance,
- Unrestrained Rage,
- Quick Reflexes,
- Superstition,
- Ghost Rager,
- Witch Hunter,
- Energy Resistance,
- Atavism Totem Lesser,
- Atavism Totem,
- Atavism Totem Greater,
- Spirit Totem, Lesser,
- Spirit Totem,
- Spirit Totem, Greater,
- Celestial Totem, Lesser,
- Celestial Totem,
- Celestial Totem, Greater,
- Daemon Totem, Lesser,
- Daemon Totem,
- Daemon Totem, Greater.

New features:
- Share Spell for animal companions,
- Druids can use armor while in wild shape (optional),
- Animal companions can now equip belts, bracers and amulets,
- Rerolls (due to Bit of Luck and similar abilities) are now shown in the battle log.


New spells:
- Command,
- Chill Touch,
- Frostbite,
- Hex Vulnerability,
- Obscuring Mist,
- Produce Flame,
- Sanctuary,
- Shillelagh,
- Blood Armor,
- Bone Fists,
- Burst of Radiance,
- Flame Blade,
- Flurry of Snowballs,
- Savage Maw,
- Force Sword,
- Ice Slick,
- Invisibility Purge,
- Righteous Vigor,
- Vine Strike,
- Winter's Grasp,
- Burning Entanglement,
- Countless Eyes,
- Deadly Juggernaut,
- Earth Tremor,
- Flame Arrow,
- Keen Edge,
- Sheet Lightning,
- Archon's Trumpet,
- Aura of Doom,
- Explosion of Rot,
- Fire Shield,
- River of Wind,
- Virtuoso Performance,
- Contingency,
- Poison Breath,
- Plant Shape I, II and III,
- Giant Form I and II,
- Blood Mist,
- Shapechange,
- Winds of Vengeance.  

Beast shape I - IV spells were also changed to be a bit more fun and more in line with pnp:
- Beast shape I allows to turn into wolf and leopard,  
- Beast shape II allows to turn into large bear (without rend) and large dire wolf (with trip),  
- Beast shape III allows to turn into large smilodon and Huge mastodon (with trample),  
- Beast shape IV allows to turn into large hodag (with toss) and large winter wolf (with trip and breath weapon)  

The changes corresponding to these spells as well as Plant Shape I, II and III were also applied to druid wildshape ability.


In addition mod also makes some changes to existing features to make them closer to pnp:
- nerfs animal compnions to make them closer to pnp : reduces physical prowess bonuses by factor of 2, natural ac bonuses by 1.5, and removes
enchanced attacks (optional),
- sets base skill points of every classes to 1/2 of pnp value, since most skills in the game correspond to 2 skills in pnp (optional),
- replaces favored enemy for Sacred Huntsmaster with animal focus as per pnp (optional),
- fixes Magic Vestment spell to work as per pnp rules,  
- Rangers now should get improvement of their favored terrain bonuses,
- Legendary proportions increases size by only one category and changes Natural Armor bonus to untyped (instead of enchancement),
- Animal Growth properly increases size of animal companion,
- Barbarian Increased Damage Reduction rage power works only during rage, same for Stalwart Defender,
- Empyreal Sorcerer Channel Energy ability depends now on Wisdom rather than Charisma,
- Channel Energy dc no longer scales with character level, but with class level that gave access to it.
- all flying creatures are immune to difficult terrain and ground based buff effects,
- flails and heavy flails critical multiplier was changed to x3 to compensate for missing trip and disarm properties (optional),
- medium range for spells was increased to 60 ft, long range to 100 ft,
- weapon swap consumes move action instead of standard,
- spell casting is not forbidden while in elemental form (optional).


Original game bug fixes:
- Vital Strike now takes standard action and extra damage dice are no longer multplied on critical hits,
- Singing Steel works properly,
- Archaelogist luck works with Lingering Performance and Singing Steel,
- Lingering Performance no longer allows to overlap different bardic performances.


Mod also changes stats of some npcs (optional)
- Valerie class was changed to Vindictive Bastard and stats to 18/10/16/8/10/15
- Harrim class was changed to Warpriest and stats to 16/10/18/10/17/6
- Amiri archetype was changed to Invulnerable Rager and her stats to 19/14/16/10/12/8
- Ekun to 14/18/10/10/14/8
- Tristian to 8/16/12/10/18/14 and repalces his primary domain to fire
- Linzi to 8/18/14/14/10/16
- Octavia to 10/16/10/18/10/14
- Regongar to 19/12/12/10/8/16
- Nok-Nok to 8/22/14/12/10/8


NOTE: Changes to animal companion bonuses, npc companions, and class skill points and some others are optional and can be disabled (set corresponding values to false,
in your_game_folder/Mods/CallOfTheWild/settings.json)

Install
- Download and install Unity Mod Manager﻿﻿ 0.13.0 or later
- Download the mod
- Build it using Visual Studio 2017 Community Edition or use prebuilt binaries from latest Releases (just drop archive into UMM GUI)
- Run the game

Big thanks to Nolanoth  for providing new icons for the mod.