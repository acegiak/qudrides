using Qud.API;
using System;
using XRL.Rules;
using XRL.UI;
using XRL.Core;
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
			Object.RegisterPartEvent(this, "TakeDamage");
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

		public void ChanceToFall(GameObject rider, int chance){
			if (Stat.Rnd2.Next(0, 101) <= chance)
			{
				if(rider.IsPlayer()){
					IPart.AddPlayerMessage("You fall from "+ParentObject.the+ParentObject.DisplayNameOnly);
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
  
            if (E.ID == "EnteredCell" && boot)
            {
                boot = false;
                if(ParentObject.GetPart<Brain>() == null){
                    return false;
                }

                XRLCore.Core.Game.ActionManager.AddActiveObject(ParentObject);

            }
            if (E.ID == "TakeDamage")
            {
                
                GameObject rider = ParentObject.pPhysics.Equipped;
				if(rider != null){
					Damage damage = E.GetParameter("Damage") as Damage;
					ChanceToFall(rider,damage.Amount);
				}


            }
			if (E.ID == "Equipped")
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("EquippingObject");
				BodyPart part = E.GetParameter("EquippingObject") as BodyPart;
				if(part.Type=="Riding"){
					IPart.AddPlayerMessage("Riding!");
					gameObjectParameter.RegisterPartEvent(this, "TakeDamage");

				}
			}
			if (E.ID == "Unequipped")
			{
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("UnequippingObject");
				gameObjectParameter2.RegisterPartEvent(this, "TakeDamage");
			}

			// if(E.ID == 	Object.RegisterPartEvent(this, "TakeDamage")){
			// 	Damage damage = E.GetParameter("Damage") as Damage;
			// 	ChanceToFall(damage.amount);
			// }
			return base.FireEvent(E);
		}
	}
}
