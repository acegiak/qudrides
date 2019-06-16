using Qud.API;
using System;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts
{
	[Serializable]
	public class acegiak_Saddle : IPart
	{
        public String AltTile;
        public String SavedTile;

		public acegiak_Saddle()
		{
		}

		public override bool SameAs(IPart p)
		{
			return false;
		}


		public override void Register(GameObject Object)
		{
			Object.RegisterPartEvent(this, "Equipped");
			Object.RegisterPartEvent(this, "Unequipped");
			Object.RegisterPartEvent(this, "GetInventoryActions");
			// Object.RegisterPartEvent(this, "AddedToInventory");
			// Object.RegisterPartEvent(this, "Dropped");
			Object.RegisterPartEvent(this, "InvCommandHarness");
			Object.RegisterPartEvent(this, "ProjectileHit");
			Object.RegisterPartEvent(this, "ThrownProjectileHit");
			Object.RegisterPartEvent(this, "WeaponAfterDamage");
			base.Register(Object);
		}
        public void Harness(GameObject harnesser,GameObject harnessee){
			IPart.AddPlayerMessage(harnesser.It+harnesser.GetVerb("attempt")+" to harness "+harnessee.the+harnessee.ShortDisplayName+" with the saddle!");

			if (harnessee.pBrain != null)
			{
				harnessee.pBrain.AdjustFeeling(harnesser, -100);
			}
            if(harnessee.MakeSave("Strength", 8, harnesser, "Strength", "Saddle Harness")){
				IPart.AddPlayerMessage(harnessee.The+harnessee.ShortDisplayName+harnessee.GetVerb("avoid")+" the saddle harness!");
			}else{

				ParentObject.ForceUnequipAndRemove(true);
				harnessee.ForceEquipObject(ParentObject,"Riding",true);
				IPart.AddPlayerMessage(harnesser.It+harnesser.GetVerb("harness")+" "+harnessee.the+harnessee.ShortDisplayName+" with the saddle!");

			}

        }

         public override bool FireEvent(Event E)
		{
            
			if (E.ID == "Equipped")
			{
				GameObject GO = E.GetGameObjectParameter("EquippingObject");

                acegiak_Ridable ridable = new acegiak_Ridable();
                if(this.AltTile != null){
                    ridable.AltTile = this.AltTile;
                }
                GO.AddPart(ridable);
			}
			else if (E.ID == "Unequipped")
			{
				GameObject GO = E.GetGameObjectParameter("UnequippingObject");;
                GO.RemovePart<acegiak_Ridable>();
			}
			// else if (E.ID == "GetInventoryActions")
			// {
			// 	E.GetParameter<EventParameterGetInventoryActions>("Actions").AddAction("Harness", 'H',  true, "&WH&yarness a creature", "InvCommandHarness", 10);
                
			// }
			// else if (E.ID == "Dropped")
			// {
			// 	GameObject taking = E.GetGameObjectParameter("DroppingObject");
			// 	taking.UnregisterPartEvent(this,"InvCommandHarness");
			// }
			// else if (E.ID == "AddedToInventory")
			// {
			// 	GameObject taking = E.GetGameObjectParameter("TakingObject");
			// 	taking.RegisterPartEvent(this,"InvCommandHarness");
			// }


			if (E.ID == "ProjectileHit" || E.ID == "ThrownProjectileHit" || E.ID == "WeaponAfterDamage")
			{
				GameObject defender = E.GetGameObjectParameter("Defender");
				GameObject attacker = E.GetGameObjectParameter("Attacker");
				if (E.GetIntParameter("Penetrations", 0) > 0 && !IsBroken() && !IsRusted())
				{
					if (defender != null && defender.baseHitpoints >= 1)
					{
						ParentObject.FireEvent(Event.New("InvCommandHarness", "Owner", defender, "Attacker", attacker));
					}
				}
			}

			if (E.ID == "InvCommandHarness")
			{
				GameObject defender = E.GetGameObjectParameter("Owner");
				GameObject attacker = E.GetGameObjectParameter("Attacker");
				
				Harness(attacker,defender);
				
				
			}
			return base.FireEvent(E);
		}
	}
}
