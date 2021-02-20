using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic.FactLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    public class Deities
    {
        static Kingmaker.Blueprints.LibraryScriptableObject library => Main.library;
        static public BlueprintFeature lamashtu;
        static public BlueprintFeature kukri_proficiency;


        static internal void create()
        {
            lamashtu = library.CopyAndAdd<BlueprintFeature>("04bc2b62273ab744092d992ed72bff41", "LamashtuFeature", "");//rovagug

            lamashtu.SetNameDescriptionIcon("Lamashtu",
                                            "Lamashtu (pronounced lah-MAHSH-too) is the mother and patroness of many misshapen and malformed creatures that crawl, slither, or flap on, above, or below the surface of Golarion. Her unholy symbol is a three-eyed jackal head, which may be represented in many ways, and her sacred animal is the jackal.\n"
                                            + "Domains: Chaos, Evil, Madness, Strength, Trickery.\nFavored Weapon: Kukri.",
                                            LoadIcons.Image2Sprite.Create(@"FeatIcons/Lamashtu.png"));
            kukri_proficiency = library.CopyAndAdd<BlueprintFeature>("70ab8880eaf6c0640887ae586556a652", "KukriProficiency", "");
            kukri_proficiency.SetNameDescription("Weapon Proficiency (Kukri)",
                                                "You become proficient with kukris and can use them as a weapon.");
            kukri_proficiency.ReplaceComponent<AddProficiencies>(a => a.WeaponProficiencies = new Kingmaker.Enums.WeaponCategory[] { Kingmaker.Enums.WeaponCategory.Kukri });

            lamashtu.ReplaceComponent<AddFeatureOnClassLevel>(a => a.Feature = kukri_proficiency);
            lamashtu.ReplaceComponent<AddStartingEquipment>(a => a.CategoryItems = new Kingmaker.Enums.WeaponCategory[] { Kingmaker.Enums.WeaponCategory.Kukri });
            lamashtu.ReplaceComponent<AddFacts>(a => a.Facts = new Kingmaker.Blueprints.Facts.BlueprintUnitFact[]
            {
                library.Get<BlueprintFeature>("8c7d778bc39fec642befc1435b00f613"), //chaos
                library.Get<BlueprintFeature>("351235ac5fc2b7e47801f63d117b656c"), //evil
                library.Get<BlueprintFeature>("c346bcc77a6613040b3aa915b1ceddec"), //madness
                library.Get<BlueprintFeature>("eaa368e08628a8641b16cd41cbd2cb33"), //trickery
                library.Get<BlueprintFeature>("58d2867520de17247ac6988a31f9e397"), //strength
                library.Get<BlueprintFeature>("dab5255d809f77c4395afc2b713e9cd6"), //allow channel negative
            });
            //lamashtu.AddComponent(Helpers.Create<RaceMechanics.PrerequisiteRace>(p => p.race = library.Get<BlueprintRace>("9d168ca7100e9314385ce66852385451")));

            var deities = library.Get<BlueprintFeatureSelection>("59e7a76987fe3b547b9cce045f4db3e4");
            deities.AllFeatures = deities.AllFeatures.AddToArray(lamashtu);

            var atheism = library.Get<BlueprintFeature>("92c0d2da0a836ce418a267093c09ca54");
            //forbid having more than one deity
            /*foreach (var d in deities.AllFeatures)
            {
                foreach (var dd in deities.AllFeatures)
                {
                    if (d != dd)
                    {
                        d.AddComponent(Helpers.PrerequisiteNoFeature(dd));
                    }
                    d.AddComponent(Helpers.PrerequisiteNoFeature(atheism));
                }
            }*/
        }
    }
}
