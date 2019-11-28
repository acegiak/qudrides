using System;
using System.Text;
using XRL.Core;
using XRL.Rules;
using XRL.World.Parts.Effects;
using XRL.World.Parts.Mutation;
using XRL.World.Parts.Skill;
using System.Collections.Generic;
using XRL.UI;

namespace XRL.World.Parts
{
	[Serializable]
	public class acegiak_MotorProp : IActivePart{

		public int PlumeLevel = 3;

        public ActivatedAbilityEntry Ability = null;

		public Guid ActivatedAbilityID = Guid.Empty;


		public acegiak_MotorProp()
		{
			WorksOnEquipper = true;
		}


        public bool AddSkill(GameObject GO)
        {
            ActivatedAbilities pAA = GO.GetPart("ActivatedAbilities") as ActivatedAbilities;

            if (pAA != null)
            {
                ActivatedAbilityID = pAA.AddAbility("Hit The Gas", "CommandAcegiakGasGasGas", "Maneuvers", -1,  "While gassing it you move 2-4 spaces each turn.", "-");
                Ability = pAA.AbilityByGuid[ActivatedAbilityID];
            }

            return true;
        }

        public bool RemoveSkill(GameObject GO)
        {
            if (ActivatedAbilityID != Guid.Empty)
            {
                ActivatedAbilities pAA = GO.GetPart("ActivatedAbilities") as ActivatedAbilities;
                pAA.RemoveAbility(ActivatedAbilityID);
            }

            return true;
        }

        public new bool IsActivePartEngaged()
		{
			if (!Ability.ToggleState)
			{
				return false;
			}
			return base.IsActivePartEngaged();
		}

		public override void Register(GameObject Object)
		{
			Object.RegisterPartEvent(this, "CellChanged");
			Object.RegisterPartEvent(this, "EffectApplied");
			Object.RegisterPartEvent(this, "EffectRemoved");
			Object.RegisterPartEvent(this, "EndTurn");
			Object.RegisterPartEvent(this, "BeginTakeAction");
			Object.RegisterPartEvent(this, "Equipped");
			Object.RegisterPartEvent(this, "GetShortDescription");
			Object.RegisterPartEvent(this, "Unequipped");
			base.Register(Object);
		}

        public bool GasCheck(){
			if(!Ability.ToggleState){
				return false;
			}
            if(!IsReady(UseCharge: true)){
                IPart.AddPlayerMessage(ParentObject.The+ParentObject.DisplayNameOnly+" sputters and stalls.");
                
                Ability.ToggleState = false;
            }
            return Ability.ToggleState;
        }

		public override bool FireEvent(Event E){
            if (E.ID == "GetShortDescription")
			{
				StringBuilder stringBuilder = Event.NewStringBuilder();
				ActivePartStatus activePartStatus = GetActivePartStatus();
				stringBuilder.Append("\n&CBurns oil to motor forwards at high speeds.");
				if (activePartStatus == ActivePartStatus.EMP)
				{
					stringBuilder.Append(" (&WEMP&C)");
				}
				else if (activePartStatus == ActivePartStatus.Unpowered)
				{
					stringBuilder.Append(" (&Kunpowered&C)");
				}
				else if (activePartStatus == ActivePartStatus.Booting && ParentObject.GetPart<BootSequence>().IsObvious())
				{
					stringBuilder.Append(" (&bwarming up&C)");
				}
				else if (activePartStatus != 0 && activePartStatus != ActivePartStatus.NeedsSubject)
				{
					stringBuilder.Append(" (&rnonfunctional&C)");
				}
				E.AddParameter("Postfix", E.GetStringParameter("Postfix") + stringBuilder.ToString());
			}
			if (E.ID == "CommandMove" && GasCheck())
			{
                string direction = E.GetStringParameter("Direction");
                string source = E.GetStringParameter("Source");

                if(source != "Gas"){

                    int num3 = 10;
                    GameObject riderObject = ParentObject.pPhysics.Equipped;
					string text3 = riderObject.pRender.Tile;

					string colorString = (string.IsNullOrEmpty(riderObject.pRender.TileColor) ? riderObject.pRender.ColorString : riderObject.pRender.TileColor);
					string detailColor = riderObject.pRender.DetailColor;
                    string text2 = riderObject.pRender.ColorString + riderObject.pRender.RenderString;
                    for(int i = Stat.Rnd2.Next(2,5);i>0;i--){

                        if (riderObject.FireEvent(Event.New("CommandMove", "Direction", direction, "EnergyCost", 0,"Source","Gas")))
                        {
                            if (Visible())
                            {
                                int num4 = XRL.Rules.Stat.RandomCosmetic(1, 3);
                                string text = string.Empty;
                                if (num4 == 1)
                                {
                                    text = string.Empty + '°';
                                }
                                if (num4 == 2)
                                {
                                    text = string.Empty + '±';
                                }
                                if (num4 == 3)
                                {
                                    text = string.Empty + '²';
                                }
                                    riderObject.ParticleBlip("&Y^K"+text, 3);
                            }
                        }
                    }
                }


			}
			if (E.ID == "EndTurn")
			{
				if (Ability.ToggleState)
				{
					ParentObject.pPhysics.Equipped.Smoke();
				}
			}
			if (E.ID == "AfterMoved")
			{
				Cell cell = E.GetParameter("FromCell") as Cell;
				Cell currentCell = ParentObject.GetCurrentCell();
				if (cell != null && cell.ParentZone == currentCell.ParentZone && PlumeLevel > 0 &&  IsReady())
				{
					string directionFromCell = currentCell.GetDirectionFromCell(cell);
					Cell cell2 = cell.GetCellFromDirection(directionFromCell);
					if (cell2 == null || cell2.ParentZone != currentCell.ParentZone)
					{
						cell2 = cell;
					}
					cell2?.ParticleBlip("&r^W" + (char)(219 + Stat.Random(0, 4)), 6);
					cell?.ParticleBlip("&R^W" + (char)(219 + Stat.Random(0, 4)), 3);
				}
			}
			if (E.ID == "AIGetOffensiveMutationList")
			{
                if (ParentObject.pPhysics != null && ParentObject.pPhysics.IsFrozen()) return true;
                List<XRL.World.AI.GoalHandlers.AICommandList> CommandList = (List<XRL.World.AI.GoalHandlers.AICommandList>)E.GetParameter("List");
                if (Ability != null) CommandList.Add(new XRL.World.AI.GoalHandlers.AICommandList("CommandAcegiakGasGasGas", 1));
                return true;
			}
            if (E.ID == "CommandAcegiakGasGasGas")
			{

				if (Ability == null)
				{
					return true;
				}
				Ability.ToggleState = !Ability.ToggleState;

			}
			if (E.ID == "Equipped")
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("EquippingObject");
				AddSkill(gameObjectParameter);
				gameObjectParameter.RegisterPartEvent(this, "CommandMove");
				gameObjectParameter.RegisterPartEvent(this, "CommandAcegiakGasGasGas");
			}
			if (E.ID == "Unequipped")
			{
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("UnequippingObject");
				RemoveSkill(gameObjectParameter2);
				gameObjectParameter2.UnregisterPartEvent(this, "CommandMove");
				gameObjectParameter2.UnregisterPartEvent(this, "CommandAcegiakGasGasGas");
			}
			return base.FireEvent(E);

        }

    }

}