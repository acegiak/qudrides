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
			Object.RegisterPartEvent(this, "InvCommandHarness");
			base.Register(Object);
		}
        public void Harness(GameObject harnesser,GameObject harnessee){
			if (harnessee.pBrain != null)
			{
				harnessee.pBrain.AdjustFeeling(harnesser, -100);
			}
            if(harnessee.MakeSave("Strength", 20, harnesser, null, "Saddle Harness")){
				IPart.AddPlayerMessage(harnessee.The+harnessee.ShortDisplayName+harnessee.GetVerb("avoid")+" the saddle harness!");
			}else{

				Event @event = Event.New("CommandForceEquipObject");
				@event.AddParameter("Object", this);
				@event.AddParameter("BodyPart", "back");
				@event.SetSilent(Silent: true);
				ParentObject.FireEvent(@event);
				IPart.AddPlayerMessage(harnesser.It+harnesser.GetVerb("harness")+harnessee.the+harnessee.ShortDisplayName+" with the saddle!");

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
			else if (E.ID == "GetInventoryActions")
			{
				//E.GetParameter<EventParameterGetInventoryActions>("Actions").AddAction("Harness", 'm',  true, "&Wh&yarness a creature", "InvCommandHarness", 10);
                
			}
			if (E.ID == "InvCommandHarness")
			{
				if (ParentObject.pPhysics != null && ParentObject.pPhysics.IsFrozen())
				{
					if (ParentObject.IsPlayer())
					{
						Popup.Show("You are frozen solid!");
					}
				}
				else
				{
					string text = PickDirectionS();
					if (!string.IsNullOrEmpty(text))
					{
						Cell cellFromDirection = ParentObject.pPhysics.CurrentCell.GetCellFromDirection(text);
						if (cellFromDirection != null)
						{
							GameObject combatTarget = cellFromDirection.GetCombatTarget(ParentObject);
							if (combatTarget != null && combatTarget != ParentObject)
							{
                				Harness(E.GetGameObjectParameter("Owner"),combatTarget);
							}
						}
					}
				}
				E.RequestInterfaceExit();
			}
			return base.FireEvent(E);
		}
	}
}
