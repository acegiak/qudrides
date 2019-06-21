using Qud.API;
using System;
using XRL.Rules;
using XRL.UI;
using XRL.Core;
using XRL.World.Capabilities;
namespace XRL.World.Parts
{
	[Serializable]
	public class acegiak_Ridable : IPart
	{
        public String AltTile;
        public String SavedTile;

        public int SavedSpeed = 100;
        public bool boot = false;


		public acegiak_Ridable()
		{
		}

		public override bool SameAs(IPart p)
		{
			return false;
		}


		public override void Register(GameObject Object)
		{
			Object.RegisterPartEvent(this, "CommandSmartUse");
			Object.RegisterPartEvent(this, "CanHaveSmartUseConversation");
			Object.RegisterPartEvent(this, "CanSmartUse");
			Object.RegisterPartEvent(this, "GetInventoryActions");
			Object.RegisterPartEvent(this, "InvCommandMount");
			Object.RegisterPartEvent(this, "InvCommandDismount");
			Object.RegisterPartEvent(this, "GetWeight");
			Object.RegisterPartEvent(this, "Dropped");
			Object.RegisterPartEvent(this, "EnteredCell");
			Object.RegisterPartEvent(this, "CommandAttack");
			Object.RegisterPartEvent(this, "Equipped");
			Object.RegisterPartEvent(this, "Unequipped");
			base.Register(Object);
		}

        public void Mount(GameObject rider){
			rider.FireEvent(Event.New("CommandForceEquipObject", "Object", ParentObject, "BodyPart", rider.GetPart<Body>().GetBody().GetFirstPart("Riding")));

            this.SavedTile = rider.pRender.Tile;
            rider.pRender.Tile = this.AltTile;
            

			SavedSpeed = -1*(ParentObject.Statistics["MoveSpeed"].Value - rider.Statistics["MoveSpeed"].Value);
            rider.Statistics["MoveSpeed"].Max = ParentObject.Statistics["MoveSpeed"].Max;
            if(SavedSpeed >0){
                rider.Statistics["MoveSpeed"].Bonus += SavedSpeed;
            }else{
                rider.Statistics["MoveSpeed"].Penalty += -SavedSpeed;
            }

        }
        public void Dismount(GameObject rider){
			rider.FireEvent(Event.New("CommandForceUnequipObject", "BodyPart", rider.GetPart<Body>().GetBody().GetFirstPart("Riding")));
            rider.FireEvent(Event.New("PerformDrop", "Object",ParentObject));
            rider.CurrentCell.GetRandomLocalAdjacentCell().AddObject(ParentObject);
            rider.pRender.Tile = this.SavedTile;
            if(SavedSpeed >0){
                rider.Statistics["MoveSpeed"].Bonus -= SavedSpeed;
            }else{
                rider.Statistics["MoveSpeed"].Penalty -= -SavedSpeed;
            }
        }

		public void ChanceToFall(GameObject rider, float chance, GameObject mount){
			if(mount.GetPart<Brain>() != null){
				float feeling = mount.GetPart<Brain>().GetFeeling(rider);
				chance += (feeling*-1)*0.01f;
				int? difference = DifficultyEvaluation.GetDifficultyRating(mount,rider);
				if(difference == null){
					difference = 0;
				}
				chance += (difference*0.01f).GetValueOrDefault(0f);

				if(rider.GetPart<Body>() != null){
					List<BodyPart> parts = rider.GetPart<Body>().GetParts();
					float limblessfall = 10f;
					foreach(BodyPart part in parts){
						if(part.Type == "Arm" ||
						part.Type == "Feet" ||
						part.Type == "Tail"){
							limblessfall= limblessfall/10f;
						}
					}
					chance += limblessfall;
				}
			}

			Double roll = Stat.Rnd2.NextDouble()*100;
			//IPart.AddPlayerMessage("fall?"+roll.ToString()+"/"+chance.ToString());
			if ( roll <= chance)
			{
				if(rider.IsPlayer()){
					IPart.AddPlayerMessage("You fall from "+mount.the+mount.DisplayNameOnly);
				}
				Dismount(rider);
			}
		}

        public override bool FireEvent(Event E)
		{
			if (E.ID == "CanSmartUse")
			{
				if (!E.GetGameObjectParameter("User").IsPlayer() )
				{
					return false;
				}
			}
			else if (E.ID == "CommandSmartUse")
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("User");
				if (!gameObjectParameter.IsPlayer() || !ParentObject.IsPlayerLed())
				{
                    if(ParentObject.Equipped != null){
                        Mount(E.GetGameObjectParameter("Owner"));
                    }else{
                        Dismount(E.GetGameObjectParameter("Owner"));
                    }
				}
			}
			else if (E.ID == "GetInventoryActions")
			{
                if(ParentObject.Equipped != null){
				    E.GetParameter<EventParameterGetInventoryActions>("Actions").AddAction("Dismount", 'm',  false, "dis&Wm&yount", "InvCommandDismount", 10);
                }else{
				    E.GetParameter<EventParameterGetInventoryActions>("Actions").AddAction("Mount", 'm', false, "&Wm&yount", "InvCommandMount", 10);
                }
			}
			if (E.ID == "InvCommandMount")
			{
                Mount(E.GetGameObjectParameter("Owner"));
				E.RequestInterfaceExit();
			}
			if (E.ID == "InvCommandDismount")
			{
                Dismount(E.GetGameObjectParameter("Owner"));
				E.RequestInterfaceExit();
			}
            if (E.ID == "GetWeight" && ParentObject.Equipped != null)
			{
                // int carrying = ParentObject.GetPart<Body>().GetWeight();
                // if(ParentObject.GetPart<Inventory>() != null){
                //     carrying += ParentObject.GetPart<Inventory>().GetWeight();
                // }
				// E.SetParameter("Weight", -1*(Stats.GetMaxWeight(ParentObject)-carrying));
				E.SetParameter("Weight", -150);
				return false;
			}
            if (E.ID == "Dropped")
            {
                boot = true;
            }
  
            if (E.ID == "EnteredCell" )
            {
				if(boot){

					boot = false;
					if(ParentObject.GetPart<Brain>() == null){
						return false;
					}

					XRLCore.Core.Game.ActionManager.AddActiveObject(ParentObject);
				}
				GameObject rider = ParentObject.pPhysics.Equipped;

				if(rider != null){
					ChanceToFall(rider,0.1f, ParentObject);
				}

            }
            if (E.ID == "TakeDamage")
            {
                
                GameObject rider = ParentObject.pPhysics.Equipped;
				if(rider != null){
					Damage damage = E.GetParameter("Damage") as Damage;
					ChanceToFall(rider,damage.Amount, ParentObject);
				}
            }
			if (E.ID == "Equipped")
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("EquippingObject");
				string Type = E.GetStringParameter("SlotType");
				BodyPart bp = E.GetParameter("BodyPart") as BodyPart;
				if(bp != null){Type = bp.Type;}
				//IPart.AddPlayerMessage("slot: "+Type);
				if(Type=="Riding"){
					//IPart.AddPlayerMessage("Riding!");
					gameObjectParameter.RegisterPartEvent(this, "TakeDamage");
					gameObjectParameter.RegisterPartEvent(this, "CellChanged");
					gameObjectParameter.RegisterPartEvent(this, "EnteredCell");

				}
			}
			if (E.ID == "Unequipped")
			{
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("UnequippingObject");
				gameObjectParameter2.UnregisterPartEvent(this, "TakeDamage");
				gameObjectParameter2.UnregisterPartEvent(this, "CellChanged");
				gameObjectParameter2.UnregisterPartEvent(this, "EnteredCell");
			}

			// if(E.ID == 	Object.RegisterPartEvent(this, "TakeDamage")){
			// 	Damage damage = E.GetParameter("Damage") as Damage;
			// 	ChanceToFall(damage.amount);
			// }
			return base.FireEvent(E);
		}
	}
}
